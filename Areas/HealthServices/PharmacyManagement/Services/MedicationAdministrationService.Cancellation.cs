using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Pembatalan dosis yang belum waktunya — <c>BE-RWI-118</c>, <c>INT-KEP-09</c> (penghentian butir resep) dan
    /// <c>INT-KEP-15</c> (penutupan episode), state matrix 5.5.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kedua metode tidak membuka transaksi dan tidak memanggil <c>SaveChanges</c>.</b> Keduanya mengubah
    /// baris terlacak pada <c>ApplicationDbContext</c> yang sama dengan pemanggil, sehingga ikut tersimpan atau
    /// ikut batal bersama transaksi pemicunya — pola yang sama dengan
    /// <c>PatientProcedureOrderService.CancelPendingOrdersForClosureAsync</c> pada langkah 5 penutupan. Galat
    /// pada langkah mana pun → nol dosis berubah.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Joko pulang pukul 14.00. Dosis 08.00 <c>Administered</c> → tetap. Dosis 12.00 <c>Missed</c> →
    /// tetap. Dosis 20.00 <c>Due</c> → <c>Cancelled</c> "perawatan ditutup". Dosis 13.00 yang belum dicatat tetap
    /// <c>Due</c> dan tampil "Tidak dicatat sebelum perawatan ditutup". Menjalankan ulang tidak mengubah apa pun.
    /// </para>
    /// </remarks>
    public partial class MedicationAdministrationService
    {
        /// <summary>
        /// Membatalkan dosis <c>Due</c> butir yang dihentikan dengan jadwal ≥ waktu henti, termasuk yang sedang
        /// menunggu cek ganda. Dipanggil <c>InpatientPrescriptionService</c> di dalam transaksi penghentian butir.
        /// </summary>
        public async Task<int> CancelDueDosesForItemAsync(
            Guid prescriptionItemId,
            DateTime stoppedAtUtc,
            Guid actorUserId,
            string reason = AlasanResepDihentikan,
            CancellationToken cancellationToken = default)
        {
            var batas = AsUtc(stoppedAtUtc);

            var dosis = await _dbContext.Set<PhmMedicationAdministration>()
                .Where(x => x.PrescriptionItemId == prescriptionItemId &&
                            !x.IsDelete &&
                            x.DoseStatus == MedicationDoseStatus.Due &&
                            x.ScheduledAt != null &&
                            x.ScheduledAt >= batas)
                .ToListAsync(cancellationToken);

            CancelTracked(dosis, reason, actorUserId, DateTime.UtcNow);

            return dosis.Count;
        }

        /// <summary>
        /// Membatalkan dosis <c>Due</c> episode dengan jadwal sesudah waktu tutup. Dipanggil
        /// <c>InpDischargeService</c> pada langkah 6 transaksi penutupan.
        /// </summary>
        public async Task<int> CancelFutureDosesForEpisodeAsync(
            Guid episodeId,
            DateTime closedAtUtc,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var batas = AsUtc(closedAtUtc);

            var dosis = await _dbContext.Set<PhmMedicationAdministration>()
                .Where(x => x.InpEpisodeId == episodeId &&
                            !x.IsDelete &&
                            x.DoseStatus == MedicationDoseStatus.Due &&
                            x.ScheduledAt != null &&
                            x.ScheduledAt > batas)
                .ToListAsync(cancellationToken);

            CancelTracked(dosis, AlasanPerawatanDitutup, actorUserId, DateTime.UtcNow);

            return dosis.Count;
        }

        /// <summary>
        /// Jumlah dosis terjadwal yang jamnya sudah lewat tetapi belum dicatat — peringatan sebelum menutup
        /// episode (<c>INT-KEP-15</c>, <c>VAL-INP-16</c>). Tidak pernah menahan penutupan.
        /// </summary>
        public Task<int> CountUnrecordedPastDosesAsync(
            Guid episodeId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            var batas = AsUtc(atUtc);

            return _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .CountAsync(x => x.InpEpisodeId == episodeId &&
                                 !x.IsDelete &&
                                 x.DoseStatus == MedicationDoseStatus.Due &&
                                 x.ScheduledAt != null &&
                                 x.ScheduledAt <= batas,
                    cancellationToken);
        }

        /// <summary>
        /// Jaring pengaman pembentukan dosis: butir episode yang sudah dihentikan tetapi dosis <c>Due</c> sesudah
        /// waktu hentinya masih terbuka. Tidak menyimpan.
        /// </summary>
        private async Task<int> CancelDueDosesOfStoppedItemsAsync(
            Guid episodeId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var dosis = await _dbContext.Set<PhmMedicationAdministration>()
                .Where(x => x.InpEpisodeId == episodeId &&
                            !x.IsDelete &&
                            x.DoseStatus == MedicationDoseStatus.Due &&
                            x.ScheduledAt != null)
                .Join(_dbContext.Set<PhmPrescriptionItem>().Where(i => i.IsStopped && i.StoppedAt != null),
                    d => d.PrescriptionItemId,
                    i => i.Id,
                    (d, i) => new { Dosis = d, i.StoppedAt })
                .Where(x => x.Dosis.ScheduledAt >= x.StoppedAt)
                .Select(x => x.Dosis)
                .ToListAsync(cancellationToken);

            CancelTracked(dosis, AlasanResepDihentikan, actorUserId, now);

            return dosis.Count;
        }

        private static void CancelTracked(List<PhmMedicationAdministration> dosis, string reason, Guid actorUserId, DateTime now)
        {
            foreach (var x in dosis)
            {
                x.DoseStatus = MedicationDoseStatus.Cancelled;
                x.StatusReason = reason;
                x.IsCancel = true;
                x.CancelDateTime = now;
                x.CancelBy = actorUserId;
                x.UpdateDateTime = now;
                x.UpdateBy = actorUserId;
            }
        }
    }
}
