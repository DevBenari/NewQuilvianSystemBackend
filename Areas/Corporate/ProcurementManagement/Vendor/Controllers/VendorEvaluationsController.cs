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
/// Penilaian vendor berkala (BE-PBJ-VD-008; PBJ-VD-API 1.0 §Vendor Evaluation). Penilai tidak memfinalkan penilaiannya
/// sendiri (PBJ-DEC-049); rekomendasi hanya saran (PBJ-DEC-048). Controller hanya meneruskan ke
/// <see cref="ProcurementVendorEvaluationService"/> (QBE-SVC-001).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/vendor-evaluations")]
[AccessController(ProcurementPermissions.VendorModuleCode, ProcurementPermissions.VendorModuleName, "Procurement Vendor Evaluation",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.VendorEvaluationResource,
    Description = "Penilaian vendor berkala per kategori dan periode", SortOrder = 4)]
[Tags("Corporate / Procurement Management / Vendor / Vendor Evaluation")]
public sealed class VendorEvaluationsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.VendorEvaluationResource;
    private readonly ProcurementVendorEvaluationService _service;

    public VendorEvaluationsController(ProcurementVendorEvaluationService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Evaluation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter penilaian vendor berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Evaluation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary([FromQuery] VendorEvaluationSummaryQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(request, cancellationToken), "Ringkasan penilaian vendor berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Evaluation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VendorEvaluationListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] VendorEvaluationQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(request, cancellationToken), "Daftar penilaian vendor berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Evaluation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken), "Detail penilaian vendor berhasil diambil.");

    [HttpGet("{id:guid}/history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor Evaluation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(id, cancellationToken), "Riwayat penilaian vendor berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Vendor Evaluation", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreateVendorEvaluationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Draf penilaian vendor berhasil dibuat.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Vendor Evaluation", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorEvaluationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken), "Penilaian vendor berhasil disimpan.");

    [HttpPost("{id:guid}/finalize")]
    [AccessAction(ProcurementPermissions.FinalizeAction, "Finalize Procurement Vendor Evaluation", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.FinalizeAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Finalize(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] FinalizeVendorEvaluationRequest? request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.FinalizeAsync(id, request, idempotencyKey, CurrentUserId(), cancellationToken), "Penilaian vendor berhasil difinalkan.");

    [HttpPost("{id:guid}/revisions")]
    [AccessAction(ProcurementPermissions.ReviseAction, "Revise Procurement Vendor Evaluation", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission(Resource, ProcurementPermissions.ReviseAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Revise(Guid id, [FromBody] ReviseVendorEvaluationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ReviseAsync(id, request, CurrentUserId(), cancellationToken), "Revisi penilaian vendor berhasil dibuat.", StatusCodes.Status201Created);

    [HttpPost("{id:guid}/cancel")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Vendor Evaluation", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorEvaluationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] CancelVendorEvaluationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, CurrentUserId(), cancellationToken), "Penilaian vendor dibatalkan.");

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
