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
[Route("api/v1/health-services/billing-management/billing/invoices")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING", "Health Service Billing Management", "Billing Invoice",
    AreaName = "HealthServices", ControllerName = "BillingInvoice", Description = "Running invoice and idempotent source charge", SortOrder = 1)]
[Tags("Health Services / Billing Management / Billing / Invoices")]
public sealed class BillingInvoicesController : ControllerBase
{
    private readonly BillingInvoiceService _service;
    public BillingInvoicesController(BillingInvoiceService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Billing Invoice", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingInvoice", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] BillingInvoiceQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<InvoiceSummaryResponse>>.Ok(result, "Invoice Billing berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Billing Invoice Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("BillingInvoice", "Read")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<InvoiceDetailResponse>.Ok(await _service.GetDetailAsync(id, cancellationToken), "Detail invoice Billing berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    [HttpPost("from-source")]
    [AccessAction("Create", "Create or Update Charge From Source", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("BillingInvoice", "Create")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FromSource(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] UpsertChargeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpsertChargeAsync(request, idempotencyKey, CurrentUserId(), cancellationToken);
            var message = result.IsReplay ? "Charge sudah diproses; hasil sebelumnya dikembalikan." : "Charge berhasil dicatat pada invoice Billing.";
            return Ok(ApiResponse<InvoiceDetailResponse>.Ok(result, message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingInvoiceConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingInvoiceValidationException exception)
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
