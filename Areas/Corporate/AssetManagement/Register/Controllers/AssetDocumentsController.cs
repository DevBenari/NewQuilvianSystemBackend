using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/register/documents")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Document",
    AreaName = "Corporate", ControllerName = "AssetDocument", Description = "Dokumen kepemilikan dan pengamanan administrasi aset", SortOrder = 4)]
[Tags("Corporate / Asset Management / Register / Asset Document")]
public sealed class AssetDocumentsController : AssetMasterControllerBase
{
    private readonly AssetDocumentService _service;

    public AssetDocumentsController(AssetDocumentService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocument", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(_service.GetMetadata()), "Metadata dokumen kepemilikan berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocument", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan dokumen kepemilikan berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocument", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetDocumentResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetDocumentPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDocumentsAsync(request, cancellationToken), "Daftar dokumen kepemilikan berhasil diambil.");

    [HttpGet("completeness")]
    [AccessAction("Read", "Read Asset Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocument", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetDocumentCompletenessResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetCompleteness([FromQuery] AssetDocumentPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCompletenessAsync(request, cancellationToken), "Daftar aset dengan dokumen wajib belum lengkap berhasil diambil.");

    [HttpGet("expiring")]
    [AccessAction("Read", "Read Asset Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocument", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetDocumentResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetExpiring([FromQuery] AssetDocumentPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetExpiringAsync(request, cancellationToken), "Daftar dokumen habis atau hampir habis berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDocument", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDocumentAsync(id, cancellationToken), "Rincian dokumen kepemilikan berhasil diambil.");

    [HttpPost]
    [Consumes("multipart/form-data")]
    [AccessAction("Create", "Create Asset Document", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetDocument", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromForm] AssetDocumentUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, cancellationToken), "Dokumen kepemilikan berhasil ditambahkan.");

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [AccessAction("Update", "Update Asset Document", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetDocument", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetDocumentResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromForm] AssetDocumentUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, cancellationToken), "Dokumen kepemilikan berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Asset Document", AccessType = AccessTypes.Delete, SortOrder = 4)]
    [AccessPermission("AssetDocument", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteAsync(id, cancellationToken), "Dokumen kepemilikan berhasil dihapus.");
}
