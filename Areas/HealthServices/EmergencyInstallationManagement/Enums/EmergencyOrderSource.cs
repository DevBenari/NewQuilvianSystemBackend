namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Asal pesanan: punya baris di sistem, atau dibuat di luar sistem.
    /// </summary>
    /// <remarks>
    /// <c>IGD-DEC-103</c>. Sengaja dibuat <b>terpisah</b> dari <c>EmergencyOrderKind</c>.
    /// Menumpangkannya — misalnya lewat nilai <c>ExternalRadiologyOrder</c> — akan
    /// menggandakan setiap jenis pesanan begitu ada jenis kedua yang dipesan di luar sistem,
    /// dan mencampur jenis pemeriksaan dengan asal pesanan: dua hal yang berubah karena sebab
    /// yang berbeda.
    /// </remarks>
    public enum EmergencyOrderSource
    {
        /// <summary>Pesanan punya baris di sistem; <c>OrderReferenceId</c> wajib terisi.</summary>
        Internal = 1,

        /// <summary>
        /// Pesanan dibuat di luar sistem. <c>ExternalReference</c> dan
        /// <c>OrderDescription</c> wajib terisi supaya tetap dapat ditelusuri.
        /// </summary>
        External = 2
    }
}
