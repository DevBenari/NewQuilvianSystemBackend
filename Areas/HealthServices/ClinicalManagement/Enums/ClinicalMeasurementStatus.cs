namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Status entri terukur Pengawasan Harian — <c>BE-RWI-119</c>, state matrix 0.5.0 bagian 5.4.
    /// </summary>
    public enum ClinicalMeasurementStatus
    {
        /// <summary>Berlaku dan dihitung.</summary>
        Active = 1,

        /// <summary>Dibatalkan beralasan; tidak dihitung, tidak dihapus.</summary>
        Cancelled = 2
    }
}
