using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Controllers;

/// <summary>
/// Dokumen legalitas vendor (BE-PBJ-VD-004; PBJ-VD-API 1.0 §Vendor Document). Berkas diunggah lewat
/// <c>POST /attachments</c> dengan <c>documentType = PBJ_VENDOR_DOCUMENT</c>. Pemeriksa wajib bukan pengunggah
/// (PBJ-DEC-046). Controller hanya meneruskan ke <see cref="ProcurementVendorDocumentService"/> (QBE-SVC-001).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/vendor-documents")]
[AccessController(ProcurementPermissions.VendorModuleCode, ProcurementPermissions.VendorModuleName, "Procurement Vendor Document",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.VendorDocumentResource,
    Description = "Dokumen legalitas vendor dan verifikasinya", SortOrder = 2)]
[Tags("Corporate / Procurement Management / Vendor / Vendor Document")]
public sealed class VendorDocumentsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.VendorDocumentResource;
    private readonly ProcurementVendorDocumentService _service;

    public VendorDocumentsController(ProcurementVendorDocumentService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter dokumen vendor berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentSummaryCountsResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary([FromQuery] Guid? supplierId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(supplierId, cancellationToken), "Ringkasan dokumen vendor berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VendorDocumentListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] VendorDocumentQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(request, cancellationToken), "Daftar dokumen vendor berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken), "Detail dokumen vendor berhasil diambil.");

    [HttpGet("{id:guid}/history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Document", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(id, cancellationToken), "Riwayat dokumen vendor berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Document", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreateVendorDocumentRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Dokumen vendor berhasil dicatat.", StatusCodes.Status201Created);

    [HttpPost("{id:guid}/verify")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Vendor Document", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Verify(
        Guid id,
        [FromBody] VerifyVendorDocumentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.VerifyAsync(id, request, idempotencyKey, CurrentUserId(), cancellationToken), "Dokumen vendor berhasil diverifikasi.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Vendor Document", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Reject(Guid id, [FromBody] RejectVendorDocumentRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectAsync(id, request, CurrentUserId(), cancellationToken), "Dokumen vendor ditolak.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Document", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] CancelVendorDocumentRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, CurrentUserId(), cancellationToken), "Dokumen vendor dibatalkan.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message, int successStatus = StatusCodes.Status200OK)
    {
        try
        {
            var result = ApiResponse<T>.Ok(await command(), message);
            return successStatus == StatusCodes.Status200OK ? Ok(result) : StatusCode(successStatus, result);
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementPeriodFilterException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementVendorForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Code == null
                ? ApiResponse<object>.Fail(403, exception.Message)
                : ApiResponse<object>.Fail(403, exception.Message, new { code = exception.Code }));
        }
        catch (ProcurementVendorConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code })); }
        catch (ProcurementVendorValidationException exception)
        {
            return UnprocessableEntity(exception.Code == null
                ? ApiResponse<object>.Fail(422, exception.Message)
                : ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code }));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
