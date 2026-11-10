using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Helpers
{
    public static class NvwPatientMasking
    {
        private const int MaxInitials = 3;
        private const string EmptyMark = "–";

        private static readonly TimeZoneInfo OperationalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

        public static string Initials(string? fullName)
        {
            var letters = (fullName ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(word => word.FirstOrDefault(char.IsLetter))
                .Where(letter => letter != default)
                .Take(MaxInitials)
                .Select(letter => $"{char.ToUpperInvariant(letter)}.")
                .ToList();

            return letters.Count == 0 ? EmptyMark : string.Concat(letters);
        }

        public static string GenderCode(Gender? gender) => gender switch
        {
            Gender.Male => "L",
            Gender.Female => "P",
            _ => EmptyMark
        };

        public static string AgeLabel(DateTime? birthDate, DateTime operationalToday)
        {
            if (!birthDate.HasValue)
                return $"Umur {EmptyMark}";

            var born = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(birthDate.Value, DateTimeKind.Utc),
                OperationalTimeZone).Date;
            var today = operationalToday.Date;

            if (born > today)
                return $"Umur {EmptyMark}";

            var years = today.Year - born.Year;
            if (born.AddYears(years) > today)
                years--;
            if (years >= 1)
                return $"{years} th";

            var months = (today.Year - born.Year) * 12 + today.Month - born.Month;
            if (today.Day < born.Day)
                months--;
            if (months >= 1)
                return $"{months} bln";

            return $"{(today - born).Days} hr";
        }
    }
}
