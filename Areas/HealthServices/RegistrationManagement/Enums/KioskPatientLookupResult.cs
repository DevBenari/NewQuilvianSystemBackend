namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Hasil pencarian pasien Kiosk (<c>BE-KSK-001</c>, <c>KSK-CONTRACT-v1</c>).
    /// </summary>
    /// <remarks>
    /// Keempat hasil dijawab <c>200</c>, bukan <c>404</c>, supaya gagal teknis tidak pernah
    /// terbaca sebagai pasien belum terdaftar (<c>KSK-DSN-003</c>, <c>KSK-INV-002</c>).
    /// </remarks>
    public enum KioskPatientLookupResult
    {
        /// <summary>Tepat satu pasien aktif cocok; Kartu Pasien dikirim.</summary>
        Found = 1,

        /// <summary>Tidak ada pasien yang cocok.</summary>
        NotFound = 2,

        /// <summary>Lebih dari satu pasien cocok; data pasien tidak dikirim (<c>KSK-DEC-006/007</c>).</summary>
        MultipleMatch = 3,

        /// <summary>Satu pasien cocok tetapi tidak aktif; pasien diarahkan ke petugas tanpa alasan (<c>KSK-DEC-017</c>).</summary>
        ContactStaff = 4
    }
}
