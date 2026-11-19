using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
/// Usulan blokir/buka blokir vendor (BE-PBJ-VD-007; PBJ-VD-API 1.0 §Vendor Status Request). Keputusan setuju/tolak/
/// kembalikan dikerjakan penyetuju di kotak masuk <c>Wfl</c> yang sudah ada. Controller hanya meneruskan ke
/// <see cref="ProcurementVendorStatusService"/> dan <see cref="ProcurementApprovalTimelineService"/> (QBE-SVC-001).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/vendor-status-requests")]
[AccessController(ProcurementPermissions.VendorModuleCode, ProcurementPermissions.VendorModuleName, "Procurement Vendor Status Request",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.VendorStatusRequestResource,
    Description = "Usulan blokir dan buka blokir vendor lewat persetujuan", SortOrder = 3)]
[Tags("Corporate / Procurement Management / Vendor / Vendor Status Request")]
public sealed class VendorStatusRequestsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.VendorStatusRequestResource;
    private readonly ProcurementVendorStatusService _service;
    private readonly ProcurementApprovalTimelineService _approvalTimeline;

    public VendorStatusRequestsController(ProcurementVendorStatusService service, ProcurementApprovalTimelineService approvalTimeline)
    {
        _service = service;
        _approvalTimeline = approvalTimeline;
    }

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Status Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter usulan status vendor berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Status Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan usulan status vendor berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Status Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VendorStatusRequestListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] VendorStatusRequestQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(request, cancellationToken), "Daftar usulan status vendor berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Status Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken), "Detail usulan status vendor berhasil diambil.");

    [HttpGet("{id:guid}/history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Status Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(id, cancellationToken), "Riwayat usulan status vendor berhasil diambil.");

    [HttpGet("{id:guid}/approval")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Status Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementApprovalTimelineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetApproval(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approvalTimeline.GetAsync(User, ProcurementDocumentTypes.VendorStatusRequest, id, cancellationToken),
            "Status persetujuan usulan berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Status Request", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreateVendorStatusRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Usulan status vendor berhasil dibuat.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Status Request", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorStatusRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken), "Usulan status vendor berhasil diubah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Status Request", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Submit(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SubmitVendorStatusRequestRequest? request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(id, request, idempotencyKey, CurrentUserId(), cancellationToken), "Usulan status vendor berhasil diajukan.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction(ProcurementPermissions.CancelAction, "Cancel Procurement Vendor Status Request", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.CancelAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] CancelVendorStatusRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, CurrentUserId(), cancellationToken), "Usulan status vendor dibatalkan.");

    /// <remarks>
    /// Delta kontrak (BE-PBJ-VD-007): sinkron ulang setelah penerapan keputusan gagal (PBJ-VD-INT §1), pola
    /// <c>PurchaseRequestVerificationsController</c> <c>POST /{id}/workflow/synchronize</c>, memakai izin <c>Create</c>.
    /// </remarks>
    [HttpPost("{id:guid}/workflow/synchronize")]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Status Request", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorStatusRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> SynchronizeWorkflow(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SynchronizeWorkflowAsync(id, CurrentUserId(), cancellationToken), "Status usulan berhasil disinkronkan dengan alur persetujuan.");

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
