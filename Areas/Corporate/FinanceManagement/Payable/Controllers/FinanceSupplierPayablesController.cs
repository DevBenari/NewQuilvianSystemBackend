using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Controllers;

/// <summary>
/// Resource permission ("SupplierPayable") mengikuti pola penamaan singkat yang sudah berjalan
/// nyata pada `FinanceReceivablesController` ("Receivable", bukan "FinanceReceivable"), bukan
/// nama resource persis `contracts/permission-audit-matrix.md` (`FinanceSupplierPayable`).
/// Hanya endpoint yang sudah punya logika service nyata yang dibangun di sini (BE-FIN-019,
/// pembaruan 23 September 2026): rincian, input manual, koreksi (maker-checker), dan pembatalan.
/// `GET /supplier-payables` (daftar berpaging) dan `GET /supplier-payables/aging` ada pada
/// kontrak tetapi service-nya belum ada — dicatat sebagai gap terbuka pada laporan task, bukan
/// dikarang di sini. `PUT /supplier-payables/{id}` juga belum ada `UpdateAsync` di service.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/supplier-payables")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_SUPPLIER_PAYABLE", "Corporate Finance Management Supplier Payable", "Supplier Payable",
    AreaName = "Corporate", ControllerName = "SupplierPayable", Description = "Utang supplier Finance — input manual, koreksi, dan pembatalan", SortOrder = 40)]
[Tags("Corporate / Finance Management / Supplier Payable")]
public sealed class FinanceSupplierPayablesController : ControllerBase
{
    private readonly FinanceSupplierPayableService _service;
    public FinanceSupplierPayablesController(FinanceSupplierPayableService service) => _service = service;

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Supplier Payable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("SupplierPayable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPayableDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var payable = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<SupplierPayableDetailResponse>.Ok(Map(payable), "Detail utang supplier berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Supplier Payable", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("SupplierPayable", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierPayableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var items = (request.Items ?? []).Select(i => new SupplierPayableItemRequest(i.Description, i.Quantity, i.UnitPrice)).ToList();
            var payable = await _service.CreateAsync(
                request.SupplierId, request.SupplierInvoiceNumber, request.SupplierInvoiceDate, request.Description,
                items, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<SupplierPayableResponse>.Ok(MapPayable(payable), "Utang supplier berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/adjustments")]
    [AccessAction("RequestAdjustment", "Request Supplier Payable Adjustment", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("SupplierPayable", "RequestAdjustment")]
    public async Task<IActionResult> RequestAdjustment(Guid id, [FromBody] RequestPayableAdjustmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RequestAdjustmentAsync(id, request.Direction, request.Amount, request.Reason, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<PayableAdjustmentResponse>.Ok(MapAdjustment(result), "Pengajuan koreksi utang supplier berhasil dibuat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/adjustments/{adjustmentId:guid}/approve")]
    [AccessAction("ApproveAdjustment", "Approve Supplier Payable Adjustment", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("SupplierPayable", "ApproveAdjustment")]
    public async Task<IActionResult> ApproveAdjustment(Guid id, Guid adjustmentId, [FromBody] DecidePayableAdjustmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAdjustmentAsync(adjustmentId, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PayableAdjustmentResponse>.Ok(MapAdjustment(result), "Koreksi utang supplier berhasil disetujui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/adjustments/{adjustmentId:guid}/reject")]
    [AccessAction("ApproveAdjustment", "Approve Supplier Payable Adjustment", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("SupplierPayable", "ApproveAdjustment")]
    public async Task<IActionResult> RejectAdjustment(Guid id, Guid adjustmentId, [FromBody] DecidePayableAdjustmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectAdjustmentAsync(adjustmentId, request.ExpectedRowVersion, request.RejectionReason ?? string.Empty, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PayableAdjustmentResponse>.Ok(MapAdjustment(result), "Koreksi utang supplier berhasil ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Supplier Payable", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("SupplierPayable", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSupplierPayableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<SupplierPayableResponse>.Ok(MapPayable(result), "Utang supplier berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    internal static SupplierPayableResponse MapPayable(FinSupplierPayable payable) => new()
    {
        Id = payable.Id,
        PayableNumber = payable.PayableNumber,
        SupplierId = payable.SupplierId,
        SupplierInvoiceNumber = payable.SupplierInvoiceNumber,
        SupplierInvoiceDate = payable.SupplierInvoiceDate,
        Description = payable.Description,
        OriginalAmount = payable.OriginalAmount,
        OutstandingAmount = payable.OutstandingAmount,
        PaidAmount = payable.PaidAmount,
        AdjustedAmount = payable.AdjustedAmount,
        DueDate = payable.DueDate,
        PaymentTermDays = payable.PaymentTermDays,
        Status = payable.Status,
        RowVersion = payable.RowVersion
    };

    internal static SupplierPayableDetailResponse Map(FinSupplierPayable payable)
    {
        var mapped = MapPayable(payable);
        return new SupplierPayableDetailResponse
        {
            Id = mapped.Id,
            PayableNumber = mapped.PayableNumber,
            SupplierId = mapped.SupplierId,
            SupplierInvoiceNumber = mapped.SupplierInvoiceNumber,
            SupplierInvoiceDate = mapped.SupplierInvoiceDate,
            Description = mapped.Description,
            OriginalAmount = mapped.OriginalAmount,
            OutstandingAmount = mapped.OutstandingAmount,
            PaidAmount = mapped.PaidAmount,
            AdjustedAmount = mapped.AdjustedAmount,
            DueDate = mapped.DueDate,
            PaymentTermDays = mapped.PaymentTermDays,
            Status = mapped.Status,
            RowVersion = mapped.RowVersion,
            Items = payable.Items.Select(i => new SupplierPayableItemResponse
            {
                Id = i.Id,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Amount = i.Amount
            }).ToList(),
            Adjustments = payable.Adjustments.Select(MapAdjustment).ToList()
        };
    }

    private static PayableAdjustmentResponse MapAdjustment(FinPayableAdjustment adjustment) => new()
    {
        Id = adjustment.Id,
        AdjustmentNumber = adjustment.AdjustmentNumber,
        PayableType = adjustment.PayableType,
        SupplierPayableId = adjustment.SupplierPayableId,
        Direction = adjustment.Direction,
        Amount = adjustment.Amount,
        Reason = adjustment.Reason,
        Status = adjustment.Status,
        RequestedBy = adjustment.RequestedBy,
        RequestedAt = adjustment.RequestedAt,
        ApprovedBy = adjustment.ApprovedBy,
        ApprovedAt = adjustment.ApprovedAt,
        RejectionReason = adjustment.RejectionReason,
        RowVersion = adjustment.RowVersion
    };

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PayableConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PayableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PayableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or PayableConflictException or PayableValidationException or PayableBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
