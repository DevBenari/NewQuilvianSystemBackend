using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    public const int DefaultIdleThresholdDays = 90;
    public const int DefaultDocumentExpiryWarningDays = 30;
    public const int DefaultUtilizationMaxMonthsForAssetHead = 12;

    public async Task<AssetSettingResponse> GetSettingAsync(CancellationToken cancellationToken = default)
    {
        var entityContext = await _entityResolver.ResolveAsync(cancellationToken);
        var setting = await _context.MstAssetSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LegalEntityId == entityContext.LegalEntityId && !x.IsDelete, cancellationToken);

        return MapSetting(setting, entityContext);
    }

    public async Task<AssetSettingResponse> UpdateSettingAsync(AssetSettingUpdateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IdleThresholdDays is < 1 or > 3650 ||
            request.DocumentExpiryWarningDays is < 1 or > 365 ||
            request.UtilizationMaxMonthsForAssetHead is < 1 or > 120)
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.SettingRange, AssetMasterMessages.SettingRange);
        }

        var entityContext = await _entityResolver.ResolveAsync(cancellationToken);
        var setting = await _context.MstAssetSettings
            .FirstOrDefaultAsync(x => x.LegalEntityId == entityContext.LegalEntityId && !x.IsDelete, cancellationToken);

        if (setting == null)
        {
            setting = new MstAssetSetting { LegalEntityId = entityContext.LegalEntityId };
            StampCreate(setting, actorUserId);
            _context.MstAssetSettings.Add(setting);
        }
        else
        {
            StampUpdate(setting, actorUserId);
        }

        setting.HospitalSiteId = entityContext.HospitalSiteId;
        setting.IdleThresholdDays = request.IdleThresholdDays;
        setting.DocumentExpiryWarningDays = request.DocumentExpiryWarningDays;
        setting.UtilizationMaxMonthsForAssetHead = request.UtilizationMaxMonthsForAssetHead;
        setting.CutoverDate = request.CutoverDate;

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetSetting.Update", new
        {
            setting.Id,
            setting.LegalEntityId,
            setting.HospitalSiteId,
            setting.IdleThresholdDays,
            setting.DocumentExpiryWarningDays,
            setting.UtilizationMaxMonthsForAssetHead,
            setting.CutoverDate
        }, actorUserId);

        return MapSetting(setting, entityContext);
    }

    private static AssetSettingResponse MapSetting(MstAssetSetting? setting, AssetEntityContext entityContext) => new()
    {
        Id = setting?.Id,
        IsSaved = setting != null,
        LegalEntityId = entityContext.LegalEntityId,
        LegalEntityName = entityContext.LegalEntityName,
        HospitalSiteId = entityContext.HospitalSiteId,
        HospitalSiteName = entityContext.HospitalSiteName,
        IdleThresholdDays = setting?.IdleThresholdDays ?? DefaultIdleThresholdDays,
        DocumentExpiryWarningDays = setting?.DocumentExpiryWarningDays ?? DefaultDocumentExpiryWarningDays,
        UtilizationMaxMonthsForAssetHead = setting?.UtilizationMaxMonthsForAssetHead ?? DefaultUtilizationMaxMonthsForAssetHead,
        CutoverDate = setting?.CutoverDate,
        UpdateDateTime = setting?.UpdateDateTime ?? setting?.CreateDateTime,
        UpdateBy = setting == null ? Guid.Empty : setting.UpdateBy == Guid.Empty ? setting.CreateBy : setting.UpdateBy
    };
}
