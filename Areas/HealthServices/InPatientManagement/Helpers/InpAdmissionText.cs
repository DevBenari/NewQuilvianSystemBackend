using System.Globalization;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers
{
    /// <summary>
    /// Pembentuk teks Workspace PPRI: zona waktu rumah sakit, tanggal berbahasa Indonesia, rupiah,
    /// dan penormalan telepon (<c>BE-RWI-193</c>, validation 15.9, <c>NFR-RWA-08</c>).
    /// </summary>
    /// <remarks>
    /// Seluruh waktu disimpan UTC dan ditampilkan menurut zona waktu profil rumah sakit, bawaan
    /// <c>Asia/Jakarta</c>. Contoh: 11.00 WIB disimpan <c>04:00Z</c>. Tidak ada identitas rumah sakit
    /// maupun kota yang ditanam di sini (<c>RWI-DEC-247</c>).
    /// </remarks>
    public static class InpAdmissionText
    {
        public const string DefaultTimeZoneId = "Asia/Jakarta";

        private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

        private static readonly string[] ShortMonths =
            { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

        private static readonly string[] LongMonths =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        /// <summary>Zona waktu rumah sakit; id yang kosong atau tidak dikenal jatuh ke <c>Asia/Jakarta</c>.</summary>
        public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            foreach (var candidate in new[] { timeZoneId, DefaultTimeZoneId, "SE Asia Standard Time" })
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(candidate.Trim());
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }

        /// <summary>Waktu UTC menjadi jam dinding rumah sakit.</summary>
        public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone)
        {
            var value = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(value, timeZone);
        }

        /// <summary>Tanggal hari ini menurut jam dinding rumah sakit.</summary>
        public static DateTime LocalToday(TimeZoneInfo timeZone)
            => ToLocal(DateTime.UtcNow, timeZone).Date;

        /// <summary>Contoh <c>07-10-2026</c>.</summary>
        public static string FormatDate(DateTime date)
            => date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

        /// <summary>Contoh <c>07-10-2026 10.32</c> dari waktu UTC.</summary>
        public static string FormatDateTime(DateTime utc, TimeZoneInfo timeZone)
            => ToLocal(utc, timeZone).ToString("dd-MM-yyyy HH.mm", CultureInfo.InvariantCulture);

        /// <summary>Contoh <c>09.50</c> dari waktu UTC.</summary>
        public static string FormatTime(DateTime utc, TimeZoneInfo timeZone)
            => ToLocal(utc, timeZone).ToString("HH.mm", CultureInfo.InvariantCulture);

        /// <summary>Contoh <c>12 Okt 2026 11.00</c> dari waktu UTC.</summary>
        public static string FormatShortDateTime(DateTime utc, TimeZoneInfo timeZone)
        {
            var local = ToLocal(utc, timeZone);

            return $"{local.Day:00} {ShortMonths[local.Month - 1]} {local.Year} {local:HH.mm}";
        }

        /// <summary>Contoh <c>12 Mar 1981</c>.</summary>
        public static string FormatShortDate(DateTime date)
            => $"{date.Day:00} {ShortMonths[date.Month - 1]} {date.Year}";

        /// <summary>Contoh <c>12 Oktober 2026</c>.</summary>
        public static string FormatLongDate(DateTime date)
            => $"{date.Day} {LongMonths[date.Month - 1]} {date.Year}";

        /// <summary>Contoh <c>Rp 3.000.000</c>.</summary>
        public static string FormatRupiah(decimal amount)
            => "Rp " + decimal.Round(amount, 0, MidpointRounding.AwayFromZero).ToString("N0", Indonesian);

        /// <summary>
        /// Telepon tanpa tanda hubung, spasi, dan titik (<c>VAL-RWA-14</c>). Contoh "0812-3456-7890" →
        /// "081234567890". Kosong dikembalikan kosong.
        /// </summary>
        public static string? NormalizePhone(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var value = raw.Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty)
                .Trim();

            return value.Length == 0 ? null : value;
        }

        /// <summary>Benar bila telepon yang sudah dinormalkan hanya angka dan tidak lebih dari batasnya.</summary>
        public static bool IsValidPhone(string? normalized, int maxDigits)
            => normalized == null || (normalized.Length <= maxDigits && normalized.All(char.IsDigit));

        public static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public static string GenderName(Gender? gender) => gender switch
        {
            Gender.Male => "Laki-laki",
            Gender.Female => "Perempuan",
            _ => "Tidak diketahui"
        };

        /// <summary>L / P untuk label pasien; kosong bila tidak diketahui.</summary>
        public static string? GenderInitial(Gender? gender) => gender switch
        {
            Gender.Male => "L",
            Gender.Female => "P",
            _ => null
        };

        public static string ReligionName(Religion religion) => religion switch
        {
            Religion.Islam => "Islam",
            Religion.ProtestantChristian => "Kristen Protestan",
            Religion.CatholicChristian => "Katolik",
            Religion.Hindu => "Hindu",
            Religion.Buddhist => "Buddha",
            Religion.Confucian => "Konghucu",
            Religion.Other => "Lainnya",
            _ => "Tidak diketahui"
        };
    }
}
