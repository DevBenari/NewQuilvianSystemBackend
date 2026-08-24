using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/billing/financial-exceptions")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION", "Health Service Billing Management", "Billing Financial Exceptions",
    AreaName = "HealthServices", ControllerName = "BillingFinancialExceptions", Description = "Refund, adjustment, and write-off case lifecycle for Billing financial exceptions", SortOrder = 5)]
[Tags("Health Services / Billing Management / Billing / Financial Exceptions")]
public sealed class BillingFinancialExceptionsController : ControllerBase
{
    private readonly BillingRefundService _service;
    private readonly BillingFinancialExceptionService _exceptionService;

    public BillingFinancialExceptionsController(
        BillingRefundService service,
        BillingFinancialExceptionService exceptionService)
    {
        _service = service;
        _exceptionService = exceptionService;
    }

    [HttpPost("refunds")]
    [AccessAction("CreateRefund", "Create Billing Refund", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("BillingRefund", "Create")]
    [ProducesResponseType(typeof(ApiResponse<RefundResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRefund(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreateRefundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(
                request, idempotencyKey, CurrentUserId(), cancellationToken);
            var statusCode = result.IsReplay
                ? StatusCodes.Status200OK
                : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Refund case sudah diajukan; hasil sebelumnya dikembalikan."
                    : "Refund case berhasil diajukan."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingRefundForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingRefundConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingRefundValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("refunds/{id:guid}/approve")]
    [AccessAction("ApproveRefund", "Approve Billing Refund", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("BillingRefund", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<RefundResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveRefund(
        Guid id,
        [FromBody] RefundApprovalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<RefundResponse>.Ok(result, "Refund case berhasil diproses."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingRefundForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingRefundConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingRefundValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("adjustments")]
    [AccessAction("CreateAdjustment", "Create Billing Adjustment", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("BillingAdjustment", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AdjustmentResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAdjustment(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreateAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _exceptionService.CreateAdjustmentAsync(
                request, idempotencyKey, CurrentUserId(), cancellationToken);
            var statusCode = result.IsReplay
                ? StatusCodes.Status200OK
                : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Adjustment sudah diajukan; hasil sebelumnya dikembalikan."
                    : "Adjustment berhasil diajukan."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinancialExceptionForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingFinancialExceptionConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingFinancialExceptionValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("adjustments/{id:guid}/approve")]
    [AccessAction("ApproveAdjustment", "Approve Billing Adjustment", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("BillingAdjustment", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AdjustmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveAdjustment(
        Guid id,
        [FromBody] AdjustmentApprovalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _exceptionService.ApproveAdjustmentAsync(
                id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<AdjustmentResponse>.Ok(result, "Adjustment berhasil disetujui dan diposting."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinancialExceptionForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingFinancialExceptionConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingFinancialExceptionValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("write-offs")]
    [AccessAction("CreateWriteOff", "Create Billing Write-off", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission("BillingWriteOff", "Create")]
    [ProducesResponseType(typeof(ApiResponse<WriteOffResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateWriteOff(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreateWriteOffRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _exceptionService.CreateWriteOffAsync(
                request, idempotencyKey, CurrentUserId(), cancellationToken);
            var statusCode = result.IsReplay
                ? StatusCodes.Status200OK
                : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Write-off sudah diajukan; hasil sebelumnya dikembalikan."
                    : "Write-off berhasil diajukan."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinancialExceptionForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingFinancialExceptionConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingFinancialExceptionValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("write-offs/{id:guid}/approve")]
    [AccessAction("ApproveWriteOff", "Approve Billing Write-off", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("BillingWriteOff", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<WriteOffResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveWriteOff(
        Guid id,
        [FromBody] WriteOffApprovalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _exceptionService.ApproveWriteOffAsync(
                id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<WriteOffResponse>.Ok(result, "Write-off berhasil disetujui dan diposting."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinancialExceptionForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingFinancialExceptionConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingFinancialExceptionValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("{type}/{id:guid}/reverse")]
    [AccessAction("Reverse", "Reverse Billing Financial Exception", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("BillingFinancialException", "Reverse")]
    [ProducesResponseType(typeof(ApiResponse<AdjustmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reverse(
        string type,
        Guid id,
        [FromBody] ReverseExceptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _exceptionService.ReverseAsync(
                type, id, request, CurrentUserId(), cancellationToken);
            var statusCode = result.IsReplay ? StatusCodes.Status200OK : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Entry sudah direversal sebelumnya; hasil sebelumnya dikembalikan."
                    : "Reversal berhasil dibuat sebagai entry baru."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinancialExceptionForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingFinancialExceptionConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingFinancialExceptionValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

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
}
