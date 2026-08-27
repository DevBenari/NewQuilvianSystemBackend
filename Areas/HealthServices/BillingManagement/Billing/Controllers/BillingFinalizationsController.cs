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
[Route("api/v1/health-services/billing-management/billing/finalizations")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINALIZATION", "Health Service Billing Management", "Billing Finalizations",
    AreaName = "HealthServices", ControllerName = "BillingFinalizations", Description = "Readiness checklist and one-time invoice finalization with departure exception", SortOrder = 6)]
[Tags("Health Services / Billing Management / Billing / Finalizations")]
public sealed class BillingFinalizationsController : ControllerBase
{
    private readonly BillingFinalizationService _service;
    private readonly BillingArApHandoffService _handoffService;

    public BillingFinalizationsController(
        BillingFinalizationService service,
        BillingArApHandoffService handoffService)
    {
        _service = service;
        _handoffService = handoffService;
    }

    [HttpGet("invoices/{invoiceId:guid}/preview")]
    [AccessAction("Preview", "Preview Billing Finalization Readiness", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingFinalization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<FinalizationPreviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview(Guid invoiceId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PreviewAsync(invoiceId, cancellationToken);
            return Ok(ApiResponse<FinalizationPreviewResponse>.Ok(
                result, "Checklist kesiapan finalisasi berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinalizationValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("invoices/{invoiceId:guid}")]
    [AccessAction("Finalize", "Finalize Billing Invoice", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("BillingFinalization", "Create")]
    [ProducesResponseType(typeof(ApiResponse<FinalizationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FinalizationPreviewResponse>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Finalize(
        Guid invoiceId,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] FinalizeInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.FinalizeAsync(
                invoiceId, request, idempotencyKey, CurrentUserId(), cancellationToken);
            var statusCode = result.IsReplay
                ? StatusCodes.Status200OK
                : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Invoice sudah difinalisasi; hasil sebelumnya dikembalikan."
                    : "Invoice berhasil difinalisasi."));
        }
        catch (BillingFinalizationBlockedException exception)
        {
            return UnprocessableEntity(new ApiResponse<FinalizationPreviewResponse>
            {
                Success = false,
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Message = exception.Message,
                Data = exception.Checklist
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingFinalizationConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingFinalizationValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpGet("{id:guid}/handoffs")]
    [AccessAction("Handoffs", "Read Billing Finalization Handoff Status", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("BillingFinalization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<HandoffStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handoffs(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _handoffService.GetHandoffStatusAsync(id, cancellationToken);
            return Ok(ApiResponse<HandoffStatusResponse>.Ok(result, "Status handoff AR/AP berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
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
