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
[Route("api/v1/corporate/asset-management/master-data/groups")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Group",
    AreaName = "Corporate", ControllerName = "AssetGroup", Description = "Master data kelompok aset", SortOrder = 3)]
[Tags("Corporate / Asset Management / Master Data / Asset Group")]
public sealed class AssetGroupsController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetGroupsController(AssetMasterDataService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Group", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetGroup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetGroupMetadataAsync(cancellationToken), "Metadata filter kelompok aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Group", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetGroup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetGroupSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetGroupSummaryAsync(cancellationToken), "Ringkasan kelompok aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Group", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetGroup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetGroupResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetGroupsAsync(request, cancellationToken), "Kelompok aset berhasil diambil.");

    [HttpGet("options")]
    [AccessAction("Read", "Read Asset Group", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetGroup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetMasterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetGroupOptionsAsync(onlyActive, search, categoryId, cancellationToken), "Pilihan kelompok aset berhasil diambil.");

    [HttpGet("inventory-codes")]
    [AccessAction("Read", "Read Asset Group", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetGroup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetInventoryCodeResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetInventoryCodes([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetInventoryCodesAsync(request, cancellationToken), "Kode inventaris berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Group", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetGroup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetGroupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetGroupAsync(id, cancellationToken), "Detail kelompok aset berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Group", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetGroup", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetGroupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetGroupUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateGroupAsync(request, CurrentUserId(), cancellationToken), "Kelompok aset berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Group", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetGroup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetGroupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetGroupUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateGroupAsync(id, request, CurrentUserId(), cancellationToken), "Kelompok aset berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Asset Group Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetGroup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetGroupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] AssetMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateGroupStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status kelompok aset berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Group", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("AssetGroup", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterDeleteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteGroupAsync(id, CurrentUserId(), cancellationToken), "Kelompok aset berhasil dihapus.");
}
