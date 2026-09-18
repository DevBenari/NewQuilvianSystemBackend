namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Status satu dosis MAR — <c>BE-RWI-115</c>, state matrix 0.5.0 bagian 5.5.
    /// </summary>
    /// <remarks>
    /// "Menunggu cek ganda" bukan nilai enum ini; ia adalah <c>Due</c> dengan <c>MedicationDoubleCheckStatus.Pending</c>.
    /// </remarks>
    public enum MedicationDoseStatus
    {
        /// <summary>Terjadwal, belum dicatat.</summary>
        Due = 1,

        /// <summary>Diberikan.</summary>
        Administered = 2,

        /// <summary>Ditahan beralasan.</summary>
        Held = 3,

        /// <summary>Ditolak pasien beralasan.</summary>
        Refused = 4,

        /// <summary>Terlewat beralasan — dicatat perawat, tidak pernah otomatis.</summary>
        Missed = 5,

        /// <summary>Dibatalkan sistem: resep dihentikan atau perawatan ditutup.</summary>
        Cancelled = 6
    }
}
