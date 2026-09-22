namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Sumber cairan masuk (1–5) dan keluar (11–19) — <c>BE-RWI-119</c>, arsitektur 0.4 bagian 11.6.
    /// </summary>
    /// <remarks>
    /// Rentang angka sekaligus menandai arahnya: sumber di bawah 10 hanya sah untuk cairan masuk, 11 ke atas untuk cairan keluar.
    /// </remarks>
    public enum FluidSourceCategory
    {
        /// <summary>Infus.</summary>
        Infusion = 1,

        /// <summary>Minum/oral.</summary>
        Oral = 2,

        /// <summary>Masuk lewat NGT.</summary>
        NasogastricIntake = 3,

        /// <summary>Darah/produk darah.</summary>
        Blood = 4,

        /// <summary>Obat — wajib tertaut satu dosis MAR <c>Administered</c>.</summary>
        Medication = 5,

        /// <summary>Urin.</summary>
        Urine = 11,

        /// <summary>Feses.</summary>
        Stool = 12,

        /// <summary>Keluar lewat NGT.</summary>
        NasogastricOutput = 13,

        /// <summary>Drain atau WSD.</summary>
        DrainOrWsd = 14,

        /// <summary>Cairan keluar lain.</summary>
        OtherOutput = 19
    }
}
