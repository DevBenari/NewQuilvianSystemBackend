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
[Route("api/v1/health-services/billing-management/billing/patient-funds")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_SETTLEMENT", "Health Service Billing Management", "Billing Settlements",
    AreaName = "HealthServices", ControllerName = "BillingSettlements", Description = "Split tender and provider-neutral payment attempt lifecycle", SortOrder = 3)]
[Tags("Health Services / Billing Management / Billing / Patient Funds")]
public sealed class BillingSettlementsController : ControllerBase
{
    private readonly BillingSettlementService _service;

    public BillingSettlementsController(BillingSettlementService service)
    {
        _service = service;
    }

    [HttpPost("settlements")]
    [AccessAction("Create", "Create Billing Settlement", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("BillingPayment", "Create")]
    [ProducesResponseType(typeof(ApiResponse<SettlementResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSettlement(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreateSettlementRequest request,
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
                    ? "Settlement sudah dibuat; hasil sebelumnya dikembalikan."
                    : "Settlement berhasil dibuat."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingSettlementConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingSettlementValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("settlements/{id:guid}/tenders")]
    [AccessAction("CreateTender", "Create Billing Tender", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("BillingPayment", "Create")]
    [ProducesResponseType(typeof(ApiResponse<TenderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TenderResponse>), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ApiResponse<TenderResponse>), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> AddTender(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreateTenderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.AddTenderAsync(
                id, request, idempotencyKey, CurrentUserId(), cancellationToken);
            var statusCode = result.IsReplay
                ? StatusCodes.Status200OK
                : StatusCodes.Status201Created;
            return StatusCode(statusCode, Success(
                result,
                statusCode,
                result.IsReplay
                    ? "Tender sudah diproses; hasil sebelumnya dikembalikan."
                    : "Tender berhasil diproses."));
        }
        catch (BillingSettlementProviderPendingException exception)
        {
            return StatusCode(exception.StatusCode, new ApiResponse<TenderResponse>
            {
                Success = false,
                StatusCode = exception.StatusCode,
                Message = exception.Message,
                Data = exception.Tender
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingSettlementConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingSettlementValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpGet("invoices/{invoiceId:guid}/settlements")]
    [AccessAction("Read", "Read Billing Settlement By Invoice", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("BillingPayment", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<SettlementResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettlementsByInvoice(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByInvoiceAsync(invoiceId, cancellationToken);
            return Ok(ApiResponse<List<SettlementResponse>>.Ok(
                result, "Riwayat settlement invoice berhasil diambil."));
        }
        catch (BillingSettlementValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpGet("settlements/{id:guid}")]
    [AccessAction("Read", "Read Billing Settlement", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("BillingPayment", "Read")]
    [ProducesResponseType(typeof(ApiResponse<SettlementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettlement(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetAsync(id, cancellationToken);
            return Ok(ApiResponse<SettlementResponse>.Ok(
                result, "Settlement dan split tender berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingSettlementValidationException exception)
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
