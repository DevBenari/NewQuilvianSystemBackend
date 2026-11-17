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
[Route("api/v1/corporate/asset-management/master-data/locations")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Location",
    AreaName = "Corporate", ControllerName = "AssetLocation", Description = "Master data lokasi aset", SortOrder = 6)]
[Tags("Corporate / Asset Management / Master Data / Asset Location")]
public sealed class AssetLocationsController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetLocationsController(AssetMasterDataService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Location", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLocation", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLocationMetadataAsync(cancellationToken), "Metadata filter lokasi aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Location", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLocation", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetLocationSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLocationSummaryAsync(cancellationToken), "Ringkasan lokasi aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Location", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLocation", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetLocationResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLocationsAsync(request, cancellationToken), "Lokasi aset berhasil diambil.");

    [HttpGet("options")]
    [AccessAction("Read", "Read Asset Location", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLocation", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetMasterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        [FromQuery] Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetLocationOptionsAsync(onlyActive, search, organizationUnitId, cancellationToken), "Pilihan lokasi aset berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Location", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLocation", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetLocationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLocationAsync(id, cancellationToken), "Detail lokasi aset berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Location", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetLocation", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetLocationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetLocationUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateLocationAsync(request, CurrentUserId(), cancellationToken), "Lokasi aset berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Location", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetLocation", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetLocationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetLocationUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateLocationAsync(id, request, CurrentUserId(), cancellationToken), "Lokasi aset berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Asset Location Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetLocation", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetLocationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] AssetMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateLocationStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status lokasi aset berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Location", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("AssetLocation", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterDeleteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteLocationAsync(id, CurrentUserId(), cancellationToken), "Lokasi aset berhasil dihapus.");
}
