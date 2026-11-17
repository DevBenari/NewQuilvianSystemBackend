using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/master-data/settings")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Setting",
    AreaName = "Corporate", ControllerName = "AssetSetting", Description = "Pengaturan modul aset per badan hukum", SortOrder = 7)]
[Tags("Corporate / Asset Management / Master Data / Asset Setting")]
public sealed class AssetSettingsController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetSettingsController(AssetMasterDataService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Asset Setting", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetSetting", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetSettingResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSettingAsync(cancellationToken), "Pengaturan aset berhasil diambil.");

    [HttpPut]
    [AccessAction("Update", "Update Asset Setting", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("AssetSetting", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetSettingResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update([FromBody] AssetSettingUpdateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateSettingAsync(request, CurrentUserId(), cancellationToken), "Pengaturan aset berhasil disimpan.");
}
