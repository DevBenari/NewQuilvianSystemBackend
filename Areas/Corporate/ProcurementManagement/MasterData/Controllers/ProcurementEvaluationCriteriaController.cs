using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Controllers;

/// <summary>
/// Master kriteria penilaian vendor per kategori (BE-PBJ-VD-002; PBJ-DEC-050, PBJ-VD-API 1.0) — sembilan endpoint baku
/// master data. <c>GET /summary?itemCategoryId=</c> memuat total bobot aktif kategori supaya layar memperingatkan bila
/// belum 100%. Controller hanya meneruskan ke <see cref="ProcurementEvaluationCriterionService"/>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/master-data/evaluation-criteria")]
[AccessController(ProcurementPermissions.MasterDataModuleCode, ProcurementPermissions.MasterDataModuleName, "Procurement Evaluation Criterion",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.EvaluationCriterionResource,
    Description = "Master kriteria penilaian vendor Pengadaan", SortOrder = 7)]
[Tags("Corporate / Procurement Management / Master Data / Evaluation Criterion")]
public sealed class ProcurementEvaluationCriteriaController : ControllerBase
{
    private const string Resource = ProcurementPermissions.EvaluationCriterionResource;
    private readonly ProcurementEvaluationCriterionService _service;

    public ProcurementEvaluationCriteriaController(ProcurementEvaluationCriterionService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Evaluation Criterion", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementEvaluationCriterionFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter kriteria penilaian berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Evaluation Criterion", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementEvaluationCriterionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetSummary([FromQuery] Guid? itemCategoryId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(itemCategoryId, cancellationToken), "Ringkasan kriteria penilaian berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Evaluation Criterion", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementEvaluationCriterionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] ProcurementEvaluationCriterionQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(request, cancellationToken), "Daftar kriteria penilaian berhasil diambil.");

    [HttpGet("options")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Evaluation Criterion", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<ProcurementEvaluationCriterionOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] Guid? itemCategoryId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetOptionsAsync(onlyActive, itemCategoryId, search, cancellationToken), "Pilihan kriteria penilaian berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Evaluation Criterion", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementEvaluationCriterionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken), "Detail kriteria penilaian berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Evaluation Criterion", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementEvaluationCriterionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] UpsertProcurementEvaluationCriterionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Kriteria penilaian berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Evaluation Criterion", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementEvaluationCriterionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpsertProcurementEvaluationCriterionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken), "Kriteria penilaian berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Evaluation Criterion", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementEvaluationCriterionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProcurementMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status kriteria penilaian berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction(ProcurementPermissions.DeleteAction, "Delete Procurement Evaluation Criterion", AccessType = AccessTypes.Delete, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.DeleteAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementMasterDeleteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Kriteria penilaian berhasil dihapus.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementPeriodFilterException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementMasterInUseException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message, new { code = exception.Code })); }
        catch (ProcurementMasterConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (ProcurementMasterValidationException exception)
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
