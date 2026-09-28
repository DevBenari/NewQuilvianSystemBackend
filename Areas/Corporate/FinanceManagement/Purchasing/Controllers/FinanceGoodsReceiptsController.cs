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
/// Tanda Terima Barang (BE-FIN-032, FIN-API-1.1 §B.2, FIN-PERM-1.1 §B.3). `GET /` (daftar
/// berpaging) belum ada service-nya — gap terbuka yang sama seperti `FinancePaymentsController`.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/purchasing/goods-receipts")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_GOODS_RECEIPT", "Corporate Finance Management Goods Receipt", "Goods Receipt",
    AreaName = "Corporate", ControllerName = "GoodsReceipt", Description = "Tanda Terima Barang Finance", SortOrder = 43)]
[Tags("Corporate / Finance Management / Purchasing / Goods Receipt")]
public sealed class FinanceGoodsReceiptsController : ControllerBase
{
    private readonly FinanceGoodsReceiptService _service;
    public FinanceGoodsReceiptsController(FinanceGoodsReceiptService service) => _service = service;

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Goods Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceGoodsReceipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var goodsReceipt = await _service.GetByIdAsync(id, cancellationToken);
        if (goodsReceipt is null) return NotFound(ApiResponse<object>.Fail(404, "Tanda Terima Barang tidak ditemukan."));
        return Ok(ApiResponse<GoodsReceiptDetailResponse>.Ok(MapDetail(goodsReceipt), "Detail Tanda Terima Barang berhasil diambil."));
    }

    [HttpPost]
    [AccessAction("Create", "Create Goods Receipt", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceGoodsReceipt", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var items = request.Items
                .Select(x => new GoodsReceiptItemRequest(x.PurchaseOrderItemId, x.ReceivedQuantity, x.Notes))
                .ToList();
            var goodsReceipt = await _service.CreateAsync(request.PurchaseOrderId, request.ReceivedDate, items, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<GoodsReceiptResponse>.Ok(Map(goodsReceipt), "Tanda Terima Barang berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Goods Receipt", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceGoodsReceipt", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] GoodsReceiptRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var goodsReceipt = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<GoodsReceiptResponse>.Ok(Map(goodsReceipt), "Tanda Terima Barang berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static GoodsReceiptResponse Map(FinGoodsReceipt goodsReceipt) => new()
    {
        Id = goodsReceipt.Id,
        GRNumber = goodsReceipt.GRNumber,
        PurchaseOrderId = goodsReceipt.PurchaseOrderId,
        ReceivedDate = goodsReceipt.ReceivedDate,
        Status = goodsReceipt.Status,
        RowVersion = goodsReceipt.RowVersion
    };

    private static GoodsReceiptDetailResponse MapDetail(FinGoodsReceipt goodsReceipt)
    {
        var mapped = Map(goodsReceipt);
        return new GoodsReceiptDetailResponse
        {
            Id = mapped.Id,
            GRNumber = mapped.GRNumber,
            PurchaseOrderId = mapped.PurchaseOrderId,
            ReceivedDate = mapped.ReceivedDate,
            Status = mapped.Status,
            RowVersion = mapped.RowVersion,
            Items = goodsReceipt.Items.Select(x => new GoodsReceiptItemResponse
            {
                Id = x.Id,
                PurchaseOrderItemId = x.PurchaseOrderItemId,
                ReceivedQuantity = x.ReceivedQuantity,
                Notes = x.Notes
            }).ToList()
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
