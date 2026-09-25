namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Jam dinding rumah sakit (Asia/Jakarta) bagi pencatatan keperawatan rawat inap revision 7 —
    /// jadwal MAR <c>BE-RWI-114</c>, hari Pengawasan Harian dan shift <c>BE-RWI-120</c>.
    /// </summary>
    /// <remarks>
    /// Basis data menyimpan UTC; jam jadwal obat dan jam shift ditulis pengguna sebagai jam lokal.
    /// Tanpa konversi ini, jadwal "08.00" tersimpan sebagai 08.00 UTC dan dosis muncul pukul 15.00 WIB.
    /// </remarks>
    public static class HospitalTimeZone
    {
        private const string IanaId = "Asia/Jakarta";
        private const string WindowsId = "SE Asia Standard Time";

        private static readonly Lazy<TimeZoneInfo> ZoneLazy = new(() =>
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(IanaId); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById(WindowsId); }
        });

        public static TimeZoneInfo Zone => ZoneLazy.Value;

        /// <summary>Waktu UTC menjadi jam dinding rumah sakit.</summary>
        public static DateTime ToLocal(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

        /// <summary>Jam dinding rumah sakit menjadi UTC ber-<c>Kind</c> <c>Utc</c> — siap disimpan ke <c>timestamptz</c>.</summary>
        public static DateTime ToUtc(DateTime local) =>
            DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Zone), DateTimeKind.Utc);

        public static DateTime ToUtc(DateOnly date, TimeOnly time) => ToUtc(date.ToDateTime(time));

        public static DateOnly TodayLocal(DateTime nowUtc) => DateOnly.FromDateTime(ToLocal(nowUtc));
    }
}
