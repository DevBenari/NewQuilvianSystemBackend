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
[Route("api/v1/corporate/asset-management/register/condition-reports")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Condition",
    AreaName = "Corporate", ControllerName = "AssetCondition", Description = "Laporan dan penetapan kondisi aset", SortOrder = 3)]
[Tags("Corporate / Asset Management / Register / Asset Condition Report")]
public sealed class AssetConditionReportsController : AssetMasterControllerBase
{
    private readonly AssetConditionService _service;

    public AssetConditionReportsController(AssetConditionService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Condition", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCondition", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(_service.GetMetadata()), "Metadata laporan kondisi berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Condition", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCondition", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetConditionSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan laporan kondisi berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Condition", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCondition", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetConditionReportResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetConditionPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetReportsAsync(request, cancellationToken), "Daftar laporan kondisi berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Condition", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCondition", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetConditionReportResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetReportAsync(id, cancellationToken), "Rincian laporan kondisi berhasil diambil.");

    [HttpPost]
    [Consumes("multipart/form-data")]
    [AccessAction("Report", "Report Asset Condition", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetCondition", "Report")]
    [ProducesResponseType(typeof(ApiResponse<AssetConditionReportResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Report([FromForm] AssetConditionReportCreateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ReportAsync(request, cancellationToken), "Laporan kondisi berhasil dikirim dan menunggu penetapan Bagian Aset.");

    [HttpPost("{id:guid}/decide")]
    [AccessAction("Decide", "Decide Asset Condition", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetCondition", "Decide")]
    [ProducesResponseType(typeof(ApiResponse<AssetConditionReportResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Decide(Guid id, [FromBody] AssetConditionDecideRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.DecideAsync(id, request, cancellationToken),
            request?.Accept == false ? "Laporan kondisi ditolak." : "Kondisi aset ditetapkan.");

    [HttpPost("{id:guid}/approve-loss")]
    [Consumes("multipart/form-data")]
    [AccessAction("ApproveLoss", "Approve Asset Loss", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetCondition", "ApproveLoss")]
    [ProducesResponseType(typeof(ApiResponse<AssetConditionReportResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveLoss(Guid id, [FromForm] AssetConditionApproveLossRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ApproveLossAsync(id, request, cancellationToken), "Laporan kehilangan atau ditemukan kembali disetujui.");

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    [AccessAction("Read", "Read Asset Condition", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCondition", "Read")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        try
        {
            var content = await _service.OpenAttachmentAsync(id, attachmentId, cancellationToken);
            return File(content.Stream, content.ContentType, content.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }

    [HttpGet("{id:guid}/loss-document")]
    [AccessAction("Read", "Read Asset Condition", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetCondition", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    public async Task<IActionResult> GetLossDocument(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _service.GetLossDocumentAsync(id, cancellationToken);
            return File(document.Content, "application/pdf", document.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }
}
