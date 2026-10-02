namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Asal satu dosis MAR — <c>BE-RWI-114</c>, kamus data 0.4 bagian 11.12.
    /// </summary>
    public enum MedicationDoseSource
    {
        /// <summary>Dibentuk dari jadwal frekuensi butir resep.</summary>
        Scheduled = 1,

        /// <summary>Pemberian PRN atau butir tanpa jadwal terkonfigurasi.</summary>
        AsNeeded = 2,

        /// <summary>Dosis hasil pelaksanaan sliding scale.</summary>
        SlidingScale = 3
    }
}
