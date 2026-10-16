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
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT", "Health Service Billing Management", "Billing Finalizations",
    AreaName = "HealthServices", ControllerName = "BillingFinalization", Description = "Readiness checklist and one-time invoice finalization with departure exception", SortOrder = 6)]
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
    public async Task<IActionResult> Preview(
        Guid invoiceId,
        [FromQuery] decimal? choiceAmount,
        [FromQuery] string? consentGivenBy,
        [FromQuery] string? financingChoice,
        [FromQuery] decimal? employeeDeferredAmount,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PreviewAsync(
                invoiceId,
                cancellationToken,
                choiceAmount: choiceAmount,
                consentGivenBy: consentGivenBy,
                financingChoice: financingChoice,
                employeeDeferredAmount: employeeDeferredAmount);
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
            object? errors = null;
            if (!string.IsNullOrEmpty(exception.Code))
            {
                errors = new[]
                {
                    new
                    {
                        code = exception.Code,
                        currentRowVersion = exception.CurrentRowVersion
                    }
                };
            }
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message, errors));
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

    [HttpGet("invoices/{invoiceId:guid}/handoffs")]
    [AccessAction("HandoffsByInvoice", "Read Billing Finalization Handoff Status By Invoice", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("BillingFinalization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<HandoffStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> HandoffsByInvoice(Guid invoiceId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _handoffService.GetHandoffStatusByInvoiceAsync(invoiceId, cancellationToken);
            return Ok(ApiResponse<HandoffStatusResponse>.Ok(result, "Status handoff AR/AP berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>
    /// Mengoreksi pemilik manfaat salah orang pada tagihan yang sudah final lewat Berita Acara Koreksi resmi
    /// (BE-FIN-109; FIN-DEC-173, FIN-DEC-187, FIN-DEC-195, FIN-DEC-200; FR-FIN-231..234; INT-BIL-FIN-002 §P.6).
    /// </summary>
    [HttpPost("invoices/{invoiceId:guid}/correct-benefit-debtor")]
    [AccessAction("CorrectBenefitDebtor", "Correct Benefit Debtor By Official Report", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("BillingFinalization", "CorrectBenefitDebtor")]
    [ProducesResponseType(typeof(ApiResponse<CorrectBenefitDebtorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CorrectBenefitDebtor(
        [FromRoute] Guid invoiceId,
        [FromBody] CorrectBenefitDebtorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _handoffService.CorrectBenefitDebtorAsync(
                invoiceId, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CorrectBenefitDebtorResponse>.Ok(
                result, result.Message));
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
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
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
