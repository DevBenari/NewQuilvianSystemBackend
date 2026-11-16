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
/// Pagu pengadaan berversi dan sisa pagu (BE-PBJ-010; PBJ-PP-API bagian Budget).
/// </summary>
/// <remarks>
/// Transaksi ber-lifecycle: tidak ada <c>GET /options</c>, <c>PATCH /{id}/status</c>, atau
/// <c>DELETE /{id}</c>. <c>GET /remaining</c> memakai <c>ProcurementBudget : Read</c> (bukan
/// <c>ProcurementRequest : Read</c> seperti kontrak 1.0) karena izin endpoint wajib memakai resource
/// controller-nya sendiri; sekaligus selaras dengan aturan "nominal pagu tidak tampil bagi pengguna tanpa
/// <c>ProcurementBudget : Read</c>".
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/budgets")]
[AccessController(ProcurementPermissions.PlanningModuleCode, ProcurementPermissions.PlanningModuleName, "Procurement Budget",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.BudgetResource,
    Description = "Pagu pengadaan per tahun, unit, kategori, dan sumber dana", SortOrder = 2)]
[Tags("Corporate / Procurement Management / Planning / Budget")]
public sealed class BudgetsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.BudgetResource;
    private readonly ProcurementBudgetService _service;

    public BudgetsController(ProcurementBudgetService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetFilterMetadataResponse>), StatusCodes.Status200OK)]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<BudgetFilterMetadataResponse>.Ok(_service.GetFilterMetadata(), "Metadata filter pagu berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(User, cancellationToken), "Ringkasan pagu berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BudgetResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] BudgetQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(User, request, cancellationToken), "Daftar pagu berhasil diambil.");

    [HttpGet("remaining")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<BudgetRemainingResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetRemaining([FromQuery] BudgetRemainingQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetRemainingAsync(User, request, cancellationToken), "Sisa pagu berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(User, id, cancellationToken), "Detail pagu berhasil diambil.");

    [HttpGet("{id:guid}/status-history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetStatusHistoryAsync(User, id, cancellationToken), "Riwayat pagu berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Budget", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(User, request, cancellationToken), "Draf pagu berhasil disimpan.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Budget", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(User, id, request, cancellationToken), "Draf pagu berhasil diubah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Budget", Description = "Mengajukan pagu ke persetujuan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Submit(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(User, id, idempotencyKey, cancellationToken), "Pagu berhasil diajukan.");

    [HttpPost("{id:guid}/revisions")]
    [AccessAction(ProcurementPermissions.ReviseAction, "Revise Procurement Budget", Description = "Membuat versi revisi dari pagu yang berlaku", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission(Resource, ProcurementPermissions.ReviseAction)]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> CreateRevision(Guid id, [FromBody] BudgetReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateRevisionAsync(User, id, request, cancellationToken), "Revisi pagu berhasil dibuat.");

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
