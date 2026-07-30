using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs;
using QuilvianSystemBackend.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    internal static class WorkflowMasterDataSupport
    {
        public static Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        public static string? NormalizeNullableString(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public static string NormalizeCode(string value)
        {
            var normalized = Regex.Replace(value.Trim().ToUpperInvariant(), "[^A-Z0-9]+", "_");
            return normalized.Trim('_');
        }

        public static string NormalizePascalValue(string value) => value.Trim();

        public static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) =>
            (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));

        public static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue
                        ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                        : null,
                    endDate.HasValue
                        ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                        : null);
            }

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }

        public static IQueryable<T> ApplyDateFilter<T>(
            IQueryable<T> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
            where T : IdentityModel
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        public static bool IsValidJson(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            try
            {
                using var document = JsonDocument.Parse(value);
                return document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static List<WorkflowMasterCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };

        public static bool IsEffectiveDateValid(DateTime? start, DateTime? end) =>
            !start.HasValue || !end.HasValue || start.Value.Date <= end.Value.Date;
    }
}
