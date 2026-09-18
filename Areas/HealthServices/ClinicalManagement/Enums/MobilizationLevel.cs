namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Tingkat mobilisasi pada observasi harian — <c>BE-RWI-119</c>.
    /// </summary>
    public enum MobilizationLevel
    {
        /// <summary>Belum dinilai.</summary>
        NotAssessed = 0,

        /// <summary>Tirah baring.</summary>
        Bedrest = 1,

        /// <summary>Duduk di tempat tidur.</summary>
        SitInBed = 2,

        /// <summary>Berjalan dengan bantuan.</summary>
        AssistedWalking = 3,

        /// <summary>Mandiri.</summary>
        Independent = 4
    }
}
