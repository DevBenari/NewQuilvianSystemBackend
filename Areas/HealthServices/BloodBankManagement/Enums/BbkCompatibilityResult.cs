namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Keputusan manusia atas pemeriksaan kecocokan satu kantong terhadap satu pasien.
    /// Quilvian tidak menghitung kompatibilitas; sistem hanya mencatat hasil yang
    /// dinyatakan petugas BDRS berwenang validasi (DEC-BD-042, INV-BD-013).
    /// </summary>
    public enum BbkCompatibilityResult
    {
        /// <summary>
        /// Kantong dinyatakan cocok terhadap pasien tujuan.
        /// </summary>
        Compatible = 0,

        /// <summary>
        /// Kantong dinyatakan tidak cocok terhadap pasien tujuan.
        /// Bukti tetap disimpan sebagai riwayat tetapi tidak membuka gerbang pemberian.
        /// </summary>
        Incompatible = 1
    }
}