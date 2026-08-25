using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/cashier/shifts")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_CASHIER_SHIFT", "Health Service Billing Management", "Cashier Shifts",
    AreaName = "HealthServices", ControllerName = "CashierShifts", Description = "Cashier shift, handover, close, variance review, and reopen lifecycle", SortOrder = 4)]
[Tags("Health Services / Billing Management / Cashier / Shifts")]
public sealed class CashierShiftsController : ControllerBase
{
    private readonly CashierShiftService _service;

    public CashierShiftsController(CashierShiftService service)
    {
        _service = service;
    }

    [HttpPost("open")]
    [AccessAction("Create", "Open Cashier Shift", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("CashierShift", "Create")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Open(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] OpenShiftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.OpenAsync(
                request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken);
            var statusCode = result.IsReplay ? StatusCodes.Status200OK : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Shift sudah dibuka; hasil sebelumnya dikembalikan."
                    : "Shift kasir berhasil dibuka."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpGet("current")]
    [AccessAction("Read", "Read Current Cashier Shift", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("CashierShift", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Current(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetCurrentAsync(CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CashierShiftResponse>.Ok(
                result, "Shift kasir aktif berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Cashier Shift By Id", AccessType = AccessTypes.Read, SortOrder = 7)]
    [AccessPermission("CashierShift", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<CashierShiftResponse>.Ok(
                result, "Shift kasir berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpPost("{id:guid}/handover")]
    [AccessAction("Handover", "Handover Cashier Shift", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("CashierShift", "Handover")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handover(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] HandoverShiftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.HandoverAsync(
                id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken);
            return Ok(ApiResponse<CashierShiftResponse>.Ok(
                result,
                result.IsReplay
                    ? "Handover sudah diproses; hasil sebelumnya dikembalikan."
                    : "Handover shift berhasil diproses."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpPost("{id:guid}/close")]
    [AccessAction("Close", "Close Cashier Shift", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("CashierShift", "Close")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CloseShiftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CloseAsync(
                id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken);
            return Ok(ApiResponse<CashierShiftResponse>.Ok(
                result,
                result.IsReplay
                    ? "Penutupan shift sudah diproses; hasil sebelumnya dikembalikan."
                    : "Shift kasir berhasil ditutup."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpPost("{id:guid}/variance-reviews")]
    [AccessAction("Review", "Review Cash Variance", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("CashierShift", "Review")]
    [ProducesResponseType(typeof(ApiResponse<CashVarianceResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> ReviewVariance(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ReviewVarianceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ReviewVarianceAsync(
                id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken);
            var statusCode = result.IsReplay ? StatusCodes.Status200OK : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Review variance sudah diproses; hasil sebelumnya dikembalikan."
                    : "Variance kas berhasil direview."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpPost("{id:guid}/reopen")]
    [AccessAction("Reopen", "Reopen Cashier Shift", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("CashierShift", "Reopen")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reopen(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ReopenShiftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ReopenAsync(
                id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken);
            return Ok(ApiResponse<CashierShiftResponse>.Ok(
                result,
                result.IsReplay
                    ? "Reopen shift sudah diproses; hasil sebelumnya dikembalikan."
                    : "Shift kasir berhasil dibuka kembali."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(
            StatusCodes.Status404NotFound, exception.Message)),
        CashierShiftForbiddenException => StatusCode(
            StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message)),
        CashierShiftConflictException => Conflict(ApiResponse<object>.Fail(
            StatusCodes.Status409Conflict, exception.Message)),
        CashierShiftValidationException => UnprocessableEntity(ApiResponse<object>.Fail(
            StatusCodes.Status422UnprocessableEntity, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or
        CashierShiftForbiddenException or
        CashierShiftConflictException or
        CashierShiftValidationException;

    private static ApiResponse<T> Success<T>(T data, int statusCode, string message) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    private string CurrentRole() =>
        User.FindFirstValue(ClaimTypes.Role)
        ?? User.FindFirstValue("role")
        ?? "AuthenticatedUser";
}
