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
[Route("api/v1/health-services/billing-management/billing/deposit-orders")]
[Route("v1/health-services/billing-management/billing/deposit-orders")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT", "Health Service Billing Management", "Billing Deposit Orders",
    AreaName = "HealthServices", ControllerName = "BillingDepositOrders", Description = "Inpatient deposit payment orders and cashier receipt workflow", SortOrder = 3)]
[Tags("Health Services / Billing Management / Billing / Deposit Orders")]
public sealed class BillingDepositOrdersController : ControllerBase
{
    private readonly BillingDepositOrderService _service;

    public BillingDepositOrdersController(BillingDepositOrderService service)
    {
        _service = service;
    }

    /// <summary>
    /// Membuat permintaan pembayaran deposit baru (mis. saat Admisi Rawat Inap).
    /// </summary>
    [HttpPost]
    [AccessAction("Create", "Create Inpatient Deposit Order", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("BillingDeposit", "Create")]
    [ProducesResponseType(typeof(ApiResponse<DepositOrderDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateDepositOrder(
        [FromBody] CreateDepositOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateOrderAsync(request, CurrentUserId(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<DepositOrderDetailDto>.Ok(result, "Order pembayaran deposit berhasil dibuat. Silakan lakukan pembayaran di Kasir."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (BillingDepositValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
        catch (BillingDepositConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
    }

    /// <summary>
    /// Mencari order deposit berdasarkan kata kunci No. RM, Nama Pasien, NIK, atau No. Order (Server-side search & paging).
    /// </summary>
    [HttpGet]
    [AccessAction("Read", "Search Inpatient Deposit Orders", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("BillingDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DepositOrderSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchDepositOrders(
        [FromQuery] DepositOrderQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.SearchOrdersAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DepositOrderSummaryDto>>.Ok(
            result, "Daftar order pembayaran deposit rawat inap berhasil diambil."));
    }

    /// <summary>
    /// Mengambil detail lengkap order deposit beserta riwayat pembayaran kasir dan posisi buku besar deposit.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Inpatient Deposit Order Detail", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("BillingDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DepositOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepositOrderDetail(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetOrderDetailAsync(id, cancellationToken);
            return Ok(ApiResponse<DepositOrderDetailDto>.Ok(
                result, "Detail order pembayaran deposit berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (BillingDepositValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    /// <summary>
    /// Memproses penerimaan pembayaran deposit kasir untuk order yang bersangkutan (pembayaran sebagian atau pelunasan).
    /// </summary>
    [HttpPost("{id:guid}/payments")]
    [AccessAction("Create", "Pay Inpatient Deposit Order", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("BillingDeposit", "Create")]
    [ProducesResponseType(typeof(ApiResponse<PayDepositOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PayDepositOrder(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] PayDepositOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (idempotencyKey != Guid.Empty)
            {
                request.IdempotencyKey = idempotencyKey;
            }

            var result = await _service.PayOrderAsync(id, request, CurrentUserId(), cancellationToken);
            var message = result.IsReplay
                ? "Pembayaran order deposit sudah diproses sebelumnya; hasil dikembalikan."
                : (result.OrderStatus == "PAID"
                    ? "Pembayaran deposit berhasil dicatat dan order telah lunas."
                    : "Pembayaran deposit berhasil dicatat (pembayaran sebagian).");

            return Ok(ApiResponse<PayDepositOrderResponse>.Ok(result, message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (BillingDepositConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (BillingDepositValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    /// <summary>
    /// Membatalkan order deposit yang belum memiliki pembayaran sah.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Create", "Cancel Inpatient Deposit Order", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission("BillingDeposit", "Create")]
    [ProducesResponseType(typeof(ApiResponse<DepositOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelDepositOrder(
        Guid id,
        [FromBody] CancelDepositOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelOrderAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<DepositOrderDetailDto>.Ok(
                result, "Order deposit berhasil dibatalkan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (BillingDepositValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    /// <summary>
    /// Mengambil data Kwitansi Bukti Penerimaan Deposit resmi.
    /// </summary>
    [HttpGet("payments/{paymentId:guid}/receipt")]
    [AccessAction("Read", "Read Inpatient Deposit Receipt", AccessType = AccessTypes.Read, SortOrder = 6)]
    [AccessPermission("BillingDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DepositReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepositReceipt(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetReceiptByPaymentIdAsync(paymentId, cancellationToken);
            return Ok(ApiResponse<DepositReceiptDto>.Ok(
                result, "Kwitansi pembayaran deposit berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
