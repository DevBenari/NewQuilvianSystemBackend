using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Controllers;

/// <summary>
/// Rencana kebutuhan tahunan berversi (BE-PBJ-009; PBJ-PP-API bagian Need Plan).
/// </summary>
/// <remarks>
/// Transaksi ber-lifecycle: tidak ada <c>GET /options</c>, <c>PATCH /{id}/status</c>, atau
/// <c>DELETE /{id}</c>. Perpindahan status memakai <c>POST /{id}/&lt;aksi&gt;</c>; persetujuan lewat kotak
/// masuk Wfl. Rencana di luar cakupan unit pengguna dijawab 404.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/need-plans")]
[AccessController(ProcurementPermissions.PlanningModuleCode, ProcurementPermissions.PlanningModuleName, "Procurement Need Plan",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.NeedPlanResource,
    Description = "Rencana kebutuhan tahunan unit beserta revisinya", SortOrder = 1)]
[Tags("Corporate / Procurement Management / Planning / Need Plan")]
public sealed class NeedPlansController : ControllerBase
{
    private const string Resource = ProcurementPermissions.NeedPlanResource;
    private readonly ProcurementNeedPlanService _service;
    private readonly ProcurementApprovalTimelineService _approvalTimeline;

    public NeedPlansController(ProcurementNeedPlanService service, ProcurementApprovalTimelineService approvalTimeline)
    {
        _service = service;
        _approvalTimeline = approvalTimeline;
    }

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanFilterMetadataResponse>), StatusCodes.Status200OK)]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<NeedPlanFilterMetadataResponse>.Ok(_service.GetFilterMetadata(), "Metadata filter rencana berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(User, cancellationToken), "Ringkasan rencana berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NeedPlanResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] NeedPlanQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(User, request, cancellationToken), "Daftar rencana berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(User, id, cancellationToken), "Detail rencana berhasil diambil.");

    [HttpGet("{id:guid}/versions")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<NeedPlanResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetVersions(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetVersionsAsync(User, id, cancellationToken), "Versi rencana berhasil diambil.");

    [HttpGet("{id:guid}/compare/{otherId:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanVersionCompareResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Compare(Guid id, Guid otherId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CompareAsync(User, id, otherId, cancellationToken), "Perbandingan versi berhasil diambil.");

    [HttpGet("{id:guid}/status-history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetStatusHistoryAsync(User, id, cancellationToken), "Riwayat rencana berhasil diambil.");

    /// <summary>Langkah dan keputusan persetujuan <c>Wfl</c> dokumen ini; <c>data</c> kosong bila belum diajukan.</summary>
    [HttpGet("{id:guid}/approval")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Need Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementApprovalTimelineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetApproval(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approvalTimeline.GetAsync(User, ProcurementDocumentTypes.NeedPlan, id, cancellationToken),
            "Persetujuan rencana berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Need Plan", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreateNeedPlanRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(User, request, cancellationToken), "Draf rencana berhasil disimpan.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Need Plan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateNeedPlanRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(User, id, request, cancellationToken), "Draf rencana berhasil diubah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Need Plan", Description = "Mengajukan rencana ke persetujuan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Submit(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(User, id, idempotencyKey, cancellationToken), "Rencana berhasil diajukan.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Need Plan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] NeedPlanReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(User, id, request, cancellationToken), "Rencana berhasil dibatalkan.");

    [HttpPost("{id:guid}/revisions")]
    [AccessAction(ProcurementPermissions.ReviseAction, "Revise Procurement Need Plan", Description = "Membuat versi revisi dari rencana yang berlaku", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission(Resource, ProcurementPermissions.ReviseAction)]
    [ProducesResponseType(typeof(ApiResponse<NeedPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> CreateRevision(Guid id, [FromBody] NeedPlanReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateRevisionAsync(User, id, request, cancellationToken), "Revisi rencana berhasil dibuat.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementPeriodFilterException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementPlanningForbiddenException exception) { return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, exception.Message)); }
        catch (ProcurementPlanningConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code, existingId = exception.ExistingId }));
        }
        catch (ProcurementPlanningValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code, lineNo = exception.LineNo }));
        }
    }
}
