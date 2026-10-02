using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Pencatatan pemberian obat — <c>BE-RWI-115</c>, <c>FR-KEP-064</c> s.d. <c>FR-KEP-068</c>,
    /// <c>VAL-KEP-30</c>, <c>VAL-KEP-32</c>, <c>VAL-KEP-33</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Contoh.</b> Ceftriaxone 1 g IV jadwal 08.00. Ns. Siti mencatat pukul 08.05 <c>Administered</c> 1 g IV →
    /// dosis <c>Administered</c>, pencatat Siti dari akun login. Pukul 20.00 <c>Held</c> "pasien muntah, lapor DPJP".
    /// Siti mencatat 2 g tanpa catatan penyimpangan → <c>400</c> "Dosis atau waktu berbeda dari jadwal. Isi
    /// catatan penyimpangan."
    /// </para>
    /// <para>
    /// <b>Serentak.</b> Setiap perpindahan status mengunci baris dosis (<c>FOR UPDATE</c>) di dalam transaksi,
    /// sehingga dua perawat yang menekan Simpan pada detik yang sama tidak menghasilkan dua pemberian — yang
    /// kedua menerima <c>409</c> "Dosis ini sudah dicatat."
    /// </para>
    /// </remarks>
    public partial class MedicationAdministrationService
    {
        public async Task<NursingResult<MedicationAdministrationResponse>> RecordAsync(
            Guid id,
            RecordMedicationAdministrationRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey ?? request.IdempotencyKey);
            var ulang = await FindReplayAsync(kunci, id, actorUserId, cancellationToken);

            if (ulang != null)
                return ulang;

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var dosis = await LockAsync(id, cancellationToken);

            if (dosis == null)
                return NotFound<MedicationAdministrationResponse>();

            var penjaga = await _writeGuard.EnsureCanWriteAsync(dosis.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<MedicationAdministrationResponse>();

            var item = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => x.Id == dosis.PrescriptionItemId)
                .Select(x => new { x.IsStopped, x.StoppedAt, x.DoseKind, x.DoseUnitSymbolSnapshot, x.DoseUnitNameSnapshot })
                .FirstAsync(cancellationToken);

            // VAL-KEP-29f — dosis insulin sliding scale lahir dari pelaksanaan. Pengecualian sempit: dosis yang
            // pelaksanaannya sudah tercatat dan cek gandanya ditolak dicatat ulang di sini; perhitungan sudah terjadi.
            if (dosis.DoseSource == MedicationDoseSource.SlidingScale || item.DoseKind == PrescriptionDoseKind.SlidingScale)
            {
                var sudahDihitung = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                    .AnyAsync(x => x.MedicationAdministrationId == dosis.Id && x.ExecutionStatus == SlidingScaleExecutionStatus.Recorded && !x.IsDelete, cancellationToken);

                if (!sudahDihitung)
                    return Conflict<MedicationAdministrationResponse>("Insulin sliding scale dicatat lewat layar sliding scale.", "SLIDING_SCALE_VIA_EXECUTION");
            }

            // VAL-KEP-30d
            if (dosis.DoseStatus != MedicationDoseStatus.Due || dosis.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending)
                return Conflict<MedicationAdministrationResponse>("Dosis ini sudah dicatat.", "DOSE_ALREADY_RECORDED");

            // VAL-KEP-30e
            if (item.IsStopped && item.StoppedAt.HasValue && dosis.ScheduledAt.HasValue && dosis.ScheduledAt.Value >= item.StoppedAt.Value)
                return Conflict<MedicationAdministrationResponse>("Resep obat ini sudah dihentikan dokter.", "PRESCRIPTION_ITEM_STOPPED");

            if (request.ExpectedRevisionNumber.HasValue && request.ExpectedRevisionNumber.Value != dosis.RevisionNumber)
                return Conflict<MedicationAdministrationResponse>("Data sudah diubah pengguna lain. Muat ulang.", "STALE_REVISION");

            var now = DateTime.UtcNow;
            var konteks = penjaga.Value!;

            if (request.DoseStatus == MedicationDoseStatus.Administered)
            {
                var galat = ValidateAdministration<MedicationAdministrationResponse>(request.ActualDose, request.ActualRoute, request.AdministeredAt, now);

                if (galat != null)
                    return galat;

                var diberikanPada = AsUtc(request.AdministeredAt!.Value);

                if (IsDeviation(dosis.PlannedDose, request.ActualDose!.Value, dosis.ScheduledAt, diberikanPada) && string.IsNullOrWhiteSpace(request.DeviationNote))
                    return BadRequest<MedicationAdministrationResponse>("Dosis atau waktu berbeda dari jadwal. Isi catatan penyimpangan.", "DEVIATION_NOTE_REQUIRED");

                dosis.ActualDose = request.ActualDose;
                dosis.ActualDoseUnitSnapshot = Truncate(dosis.PlannedDoseUnitSnapshot ?? item.DoseUnitSymbolSnapshot ?? item.DoseUnitNameSnapshot, 50);
                dosis.ActualRouteSnapshot = Truncate(request.ActualRoute, 100);
                dosis.AdministeredAt = diberikanPada;
                dosis.StatusReason = null;
                dosis.DeviationNote = Truncate(request.DeviationNote, 500);

                // VAL-KEP-31 — obat high-alert menunggu perawat kedua; status tetap Due.
                if (dosis.IsHighAlertSnapshot)
                {
                    dosis.DoseStatus = MedicationDoseStatus.Due;
                    dosis.DoubleCheckStatus = MedicationDoubleCheckStatus.Pending;
                }
                else
                {
                    dosis.DoseStatus = MedicationDoseStatus.Administered;
                    dosis.DoubleCheckStatus = MedicationDoubleCheckStatus.NotRequired;
                }
            }
            else if (request.DoseStatus is MedicationDoseStatus.Held or MedicationDoseStatus.Refused or MedicationDoseStatus.Missed)
            {
                // VAL-KEP-30b
                if (string.IsNullOrWhiteSpace(request.StatusReason))
                    return BadRequest<MedicationAdministrationResponse>($"Isi alasan dosis {DoseStatusLabel(request.DoseStatus).ToLowerInvariant()}.", "STATUS_REASON_REQUIRED");

                dosis.DoseStatus = request.DoseStatus;
                dosis.DoubleCheckStatus = MedicationDoubleCheckStatus.NotRequired;
                dosis.StatusReason = Truncate(request.StatusReason, 500);
                dosis.DeviationNote = Truncate(request.DeviationNote, 500);
                dosis.ActualDose = null;
                dosis.ActualDoseUnitSnapshot = null;
                dosis.ActualRouteSnapshot = null;
                dosis.AdministeredAt = null;
            }
            else
            {
                return BadRequest<MedicationAdministrationResponse>("Status pencatatan hanya Diberikan, Ditahan, Ditolak pasien, atau Terlewat.", "INVALID_DOSE_STATUS");
            }

            // Pencatatan ulang setelah cek ganda ditolak: jejak penolakan sudah tersimpan pada revisi.
            dosis.DoubleCheckedByEmployeeId = null;
            dosis.DoubleCheckedByUserId = null;
            dosis.DoubleCheckedAt = null;
            dosis.DoubleCheckNote = null;

            dosis.RecordedByEmployeeId = konteks.ActorEmployeeId;
            dosis.RecordedByUserId = actorUserId;
            dosis.RecordedAt = now;
            dosis.IdempotencyKey = kunci ?? dosis.IdempotencyKey;
            dosis.UpdateDateTime = now;
            dosis.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaksi.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException) when (kunci != null)
            {
                await transaksi.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return await FindReplayAsync(kunci, id, actorUserId, cancellationToken)
                       ?? Conflict<MedicationAdministrationResponse>("Dosis ini sudah dicatat.", "DOSE_ALREADY_RECORDED");
            }

            // StatusReason dan DeviationNote sensitif — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "MedicationAdministration.Record", "Perawat mencatat hasil dosis MAR.",
                new { dosis.Id, dosis.AdministrationNumber, dosis.DoseStatus, dosis.DoubleCheckStatus, RecordedBy = actorUserId });

            return NursingResult<MedicationAdministrationResponse>.Ok(
                await ToResponseAsync(dosis, actorUserId, false, cancellationToken),
                dosis.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending
                    ? "Pemberian tercatat dan menunggu cek ganda perawat kedua."
                    : "Hasil dosis berhasil dicatat.");
        }

        /// <summary>Pemberian obat sesuai kebutuhan — <c>FR-KEP-067</c>, <c>VAL-KEP-32a</c>/<c>32b</c>.</summary>
        public Task<NursingResult<MedicationAdministrationResponse>> RecordAsNeededAsync(
            RecordAsNeededAdministrationRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            RecordNewAdministrationAsync(
                asNeeded: true,
                request.PrescriptionItemId,
                request.ActualDose,
                request.ActualRoute,
                request.AdministeredAt,
                prnIndication: request.PrnIndication,
                unscheduledReason: null,
                deviationNote: request.DeviationNote,
                idempotencyKey ?? request.IdempotencyKey,
                user,
                actorUserId,
                cancellationToken);

        /// <summary>
        /// Pemberian butir berfrekuensi yang jadwalnya belum dikonfigurasi — <c>VAL-KEP-32c</c>, arsitektur 11.5.11
        /// "Selama kosong". Baris lahir sebagai <c>AsNeeded</c>; alasannya disimpan pada <c>DeviationNote</c>.
        /// </summary>
        public Task<NursingResult<MedicationAdministrationResponse>> RecordUnscheduledAsync(
            RecordUnscheduledAdministrationRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            RecordNewAdministrationAsync(
                asNeeded: false,
                request.PrescriptionItemId,
                request.ActualDose,
                request.ActualRoute,
                request.AdministeredAt,
                prnIndication: null,
                unscheduledReason: request.UnscheduledReason,
                deviationNote: null,
                idempotencyKey ?? request.IdempotencyKey,
                user,
                actorUserId,
                cancellationToken);

        private async Task<NursingResult<MedicationAdministrationResponse>> RecordNewAdministrationAsync(
            bool asNeeded,
            Guid prescriptionItemId,
            decimal? actualDose,
            string? actualRoute,
            DateTime? administeredAt,
            string? prnIndication,
            string? unscheduledReason,
            string? deviationNote,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var sebagaiPrn = asNeeded;
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey);
            var ulang = await FindReplayAsync(kunci, null, actorUserId, cancellationToken);

            if (ulang != null)
                return ulang;

            var item = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => x.Id == prescriptionItemId && !x.IsDelete && x.IsActive &&
                            !x.Prescription!.IsDelete &&
                            x.Prescription.InpEpisodeId != null &&
                            x.Prescription.PrescriptionStatus == PrescriptionStatus.Submitted &&
                            x.Prescription.PrescriptionOrderType != PrescriptionOrderType.Discharge)
                .Select(x => new
                {
                    x.Id,
                    x.PrescriptionId,
                    EpisodeId = x.Prescription!.InpEpisodeId!.Value,
                    x.DrugId,
                    x.Dose,
                    x.DoseUnitSymbolSnapshot,
                    x.DoseUnitNameSnapshot,
                    x.FrequencyCode,
                    x.IsAsNeeded,
                    x.IsStopped,
                    x.StoppedAt,
                    x.IsHighAlertSnapshot,
                    x.DoseKind
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return NursingResult<MedicationAdministrationResponse>.Fail(StatusCodes.Status404NotFound, "Butir resep rawat inap tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(item.EpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<MedicationAdministrationResponse>();

            var konteks = penjaga.Value!;

            // VAL-KEP-29f
            if (item.DoseKind == PrescriptionDoseKind.SlidingScale)
                return Conflict<MedicationAdministrationResponse>("Insulin sliding scale dicatat lewat layar sliding scale.", "SLIDING_SCALE_VIA_EXECUTION");

            if (sebagaiPrn)
            {
                // VAL-KEP-32a, 32b
                if (!item.IsAsNeeded)
                    return Conflict<MedicationAdministrationResponse>("Obat ini bukan obat sesuai kebutuhan.", "NOT_AS_NEEDED_ITEM");

                if (string.IsNullOrWhiteSpace(prnIndication))
                    return BadRequest<MedicationAdministrationResponse>("Isi indikasi pemberian.", "PRN_INDICATION_REQUIRED");
            }
            else
            {
                if (item.IsAsNeeded)
                    return Conflict<MedicationAdministrationResponse>("Obat ini obat sesuai kebutuhan. Catat lewat pemberian PRN.", "AS_NEEDED_ITEM");

                if (string.IsNullOrWhiteSpace(unscheduledReason))
                    return BadRequest<MedicationAdministrationResponse>("Isi alasan pemberian tanpa jadwal.", "UNSCHEDULED_REASON_REQUIRED");

                // VAL-KEP-32c
                if (!string.IsNullOrWhiteSpace(item.FrequencyCode))
                {
                    var jadwal = await ResolveScheduleAsync(new[] { item.FrequencyCode }, konteks.ServiceUnitId, cancellationToken);

                    if (jadwal.ContainsKey(NormalizeFrequencyCode(item.FrequencyCode)!))
                        return Conflict<MedicationAdministrationResponse>("Jadwal obat ini sudah tersedia. Catat pada kolom jam yang sesuai.", "SCHEDULE_ALREADY_CONFIGURED");
                }
            }

            var now = DateTime.UtcNow;
            var galat = ValidateAdministration<MedicationAdministrationResponse>(actualDose, actualRoute, administeredAt, now);

            if (galat != null)
                return galat;

            var diberikanPada = AsUtc(administeredAt!.Value);

            if (item.IsStopped && item.StoppedAt.HasValue && diberikanPada >= item.StoppedAt.Value)
                return Conflict<MedicationAdministrationResponse>("Resep obat ini sudah dihentikan dokter.", "PRESCRIPTION_ITEM_STOPPED");

            var catatan = sebagaiPrn ? Truncate(deviationNote, 500) : Truncate(unscheduledReason, 500);

            // VAL-KEP-30c — PRN berdosis berbeda dari resep wajib bercatatan penyimpangan.
            if (sebagaiPrn && actualDose!.Value != item.Dose && string.IsNullOrWhiteSpace(catatan))
                return BadRequest<MedicationAdministrationResponse>("Dosis atau waktu berbeda dari jadwal. Isi catatan penyimpangan.", "DEVIATION_NOTE_REQUIRED");

            var setting = await GetEffectiveSettingAsync(cancellationToken);
            string nomor;

            try
            {
                nomor = await AllocateNumberAsync(actorUserId, cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return NursingResult<MedicationAdministrationResponse>.Fail(StatusCodes.Status422UnprocessableEntity,
                    "Nomor dosis MAR gagal diterbitkan. Hubungi administrator sistem.");
            }

            var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.Id == item.EpisodeId)
                .Select(x => new { x.EncounterId, x.PatientId })
                .FirstAsync(cancellationToken);

            var dosis = new PhmMedicationAdministration
            {
                Id = Guid.NewGuid(),
                AdministrationNumber = nomor,
                PrescriptionId = item.PrescriptionId,
                PrescriptionItemId = item.Id,
                EncounterId = episode.EncounterId,
                InpEpisodeId = item.EpisodeId,
                PatientId = episode.PatientId,
                DrugId = item.DrugId,
                DoseSource = MedicationDoseSource.AsNeeded,
                ScheduledAt = null,
                DoseStatus = item.IsHighAlertSnapshot ? MedicationDoseStatus.Due : MedicationDoseStatus.Administered,
                DoubleCheckStatus = item.IsHighAlertSnapshot ? MedicationDoubleCheckStatus.Pending : MedicationDoubleCheckStatus.NotRequired,
                PlannedDose = item.Dose,
                PlannedDoseUnitSnapshot = Truncate(item.DoseUnitSymbolSnapshot ?? item.DoseUnitNameSnapshot, 50),
                ActualDose = actualDose,
                ActualDoseUnitSnapshot = Truncate(item.DoseUnitSymbolSnapshot ?? item.DoseUnitNameSnapshot, 50),
                ActualRouteSnapshot = Truncate(actualRoute, 100),
                AdministeredAt = diberikanPada,
                RecordedByEmployeeId = konteks.ActorEmployeeId,
                RecordedByUserId = actorUserId,
                RecordedAt = now,
                DeviationNote = catatan,
                IsHighAlertSnapshot = item.IsHighAlertSnapshot,
                PrnIndication = sebagaiPrn ? Truncate(prnIndication, 500) : null,
                PrnEvaluationDueAt = sebagaiPrn && setting.PrnEvaluationMinutes.HasValue ? diberikanPada.AddMinutes(setting.PrnEvaluationMinutes.Value) : null,
                RevisionNumber = 0,
                IdempotencyKey = kunci,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmMedicationAdministration>().Add(dosis);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (kunci != null)
            {
                _dbContext.ChangeTracker.Clear();
                return await FindReplayAsync(kunci, null, actorUserId, cancellationToken)
                       ?? Conflict<MedicationAdministrationResponse>("Pemberian ini sudah tercatat.", "DOSE_ALREADY_RECORDED");
            }

            await _loggerService.InfoAsync(LogCategory, sebagaiPrn ? "MedicationAdministration.RecordAsNeeded" : "MedicationAdministration.RecordUnscheduled",
                sebagaiPrn ? "Perawat mencatat pemberian obat sesuai kebutuhan." : "Perawat mencatat pemberian butir tanpa jadwal terkonfigurasi.",
                new { dosis.Id, dosis.AdministrationNumber, dosis.PrescriptionItemId, dosis.DoseStatus, dosis.DoubleCheckStatus, RecordedBy = actorUserId });

            return NursingResult<MedicationAdministrationResponse>.Ok(
                await ToResponseAsync(dosis, actorUserId, false, cancellationToken),
                dosis.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending
                    ? "Pemberian tercatat dan menunggu cek ganda perawat kedua."
                    : "Pemberian obat berhasil dicatat.",
                StatusCodes.Status201Created);
        }

        /// <summary>
        /// Koreksi hasil yang sudah tercatat — <c>FR-KEP-068</c>, <c>VAL-KEP-33</c>, <c>RWI-DEC-116</c> (c). Nilai lama
        /// disimpan pada revisi; baris berlaku diperbarui. Entri intake yang menunjuk dosis yang tidak lagi
        /// <c>Administered</c> ditandai dalam transaksi yang sama (<c>INT-KEP-10</c>, usulan <c>G-26</c>).
        /// </summary>
        public async Task<NursingResult<MedicationAdministrationResponse>> CorrectAsync(
            Guid id,
            CorrectMedicationAdministrationRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.CorrectionReason))
                return BadRequest<MedicationAdministrationResponse>("Isi alasan koreksi.", "CORRECTION_REASON_REQUIRED");

            if (!request.ExpectedRevisionNumber.HasValue)
                return BadRequest<MedicationAdministrationResponse>("Muat ulang dosis sebelum mengoreksi.", "DETAIL_NOT_LOADED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var dosis = await LockAsync(id, cancellationToken);

            if (dosis == null)
                return NotFound<MedicationAdministrationResponse>();

            var penjaga = await _writeGuard.EnsureCanWriteAsync(dosis.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<MedicationAdministrationResponse>();

            if (request.ExpectedRevisionNumber.Value != dosis.RevisionNumber)
                return Conflict<MedicationAdministrationResponse>("Data sudah diubah pengguna lain.", "STALE_REVISION");

            if (dosis.DoseStatus is not (MedicationDoseStatus.Administered or MedicationDoseStatus.Held or MedicationDoseStatus.Refused or MedicationDoseStatus.Missed))
                return Conflict<MedicationAdministrationResponse>("Hanya dosis yang sudah dicatat yang dapat dikoreksi.", "DOSE_NOT_RECORDED");

            if (request.DoseStatus == MedicationDoseStatus.Due || !Enum.IsDefined(request.DoseStatus))
                return BadRequest<MedicationAdministrationResponse>("Dosis yang sudah dicatat tidak dapat dibuka kembali menjadi belum dicatat.", "INVALID_DOSE_STATUS");

            var pelaksanaan = await _dbContext.Set<PhmSlidingScaleExecution>()
                .FirstOrDefaultAsync(x => x.MedicationAdministrationId == dosis.Id && x.ExecutionStatus == SlidingScaleExecutionStatus.Recorded && !x.IsDelete, cancellationToken);

            // VAL-KEP-33b
            if (pelaksanaan != null && request.DoseStatus is not (MedicationDoseStatus.Administered or MedicationDoseStatus.Cancelled))
                return Conflict<MedicationAdministrationResponse>("Dosis sliding scale hanya dapat dikoreksi menjadi diberikan atau dibatalkan.", "SLIDING_SCALE_CORRECTION_LIMITED");

            // Delta: dosis high-alert yang belum diberikan tidak dapat "dikoreksi" menjadi diberikan tanpa perawat kedua.
            if (dosis.IsHighAlertSnapshot && dosis.DoseStatus != MedicationDoseStatus.Administered && request.DoseStatus == MedicationDoseStatus.Administered)
                return Conflict<MedicationAdministrationResponse>(
                    "Dosis high-alert yang belum diberikan tidak dapat dikoreksi menjadi diberikan tanpa cek ganda. Catat sebagai pemberian baru agar diperiksa perawat kedua.",
                    "HIGH_ALERT_CORRECTION_REQUIRES_DOUBLE_CHECK");

            var now = DateTime.UtcNow;

            if (request.DoseStatus == MedicationDoseStatus.Administered)
            {
                var galat = ValidateAdministration<MedicationAdministrationResponse>(request.ActualDose, request.ActualRoute, request.AdministeredAt, now);

                if (galat != null)
                    return galat;

                if (IsDeviation(dosis.PlannedDose, request.ActualDose!.Value, dosis.ScheduledAt, AsUtc(request.AdministeredAt!.Value)) &&
                    string.IsNullOrWhiteSpace(request.DeviationNote))
                    return BadRequest<MedicationAdministrationResponse>("Dosis atau waktu berbeda dari jadwal. Isi catatan penyimpangan.", "DEVIATION_NOTE_REQUIRED");
            }
            else if (string.IsNullOrWhiteSpace(request.StatusReason))
            {
                return BadRequest<MedicationAdministrationResponse>($"Isi alasan dosis {DoseStatusLabel(request.DoseStatus).ToLowerInvariant()}.", "STATUS_REASON_REQUIRED");
            }

            var statusLama = dosis.DoseStatus;
            var nomorRevisi = dosis.RevisionNumber + 1;

            _dbContext.Set<PhmMedicationAdministrationRevision>().Add(new PhmMedicationAdministrationRevision
            {
                Id = Guid.NewGuid(),
                AdministrationId = dosis.Id,
                RevisionNumber = nomorRevisi,
                RevisionKind = RevisionKindCorrection,
                PreviousDoseStatus = dosis.DoseStatus,
                PreviousActualDose = dosis.ActualDose,
                PreviousActualRouteSnapshot = dosis.ActualRouteSnapshot,
                PreviousAdministeredAt = dosis.AdministeredAt,
                PreviousStatusReason = dosis.StatusReason,
                PreviousDeviationNote = dosis.DeviationNote,
                PreviousRecordedByUserId = dosis.RecordedByUserId,
                CorrectionReason = Truncate(request.CorrectionReason, 500)!,
                CorrectedByUserId = actorUserId,
                CorrectedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            dosis.DoseStatus = request.DoseStatus;

            if (request.DoseStatus == MedicationDoseStatus.Administered)
            {
                dosis.ActualDose = request.ActualDose;
                dosis.ActualDoseUnitSnapshot ??= dosis.PlannedDoseUnitSnapshot;
                dosis.ActualRouteSnapshot = Truncate(request.ActualRoute, 100);
                dosis.AdministeredAt = AsUtc(request.AdministeredAt!.Value);
                dosis.StatusReason = null;
            }
            else
            {
                dosis.ActualDose = null;
                dosis.ActualRouteSnapshot = null;
                dosis.AdministeredAt = null;
                dosis.StatusReason = Truncate(request.StatusReason, 500);
            }

            dosis.DeviationNote = Truncate(request.DeviationNote, 500);
            dosis.RevisionNumber = nomorRevisi;
            dosis.UpdateDateTime = now;
            dosis.UpdateBy = actorUserId;

            if (request.DoseStatus == MedicationDoseStatus.Cancelled)
            {
                dosis.IsCancel = true;
                dosis.CancelDateTime = now;
                dosis.CancelBy = actorUserId;

                // State matrix 5.6 — pelaksanaan sliding scale ikut dibatalkan bersama dosisnya.
                if (pelaksanaan != null)
                {
                    pelaksanaan.ExecutionStatus = SlidingScaleExecutionStatus.Cancelled;
                    pelaksanaan.IsCancel = true;
                    pelaksanaan.CancelDateTime = now;
                    pelaksanaan.CancelBy = actorUserId;
                    pelaksanaan.UpdateDateTime = now;
                    pelaksanaan.UpdateBy = actorUserId;
                }
            }

            if (statusLama == MedicationDoseStatus.Administered && request.DoseStatus != MedicationDoseStatus.Administered)
                await _dailyMonitoringService.FlagLinkedFluidEntryAsync(dosis.Id, now, actorUserId, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            // CorrectionReason sensitif — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "MedicationAdministration.Correct", "Perawat mengoreksi hasil dosis MAR.",
                new { dosis.Id, dosis.AdministrationNumber, PreviousStatus = statusLama, dosis.DoseStatus, dosis.RevisionNumber, CorrectedBy = actorUserId });

            return NursingResult<MedicationAdministrationResponse>.Ok(await ToResponseAsync(dosis, actorUserId, false, cancellationToken), "Koreksi dosis berhasil disimpan.");
        }

        public async Task<NursingResult<MedicationAdministrationResponse>> RecordPrnEvaluationAsync(
            Guid id,
            PrnEvaluationRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.PrnEvaluationNote))
                return BadRequest<MedicationAdministrationResponse>("Isi evaluasi efek obat.", "PRN_EVALUATION_REQUIRED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var dosis = await LockAsync(id, cancellationToken);

            if (dosis == null)
                return NotFound<MedicationAdministrationResponse>();

            var penjaga = await _writeGuard.EnsureCanWriteAsync(dosis.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<MedicationAdministrationResponse>();

            if (dosis.DoseSource != MedicationDoseSource.AsNeeded || string.IsNullOrWhiteSpace(dosis.PrnIndication) || dosis.DoseStatus != MedicationDoseStatus.Administered)
                return Conflict<MedicationAdministrationResponse>("Evaluasi hanya untuk obat sesuai kebutuhan yang sudah diberikan.", "NOT_ADMINISTERED_PRN");

            if (dosis.PrnEvaluatedAt.HasValue)
                return Conflict<MedicationAdministrationResponse>("Evaluasi obat sesuai kebutuhan ini sudah dicatat.", "PRN_ALREADY_EVALUATED");

            var now = DateTime.UtcNow;
            dosis.PrnEvaluationNote = Truncate(request.PrnEvaluationNote, 1000);
            dosis.PrnEvaluatedAt = now;
            dosis.PrnEvaluatedByUserId = actorUserId;
            dosis.UpdateDateTime = now;
            dosis.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "MedicationAdministration.PrnEvaluation", "Perawat mencatat evaluasi efek obat sesuai kebutuhan.",
                new { dosis.Id, dosis.AdministrationNumber, EvaluatedBy = actorUserId });

            return NursingResult<MedicationAdministrationResponse>.Ok(await ToResponseAsync(dosis, actorUserId, false, cancellationToken), "Evaluasi obat sesuai kebutuhan berhasil disimpan.");
        }

        // =====================================================================
        // Pembantu pencatatan
        // =====================================================================

        /// <summary>Mengunci baris dosis sampai transaksi selesai. Wajib dipanggil di dalam transaksi.</summary>
        private Task<PhmMedicationAdministration?> LockAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.Set<PhmMedicationAdministration>()
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM public.""PhmMedicationAdministration""
                    WHERE ""Id"" = {id}
                      AND ""IsDelete"" = false
                    FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Kiriman ulang berkunci sama mengembalikan dosis yang sudah tercatat — <c>FR-KEP-065</c>. Kunci yang sudah
        /// menempel pada dosis lain ditolak.
        /// </summary>
        private async Task<NursingResult<MedicationAdministrationResponse>?> FindReplayAsync(
            string? kunci,
            Guid? expectedId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (kunci == null)
                return null;

            var ada = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

            if (ada == null)
                return null;

            if (expectedId.HasValue && ada.Id != expectedId.Value)
                return Conflict<MedicationAdministrationResponse>("Kunci permintaan sudah dipakai untuk dosis lain. Muat ulang halaman.", "IDEMPOTENCY_KEY_REUSED");

            return NursingResult<MedicationAdministrationResponse>.Ok(
                await ToResponseAsync(ada, actorUserId, true, cancellationToken),
                "Pemberian ini sudah tercatat sebelumnya.",
                isReplay: true);
        }

        /// <summary><c>VAL-KEP-30a</c> dan <c>VAL-KEP-30f</c>.</summary>
        private static NursingResult<T>? ValidateAdministration<T>(decimal? dose, string? route, DateTime? administeredAt, DateTime now)
        {
            if (!dose.HasValue || dose.Value <= 0 || string.IsNullOrWhiteSpace(route) || !administeredAt.HasValue)
                return BadRequest<T>("Isi dosis, rute, dan waktu pemberian.", "ADMINISTRATION_FIELDS_REQUIRED");

            if (AsUtc(administeredAt.Value) > now.AddMinutes(FutureToleranceMinutes))
                return BadRequest<T>("Waktu pemberian tidak boleh di masa depan.", "ADMINISTERED_AT_IN_FUTURE");

            return null;
        }

        /// <summary><c>VAL-KEP-30c</c>: dosis aktual ≠ dosis rencana, atau waktu di luar ±60 menit dari jadwal.</summary>
        internal static bool IsDeviation(decimal? plannedDose, decimal actualDose, DateTime? scheduledAt, DateTime administeredAt) =>
            (plannedDose.HasValue && plannedDose.Value != actualDose) ||
            (scheduledAt.HasValue && Math.Abs((administeredAt - scheduledAt.Value).TotalMinutes) > DeviationWindowMinutes);

        /// <summary>Waktu kiriman tanpa zona dibaca sebagai UTC, seperti seluruh kontrak waktu API.</summary>
        internal static DateTime AsUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
