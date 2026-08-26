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
[Route("api/v1/health-services/billing-management/master-data/room-charge-policies")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA", "Health Service Billing Management Master Data", "Room Charge Policy",
    AreaName = "HealthServices", ControllerName = "RoomChargePolicy", Description = "Effective-dated room charge policy", SortOrder = 26)]
[Tags("Health Services / Billing Management / Master Data / Room Charge Policy")]
public sealed class RoomChargePoliciesController : ControllerBase
{
    private readonly RoomChargePolicyService _service;
    public RoomChargePoliciesController(RoomChargePolicyService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Room Charge Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("RoomChargePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RoomChargePolicyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] RoomChargePolicyQuery request, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<PagedResult<RoomChargePolicyResponse>>.Ok(await _service.GetPagedAsync(request, cancellationToken), "Room charge policy berhasil diambil.")); }
        catch (RoomChargePolicyValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Room Charge Policy", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("RoomChargePolicy", "Create")]
    public Task<IActionResult> Create([FromBody] CreateRoomChargePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Room Charge Policy", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("RoomChargePolicy", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomChargePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [AccessAction("Update", "Deactivate Room Charge Policy", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("RoomChargePolicy", "Update")]
    public Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivatePolicyRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeactivateAsync(id, request, CurrentUserId(), cancellationToken));

    private async Task<IActionResult> ExecuteAsync(Func<Task<RoomChargePolicyResponse>> command)
    {
        try { return Ok(ApiResponse<RoomChargePolicyResponse>.Ok(await command(), "Room charge policy berhasil diproses.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (RoomChargePolicyConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (RoomChargePolicyValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }
    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
