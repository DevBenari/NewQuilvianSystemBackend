namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Subjek pernyataan Selisih Biaya, disimpan sebagai kode, bukan teks tampilan V1
    /// (<c>BE-RWI-192</c>, <c>FR-RWA-103</c>).
    /// </summary>
    public enum InpCostDifferenceSubject
    {
        /// <summary>Diri saya sendiri.</summary>
        Self = 1,

        /// <summary>Istri saya.</summary>
        Wife = 2,

        /// <summary>Suami saya.</summary>
        Husband = 3,

        /// <summary>Anak saya.</summary>
        Child = 4,

        /// <summary>Saudara kandung lainnya — keterangan wajib.</summary>
        OtherSibling = 5
    }
}
