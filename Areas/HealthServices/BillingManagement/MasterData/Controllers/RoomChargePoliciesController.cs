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

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Room Charge Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("RoomChargePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<RoomChargePolicyFilterMetadataResponse>), StatusCodes.Status200OK)]
    public IActionResult GetFilterMetadata() =>
        Ok(ApiResponse<RoomChargePolicyFilterMetadataResponse>.Ok(
            _service.GetFilterMetadata(), "Metadata filter room charge policy berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Room Charge Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("RoomChargePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<RoomChargePolicySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<RoomChargePolicySummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan room charge policy berhasil diambil."));

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

    [HttpGet("options")]
    [AccessAction("Read", "Read Room Charge Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("RoomChargePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<RoomChargePolicyOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<RoomChargePolicyOptionResponse>>.Ok(
            await _service.GetOptionsAsync(onlyActive, search, cancellationToken),
            "Data pilihan room charge policy berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Room Charge Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("RoomChargePolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<RoomChargePolicyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<RoomChargePolicyResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail room charge policy berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message)); }
    }

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

    [HttpPost("{id:guid}/activate")]
    [AccessAction("Update", "Activate Room Charge Policy", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("RoomChargePolicy", "Update")]
    public Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ActivateAsync(id, CurrentUserId(), cancellationToken));

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Room Charge Policy", AccessType = AccessTypes.Delete, SortOrder = 6)]
    [AccessPermission("RoomChargePolicy", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<RoomChargePolicyDeleteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<RoomChargePolicyDeleteResponse>.Ok(await _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Room charge policy berhasil dihapus.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (RoomChargePolicyValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

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
