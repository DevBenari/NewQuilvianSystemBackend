using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Pengesahan dan penguncian catatan sesi oleh dokter penanggung jawab sesi (<c>BE-HMD-17</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satu transaksi utuh.</b> Kelengkapan diperiksa, dokumen didaftarkan ke daftar keutuhan
    /// Rekam Medis sebagai jenis <c>HemodialysisSession</c> yang tertanda tangan, sidik jari isi
    /// klinisnya dihitung, sesi menjadi <c>Finalized</c>, dan <c>TrxPatientProcedure</c> menjadi
    /// <c>Completed</c>. Bila satu langkah gagal, seluruhnya dibatalkan dan sesi tetap
    /// <c>AwaitingFinalization</c>.
    /// </para>
    /// <para>
    /// <b>Kewenangan yang melekat pada data, bukan pada peran.</b> Butir hak akses
    /// <c>HemodialysisRecord : Finalize</c> hanya menjawab "boleh mengesahkan". Yang boleh
    /// mengesahkan sesi <i>ini</i> hanyalah dokter yang tercatat sebagai penanggung jawabnya.
    /// Contoh: Dokter B memegang butir itu, tetapi penanggung jawab sesi Pasien C adalah Dokter A —
    /// Dokter B ditolak <c>403 HMD-VAL-072</c>.
    /// </para>
    /// <para>
    /// <b>Dua pelaku, dua kolom</b> (syarat 2). Penulis dokumen pada daftar keutuhan adalah perawat
    /// yang menyelesaikan dokumentasi (<c>DocumentedByUserId</c>), sedangkan penanda tangannya dokter
    /// penanggung jawab (<c>SignedByUserId</c>). Karena penulisnya perawat, perawat itulah yang dapat
    /// menambahkan koreksi lewat addendum Rekam Medis — catatan asli tetap utuh di sampingnya.
    /// </para>
    /// <para>
    /// <b>Billing di luar transaksi.</b> Setelah transaksi di-<c>commit</c>, penyerahan ke Billing
    /// dijalankan terpisah; kegagalannya tidak membuka kembali catatan.
    /// </para>
    /// </remarks>
    public class HmdSessionFinalizationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalDocumentIntegrityService _integrityService;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly HmdBillingHandoffService _billingHandoffService;

        public HmdSessionFinalizationService(
            ApplicationDbContext dbContext,
            ClinicalDocumentIntegrityService integrityService,
            InpatientClinicalContextService clinicalContextService,
            HmdBillingHandoffService billingHandoffService)
        {
            _dbContext = dbContext;
            _integrityService = integrityService;
            _clinicalContextService = clinicalContextService;
            _billingHandoffService = billingHandoffService;
        }

        /// <summary>Dokter penanggung jawab sesi mengesahkan dan mengunci catatan sesi.</summary>
        public async Task<HmdResult<HmdSessionResponse>> FinalizeAsync(
            Guid id,
            FinalizeHmdSessionRequest request,
            HmdActor actor,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var locked = await SignWithinTransactionAsync(id, actor, deviceInfo, ipAddress, cancellationToken);
            if (!locked.IsSuccess)
                return locked;

            // Transaksi pengesahan sudah di-commit. Baru sekarang Billing diberi tahu.
            if (locked.Value!.BillingHandoffStatus == HmdBillingHandoffStatus.Pending)
                await _billingHandoffService.HandOffAsync(id, actor.UserId, cancellationToken);

            return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Dokter penanggung jawab mengembalikan dokumentasi kepada perawat untuk dilengkapi, dengan
        /// alasan wajib (<c>HMD-VAL-071</c>). Sesi kembali ke <c>Completed</c> atau <c>Stopped</c>.
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> ReturnForCompletionAsync(
            Guid id,
            ReturnHmdSessionRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.Val071, HmdMessages.Val071);

            var session = await _dbContext.HmdSessions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (session == null)
                return NotFound();

            if (session.SessionStatus == HmdSessionStatus.Finalized)
                return HmdResult<HmdSessionResponse>.Locked(HmdErrorCodes.Val075, HmdMessages.Val075);

            if (session.SessionStatus != HmdSessionStatus.AwaitingFinalization)
                return InvalidState(session.SessionStatus);

            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actor.Principal, actor.UserId, cancellationToken);
            if (!doctorId.HasValue || doctorId.Value != session.ResponsibleDoctorId)
            {
                return HmdResult<HmdSessionResponse>.Forbidden(
                    HmdErrorCodes.Val072, "Pengembalian dokumentasi hanya dapat dilakukan dokter penanggung jawab sesi.");
            }

            var now = DateTime.UtcNow;
            session.SessionStatus = session.StopReason.HasValue ? HmdSessionStatus.Stopped : HmdSessionStatus.Completed;
            session.ReturnReason = reason;
            session.DocumentedByUserId = null;
            session.DocumentedAt = null;
            session.UpdateDateTime = now;
            session.UpdateBy = actor.UserId;
            session.Version++;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }

            return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);
        }

        private async Task<HmdResult<HmdSessionResponse>> SignWithinTransactionAsync(
            Guid id,
            HmdActor actor,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await HmdServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"HMD_SESSION_{id:N}");

            var session = await _dbContext.HmdSessions
                .Include(x => x.Episode)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (session == null)
                return NotFound();

            if (session.SessionStatus == HmdSessionStatus.Finalized)
                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.Val075, "Catatan sesi ini sudah disahkan.");

            if (session.SessionStatus != HmdSessionStatus.AwaitingFinalization)
                return InvalidState(session.SessionStatus);

            // HMD-VAL-072: hanya dokter penanggung jawab SESI INI, diturunkan dari relasi akun.
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actor.Principal, actor.UserId, cancellationToken);
            if (!doctorId.HasValue || !session.ResponsibleDoctorId.HasValue || doctorId.Value != session.ResponsibleDoctorId.Value)
                return HmdResult<HmdSessionResponse>.Forbidden(HmdErrorCodes.Val072, HmdMessages.Val072);

            var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, session.Episode!.ServiceUnitId, cancellationToken);
            if (setting == null)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.SettingMissing, HmdMessages.SettingMissing);

            if (setting.RequireDifferentSigner && session.DocumentedByUserId == actor.UserId)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val074, HmdMessages.Val074);

            var missing = await HmdSessionService.FindMissingDocumentationAsync(session, _dbContext, cancellationToken);
            if (missing.Count > 0 || !session.DocumentedByUserId.HasValue || !session.EncounterId.HasValue)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val070, HmdMessages.Val070, missing);

            var now = DateTime.UtcNow;

            try
            {
                await _integrityService.RegisterCountersignedAsync(
                    ClinicalDocumentKind.HemodialysisSession,
                    session.Id,
                    session.Episode.PatientId,
                    session.EncounterId.Value,
                    authorUserId: session.DocumentedByUserId.Value,
                    signerUserId: actor.UserId,
                    deviceInfo: deviceInfo,
                    ipAddress: ipAddress,
                    nowUtc: now,
                    cancellationToken: cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val073, HmdMessages.Val073);
            }

            session.RecordHash = await ComputeRecordHashAsync(session, cancellationToken);
            session.SessionStatus = HmdSessionStatus.Finalized;
            session.SignedByUserId = actor.UserId;
            session.SignedAt = now;
            session.BillingHandoffStatus = session.StopReason.HasValue
                ? HmdBillingHandoffStatus.NotRequired
                : HmdBillingHandoffStatus.Pending;
            session.UpdateDateTime = now;
            session.UpdateBy = actor.UserId;
            session.Version++;

            if (session.PatientProcedureId.HasValue)
            {
                var procedure = await _dbContext.Set<TrxPatientProcedure>()
                    .FirstOrDefaultAsync(x => x.Id == session.PatientProcedureId.Value, cancellationToken);
                if (procedure != null)
                {
                    procedure.ProcedureStatus = PatientProcedureStatus.Completed;
                    procedure.CompletedAt = session.EndedAt ?? now;
                    procedure.IsExecuted = true;
                    procedure.ExecutedAt = session.EndedAt ?? now;
                    procedure.ExecutedByUserId = session.DocumentedByUserId;
                    if (session.StopReason.HasValue)
                        procedure.IsBillable = false;
                    procedure.UpdateDateTime = now;
                    procedure.UpdateBy = actor.UserId;
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }
            catch (DbUpdateException)
            {
                // Pendaftaran keutuhan atau penyimpanan gagal: seluruh langkah dibatalkan, sesi tetap
                // AwaitingFinalization (HMD-VAL-073).
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val073, HmdMessages.Val073);
            }

            return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Sidik jari SHA-256 isi klinis sesi pada saat pengesahan. Disimpan pada
        /// <c>HmdSession.RecordHash</c> sebagai bukti keutuhan: perubahan apa pun terhadap isi sesi
        /// sesudahnya akan menghasilkan sidik jari yang berbeda.
        /// </summary>
        private async Task<string> ComputeRecordHashAsync(HmdSession session, CancellationToken cancellationToken)
        {
            var id = session.Id;

            var content = new
            {
                session.Id,
                session.SessionNumber,
                session.EpisodeId,
                session.PrescriptionId,
                session.EncounterId,
                session.MachineId,
                session.StationId,
                session.ResponsibleDoctorId,
                session.StartedAt,
                session.EndedAt,
                session.ActualDurationMinutes,
                session.ActualUltrafiltrationMl,
                session.StopReason,
                session.StopNote,
                session.DeviationNote,
                session.Disposition,
                session.DispositionNote,
                session.DocumentedByUserId,
                session.DocumentedAt,
                Checklist = await _dbContext.HmdSessionChecklists.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.ChecklistItemId)
                    .Select(x => new { x.ChecklistItemId, x.Result, x.IsOverridden, x.OverrideReason, x.VerifiedByUserId })
                    .ToListAsync(cancellationToken),
                Assessments = await _dbContext.HmdSessionAssessments.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.Phase)
                    .Select(x => new { x.Phase, x.BodyWeightKg, x.PatientVitalSignId, x.Complaint, x.AccessConditionNote, x.PatientCondition, x.TargetAchieved })
                    .ToListAsync(cancellationToken),
                Observations = await _dbContext.HmdSessionObservations.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.SequenceNumber)
                    .Select(x => new
                    {
                        x.SequenceNumber, x.ObservedAt, x.SystolicBp, x.DiastolicBp, x.PulseRate, x.RespiratoryRate,
                        x.TemperatureC, x.OxygenSaturation, x.BloodFlowRate, x.DialysateFlowRate,
                        x.TransmembranePressure, x.VenousPressure, x.ArterialPressure, x.UltrafiltrationVolumeMl, x.Note
                    })
                    .ToListAsync(cancellationToken),
                Medications = await _dbContext.HmdSessionMedications.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.AdministeredAt).ThenBy(x => x.Id)
                    .Select(x => new { x.DrugId, x.Dose, x.DoseUnit, x.Route, x.AdministeredAt, x.AdministeredByUserId, x.Note })
                    .ToListAsync(cancellationToken),
                Complications = await _dbContext.HmdSessionComplications.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.DetectedAt).ThenBy(x => x.Id)
                    .Select(x => new { x.ComplicationType, x.DetectedAt, x.SignsAndSymptoms, x.Intervention, x.Outcome, x.SessionImpact })
                    .ToListAsync(cancellationToken)
            };

            var json = JsonSerializer.Serialize(content);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        }

        private async Task<HmdSessionResponse?> ReadSessionAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await HmdSessionProjection.Project(_dbContext.HmdSessions.AsNoTracking().Where(x => x.Id == id))
                .FirstOrDefaultAsync(cancellationToken);
            return row == null ? null : HmdSessionProjection.Label(row);
        }

        private static HmdResult<HmdSessionResponse> NotFound() =>
            HmdResult<HmdSessionResponse>.NotFound("Sesi hemodialisa tidak ditemukan atau sudah dihapus.");

        private static HmdResult<HmdSessionResponse> InvalidState(HmdSessionStatus current) =>
            HmdResult<HmdSessionResponse>.Rule(
                HmdErrorCodes.InvalidTransition,
                $"Tindakan ini tidak dapat dilakukan pada sesi berstatus {HmdLabels.SessionStatus(current).ToLowerInvariant()}.");
    }
}
