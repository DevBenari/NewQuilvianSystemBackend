using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/master-data/petty-cash-categories")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA", "Health Service Billing Management Master Data", "Petty Cash Category",
    AreaName = "HealthServices", ControllerName = "PettyCashCategory", Description = "Master data kategori pengeluaran kas kecil", SortOrder = 27)]
[Tags("Health Services / Billing Management / Master Data / Petty Cash Category")]
public sealed class PettyCashCategoriesController : ControllerBase
{
    private readonly PettyCashCategoryService _service;
    public PettyCashCategoriesController(PettyCashCategoryService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Petty Cash Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<PettyCashCategoryFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter kategori petty cash berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Petty Cash Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashCategorySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<PettyCashCategorySummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan kategori petty cash berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Petty Cash Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PettyCashCategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PettyCashCategoryQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<PettyCashCategoryResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Kategori petty cash berhasil diambil."));

    [HttpGet("options")]
    [AccessAction("Read", "Read Petty Cash Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<PettyCashCategoryOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<PettyCashCategoryOptionResponse>>.Ok(
            await _service.GetOptionsAsync(onlyActive, search, cancellationToken),
            "Data pilihan kategori petty cash berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Petty Cash Category", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashCategory", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<PettyCashCategoryResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail kategori petty cash berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Petty Cash Category", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PettyCashCategory", "Create")]
    public Task<IActionResult> Create([FromBody] CreatePettyCashCategoryRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Petty Cash Category", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PettyCashCategory", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdatePettyCashCategoryRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Petty Cash Category Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PettyCashCategory", "Update")]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePettyCashCategoryStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateStatusAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Petty Cash Category", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("PettyCashCategory", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashCategoryDeleteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<PettyCashCategoryDeleteResponse>.Ok(await _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Kategori petty cash berhasil dihapus.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (PettyCashCategoryInUseException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<PettyCashCategoryResponse>> command)
    {
        try { return Ok(ApiResponse<PettyCashCategoryResponse>.Ok(await command(), "Kategori petty cash berhasil diproses.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (PettyCashCategoryConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (PettyCashCategoryValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
