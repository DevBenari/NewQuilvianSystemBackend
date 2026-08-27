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
    private readonly BillingCalculationService _calculationService;
    private readonly BillingDiscountService _discountService;

    public BillingInvoicesController(
        BillingInvoiceService service,
        BillingCalculationService calculationService,
        BillingDiscountService discountService)
    {
        _service = service;
        _calculationService = calculationService;
        _discountService = discountService;
    }

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

    [HttpPost("{id:guid}/recalculate")]
    [AccessAction("Update", "Recalculate Billing Invoice", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("BillingInvoice", "Update")]
    [ProducesResponseType(typeof(ApiResponse<CalculationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Recalculate(
        Guid id,
        [FromBody] RecalculateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _calculationService.RecalculateAsync(
                id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CalculationResponse>.Ok(
                result, "Versi kalkulasi invoice berhasil dibuat."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingCalculationConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingCalculationValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/void")]
    [AccessAction("Update", "Void Eligible Billing Invoice Item", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("BillingInvoice", "Update")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VoidItem(
        Guid id,
        Guid itemId,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] VoidInvoiceItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.VoidItemAsync(
                id, itemId, request, idempotencyKey, CurrentUserId(), cancellationToken);
            var message = result.IsReplay
                ? "Pembatalan item sudah diproses; hasil sebelumnya dikembalikan."
                : "Item invoice berhasil dibatalkan dan versi kalkulasi baru dibuat.";
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
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("{id:guid}/kwitansi")]
    [AccessAction("Update", "Get or Allocate Kwitansi Number", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("BillingInvoice", "Update")]
    [ProducesResponseType(typeof(ApiResponse<KwitansiNumberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrAllocateKwitansiNumber(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetOrAllocateKwitansiNumberAsync(id, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<KwitansiNumberResponse>.Ok(result, "Nomor Kwitansi berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingInvoiceConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
    }

    [HttpPost("{id:guid}/discounts")]
    [AccessAction("Create", "Apply Billing Discount", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission("BillingDiscount", "Create")]
    [ProducesResponseType(typeof(ApiResponse<DiscountResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApplyDiscount(
        Guid id,
        [FromBody] ApplyDiscountRequest request,
        CancellationToken cancellationToken) =>
        ExecuteDiscountCommandAsync(() => _discountService.ApplyAsync(
            id, request, CurrentUserId(), cancellationToken), "Diskon berhasil diterapkan pada invoice.");

    [HttpPost("{id:guid}/discounts/{discountId:guid}/approve")]
    [AccessAction("Approve", "Approve Own Doctor Discount", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("BillingDoctorDiscount", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<DiscountResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveDoctorDiscount(
        Guid id,
        Guid discountId,
        [FromBody] ApproveDiscountRequest request,
        CancellationToken cancellationToken) =>
        ExecuteDiscountCommandAsync(() => _discountService.ApproveDoctorAsync(
            id, discountId, request, CurrentUserId(), cancellationToken), "Diskon jasa dokter berhasil disetujui.");

    private async Task<IActionResult> ExecuteDiscountCommandAsync(
        Func<Task<DiscountResponse>> command,
        string successMessage)
    {
        try
        {
            return Ok(ApiResponse<DiscountResponse>.Ok(await command(), successMessage));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingDiscountForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
        catch (BillingDiscountConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingDiscountValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
