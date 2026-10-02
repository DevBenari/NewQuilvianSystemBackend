using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Controllers;

/// <summary>
/// Tukar Faktur (BE-FIN-033, FIN-API-1.1 §B.3, FIN-PERM-1.1 §B.3). `GET /` (daftar berpaging)
/// ditambahkan menyusul (penyelesaian `BE-FIN-033`, lihat laporan task).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/purchasing/invoice-exchanges")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_INVOICE_EXCHANGE", "Corporate Finance Management Invoice Exchange", "Invoice Exchange",
    AreaName = "Corporate", ControllerName = "InvoiceExchange", Description = "Tukar Faktur Finance — checkpoint serah-terima dokumen faktur supplier", SortOrder = 44)]
[Tags("Corporate / Finance Management / Purchasing / Invoice Exchange")]
public sealed class FinanceInvoiceExchangesController : ControllerBase
{
    private readonly FinanceInvoiceExchangeService _service;
    public FinanceInvoiceExchangesController(FinanceInvoiceExchangeService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Invoice Exchange", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceInvoiceExchange", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceExchangeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] InvoiceExchangeQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        var mapped = new PagedResult<InvoiceExchangeResponse>
        {
            Items = result.Items.Select(Map).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalData = result.TotalData,
            TotalPage = result.TotalPage
        };
        return Ok(ApiResponse<PagedResult<InvoiceExchangeResponse>>.Ok(mapped, "Daftar Tukar Faktur berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Invoice Exchange", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceInvoiceExchange", "Read")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceExchangeDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoiceExchange = await _service.GetByIdAsync(id, cancellationToken);
        if (invoiceExchange is null) return NotFound(ApiResponse<object>.Fail(404, "Tukar Faktur tidak ditemukan."));
        return Ok(ApiResponse<InvoiceExchangeDetailResponse>.Ok(MapDetail(invoiceExchange), "Detail Tukar Faktur berhasil diambil."));
    }

    [HttpPost]
    [AccessAction("Create", "Create Invoice Exchange", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceInvoiceExchange", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceExchangeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var invoiceExchange = await _service.CreateAsync(
                request.SupplierId, request.PurchaseOrderId, request.GoodsReceiptId,
                request.SupplierInvoiceNumber, request.SupplierInvoiceDate, request.ReceivedDate,
                CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<InvoiceExchangeResponse>.Ok(Map(invoiceExchange), "Tukar Faktur berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Invoice Exchange", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceInvoiceExchange", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] InvoiceExchangeRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var invoiceExchange = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InvoiceExchangeResponse>.Ok(Map(invoiceExchange), "Tukar Faktur berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static InvoiceExchangeResponse Map(FinInvoiceExchange invoiceExchange) => new()
    {
        Id = invoiceExchange.Id,
        ExchangeNumber = invoiceExchange.ExchangeNumber,
        SupplierId = invoiceExchange.SupplierId,
        PurchaseOrderId = invoiceExchange.PurchaseOrderId,
        GoodsReceiptId = invoiceExchange.GoodsReceiptId,
        SupplierInvoiceNumber = invoiceExchange.SupplierInvoiceNumber,
        SupplierInvoiceDate = invoiceExchange.SupplierInvoiceDate,
        ReceivedDate = invoiceExchange.ReceivedDate,
        EstimatedDueDate = invoiceExchange.EstimatedDueDate,
        Status = invoiceExchange.Status,
        RowVersion = invoiceExchange.RowVersion
    };

    private static InvoiceExchangeDetailResponse MapDetail(FinInvoiceExchange invoiceExchange)
    {
        var mapped = Map(invoiceExchange);
        return new InvoiceExchangeDetailResponse
        {
            Id = mapped.Id,
            ExchangeNumber = mapped.ExchangeNumber,
            SupplierId = mapped.SupplierId,
            PurchaseOrderId = mapped.PurchaseOrderId,
            GoodsReceiptId = mapped.GoodsReceiptId,
            SupplierInvoiceNumber = mapped.SupplierInvoiceNumber,
            SupplierInvoiceDate = mapped.SupplierInvoiceDate,
            ReceivedDate = mapped.ReceivedDate,
            EstimatedDueDate = mapped.EstimatedDueDate,
            Status = mapped.Status,
            RowVersion = mapped.RowVersion
        };
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PurchasingForbiddenException => StatusCode(403, ApiResponse<object>.Fail(403, exception.Message)),
        PurchasingConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PurchasingValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PurchasingBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or PurchasingForbiddenException or PurchasingConflictException
        or PurchasingValidationException or PurchasingBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
