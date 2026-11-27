using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers
{
    /// <summary>
    /// Helper bersama untuk menormalkan parameter filter tanggal/waktu pada layanan
    /// <c>InPatientManagement</c> menjadi <see cref="DateTimeKind.Utc"/> sebelum dikirim ke
    /// PostgreSQL (<c>timestamp with time zone</c>).
    /// </summary>
    /// <remarks>
    /// Query tanggal polos <c>YYYY-MM-DD</c> maupun ISO lokal tanpa zona waktu (<c>DateTimeKind.Unspecified</c>)
    /// ditafsirkan sebagai waktu operasional rumah sakit (<c>Asia/Jakarta</c>, WIB) via
    /// <see cref="AppDateTimeHelper"/> (keputusan <c>K-02</c> / <c>FIX-EPS-045</c>).
    /// </remarks>
    internal static class InpDateRangeHelper
    {
        private static readonly TimeSpan MinJakartaOffsetGuard = TimeSpan.FromHours(7);

        /// <summary>
        /// Menormalkan nilai <see cref="DateTime"/> ke <see cref="DateTimeKind.Utc"/>.
        /// Nilai <c>Utc</c> dipertahankan, <c>Local</c> dikonversi ke UTC, dan <c>Unspecified</c>
        /// ditafsirkan sebagai zona <c>Asia/Jakarta</c>.
        /// </summary>
        public static DateTime NormalizeToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified && value <= DateTime.MinValue + MinJakartaOffsetGuard)
            {
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            }

            return AppDateTimeHelper.ToUtc(value);
        }

        /// <summary>
        /// Menormalkan batas awal periode filter menjadi <see cref="DateTimeKind.Utc"/>.
        /// Input tanggal tanpa jam (<c>TimeOfDay == TimeSpan.Zero</c> dan <c>Unspecified</c>)
        /// dikonversi dari awal hari operasional WIB (<c>00:00:00+07:00</c>).
        /// </summary>
        public static DateTime NormalizeStartUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified && value.TimeOfDay == TimeSpan.Zero)
            {
                if (value.Date <= DateTime.MinValue.Date)
                {
                    return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                }

                return AppDateTimeHelper.OperationalDateToUtc(value);
            }

            return NormalizeToUtc(value);
        }

        /// <summary>
        /// Menentukan batas akhir periode dalam <see cref="DateTimeKind.Utc"/>.
        /// Jika input bertipe tanggal tanpa jam (<c>TimeOfDay == TimeSpan.Zero</c>), batas dibuat
        /// eksklusif (<c>&lt; hari+1 00:00 WIB</c> atau <c>UTC</c>); jika memuat komponen jam
        /// (mis. <c>23:59:59</c>), batas bersifat inklusif (<c>&lt;=</c>).
        /// </summary>
        public static (DateTime EndUtc, bool IsExclusive) ResolveEndUtc(DateTime value)
        {
            if (value.TimeOfDay == TimeSpan.Zero)
            {
                if (value.Date >= DateTime.MaxValue.Date)
                {
                    return (DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc), false);
                }

                var nextDay = value.AddDays(1);
                var nextDayUtc = value.Kind == DateTimeKind.Unspecified
                    ? AppDateTimeHelper.OperationalDateToUtc(nextDay)
                    : NormalizeToUtc(nextDay);

                return (nextDayUtc, true);
            }

            return (NormalizeToUtc(value), false);
        }
    }
}
