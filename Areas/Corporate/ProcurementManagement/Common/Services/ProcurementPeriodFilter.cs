using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;

/// <summary>
/// Saringan Tanggal awal, Tanggal akhir, dan Periode pada daftar Pengadaan (PBJ-DEC-055, PBJ-FE-004).
/// Basis tanggal = tanggal data dibuat (<c>CreateDateTime</c>, disimpan UTC); batas hari mengikuti
/// tanggal operasional aplikasi (WIB), sehingga data yang dibuat dini hari tidak jatuh ke hari lain.
/// </summary>
/// <remarks>
/// Contoh: <c>customPeriod=thismonth</c> pada 17-11-2026 → dari 01-11-2026 00.00 WIB sampai sebelum
/// 01-12-2026 00.00 WIB. Tanggal akhir inklusif. Rentang terbalik atau periode tak dikenal ditolak
/// dengan <see cref="ProcurementPeriodFilterException"/> (422 <c>PBJ-VAL-094</c>).
/// </remarks>
public static class ProcurementPeriodFilter
{
    public const string ValidationCode = "PBJ-VAL-094";

    public static ProcurementPeriodRange Resolve(DateOnly? startDate, DateOnly? endDate, string? customPeriod)
    {
        var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        DateOnly? from;
        DateOnly? to;

        switch (customPeriod?.Trim().ToLowerInvariant())
        {
            case null or "" or "custom":
                from = startDate;
                to = endDate;
                break;
            case "today":
                from = today;
                to = today;
                break;
            case "last7days":
                from = today.AddDays(-6);
                to = today;
                break;
            case "last30days":
                from = today.AddDays(-29);
                to = today;
                break;
            case "thismonth":
                from = monthStart;
                to = monthStart.AddMonths(1).AddDays(-1);
                break;
            case "lastmonth":
                from = monthStart.AddMonths(-1);
                to = monthStart.AddDays(-1);
                break;
            default:
                throw new ProcurementPeriodFilterException("Periode tidak dikenal.");
        }

        if (from.HasValue && to.HasValue && to.Value < from.Value)
            throw new ProcurementPeriodFilterException("Tanggal akhir tidak boleh sebelum tanggal awal.");

        return new ProcurementPeriodRange(
            from.HasValue ? AppDateTimeHelper.OperationalDateToUtc(from.Value.ToDateTime(TimeOnly.MinValue)) : null,
            to.HasValue ? AppDateTimeHelper.OperationalDateToUtc(to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue)) : null);
    }

    /// <summary>Menyaring data yang dibuat di dalam rentang hasil <see cref="Resolve"/>.</summary>
    public static IQueryable<T> WhereCreatedWithin<T>(this IQueryable<T> query, ProcurementPeriodRange range)
        where T : IdentityModel
    {
        if (range.FromUtc is { } fromUtc) query = query.Where(x => x.CreateDateTime >= fromUtc);
        if (range.ToExclusiveUtc is { } toUtc) query = query.Where(x => x.CreateDateTime < toUtc);
        return query;
    }
}

/// <summary>Batas UTC saringan tanggal: <c>FromUtc</c> inklusif, <c>ToExclusiveUtc</c> eksklusif.</summary>
public sealed record ProcurementPeriodRange(DateTime? FromUtc, DateTime? ToExclusiveUtc);

/// <summary>Isian saringan tanggal tidak sah (HTTP 422, <c>PBJ-VAL-094</c>).</summary>
public sealed class ProcurementPeriodFilterException(string message) : Exception(message)
{
    public string Code { get; } = ProcurementPeriodFilter.ValidationCode;
}
