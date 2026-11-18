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
[Route("api/v1/corporate/asset-management/master-data/classifications")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Classification",
    AreaName = "Corporate", ControllerName = "AssetClassification", Description = "Master data klasifikasi aset", SortOrder = 2)]
[Tags("Corporate / Asset Management / Master Data / Asset Classification")]
public sealed class AssetClassificationsController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetClassificationsController(AssetMasterDataService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Classification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetClassification", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetClassificationMetadataAsync(cancellationToken), "Metadata filter klasifikasi aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Classification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetClassification", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetClassificationSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetClassificationSummaryAsync(cancellationToken), "Ringkasan klasifikasi aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Classification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetClassification", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetClassificationResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetClassificationsAsync(request, cancellationToken), "Klasifikasi aset berhasil diambil.");

    [HttpGet("options")]
    [AccessAction("Read", "Read Asset Classification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetClassification", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetMasterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetClassificationOptionsAsync(onlyActive, search, cancellationToken), "Pilihan klasifikasi aset berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Classification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetClassification", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetClassificationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetClassificationAsync(id, cancellationToken), "Detail klasifikasi aset berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Classification", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetClassification", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetClassificationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetClassificationUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateClassificationAsync(request, CurrentUserId(), cancellationToken), "Klasifikasi aset berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Classification", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetClassification", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetClassificationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetClassificationUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateClassificationAsync(id, request, CurrentUserId(), cancellationToken), "Klasifikasi aset berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Asset Classification Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetClassification", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetClassificationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] AssetMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateClassificationStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status klasifikasi aset berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Classification", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("AssetClassification", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterDeleteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteClassificationAsync(id, CurrentUserId(), cancellationToken), "Klasifikasi aset berhasil dihapus.");
}
