using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Jatuh tempo surat Pelunasan Deposit — <c>BE-RWI-202</c>, <c>RWI-DEC-231</c>, <c>248</c>,
    /// <c>261</c>, <c>VAL-RWA-19</c>. Fungsi murni.
    /// </summary>
    /// <remarks>
    /// Bawaan = hari kerja Senin–Jumat berikutnya sesudah tanggal surat, pukul 11.00 waktu rumah
    /// sakit, dipotong ke batas tanggal surat + <c>FollowUpIntervalDays</c> kebijakan deposit Billing
    /// walaupun batas itu jatuh pada hari Sabtu atau libur. Kalender libur tidak dibaca.
    ///
    /// <para>Contoh:</para>
    /// <list type="bullet">
    /// <item><description>Surat Jumat 9 Oktober 2026, interval 3 → bawaan Senin 12 Oktober 11.00 WIB;
    /// 13 Oktober ditolak "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit."</description></item>
    /// <item><description>Surat Jumat 9 Oktober, interval 1 → batas Sabtu 10 Oktober, bawaan Sabtu 10 Oktober 11.00 WIB.</description></item>
    /// <item><description>Surat Kamis 8 Oktober, interval 3 → bawaan Jumat 9 Oktober 11.00 WIB.</description></item>
    /// <item><description>Interval 0 → bawaan sama dengan tanggal surat.</description></item>
    /// </list>
    /// </remarks>
    public static class InpDepositDueDateCalculator
    {
        /// <summary>Jam jatuh tempo pada formulir V1 ("Pk. 11.00 WIB", <c>RWI-DEC-231</c>).</summary>
        public const int DueHour = 11;

        public static DateTime MaxDueDate(DateTime statementDate, int intervalDays)
            => statementDate.Date.AddDays(Math.Max(0, intervalDays));

        public static DateTime NextWorkingDay(DateTime date)
        {
            var next = date.Date.AddDays(1);

            while (next.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                next = next.AddDays(1);
            }

            return next;
        }

        public static DateTime DefaultDueDate(DateTime statementDate, int intervalDays)
        {
            var next = NextWorkingDay(statementDate);
            var max = MaxDueDate(statementDate, intervalDays);

            return next > max ? max : next;
        }

        /// <summary>Tanggal jatuh tempo pukul 11.00 waktu rumah sakit, disimpan UTC.</summary>
        public static DateTime ToDueAtUtc(DateTime dueDate, TimeZoneInfo timeZone)
            => TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(dueDate.Date.AddHours(DueHour), DateTimeKind.Unspecified),
                timeZone);

        /// <summary>
        /// Pesan penolakan bila jatuh tempo di luar batas; kosong bila sah. Tanpa interval
        /// (Billing tidak terbaca) hanya batas bawah yang diperiksa — batas atas diperiksa ulang
        /// saat dikunci, ketika Billing wajib terbaca.
        /// </summary>
        public static string? Validate(DateTime dueDate, DateTime statementDate, int? intervalDays)
        {
            if (dueDate.Date < statementDate.Date)
            {
                return "Jatuh tempo tidak boleh sebelum tanggal surat.";
            }

            if (intervalDays.HasValue)
            {
                var max = MaxDueDate(statementDate, intervalDays.Value);

                if (dueDate.Date > max)
                {
                    return $"Jatuh tempo paling lambat {InpAdmissionText.FormatLongDate(max)} menurut kebijakan deposit.";
                }
            }

            return null;
        }
    }
}
