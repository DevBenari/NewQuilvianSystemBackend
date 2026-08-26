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
