namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Penutupan otomatis pendaftaran online yang tidak check-in sampai tanggal kunjungan berakhir
    /// (<c>KSK-DEC-037</c>, <c>KSK-DSN-PO-015</c>). Dipakai layanan penutupan harian pada
    /// <c>BE-KSK-PO-008</c>.
    /// </summary>
    public class OnlineRegistrationClosureOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>Jam lokal WIB format <c>HH:mm</c>; bawaan sesudah pergantian hari.</summary>
        public string RunAtLocalTime { get; set; } = "00:05";
    }
}
