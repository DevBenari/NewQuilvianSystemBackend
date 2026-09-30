namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Jenis nilai yang dipakai Kiosk untuk mencari satu pasien (<c>BE-KSK-001</c>,
    /// <c>KSK-CONTRACT-v1</c>).
    /// </summary>
    /// <remarks>
    /// Layar Cek Nomor Rekam Medis hanya menawarkan <see cref="IdentityNumber"/> dan
    /// <see cref="PhoneNumber"/> (KSK-RM-002). Dua nilai lainnya dipakai langkah Identifikasi
    /// Pasien Lama untuk mengenali kartu asuransi/member yang dipindai tanpa membentuk sesi
    /// kiosk (<c>KSK-DSN-004</c>). Dikirim sebagai angka pada JSON.
    /// </remarks>
    public enum KioskPatientLookupSearchType
    {
        IdentityNumber = 1,
        PhoneNumber = 2,
        InsuranceCardNumber = 3,
        MemberNumber = 4
    }
}
