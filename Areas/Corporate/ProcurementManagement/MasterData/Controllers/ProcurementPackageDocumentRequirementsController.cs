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
/// Master dokumen wajib paket (BE-PBJ-PS-002; PBJ-PS-API 1.0) — sembilan endpoint baku master data. Controller hanya meneruskan ke
/// <see cref="ProcurementPackageDocumentRequirementService"/> (QBE-SVC-001).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/master-data/package-document-requirements")]
[AccessController(ProcurementPermissions.SourcingModuleCode, ProcurementPermissions.SourcingModuleName, "Procurement Package Document Requirement",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.PackageDocumentRequirementResource,
    Description = "Master dokumen wajib paket per metode atau per jenis pengadaan", SortOrder = 8)]
[Tags("Corporate / Procurement Management / Master Data / Package Document Requirement")]
public sealed class ProcurementPackageDocumentRequirementsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.PackageDocumentRequirementResource;
    private readonly ProcurementPackageDocumentRequirementService _service;

    public ProcurementPackageDocumentRequirementsController(ProcurementPackageDocumentRequirementService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Document Requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementPackageDocumentRequirementFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter dokumen wajib paket berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Document Requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementPackageDocumentRequirementSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan dokumen wajib paket berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Document Requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementPackageDocumentRequirementResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] ProcurementPackageDocumentRequirementQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(request, cancellationToken), "Daftar dokumen wajib paket berhasil diambil.");

    [HttpGet("options")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Document Requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<ProcurementPackageDocumentRequirementOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] Guid? methodId = null,
        [FromQuery] Guid? procurementTypeId = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetOptionsAsync(onlyActive, methodId, procurementTypeId, cancellationToken), "Pilihan dokumen wajib paket berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Document Requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementPackageDocumentRequirementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken), "Detail dokumen wajib paket berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Package Document Requirement", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementPackageDocumentRequirementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] UpsertProcurementPackageDocumentRequirementRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Dokumen wajib paket berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package Document Requirement", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementPackageDocumentRequirementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpsertProcurementPackageDocumentRequirementRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken), "Dokumen wajib paket berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package Document Requirement", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementPackageDocumentRequirementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProcurementMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status dokumen wajib paket berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction(ProcurementPermissions.DeleteAction, "Delete Procurement Package Document Requirement", AccessType = AccessTypes.Delete, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.DeleteAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementMasterDeleteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Dokumen wajib paket berhasil dihapus.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementPeriodFilterException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementMasterInUseException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message, new { code = exception.Code })); }
        catch (ProcurementMasterBadRequestException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
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
