using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Controllers;

/// <summary>
/// Rencana paket hasil konsolidasi (BE-PBJ-011; PBJ-PP-API bagian Package Plan).
/// </summary>
/// <remarks>
/// Transaksi ber-lifecycle: tidak ada <c>GET /options</c>, <c>PATCH /{id}/status</c>, atau
/// <c>DELETE /{id}</c>. Rencana paket dibatalkan lewat <c>POST /{id}/cancel</c>.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/package-plans")]
[AccessController(ProcurementPermissions.PlanningModuleCode, ProcurementPermissions.PlanningModuleName, "Procurement Package Plan",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.PackagePlanResource,
    Description = "Konsolidasi kebutuhan unit dan rencana paket pengadaan", SortOrder = 3)]
[Tags("Corporate / Procurement Management / Planning / Package Plan")]
public sealed class PackagePlansController : ControllerBase
{
    private const string Resource = ProcurementPermissions.PackagePlanResource;
    private readonly ProcurementPackagePlanService _service;

    public PackagePlansController(ProcurementPackagePlanService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanFilterMetadataResponse>), StatusCodes.Status200OK)]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<PackagePlanFilterMetadataResponse>.Ok(_service.GetFilterMetadata(), "Metadata filter rencana paket berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(User, cancellationToken), "Ringkasan rencana paket berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PackagePlanResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] PackagePlanQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(User, request, cancellationToken), "Daftar rencana paket berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(User, id, cancellationToken), "Detail rencana paket berhasil diambil.");

    [HttpGet("{id:guid}/status-history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetStatusHistoryAsync(User, id, cancellationToken), "Riwayat rencana paket berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Package Plan", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreatePackagePlanRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(User, request, cancellationToken), "Draf rencana paket berhasil disimpan.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package Plan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdatePackagePlanRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(User, id, request, cancellationToken), "Draf rencana paket berhasil diubah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Package Plan", Description = "Mengajukan rencana paket ke persetujuan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Submit(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(User, id, idempotencyKey, cancellationToken), "Rencana paket berhasil diajukan.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction(ProcurementPermissions.CancelAction, "Cancel Procurement Package Plan", Description = "Membatalkan rencana paket", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission(Resource, ProcurementPermissions.CancelAction)]
    [ProducesResponseType(typeof(ApiResponse<PackagePlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] PackagePlanReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(User, id, request, cancellationToken), "Rencana paket berhasil dibatalkan.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
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
