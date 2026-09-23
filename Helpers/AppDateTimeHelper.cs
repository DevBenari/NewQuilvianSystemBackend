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
            /// Mengubah sebuah <b>tanggal operasional</b> menjadi saat UTC pada pukul
            /// <c>00:00</c> zona waktu aplikasi.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Dipakai menyusun batas rentang terhadap kolom yang <b>disimpan dalam UTC</b>,
            /// misalnya <c>CreateDateTime</c>. Tanpa konversi ini, menstempel tanggal
            /// operasional dengan <c>DateTimeKind.Utc</c> menggeser batasnya sebesar selisih
            /// zona waktu — tujuh jam untuk <c>Asia/Jakarta</c> — sehingga kejadian yang
            /// lahir dini hari waktu setempat jatuh ke hari yang keliru.
            /// </para>
            /// <para>
            /// Contoh: tanggal operasional <c>2026-09-23</c> memulangkan
            /// <c>2026-09-22T17:00:00Z</c>.
            /// </para>
            /// </remarks>
            public static DateTime OperationalDateToUtc(DateTime operationalDate)
            {
                var localMidnight = DateTime.SpecifyKind(
                    operationalDate.Date,
                    DateTimeKind.Unspecified);

                return TimeZoneInfo.ConvertTimeToUtc(localMidnight, AppTimeZone);
            }
        }
    }
}
