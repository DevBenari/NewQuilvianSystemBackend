using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Cek ganda obat high-alert — <c>BE-RWI-116</c>, <c>VAL-KEP-31</c>, <c>INV-KEP-05</c>, state matrix 5.5.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Contoh.</b> Insulin aspart 6 unit SC pukul 11.00. Ns. Siti mencatat diberikan → dosis tetap <c>Due</c>
    /// dengan cek ganda <c>Pending</c> dan muncul pada daftar tunggu unit. Siti mencoba mengonfirmasi sendiri →
    /// <c>403</c> "Cek ganda harus dilakukan perawat lain." Ns. Dewi mengonfirmasi → <c>Administered</c>. Bila Dewi
    /// menolak "dosis dihitung dari GDS pasien lain", isian pemberian dikosongkan dan disimpan pada revisi; dosis
    /// kembali <c>Due</c> beralasan dan tidak dapat dianggap diberikan.
    /// </para>
    /// <para>
    /// Obat bukan high-alert berstatus <c>NotRequired</c> dan tidak pernah melewati jalur ini. Kewenangan
    /// "perawat kedua" adalah butir <c>MedicationAdministration : DoubleCheck</c> di layar Akses Role; yang dijaga
    /// kode hanya bahwa orangnya berbeda dari pencatat dan ditempatkan di unit pasien.
    /// </para>
    /// </remarks>
    public partial class MedicationAdministrationService
    {
        public const string DecisionConfirm = "Confirm";
        public const string DecisionReject = "Reject";

        public async Task<NursingResult<MedicationAdministrationResponse>> DoubleCheckAsync(
            Guid id,
            DoubleCheckRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var keputusan = request.Decision?.Trim();
            var konfirmasi = string.Equals(keputusan, DecisionConfirm, StringComparison.OrdinalIgnoreCase);
            var tolak = string.Equals(keputusan, DecisionReject, StringComparison.OrdinalIgnoreCase);

            if (!konfirmasi && !tolak)
                return BadRequest<MedicationAdministrationResponse>("Pilih keputusan cek ganda: Confirm atau Reject.", "INVALID_DECISION");

            // VAL-KEP-31c
            if (tolak && string.IsNullOrWhiteSpace(request.Note))
                return BadRequest<MedicationAdministrationResponse>("Isi alasan penolakan.", "REJECT_NOTE_REQUIRED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var dosis = await LockAsync(id, cancellationToken);

            if (dosis == null)
                return NotFound<MedicationAdministrationResponse>();

            var penjaga = await _writeGuard.EnsureCanWriteAsync(dosis.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<MedicationAdministrationResponse>();

            var konteks = penjaga.Value!;

            // VAL-KEP-31b
            if (dosis.DoseStatus != MedicationDoseStatus.Due || dosis.DoubleCheckStatus != MedicationDoubleCheckStatus.Pending)
                return Conflict<MedicationAdministrationResponse>("Dosis ini tidak sedang menunggu cek ganda.", "NOT_PENDING_DOUBLE_CHECK");

            // VAL-KEP-31a — dibandingkan pada akun dan pada pegawai, supaya dua akun satu orang tetap tertolak.
            if (dosis.RecordedByUserId == actorUserId || dosis.RecordedByEmployeeId == konteks.ActorEmployeeId)
                return NursingResult<MedicationAdministrationResponse>.Fail(StatusCodes.Status403Forbidden, "Cek ganda harus dilakukan perawat lain.", "DOUBLE_CHECKER_IS_RECORDER");

            if (request.ExpectedRevisionNumber.HasValue && request.ExpectedRevisionNumber.Value != dosis.RevisionNumber)
                return Conflict<MedicationAdministrationResponse>("Data sudah diubah pengguna lain.", "STALE_REVISION");

            var now = DateTime.UtcNow;

            dosis.DoubleCheckedByEmployeeId = konteks.ActorEmployeeId;
            dosis.DoubleCheckedByUserId = actorUserId;
            dosis.DoubleCheckedAt = now;
            dosis.DoubleCheckNote = Truncate(request.Note, 500);
            dosis.UpdateDateTime = now;
            dosis.UpdateBy = actorUserId;

            if (konfirmasi)
            {
                dosis.DoseStatus = MedicationDoseStatus.Administered;
                dosis.DoubleCheckStatus = MedicationDoubleCheckStatus.Confirmed;
            }
            else
            {
                var nomorRevisi = dosis.RevisionNumber + 1;

                _dbContext.Set<PhmMedicationAdministrationRevision>().Add(new PhmMedicationAdministrationRevision
                {
                    Id = Guid.NewGuid(),
                    AdministrationId = dosis.Id,
                    RevisionNumber = nomorRevisi,
                    RevisionKind = RevisionKindDoubleCheckRejected,
                    PreviousDoseStatus = dosis.DoseStatus,
                    PreviousActualDose = dosis.ActualDose,
                    PreviousActualRouteSnapshot = dosis.ActualRouteSnapshot,
                    PreviousAdministeredAt = dosis.AdministeredAt,
                    PreviousStatusReason = dosis.StatusReason,
                    PreviousDeviationNote = dosis.DeviationNote,
                    PreviousRecordedByUserId = dosis.RecordedByUserId,
                    CorrectionReason = Truncate(request.Note, 500)!,
                    CorrectedByUserId = actorUserId,
                    CorrectedAt = now,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                // Kembali Due beralasan; isian pemberian dikosongkan dan tersimpan pada revisi.
                dosis.DoseStatus = MedicationDoseStatus.Due;
                dosis.DoubleCheckStatus = MedicationDoubleCheckStatus.Rejected;
                dosis.ActualDose = null;
                dosis.ActualDoseUnitSnapshot = null;
                dosis.ActualRouteSnapshot = null;
                dosis.AdministeredAt = null;
                dosis.DeviationNote = null;
                dosis.RecordedByEmployeeId = null;
                dosis.RecordedByUserId = null;
                dosis.RecordedAt = null;
                dosis.RevisionNumber = nomorRevisi;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            // DoubleCheckNote sensitif — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "MedicationAdministration.DoubleCheck", "Perawat kedua memutuskan cek ganda dosis high-alert.",
                new { dosis.Id, dosis.AdministrationNumber, dosis.DoubleCheckStatus, CheckedBy = actorUserId });

            return NursingResult<MedicationAdministrationResponse>.Ok(
                await ToResponseAsync(dosis, actorUserId, false, cancellationToken),
                konfirmasi ? "Cek ganda dikonfirmasi. Dosis tercatat diberikan." : "Cek ganda ditolak. Dosis kembali belum diberikan.");
        }

        /// <summary>Dosis menunggu cek ganda di satu unit — <c>BE-RWI-116</c> kriteria 5.</summary>
        public async Task<NursingResult<List<MedicationAdministrationListItem>>> GetDoubleCheckWorklistAsync(
            Guid? serviceUnitId,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!serviceUnitId.HasValue || serviceUnitId.Value == Guid.Empty)
                return BadRequest<List<MedicationAdministrationListItem>>("Pilih unit layanan.", "SERVICE_UNIT_REQUIRED");

            var pegawai = await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (!pegawai.HasValue)
                return NursingResult<List<MedicationAdministrationListItem>>.Fail(StatusCodes.Status403Forbidden,
                    InpatientClinicalContextService.PenolakanPerawatTanpaPegawai, NursingEpisodeWriteGuard.KodeTanpaPegawai);

            var now = DateTime.UtcNow;

            if (!await _contextService.IsEmployeeAssignedToUnitAsync(serviceUnitId.Value, pegawai.Value, now, cancellationToken))
                return NursingResult<List<MedicationAdministrationListItem>>.Fail(StatusCodes.Status403Forbidden,
                    NursingEpisodeWriteGuard.PenolakanUnitLain, NursingEpisodeWriteGuard.KodeUnitLain);

            var episodeIds = _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.ServiceUnitId == serviceUnitId.Value && !x.IsDelete &&
                            (x.EpisodeStatus == InpEpisodeStatus.Admitted || x.EpisodeStatus == InpEpisodeStatus.DischargePending))
                .Select(x => x.Id);

            var dosis = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.DoseStatus == MedicationDoseStatus.Due &&
                            x.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending &&
                            episodeIds.Contains(x.InpEpisodeId))
                .OrderBy(x => x.RecordedAt)
                .ToListAsync(cancellationToken);

            var setting = await GetEffectiveSettingAsync(cancellationToken);
            var hasil = await ToListItemsAsync(dosis, actorUserId, null, InpEpisodeStatus.Admitted, setting.MissedAfterMinutes, now, cancellationToken);

            return NursingResult<List<MedicationAdministrationListItem>>.Ok(hasil, "Daftar tunggu cek ganda berhasil diambil.");
        }
    }
}
