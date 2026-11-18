using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed record AssetRegisterSettingValues(int IdleThresholdDays, int DocumentExpiryWarningDays, int UtilizationMaxMonthsForAssetHead);

public static class AssetRegisterSettingReader
{
    public static async Task<AssetRegisterSettingValues> ReadAsync(ApplicationDbContext context, Guid legalEntityId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        var setting = await context.MstAssetSettings
            .AsNoTracking()
            .Where(x => x.LegalEntityId == legalEntityId && !x.IsDelete)
            .Select(x => new { x.IdleThresholdDays, x.DocumentExpiryWarningDays, x.UtilizationMaxMonthsForAssetHead })
            .FirstOrDefaultAsync(cancellationToken);

        return new AssetRegisterSettingValues(
            setting?.IdleThresholdDays ?? AssetMasterDataService.DefaultIdleThresholdDays,
            setting?.DocumentExpiryWarningDays ?? AssetMasterDataService.DefaultDocumentExpiryWarningDays,
            setting?.UtilizationMaxMonthsForAssetHead ?? AssetMasterDataService.DefaultUtilizationMaxMonthsForAssetHead);
    }

    public static DateOnly Today() => DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

    public static DateTime IdleCutoffUtc(int idleThresholdDays) =>
        AppDateTimeHelper.OperationalDateToUtc(Today().AddDays(-idleThresholdDays).ToDateTime(TimeOnly.MinValue));

    public static DateOnly OperationalDateOf(DateTime value) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(AppDateTimeHelper.ToUtc(value), OperationalTimeZone));

    public static int OperationalDaysSince(DateTime value) => Today().DayNumber - OperationalDateOf(value).DayNumber;

    private static readonly TimeZoneInfo OperationalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
}
