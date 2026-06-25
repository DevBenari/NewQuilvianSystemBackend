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
        }
    }
}
