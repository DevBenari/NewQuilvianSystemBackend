namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Cara pemberian obat bawaan pasien — <c>BE-RWI-101</c>, isian V1 <c>RWI-FACT-032</c>,
    /// arsitektur backend 0.5 bagian 11.5.11.
    /// </summary>
    /// <remarks>
    /// Tidak berbawaan: perawat wajib memilih cara pemberian, karena Metformin oral dan insulin
    /// injeksi tidak boleh tercatat sama hanya karena isiannya terlewat.
    /// </remarks>
    public enum HomeMedicationRoute
    {
        Oral = 1,
        Injection = 2,
        Topical = 3,
        Inhalation = 4,
        Rectal = 5,
        Infusion = 6,
        Other = 9
    }
}
