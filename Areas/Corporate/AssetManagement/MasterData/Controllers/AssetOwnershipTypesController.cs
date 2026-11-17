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
[Route("api/v1/corporate/asset-management/master-data/ownership-types")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Ownership Type",
    AreaName = "Corporate", ControllerName = "AssetOwnershipType", Description = "Master data jenis kepemilikan aset", SortOrder = 4)]
[Tags("Corporate / Asset Management / Master Data / Asset Ownership Type")]
public sealed class AssetOwnershipTypesController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetOwnershipTypesController(AssetMasterDataService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Ownership Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetOwnershipType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetOwnershipTypeMetadataAsync(cancellationToken), "Metadata filter jenis kepemilikan aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Ownership Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetOwnershipType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetOwnershipTypeSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetOwnershipTypeSummaryAsync(cancellationToken), "Ringkasan jenis kepemilikan aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Ownership Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetOwnershipType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetOwnershipTypeResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetOwnershipTypesAsync(request, cancellationToken), "Jenis kepemilikan aset berhasil diambil.");

    [HttpGet("options")]
    [AccessAction("Read", "Read Asset Ownership Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetOwnershipType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetMasterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetOwnershipTypeOptionsAsync(onlyActive, search, cancellationToken), "Pilihan jenis kepemilikan aset berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Ownership Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetOwnershipType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetOwnershipTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetOwnershipTypeAsync(id, cancellationToken), "Detail jenis kepemilikan aset berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Ownership Type", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetOwnershipType", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetOwnershipTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetOwnershipTypeUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateOwnershipTypeAsync(request, CurrentUserId(), cancellationToken), "Jenis kepemilikan aset berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Ownership Type", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetOwnershipType", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetOwnershipTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetOwnershipTypeUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateOwnershipTypeAsync(id, request, CurrentUserId(), cancellationToken), "Jenis kepemilikan aset berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Asset Ownership Type Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetOwnershipType", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetOwnershipTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] AssetMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateOwnershipTypeStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status jenis kepemilikan aset berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Ownership Type", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("AssetOwnershipType", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterDeleteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteOwnershipTypeAsync(id, CurrentUserId(), cancellationToken), "Jenis kepemilikan aset berhasil dihapus.");
}
