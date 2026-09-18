namespace QuilvianSystemBackend.Helpers
{
    namespace QuilvianSystemBackend.Helpers
    {
        public static class AppDateTimeHelper
        {
            private static readonly TimeZoneInfo AppTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

            public static DateTime UtcNow()
            {
                return DateTime.UtcNow;
            }

            public static DateTime LocalNow()
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AppTimeZone);
            }

            public static DateTime OperationalDate()
            {
                return LocalNow().Date;
            }

            public static DateTime ResolveOperationalDate(DateTime? date)
            {
                return date?.Date ?? OperationalDate();
            }

            /// <summary>
            /// Mengubah nilai yang datang dari pemanggil menjadi UTC yang dapat ditulis Npgsql.
            ///
            /// <b>Kenapa ini dibutuhkan.</b> Ruas tanggal pada query string yang ditulis
            /// <c>YYYY-MM-DD</c> — bentuk yang memang dikontrakkan dan memang dikirim layar —
            /// terikat sebagai <see cref="DateTimeKind.Unspecified"/>, dan Npgsql
            /// <b>menolak</b> menulisnya ke kolom <c>timestamp with time zone</c>. Tanpa
            /// penormalan, penyaring tanggal menjawab <c>500</c>, bukan daftar.
            ///
            /// Nilai tanpa zona dibaca sebagai <b>jam dinding WIB</b> — bukan UTC — karena
            /// itulah yang dimaksud petugas ketika mengetik satu tanggal.
            /// </summary>
            public static DateTime ToUtc(DateTime value)
            {
                return value.Kind switch
                {
                    DateTimeKind.Utc => value,
                    DateTimeKind.Local => value.ToUniversalTime(),
                    _ => TimeZoneInfo.ConvertTimeToUtc(
                        DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
                        AppTimeZone)
                };
            }
        }
    }
}
