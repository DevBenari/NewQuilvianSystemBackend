using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Penyusun <see cref="LabRescheduleSnapshot"/> untuk sekumpulan pesanan — <b>satu-satunya</b>
    /// tempat potret kelayakan dibaca dari basis data (<c>BE-LAB-103</c>, <c>INV-67</c>). Dipakai
    /// penjaga <c>PUT reschedule</c>, detail pesanan, daftar pantau, dan kiosk, sehingga keempatnya
    /// menilai pesanan yang sama dari potret yang sama lalu menyerahkannya ke
    /// <see cref="LabRescheduleEligibility"/>.
    /// </summary>
    /// <remarks>
    /// <b>Dua kueri untuk seluruh halaman</b>, berapa pun jumlah barisnya: satu proyeksi pesanan
    /// (status, kunjungan, CITO, MCU) dan satu daftar wadah berkelompok — pola
    /// <c>LabOrderResultProgressRules.ReadAsync</c>, bukan sub-query per baris. Penjaga lintas modul
    /// hanya <b>membaca</b>: status kunjungan (Registrasi) dan ada/tidaknya komponen order MCU (MCU).
    /// </remarks>
    internal static class LabRescheduleSnapshotReader
    {
        public static async Task<Dictionary<Guid, LabRescheduleSnapshot>> ReadAsync(
            ApplicationDbContext dbContext,
            IReadOnlyCollection<Guid> orderIds,
            CancellationToken cancellationToken)
        {
            if (orderIds.Count == 0)
                return new Dictionary<Guid, LabRescheduleSnapshot>();

            var ids = orderIds.Distinct().ToList();

            var pesanan = await dbContext.LabOrders
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.OrderStatus,
                    x.StatusBeforeHold,
                    x.PlannedDate,
                    EncounterStatus = x.Encounter != null ? (EncounterStatus?)x.Encounter.EncounterStatus : null,

                    // VAL-163: CITO permintaan (belum berwadah) dan CITO pemeriksaan (sudah
                    // berwadah) — membaca pemeriksaan saja akan meloloskan CITO yang belum
                    // diambil sampelnya.
                    HasActiveCito =
                        dbContext.LabOrderedProcedures.Any(p =>
                            p.LabOrderId == x.Id &&
                            !p.IsDelete &&
                            p.OrderedStatus == LabOrderedProcedureStatus.Ordered &&
                            p.Urgency == LabExaminationUrgency.Cito) ||
                        dbContext.LabExaminations.Any(e =>
                            e.LabOrderId == x.Id &&
                            !e.IsDelete &&
                            e.Urgency == LabExaminationUrgency.Cito &&
                            e.ExaminationStatus != LabExaminationStatus.Voided &&
                            e.ExaminationStatus != LabExaminationStatus.Cancelled),

                    // VAL-164: tanda paket saja tidak cukup — komponen MCU di luar paket juga
                    // menautkan pesanan lewat McuOrderComponent.LabOrderId (LAB-DEC-272).
                    IsMcuLinked =
                        x.CoveringMcuOrderId != null ||
                        dbContext.McuOrderComponents.Any(c => c.LabOrderId == x.Id && !c.IsDelete)
                })
                .ToListAsync(cancellationToken);

            var wadah = await dbContext.LabSpecimens
                .AsNoTracking()
                .Where(s => ids.Contains(s.LabOrderId) && !s.IsDelete)
                .Select(s => new { s.LabOrderId, s.SpecimenStatus, s.StatusBeforeHold })
                .ToListAsync(cancellationToken);

            var wadahPerPesanan = wadah
                .GroupBy(s => s.LabOrderId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<LabRescheduleSpecimenState>)g
                        .Select(s => new LabRescheduleSpecimenState(s.SpecimenStatus, s.StatusBeforeHold))
                        .ToList());

            return pesanan.ToDictionary(
                x => x.Id,
                x => new LabRescheduleSnapshot(
                    x.OrderStatus,
                    x.StatusBeforeHold,
                    x.PlannedDate,
                    wadahPerPesanan.TryGetValue(x.Id, out var daftar)
                        ? daftar
                        : Array.Empty<LabRescheduleSpecimenState>(),
                    x.EncounterStatus,
                    x.HasActiveCito,
                    x.IsMcuLinked));
        }
    }
}
