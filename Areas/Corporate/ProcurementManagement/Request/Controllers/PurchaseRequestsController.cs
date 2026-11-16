using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Controllers;

/// <summary>
/// Permintaan pengadaan (PR) — draf, daftar, ringkasan, detail, riwayat status (BE-PBJ-006); ajukan,
/// batal, tarik, dan pilihan baris rencana (BE-PBJ-007).
/// </summary>
/// <remarks>
/// Transaksi ber-lifecycle, bukan master data: tidak ada <c>GET /options</c>,
/// <c>PATCH /{id}/status</c>, atau <c>DELETE /{id}</c>. Perpindahan status memakai
/// <c>POST /{id}/&lt;aksi&gt;</c>: <c>submit</c> dan <c>cancel</c> (termasuk tarik dari persetujuan);
/// verifikasi ada di <c>PurchaseRequestVerificationsController</c> (BE-PBJ-008).
/// PR di luar cakupan unit pengguna dijawab 404.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/purchase-requests")]
[AccessController(ProcurementPermissions.RequestModuleCode, ProcurementPermissions.RequestModuleName, "Procurement Request",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.PurchaseRequestResource,
    Description = "Permintaan pengadaan barang/jasa dari unit", SortOrder = 1)]
[Tags("Corporate / Procurement Management / Request / Purchase Request")]
public sealed class PurchaseRequestsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.PurchaseRequestResource;
    private readonly ProcurementPurchaseRequestService _service;
    private readonly ProcurementApprovalTimelineService _approvalTimeline;

    public PurchaseRequestsController(ProcurementPurchaseRequestService service, ProcurementApprovalTimelineService approvalTimeline)
    {
        _service = service;
        _approvalTimeline = approvalTimeline;
    }

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestFilterMetadataResponse>), StatusCodes.Status200OK)]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<PurchaseRequestFilterMetadataResponse>.Ok(
            _service.GetFilterMetadata(), "Metadata filter permintaan berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(User, cancellationToken), "Ringkasan permintaan berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseRequestListItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] PurchaseRequestQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(User, request, cancellationToken), "Daftar permintaan berhasil diambil.");

    [HttpGet("plan-line-options")]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Request", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<List<PlanLineQuotaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetPlanLineOptions([FromQuery] PlanLineOptionsQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPlanLineOptionsAsync(User, request, cancellationToken), "Pilihan baris rencana berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(User, id, cancellationToken), "Detail permintaan berhasil diambil.");

    [HttpGet("{id:guid}/status-history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetStatusHistoryAsync(User, id, cancellationToken), "Riwayat permintaan berhasil diambil.");

    /// <summary>Langkah dan keputusan persetujuan <c>Wfl</c> dokumen ini; <c>data</c> kosong bila belum diajukan.</summary>
    [HttpGet("{id:guid}/approval")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementApprovalTimelineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetApproval(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approvalTimeline.GetAsync(User, ProcurementDocumentTypes.PurchaseRequest, id, cancellationToken),
            "Persetujuan permintaan berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Request", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreatePurchaseRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(User, request, cancellationToken), "Draf permintaan berhasil disimpan.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Request", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(User, id, request, cancellationToken), "Draf permintaan berhasil diubah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Request", Description = "Mengajukan permintaan untuk diverifikasi", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Submit(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(User, id, idempotencyKey, cancellationToken), "Permintaan berhasil diajukan.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction(ProcurementPermissions.CancelAction, "Cancel Procurement Request", Description = "Membatalkan atau menarik permintaan oleh pengusul", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission(Resource, ProcurementPermissions.CancelAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] CancelPurchaseRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(User, id, request, cancellationToken), "Permintaan berhasil dibatalkan.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementRequestForbiddenException exception) { return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, exception.Message)); }
        catch (ProcurementRequestConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code })); }
        catch (ProcurementRequestValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code, lineNo = exception.LineNo }));
        }
    }
}
