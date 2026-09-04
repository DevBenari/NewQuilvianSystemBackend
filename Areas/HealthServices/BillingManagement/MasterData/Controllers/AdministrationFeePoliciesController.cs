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
[Route("api/v1/health-services/billing-management/master-data/administration-fee-policies")]
[AccessController(
    "HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA",
    "Health Service Billing Management Master Data",
    "Administration Fee Policy",
    AreaName = "HealthServices",
    ControllerName = "AdministrationFeePolicy",
    Description = "Effective-dated administration fee policy",
    SortOrder = 23)]
[Tags("Health Services / Billing Management / Master Data / Administration Fee Policy")]
public sealed class AdministrationFeePoliciesController : ControllerBase
{
    private readonly AdministrationFeePolicyService _service;

    public AdministrationFeePoliciesController(AdministrationFeePolicyService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Administration Fee Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AdministrationFeePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyFilterMetadataResponse>), StatusCodes.Status200OK)]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<AdministrationFeePolicyFilterMetadataResponse>.Ok(
            _service.GetFilterMetadata(), "Metadata filter policy biaya administrasi berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Administration Fee Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AdministrationFeePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<AdministrationFeePolicySummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan policy biaya administrasi berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Administration Fee Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AdministrationFeePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdministrationFeePolicyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] AdministrationFeePolicyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<AdministrationFeePolicyResponse>>.Ok(result, "Policy biaya administrasi berhasil diambil."));
        }
        catch (AdministrationFeePolicyValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost]
    [AccessAction("Create", "Create Administration Fee Policy", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AdministrationFeePolicy", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] CreateAdministrationFeePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpGet("options")]
    [AccessAction("Read", "Read Administration Fee Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AdministrationFeePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AdministrationFeePolicyOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions(
        [FromQuery] string? serviceType,
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<AdministrationFeePolicyOptionResponse>>.Ok(
            await _service.GetOptionsAsync(serviceType, onlyActive, search, cancellationToken),
            "Data pilihan policy biaya administrasi berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Administration Fee Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AdministrationFeePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<AdministrationFeePolicyResponse>.Ok(result, "Detail policy biaya administrasi berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Administration Fee Policy", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AdministrationFeePolicy", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateAdministrationFeePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [AccessAction("Update", "Deactivate Administration Fee Policy", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AdministrationFeePolicy", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivatePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.DeactivateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [AccessAction("Update", "Activate Administration Fee Policy", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AdministrationFeePolicy", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.ActivateAsync(id, CurrentUserId(), cancellationToken));

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Administration Fee Policy", AccessType = AccessTypes.Delete, SortOrder = 6)]
    [AccessPermission("AdministrationFeePolicy", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<AdministrationFeePolicyDeleteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.DeleteAsync(id, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<AdministrationFeePolicyDeleteResponse>.Ok(result, "Policy biaya administrasi berhasil dihapus."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (AdministrationFeePolicyValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    private async Task<IActionResult> ExecuteCommandAsync(Func<Task<AdministrationFeePolicyResponse>> command)
    {
        try
        {
            var result = await command();
            return Ok(ApiResponse<AdministrationFeePolicyResponse>.Ok(result, "Policy biaya administrasi berhasil diproses."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (AdministrationFeePolicyConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (AdministrationFeePolicyValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
