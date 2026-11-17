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
[Route("api/v1/corporate/asset-management/master-data/document-types")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MASTER_DATA", "Corporate Asset Management Master Data", "Asset Document Type",
    AreaName = "Corporate", ControllerName = "AssetDocumentType", Description = "Master data jenis dokumen aset", SortOrder = 5)]
[Tags("Corporate / Asset Management / Master Data / Asset Document Type")]
public sealed class AssetDocumentTypesController : AssetMasterControllerBase
{
    private readonly AssetMasterDataService _service;

    public AssetDocumentTypesController(AssetMasterDataService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Document Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocumentType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDocumentTypeMetadataAsync(cancellationToken), "Metadata filter jenis dokumen aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Document Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocumentType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentTypeSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDocumentTypeSummaryAsync(cancellationToken), "Ringkasan jenis dokumen aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Document Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocumentType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetDocumentTypeResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetMasterPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDocumentTypesAsync(request, cancellationToken), "Jenis dokumen aset berhasil diambil.");

    [HttpGet("options")]
    [AccessAction("Read", "Read Asset Document Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocumentType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetMasterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetDocumentTypeOptionsAsync(onlyActive, search, cancellationToken), "Pilihan jenis dokumen aset berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Document Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocumentType", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDocumentTypeAsync(id, cancellationToken), "Detail jenis dokumen aset berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Document Type", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetDocumentType", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetDocumentTypeUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateDocumentTypeAsync(request, CurrentUserId(), cancellationToken), "Jenis dokumen aset berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Document Type", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetDocumentType", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetDocumentTypeUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateDocumentTypeAsync(id, request, CurrentUserId(), cancellationToken), "Jenis dokumen aset berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Asset Document Type Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetDocumentType", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentTypeResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] AssetMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateDocumentTypeStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status jenis dokumen aset berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Document Type", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("AssetDocumentType", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterDeleteResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteDocumentTypeAsync(id, CurrentUserId(), cancellationToken), "Jenis dokumen aset berhasil dihapus.");
}
