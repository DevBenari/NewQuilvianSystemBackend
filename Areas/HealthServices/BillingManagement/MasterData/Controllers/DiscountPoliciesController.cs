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
[Route("api/v1/health-services/billing-management/master-data/discount-policies")]
[AccessController(
    "HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA",
    "Health Service Billing Management Master Data",
    "Discount Policy",
    AreaName = "HealthServices",
    ControllerName = "DiscountPolicy",
    Description = "Effective-dated billing discount policy",
    SortOrder = 24)]
[Tags("Health Services / Billing Management / Master Data / Discount Policy")]
public sealed class DiscountPoliciesController : ControllerBase
{
    private readonly DiscountPolicyService _service;

    public DiscountPoliciesController(DiscountPolicyService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Discount Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("DiscountPolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DiscountPolicyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] DiscountPolicyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<DiscountPolicyResponse>>.Ok(result, "Policy diskon berhasil diambil."));
        }
        catch (DiscountPolicyValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost]
    [AccessAction("Create", "Create Discount Policy", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("DiscountPolicy", "Create")]
    public Task<IActionResult> Create([FromBody] CreateDiscountPolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Discount Policy", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("DiscountPolicy", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountPolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [AccessAction("Update", "Deactivate Discount Policy", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("DiscountPolicy", "Update")]
    public Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivatePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteCommandAsync(() => _service.DeactivateAsync(id, request, CurrentUserId(), cancellationToken));

    private async Task<IActionResult> ExecuteCommandAsync(Func<Task<DiscountPolicyResponse>> command)
    {
        try
        {
            var result = await command();
            return Ok(ApiResponse<DiscountPolicyResponse>.Ok(result, "Policy diskon berhasil diproses."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (DiscountPolicyConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (DiscountPolicyValidationException exception)
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
