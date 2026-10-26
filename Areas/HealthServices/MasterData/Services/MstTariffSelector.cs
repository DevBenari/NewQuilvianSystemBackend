using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    public enum MstTariffSelectionStatus
    {
        /// <summary>Tepat satu harga yang sah untuk konteks ini.</summary>
        Resolved = 1,

        /// <summary>Tidak ada tarif yang berlaku untuk konteks ini.</summary>
        NotFound = 2,

        /// <summary>
        /// Dua tarif atau lebih sama-sama paling spesifik tetapi berharga berbeda. Tidak ada aturan yang
        /// sah untuk memilih salah satunya, jadi tidak ada yang dipilih.
        /// </summary>
        Ambiguous = 3,
    }

    /// <param name="Status">Hasil pemilihan.</param>
    /// <param name="Chosen">Tarif terpilih; <c>null</c> bila <see cref="MstTariffSelectionStatus.NotFound"/> atau <see cref="MstTariffSelectionStatus.Ambiguous"/>.</param>
    /// <param name="Tied">Seluruh tarif yang sama spesifik; dipakai melaporkan konflik.</param>
    public sealed record MstTariffSelection(
        MstTariffSelectionStatus Status,
        MstTariff? Chosen,
        IReadOnlyList<MstTariff> Tied);

    /// <summary>
    /// Memilih satu tarif dari calon tarif yang SUDAH disaring berlaku (aktif, tidak dihapus, tidak dibatalkan,
    /// dan dalam rentang tanggal efektif). Aturannya sama dengan <c>BillingSourceTariffResolver</c> supaya harga
    /// yang tampil di layar pemesanan sama dengan harga yang kelak ditagih Billing:
    /// <list type="number">
    ///   <item><description>Tarif harus berlaku untuk klinik kunjungan (tarif tanpa klinik berlaku untuk semua) dan
    ///   kelas pasien (tarif tanpa kelas berlaku umum). Tarif kelas lain tidak pernah dipakai.</description></item>
    ///   <item><description>Yang paling spesifik menang: klinik + kelas, lalu klinik, lalu kelas, lalu umum.</description></item>
    ///   <item><description>Bila beberapa tarif sama spesifik: harga yang sama berarti salinan master yang aman
    ///   (dipilih menurut kode tarif); harga berbeda berarti konflik master dan hasilnya
    ///   <see cref="MstTariffSelectionStatus.Ambiguous"/> — bukan dipilih acak, bukan yang termurah, bukan yang termahal.</description></item>
    /// </list>
    /// </summary>
    public static class MstTariffSelector
    {
        public static int Specificity(MstTariff tariff) =>
            (tariff.ClinicId != null ? 2 : 0) + (tariff.PatientClassId != null ? 1 : 0);

        public static bool IsEligible(MstTariff tariff, Guid? patientClassId, Guid? clinicId) =>
            (tariff.ClinicId == null || tariff.ClinicId == clinicId) &&
            (tariff.PatientClassId == null || tariff.PatientClassId == patientClassId);

        public static MstTariffSelection Select(
            IEnumerable<MstTariff> candidates,
            Guid? patientClassId,
            Guid? clinicId)
        {
            ArgumentNullException.ThrowIfNull(candidates);

            var eligible = candidates
                .Where(x => IsEligible(x, patientClassId, clinicId))
                .ToList();

            if (eligible.Count == 0)
            {
                return new MstTariffSelection(MstTariffSelectionStatus.NotFound, null, Array.Empty<MstTariff>());
            }

            var top = eligible.Max(Specificity);
            var tied = eligible
                .Where(x => Specificity(x) == top)
                .OrderBy(x => x.TariffCode, StringComparer.Ordinal)
                .ToList();

            if (tied.Select(x => x.NormalPrice).Distinct().Count() > 1)
            {
                return new MstTariffSelection(MstTariffSelectionStatus.Ambiguous, null, tied);
            }

            return new MstTariffSelection(MstTariffSelectionStatus.Resolved, tied[0], tied);
        }
    }
}
