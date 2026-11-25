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
[Route("api/v1/corporate/asset-management/register/imports")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Import",
    AreaName = "Corporate", ControllerName = "AssetImport", Description = "Impor data awal aset dengan dua persetujuan", SortOrder = 6)]
[Tags("Corporate / Asset Management / Register / Asset Import")]
public sealed class AssetImportsController : AssetMasterControllerBase
{
    private readonly AssetImportService _service;

    public AssetImportsController(AssetImportService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Import", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetImport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(_service.GetMetadata()), "Metadata impor data awal berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Import", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetImport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan impor data awal berhasil diambil.");

    [HttpGet("template")]
    [AccessAction("Read", "Read Asset Import", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetImport", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, AssetImportWorkbook.ContentType)]
    public async Task<IActionResult> GetTemplate(CancellationToken cancellationToken)
    {
        try
        {
            var template = await _service.BuildTemplateAsync(cancellationToken);
            return File(template.Content, AssetImportWorkbook.ContentType, template.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }

    [HttpGet]
    [AccessAction("Read", "Read Asset Import", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetImport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetImportBatchResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetImportPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetBatchesAsync(request, cancellationToken), "Daftar batch impor berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Import", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetImport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportBatchResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetBatchAsync(id, cancellationToken), "Rincian batch impor berhasil diambil.");

    [HttpGet("{id:guid}/rows")]
    [AccessAction("Read", "Read Asset Import", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetImport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetImportRowResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetRows(Guid id, [FromQuery] AssetImportRowPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetRowsAsync(id, request, cancellationToken), "Baris batch impor berhasil diambil.");

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_000_000)]
    [AccessAction("Upload", "Upload Asset Import", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetImport", "Upload")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportBatchResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Upload([FromForm] AssetImportUploadRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UploadAsync(request, cancellationToken), "Berkas impor berhasil diunggah dan divalidasi.");

    [HttpPost("{id:guid}/approve-asset")]
    [AccessAction("ApproveAsset", "Approve Asset Import Data", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetImport", "ApproveAsset")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportBatchResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveAsset(Guid id, [FromBody] AssetImportDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ApproveAssetAsync(id, request, cancellationToken), "Data aset batch impor disetujui.");

    [HttpPost("{id:guid}/approve-value")]
    [AccessAction("ApproveValue", "Approve Asset Import Value", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetImport", "ApproveValue")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportBatchResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveValue(Guid id, [FromBody] AssetImportDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.ApproveValueAsync(id, request, cancellationToken),
            request?.Accept == false ? "Batch impor ditolak Akuntansi." : "Batch impor disetujui dan diterapkan.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction("ApproveAsset", "Approve Asset Import Data", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetImport", "ApproveAsset")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportBatchResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Reject(Guid id, [FromBody] AssetImportDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectAsync(id, request, cancellationToken), "Batch impor ditolak.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Upload", "Upload Asset Import", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetImport", "Upload")]
    [ProducesResponseType(typeof(ApiResponse<AssetImportBatchResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, cancellationToken), "Batch impor dibatalkan.");
}
