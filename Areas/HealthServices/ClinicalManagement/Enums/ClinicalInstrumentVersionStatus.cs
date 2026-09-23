namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Status versi instrumen klinis — <c>BE-RWI-108</c>, state matrix 0.5.0 bagian 5.1.
    /// </summary>
    /// <remarks>
    /// <c>Approved</c> dan <c>Retired</c> tidak pernah kembali ke <c>Draft</c>; versi baru dibuat dari salinannya.
    /// </remarks>
    public enum ClinicalInstrumentVersionStatus
    {
        /// <summary>Konsep yang masih dapat diubah.</summary>
        Draft = 1,

        /// <summary>Disahkan; beku dan dipakai pasien. Paling banyak satu per instrumen.</summary>
        Approved = 2,

        /// <summary>Dipensiunkan; terminal.</summary>
        Retired = 3
    }
}
