using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabBillingHandoffResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Penyerahan tindakan sesi terapi rehab ke Billing — <c>BE-RHB-013</c>, INT-RHB-08/09,
    /// <c>state-transition-matrix.md</c> §6. Disetujui pemilik ClinicalManagement/Billing lewat
    /// <c>RHB-OQ-015</c> (2026-11-09).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua langkah, mengikuti preseden Hemodialisa (<c>HmdBillingHandoffService</c>).</b>
    /// (1) <see cref="CreateProceduresAsync"/> membentuk satu <c>TrxPatientProcedure</c> per intervensi
    /// di <b>dalam</b> transaksi tanda tangan sesi (INT-RHB-08): <c>IdempotencyKey = RHB-SESINT-{intervensi:N}</c>,
    /// <c>ProcedureSource = RehabilitationSession</c>, <c>DoctorId = InstructingDoctorId</c> = dokter
    /// penanggung jawab episode. (2) <see cref="HandOffAsync"/> memancarkan fakta kelayakan charge per
    /// tindakan lewat <see cref="ClinicalMilestoneFactProducer"/> <b>sesudah</b> commit (INT-RHB-09).
    /// </para>
    /// <para>
    /// <b>Tanpa tagihan ganda.</b> Sidik jari fakta memakai id tindakan dan waktu tanda tangan yang
    /// tersimpan, sehingga pengulangan dijawab Billing <c>Replayed</c>. Kunci idempoten tindakan
    /// (indeks unik) mencegah tindakan kembar bila backfill diulang.
    /// </para>
    /// <para>
    /// <b>Kegagalan Billing tidak membuka kembali sesi.</b> Sesi tetap Final; hanya
    /// <c>BillingHandoffStatus</c> menjadi <c>Failed</c>/<c>Pending</c> untuk diulang lewat
    /// <c>POST /{id}/billing-handoff/retry</c>. Pesan galat hanya memuat kode penolakan dan jumlah
    /// tindakan, tanpa data klinis (AC 5).
    /// </para>
    /// </remarks>
    public class RhbBillingHandoffService
    {
        private const string UnitName = "Kali";
        private const int MaxErrorLength = 1000;

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalMilestoneFactProducer _factProducer;

        public RhbBillingHandoffService(ApplicationDbContext dbContext, ClinicalMilestoneFactProducer factProducer)
        {
            _dbContext = dbContext;
            _factProducer = factProducer;
        }

        public static string IdempotencyKey(Guid interventionId) => $"RHB-SESINT-{interventionId:N}";

        // =====================================================================
        // Langkah 1 — tindakan pasien (di dalam transaksi pemanggil)
        // =====================================================================

        /// <summary>
        /// Membentuk <c>TrxPatientProcedure</c> untuk setiap intervensi sesi yang belum punya tindakan,
        /// lalu menautkannya ke intervensi. Tidak menyimpan; pemanggil yang menyimpan dan meng-commit.
        /// Mengembalikan jumlah intervensi bertindakan (baru maupun yang sudah ada).
        /// </summary>
        public async Task<int> CreateProceduresAsync(RhbSession session, Guid actorUserId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (session.SessionStatus == RehabSessionStatus.NotDone)
                return 0;

            var interventions = await _dbContext.RhbSessionInterventions
                .Where(x => x.SessionId == session.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            if (interventions.Count == 0)
                return 0;

            var pending = interventions.Where(x => !x.PatientProcedureId.HasValue).ToList();
            if (pending.Count == 0)
                return interventions.Count;

            var episode = await _dbContext.RhbEpisodes.AsNoTracking()
                .Where(x => x.Id == session.EpisodeId)
                .Select(x => new { x.PatientId, x.ResponsibleDoctorId })
                .FirstAsync(cancellationToken);
            var encounter = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(x => x.Id == session.EncounterId)
                .Select(x => new { x.ServiceUnitId, x.ClinicId })
                .FirstOrDefaultAsync(cancellationToken);

            var procedureIds = pending.Select(x => x.ProcedureId).Distinct().ToList();
            var procedures = await _dbContext.Set<MstProcedure>().AsNoTracking()
                .Where(x => procedureIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            // Kunci yang sudah ada (mis. backfill yang terputus) dipakai ulang, bukan dibuat ganda.
            var keys = pending.Select(x => IdempotencyKey(x.Id)).ToList();
            var existing = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
                .Where(x => x.IdempotencyKey != null && keys.Contains(x.IdempotencyKey) && !x.IsDelete)
                .ToDictionaryAsync(x => x.IdempotencyKey!, x => x.Id, cancellationToken);

            var performedAt = session.StartedAt ?? session.SignedAt ?? nowUtc;
            var completedAt = session.EndedAt ?? session.SignedAt ?? nowUtc;

            foreach (var intervention in pending)
            {
                var key = IdempotencyKey(intervention.Id);
                if (existing.TryGetValue(key, out var existingId))
                {
                    intervention.PatientProcedureId = existingId;
                    continue;
                }

                procedures.TryGetValue(intervention.ProcedureId, out var master);
                var procedure = new TrxPatientProcedure
                {
                    EncounterId = session.EncounterId,
                    PatientId = episode.PatientId,
                    DoctorId = episode.ResponsibleDoctorId,
                    InstructingDoctorId = episode.ResponsibleDoctorId,
                    ServiceUnitId = encounter?.ServiceUnitId,
                    ClinicId = encounter?.ClinicId,
                    InpEpisodeId = session.InpEpisodeId,
                    IdempotencyKey = key,
                    ProcedureId = intervention.ProcedureId,
                    ProcedureCodeSnapshot = master?.ProcedureCode ?? string.Empty,
                    ProcedureNameSnapshot = master?.ProcedureName ?? string.Empty,
                    ProcedureTypeSnapshot = master?.ProcedureType,
                    ProcedureCategoryNameSnapshot = master?.ProcedureCategoryName,
                    ProcedureMasterType = "Master",
                    IsFromMasterProcedure = true,
                    ProcedureSource = PatientProcedureSource.RehabilitationSession,
                    ProcedureStatus = PatientProcedureStatus.Completed,
                    ProcedureDateTime = performedAt,
                    StartedAt = performedAt,
                    CompletedAt = completedAt,
                    Quantity = intervention.Quantity > 0 ? intervention.Quantity : 1,
                    UnitNameSnapshot = UnitName,
                    IsBillable = true,
                    IsExecuted = true,
                    ExecutedAt = completedAt,
                    ExecutedByUserId = session.TherapistUserId,
                    PerformedAt = performedAt,
                    PerformedByUserId = session.TherapistUserId,
                    OrderedByUserId = session.TherapistUserId,
                    // Hanya nomor sesi; parameter aktual dan respons pasien tetap di catatan rehab.
                    ClinicalNote = $"Sesi rehab {session.SessionNumber}.",
                    CreateDateTime = nowUtc,
                    CreateBy = actorUserId
                };

                _dbContext.Set<TrxPatientProcedure>().Add(procedure);
                intervention.PatientProcedureId = procedure.Id;
                intervention.UpdateDateTime = nowUtc;
                intervention.UpdateBy = actorUserId;
            }

            return interventions.Count;
        }

        // =====================================================================
        // Langkah 2 — fakta charge (di luar transaksi)
        // =====================================================================

        /// <summary>
        /// Memancarkan fakta kelayakan charge untuk setiap tindakan sesi. Wajib dipanggil di luar
        /// transaksi. Hasil gabungan: semua <c>Emitted</c>/<c>Replayed</c> → <c>Succeeded</c>; ada yang
        /// ditolak → <c>Failed</c>; selebihnya tidak pasti → <c>Pending</c>.
        /// </summary>
        public async Task HandOffAsync(Guid sessionId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await _dbContext.RhbSessions.FirstAsync(x => x.Id == sessionId, cancellationToken);
            if (session.RecordStatus != RehabRecordStatus.Final ||
                session.BillingHandoffStatus is RehabBillingHandoffStatus.Succeeded or RehabBillingHandoffStatus.NotRequired)
                return;

            var now = DateTime.UtcNow;
            var procedureIds = await _dbContext.RhbSessionInterventions.AsNoTracking()
                .Where(x => x.SessionId == sessionId && !x.IsDelete && x.PatientProcedureId != null)
                .Select(x => x.PatientProcedureId!.Value)
                .ToListAsync(cancellationToken);
            var procedures = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
                .Where(x => procedureIds.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            if (procedures.Count == 0)
            {
                session.BillingHandoffStatus = RehabBillingHandoffStatus.Failed;
                session.BillingHandoffError = "Tindakan pasien untuk sesi ini tidak ditemukan.";
                Touch(session, actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            var succeeded = 0;
            var unknown = 0;
            var rejectedCodes = new List<string>();
            var occurredAt = session.SignedAt ?? now;

            try
            {
                foreach (var procedure in procedures.Where(x => x.IsBillable))
                {
                    var emission = await _factProducer.EmitChargeEligibilityAsync(BuildFactRequest(procedure, occurredAt, sessionId), actorUserId, cancellationToken);
                    switch (emission.Kind)
                    {
                        case ClinicalFactEmissionKind.Emitted:
                        case ClinicalFactEmissionKind.Replayed:
                            succeeded++;
                            break;
                        case ClinicalFactEmissionKind.OutcomeUnknown:
                            unknown++;
                            break;
                        default:
                            rejectedCodes.Add(string.IsNullOrWhiteSpace(emission.Code) ? emission.Kind.ToString() : emission.Code!);
                            break;
                    }
                }

                // Hasil emisi bisa mengubah pelacak entity; muat ulang sesi sebelum menulis statusnya.
                session = await _dbContext.RhbSessions.FirstAsync(x => x.Id == sessionId, cancellationToken);
                var billable = procedures.Count(x => x.IsBillable);
                if (rejectedCodes.Count > 0)
                {
                    session.BillingHandoffStatus = RehabBillingHandoffStatus.Failed;
                    session.BillingHandoffError = Truncate(
                        $"Billing menolak {rejectedCodes.Count} dari {billable} tindakan ({string.Join(", ", rejectedCodes.Distinct())}). Periksa tarif tindakan, lalu ulangi.");
                }
                else if (unknown > 0)
                {
                    session.BillingHandoffStatus = RehabBillingHandoffStatus.Pending;
                    session.BillingHandoffError = $"Hasil penyerahan {unknown} dari {billable} tindakan belum dapat dipastikan. Ulangi beberapa saat lagi.";
                }
                else
                {
                    session.BillingHandoffStatus = RehabBillingHandoffStatus.Succeeded;
                    session.BillingHandoffAt = now;
                    session.BillingHandoffError = null;
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Billing gagal dihubungi. Catatan klinis tetap Final; status penyerahan saja yang gagal.
                DetachPending();
                session = await _dbContext.RhbSessions.FirstAsync(x => x.Id == sessionId, cancellationToken);
                session.BillingHandoffStatus = RehabBillingHandoffStatus.Failed;
                session.BillingHandoffError = $"Billing tidak dapat dihubungi ({exception.GetType().Name}). Ulangi beberapa saat lagi.";
            }

            Touch(session, actorUserId, now);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // =====================================================================
        // Endpoint
        // =====================================================================

        public async Task<R> GetStatusAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var response = await ReadAsync(sessionId, cancellationToken);
            return response == null ? R.NotFound("Sesi terapi tidak ditemukan.") : R.Ok(response);
        }

        /// <summary>
        /// Mengulang penyerahan (<c>Pending</c>/<c>Failed</c>) dan melakukan backfill untuk sesi final
        /// yang ditandatangani sebelum handoff tersedia (<c>NotRequired</c> tetapi punya intervensi tanpa
        /// tindakan, AC 4). Sesi <c>Succeeded</c> dijawab apa adanya.
        /// </summary>
        public async Task<R> RetryAsync(Guid sessionId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await _dbContext.RhbSessions.FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDelete, cancellationToken);
            if (session == null)
                return R.NotFound("Sesi terapi tidak ditemukan.");
            if (session.RecordStatus != RehabRecordStatus.Final)
                return R.Conflict(RhbErrorCodes.InvalidState, "Penyerahan ke Billing hanya dapat diulang setelah sesi ditandatangani.");

            if (session.BillingHandoffStatus == RehabBillingHandoffStatus.NotRequired)
            {
                var missing = await _dbContext.RhbSessionInterventions.AsNoTracking()
                    .AnyAsync(x => x.SessionId == sessionId && !x.IsDelete && x.PatientProcedureId == null, cancellationToken);
                if (session.SessionStatus != RehabSessionStatus.NotDone && missing)
                {
                    // Backfill: sesi lama dibentuk tindakannya dalam satu transaksi, lalu diserahkan.
                    var now = DateTime.UtcNow;
                    await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                    await RhbServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"RHB_SESSION_BILLING_{sessionId:N}");
                    var count = await CreateProceduresAsync(session, actorUserId, now, cancellationToken);
                    session.BillingHandoffStatus = count > 0 ? RehabBillingHandoffStatus.Pending : RehabBillingHandoffStatus.NotRequired;
                    Touch(session, actorUserId, now);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
            }

            if (session.BillingHandoffStatus is RehabBillingHandoffStatus.Pending or RehabBillingHandoffStatus.Failed)
                await HandOffAsync(sessionId, actorUserId, cancellationToken);

            return R.Ok((await ReadAsync(sessionId, cancellationToken))!);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>Bentuk fakta sama dengan <c>PatientProcedureController</c> dan Hemodialisa.</summary>
        private static ClinicalMilestoneFactRequest BuildFactRequest(TrxPatientProcedure procedure, DateTime occurredAt, Guid sessionId)
        {
            var hasQuantity = procedure.Quantity > 0 && !string.IsNullOrWhiteSpace(procedure.UnitNameSnapshot);
            return new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.ProcedureSourceContext,
                SourceAggregateId = procedure.Id,
                EffectType = BillingSourceContract.ProcedureChargeEffectType,
                EncounterId = procedure.EncounterId,
                OccurredAt = occurredAt,
                Quantity = hasQuantity ? procedure.Quantity : null,
                Unit = hasQuantity ? procedure.UnitNameSnapshot : null,
                TariffSnapshot = System.Text.Json.JsonSerializer.Serialize(new
                {
                    source = "ClinicalSnapshot",
                    procedureCode = procedure.ProcedureCodeSnapshot,
                    procedureName = procedure.ProcedureNameSnapshot,
                    unitPrice = procedure.UnitPrice,
                    totalPrice = procedure.TotalPrice,
                    isFreeOfCharge = procedure.IsFreeOfCharge,
                    isBillable = procedure.IsBillable
                }),
                CorrelationId = sessionId
            };
        }

        private async Task<RehabBillingHandoffResponse?> ReadAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var session = await _dbContext.RhbSessions.AsNoTracking()
                .Where(x => x.Id == sessionId && !x.IsDelete)
                .Select(x => new RehabBillingHandoffResponse
                {
                    SessionId = x.Id,
                    SessionNumber = x.SessionNumber,
                    SessionStatus = x.SessionStatus,
                    RecordStatus = x.RecordStatus,
                    BillingHandoffStatus = x.BillingHandoffStatus,
                    BillingHandoffAt = x.BillingHandoffAt,
                    BillingHandoffError = x.BillingHandoffError
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (session == null)
                return null;

            session.SessionStatusName = RhbSessionService.StatusName(session.SessionStatus);
            session.RecordStatusName = RhbEpisodeService.RecordStatusName(session.RecordStatus);
            session.BillingHandoffStatusName = RhbSessionService.BillingName(session.BillingHandoffStatus);

            session.Items = await (
                from intervention in _dbContext.RhbSessionInterventions.AsNoTracking()
                where intervention.SessionId == sessionId && !intervention.IsDelete
                join master in _dbContext.Set<MstProcedure>().AsNoTracking() on intervention.ProcedureId equals master.Id into masters
                from master in masters.DefaultIfEmpty()
                join procedure in _dbContext.Set<TrxPatientProcedure>().AsNoTracking() on intervention.PatientProcedureId equals (Guid?)procedure.Id into procedures
                from procedure in procedures.DefaultIfEmpty()
                orderby intervention.CreateDateTime
                select new RehabBillingHandoffItem
                {
                    InterventionId = intervention.Id,
                    ProcedureId = intervention.ProcedureId,
                    ProcedureCode = master != null ? master.ProcedureCode : null,
                    ProcedureName = master != null ? master.ProcedureName : null,
                    Quantity = intervention.Quantity,
                    PatientProcedureId = intervention.PatientProcedureId,
                    IsBillable = procedure != null ? procedure.IsBillable : null,
                    IsBillingGenerated = procedure != null ? procedure.IsBillingGenerated : null
                }).ToListAsync(cancellationToken);

            return session;
        }

        private static void Touch(RhbSession session, Guid actorUserId, DateTime nowUtc)
        {
            session.UpdateDateTime = nowUtc;
            session.UpdateBy = actorUserId;
        }

        private void DetachPending()
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    entry.State = EntityState.Detached;
            }
        }

        private static string Truncate(string value) => value.Length > MaxErrorLength ? value[..MaxErrorLength] : value;
    }
}
