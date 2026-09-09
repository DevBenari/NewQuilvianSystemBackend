using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/petty-cash/budget")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_PETTY_CASH", "Health Service Billing Management Petty Cash", "Petty Cash Budget",
    AreaName = "HealthServices", ControllerName = "PettyCashBudget", Description = "Kolam anggaran dan saldo berjalan kas kecil", SortOrder = 1)]
[Tags("Health Services / Billing Management / Petty Cash / Budget")]
public sealed class PettyCashBudgetController : ControllerBase
{
    private readonly PettyCashBudgetService _service;
    public PettyCashBudgetController(PettyCashBudgetService service) => _service = service;

    [HttpGet("current")]
    [AccessAction("Read", "Read Petty Cash Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashBudget", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashBudgetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<PettyCashBudgetResponse>.Ok(
                await _service.GetCurrentAsync(cancellationToken), "Saldo kas kecil berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpGet("movements")]
    [AccessAction("Read", "Read Petty Cash Budget", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashBudget", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PettyCashBudgetMovementResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovements([FromQuery] PettyCashBudgetMovementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<PagedResult<PettyCashBudgetMovementResponse>>.Ok(
                await _service.GetMovementsAsync(request, cancellationToken), "Riwayat pergerakan anggaran kas kecil berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost("top-ups")]
    [AccessAction("TopUp", "Top Up Petty Cash Budget", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("PettyCashBudget", "TopUp")]
    public Task<IActionResult> TopUp(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] PettyCashBudgetTopUpRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.TopUpAsync(request, idempotencyKey, CurrentUserId(), cancellationToken),
            "Anggaran kas kecil berhasil ditambah.");

    [HttpPost("adjustments")]
    [AccessAction("Adjust", "Adjust Petty Cash Budget", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PettyCashBudget", "Adjust")]
    public Task<IActionResult> Adjust(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] PettyCashBudgetAdjustmentRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.AdjustAsync(request, idempotencyKey, CurrentUserId(), cancellationToken),
            "Saldo kas kecil berhasil dikoreksi.");

    private async Task<IActionResult> ExecuteAsync(Func<Task<PettyCashBudgetResponse>> command, string successMessage)
    {
        try { return Ok(ApiResponse<PettyCashBudgetResponse>.Ok(await command(), successMessage)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (PettyCashBudgetConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (PettyCashBudgetInsufficientBalanceException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
        catch (PettyCashBudgetValidationException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
