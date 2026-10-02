using System;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Helper statis terpusat untuk perhitungan tanggal bisnis dan batas periode menurut kalender WIB (FIN-DES-082, FIN-DEC-116).
/// Menjadi satu-satunya tempat konversi zona waktu di Finance dengan fallback SE Asia Standard Time jika Asia/Jakarta tidak tersedia.
/// </summary>
public static class FinanceBusinessDate
{
    public const string BusinessTimeZoneId = "Asia/Jakarta";
    private const string WindowsBusinessTimeZoneId = "SE Asia Standard Time";

    private static readonly Lazy<TimeZoneInfo> LazyTimeZone = new(() =>
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(BusinessTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(WindowsBusinessTimeZoneId);
        }
    });

    /// <summary>
    /// Zona waktu bisnis (WIB / UTC+7).
    /// </summary>
    public static TimeZoneInfo BusinessTimeZone => LazyTimeZone.Value;

    /// <summary>
    /// Mengonversi tanda waktu UTC (DateTimeOffset) menjadi DateOnly menurut kalender WIB.
    /// </summary>
    public static DateOnly ToDateOnly(DateTimeOffset instant)
    {
        var local = TimeZoneInfo.ConvertTime(instant, BusinessTimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    /// <summary>
    /// Mengonversi DateTime menjadi DateOnly menurut kalender WIB.
    /// </summary>
    public static DateOnly ToDateOnly(DateTime instant)
    {
        var offset = instant.Kind switch
        {
            DateTimeKind.Utc => new DateTimeOffset(instant, TimeSpan.Zero),
            DateTimeKind.Local => new DateTimeOffset(instant),
            _ => new DateTimeOffset(instant, TimeSpan.Zero)
        };
        return ToDateOnly(offset);
    }

    /// <summary>
    /// Memulangkan tanggal hari ini menurut kalender WIB.
    /// </summary>
    public static DateOnly Today() => ToDateOnly(DateTimeOffset.UtcNow);

    /// <summary>
    /// Mengonversi tanda waktu menjadi waktu lokal bisnis (WIB).
    /// </summary>
    public static DateTimeOffset ToBusinessTime(DateTimeOffset instant)
    {
        return TimeZoneInfo.ConvertTime(instant, BusinessTimeZone);
    }

    /// <summary>
    /// Menghasilkan batas awal hari (00:00:00 WIB) dalam bentuk DateTimeOffset UTC.
    /// </summary>
    public static DateTimeOffset GetStartOfDayUtc(DateOnly date)
    {
        var localDateTime = date.ToDateTime(TimeOnly.MinValue);
        var offset = BusinessTimeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, offset).ToUniversalTime();
    }

    /// <summary>
    /// Menghasilkan batas akhir hari (awal hari berikutnya 00:00:00 WIB) dalam bentuk DateTimeOffset UTC.
    /// </summary>
    public static DateTimeOffset GetEndOfDayUtc(DateOnly date)
    {
        var nextDayLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var offset = BusinessTimeZone.GetUtcOffset(nextDayLocal);
        return new DateTimeOffset(nextDayLocal, offset).ToUniversalTime();
    }

    /// <summary>
    /// Menghasilkan batas akhir periode (23:59:59.999 WIB) dalam bentuk DateTimeOffset UTC.
    /// </summary>
    public static DateTimeOffset GetEndOfPeriodUtc(DateOnly periodEndDate)
    {
        var localDateTime = periodEndDate.ToDateTime(new TimeOnly(23, 59, 59, 999));
        var offset = BusinessTimeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, offset).ToUniversalTime();
    }

    /// <summary>
    /// Memulangkan tanggal akhir bulan untuk tahun dan bulan tertentu.
    /// </summary>
    public static DateOnly GetPeriodEndDate(int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        return new DateOnly(year, month, daysInMonth);
    }

    /// <summary>
    /// Memeriksa apakah suatu tanggal merupakan tanggal akhir bulan pada kalender WIB.
    /// </summary>
    public static bool IsPeriodEndDate(DateOnly date)
    {
        var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        return date.Day == daysInMonth;
    }
}
