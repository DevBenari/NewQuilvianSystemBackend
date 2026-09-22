namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Status cek ganda obat high-alert — <c>BE-RWI-116</c>, <c>VAL-KEP-31</c>.
    /// </summary>
    public enum MedicationDoubleCheckStatus
    {
        /// <summary>Obat bukan high-alert.</summary>
        NotRequired = 0,

        /// <summary>Menunggu perawat kedua.</summary>
        Pending = 1,

        /// <summary>Dikonfirmasi perawat kedua yang berbeda.</summary>
        Confirmed = 2,

        /// <summary>Ditolak perawat kedua; dosis kembali Due.</summary>
        Rejected = 3
    }
}
