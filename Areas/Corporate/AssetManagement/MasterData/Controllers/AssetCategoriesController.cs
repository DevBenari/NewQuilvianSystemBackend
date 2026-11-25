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
[Route("api/v1/corporate/asset-management/master-data/categories")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Category",
    AreaName = "Corporate", ControllerName = "AssetCategory", Description = "Master data kategori aset", SortOrder = 1)]
[Tags("Corporate / Asset Management / Master Data / Asset Category")]
public sealed class AssetCategoriesController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetCategoriesController(AssetMasterDataService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCategoryMetadataAsync(cancellationToken), "Metadata filter kategori aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetCategorySummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCategorySummaryAsync(cancellationToken), "Ringkasan kategori aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetCategoryResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCategoriesAsync(request, cancellationToken), "Kategori aset berhasil diambil.");

    [HttpGet("options")]
    [AccessAction("Read", "Read Asset Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetMasterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetCategoryOptionsAsync(onlyActive, search, cancellationToken), "Pilihan kategori aset berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetCategoryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCategoryAsync(id, cancellationToken), "Detail kategori aset berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Category", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetCategory", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetCategoryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetCategoryUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateCategoryAsync(request, CurrentUserId(), cancellationToken), "Kategori aset berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Category", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetCategory", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetCategoryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetCategoryUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateCategoryAsync(id, request, CurrentUserId(), cancellationToken), "Kategori aset berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Asset Category Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetCategory", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetCategoryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] AssetMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateCategoryStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status kategori aset berhasil diubah.");

    [HttpPut("{id:guid}/document-requirements")]
    [AccessAction("Update", "Update Asset Category Document Requirements", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetCategory", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetCategoryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> SetDocumentRequirements(Guid id, [FromBody] List<Guid> documentTypeIds, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetCategoryDocumentRequirementsAsync(id, documentTypeIds, CurrentUserId(), cancellationToken), "Dokumen wajib kategori aset berhasil ditetapkan.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Category", AccessType = AccessTypes.Delete, SortOrder = 6)]
    [AccessPermission("AssetCategory", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterDeleteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteCategoryAsync(id, CurrentUserId(), cancellationToken), "Kategori aset berhasil dihapus.");
}
