using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/master-data/tax-rules")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA", "Health Service Billing Management Master Data", "Tax Rule",
    AreaName = "HealthServices", ControllerName = "TaxRule", Description = "Effective-dated billing tax rule", SortOrder = 25)]
[Tags("Health Services / Billing Management / Master Data / Tax Rule")]
public sealed class TaxRulesController : ControllerBase
{
    private readonly TaxRuleService _service;
    public TaxRulesController(TaxRuleService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Tax Rule", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("TaxRule", "Read")]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<TaxRuleFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter tax rule berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Tax Rule", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("TaxRule", "Read")]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<TaxRuleSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan tax rule berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Tax Rule", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("TaxRule", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaxRuleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] TaxRuleQuery request, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<PagedResult<TaxRuleResponse>>.Ok(await _service.GetPagedAsync(request, cancellationToken), "Tax rule berhasil diambil.")); }
        catch (TaxRuleValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Tax Rule", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("TaxRule", "Create")]
    public Task<IActionResult> Create([FromBody] CreateTaxRuleRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpGet("options")]
    [AccessAction("Read", "Read Tax Rule", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("TaxRule", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<TaxRuleOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions(
        [FromQuery] string? taxableCategory,
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<TaxRuleOptionResponse>>.Ok(
            await _service.GetOptionsAsync(taxableCategory, onlyActive, search, cancellationToken),
            "Data pilihan tax rule berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Tax Rule", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("TaxRule", "Read")]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<TaxRuleResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail tax rule berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message)); }
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Tax Rule", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("TaxRule", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRuleRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [AccessAction("Update", "Deactivate Tax Rule", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("TaxRule", "Update")]
    public Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivatePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeactivateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [AccessAction("Update", "Activate Tax Rule", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("TaxRule", "Update")]
    public Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ActivateAsync(id, CurrentUserId(), cancellationToken));

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Tax Rule", AccessType = AccessTypes.Delete, SortOrder = 6)]
    [AccessPermission("TaxRule", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<TaxRuleDeleteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<TaxRuleDeleteResponse>.Ok(await _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Tax rule berhasil dihapus.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (TaxRuleValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<TaxRuleResponse>> command)
    {
        try { return Ok(ApiResponse<TaxRuleResponse>.Ok(await command(), "Tax rule berhasil diproses.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (TaxRuleConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (TaxRuleValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }
    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
