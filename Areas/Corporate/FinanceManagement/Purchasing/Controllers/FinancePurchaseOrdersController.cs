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
/// Purchase Order (BE-FIN-032, FIN-API-1.1 §B.1, FIN-PERM-1.1 §B.2). Resource permission
/// memakai nama penuh kontrak (`FinancePurchaseOrder`) — berbeda dari `FinancePaymentsController`
/// yang memakai singkatan "Payment" sebagai utang teknis existing yang sudah didokumentasikan;
/// submodul Purchasing belum punya controller sebelumnya sehingga tidak ada pola legacy yang
/// perlu ditiru di sini (QBE canonical berlaku penuh untuk NEW CODE).
///
/// `GET /` (daftar berpaging) ditambahkan menyusul (penyelesaian `BE-FIN-032`, lihat laporan
/// task) — gap yang sebelumnya sama seperti `FinancePaymentsController`.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/purchasing/purchase-orders")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_PURCHASE_ORDER", "Corporate Finance Management Purchase Order", "Purchase Order",
    AreaName = "Corporate", ControllerName = "PurchaseOrder", Description = "Purchase Order Finance — pencatatan dan persetujuan berjenjang", SortOrder = 42)]
[Tags("Corporate / Finance Management / Purchasing / Purchase Order")]
public sealed class FinancePurchaseOrdersController : ControllerBase
{
    private readonly FinancePurchaseOrderService _service;
    public FinancePurchaseOrdersController(FinancePurchaseOrderService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Purchase Order", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchaseOrder", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseOrderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] PurchaseOrderQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        var mapped = new PagedResult<PurchaseOrderResponse>
        {
            Items = result.Items.Select(Map).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalData = result.TotalData,
            TotalPage = result.TotalPage
        };
        return Ok(ApiResponse<PagedResult<PurchaseOrderResponse>>.Ok(mapped, "Daftar Purchase Order berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Purchase Order", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchaseOrder", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _service.GetByIdAsync(id, cancellationToken);
        if (purchaseOrder is null) return NotFound(ApiResponse<object>.Fail(404, "Purchase Order tidak ditemukan."));
        return Ok(ApiResponse<PurchaseOrderDetailResponse>.Ok(MapDetail(purchaseOrder), "Detail Purchase Order berhasil diambil."));
    }

    [HttpPost]
    [AccessAction("Create", "Create Purchase Order", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinancePurchaseOrder", "Create")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var items = MapItemRequests(request.Items);
            var purchaseOrder = await _service.CreateAsync(request.SupplierId, items, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<PurchaseOrderResponse>.Ok(Map(purchaseOrder), "Purchase Order berhasil disusun."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Purchase Order", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinancePurchaseOrder", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var items = MapItemRequests(request.Items);
            var purchaseOrder = await _service.UpdateAsync(id, request.ExpectedRowVersion, items, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchaseOrderResponse>.Ok(Map(purchaseOrder), "Purchase Order berhasil diperbarui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Purchase Order", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinancePurchaseOrder", "Submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] PurchaseOrderRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var purchaseOrder = await _service.SubmitAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchaseOrderResponse>.Ok(Map(purchaseOrder), "Purchase Order berhasil diajukan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Purchase Order", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinancePurchaseOrder", "Approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] PurchaseOrderRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var purchaseOrder = await _service.ApproveAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchaseOrderResponse>.Ok(Map(purchaseOrder), "Purchase Order berhasil disetujui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Approve", "Approve Purchase Order", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinancePurchaseOrder", "Approve")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var purchaseOrder = await _service.RejectAsync(id, request.ExpectedRowVersion, request.RejectionReason, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchaseOrderResponse>.Ok(Map(purchaseOrder), "Purchase Order berhasil ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Purchase Order", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("FinancePurchaseOrder", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] PurchaseOrderRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var purchaseOrder = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchaseOrderResponse>.Ok(Map(purchaseOrder), "Purchase Order berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static List<PurchaseOrderItemRequest> MapItemRequests(List<PurchaseOrderItemRequestDto> items) =>
        items.Select(x => new PurchaseOrderItemRequest(x.ProductCategory, x.ProductName, x.Unit, x.Quantity, x.UnitPrice)).ToList();

    private static PurchaseOrderResponse Map(FinPurchaseOrder purchaseOrder) => new()
    {
        Id = purchaseOrder.Id,
        PONumber = purchaseOrder.PONumber,
        SupplierId = purchaseOrder.SupplierId,
        Status = purchaseOrder.Status,
        TotalAmount = purchaseOrder.TotalAmount,
        ApprovalTier = purchaseOrder.ApprovalTier,
        RequestedByUserId = purchaseOrder.RequestedByUserId,
        RequestedAt = purchaseOrder.RequestedAt,
        ApprovedByUserId = purchaseOrder.ApprovedByUserId,
        ApprovedAt = purchaseOrder.ApprovedAt,
        RowVersion = purchaseOrder.RowVersion
    };

    private static PurchaseOrderDetailResponse MapDetail(FinPurchaseOrder purchaseOrder)
    {
        var mapped = Map(purchaseOrder);
        return new PurchaseOrderDetailResponse
        {
            Id = mapped.Id,
            PONumber = mapped.PONumber,
            SupplierId = mapped.SupplierId,
            Status = mapped.Status,
            TotalAmount = mapped.TotalAmount,
            ApprovalTier = mapped.ApprovalTier,
            RequestedByUserId = mapped.RequestedByUserId,
            RequestedAt = mapped.RequestedAt,
            ApprovedByUserId = mapped.ApprovedByUserId,
            ApprovedAt = mapped.ApprovedAt,
            RowVersion = mapped.RowVersion,
            Items = purchaseOrder.Items.Select(x => new PurchaseOrderItemResponse
            {
                Id = x.Id,
                ProductCategory = x.ProductCategory,
                ProductName = x.ProductName,
                Unit = x.Unit,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.LineTotal
            }).ToList(),
            GoodsReceipts = purchaseOrder.GoodsReceipts
                .OrderByDescending(x => x.ReceivedDate)
                .Select(x => new PurchaseOrderGoodsReceiptSummaryResponse
                {
                    Id = x.Id,
                    GRNumber = x.GRNumber,
                    ReceivedDate = x.ReceivedDate,
                    Status = x.Status
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
