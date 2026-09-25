using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Controllers;

/// <summary>
/// Route dan resource permission ("Receipt") mengikuti route `/receipts/{id}/allocations...` pada
/// `contracts/permission-audit-matrix.md` (resource `FinanceReceipt` di dokumen itu — dipakai di
/// sini sebagai "Receipt" mengikuti pola penamaan singkat yang sudah berjalan nyata pada
/// `FinanceReceivablesController` ("Receivable", bukan "FinanceReceivable"), bukan penyimpangan).
/// Hanya endpoint yang sudah punya logika service nyata yang dibangun di sini (BE-FIN-018,
/// pembaruan 23 September 2026): rincian satu penerimaan, alokasi, dan pembalikan alokasi.
/// `GET /receipts` (daftar), `GET /receipts/register`, `GET /receipts/shift-reconciliation`,
/// `POST /receipts` (pembuatan manual), dan `POST /receipts/{id}/reverse` (pembalikan penerimaan
/// penuh secara manual) ada pada kontrak `FIN-API-1.0`/`FIN-PERM-1.0` tetapi service-nya belum
/// ada — dicatat sebagai gap terbuka pada laporan task, bukan dikarang di sini.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/receipts")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIPT", "Corporate Finance Management Receipt", "Receipt",
    AreaName = "Corporate", ControllerName = "Receipt", Description = "Penerimaan Finance dari tender Billing — alokasi ke piutang dan pembaliknya", SortOrder = 31)]
[Tags("Corporate / Finance Management / Receipt")]
public sealed class FinanceReceiptsController : ControllerBase
{
    private readonly FinanceReceiptService _service;
    public FinanceReceiptsController(FinanceReceiptService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Receipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FinReceiptResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] FinReceiptQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<FinReceiptResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar penerimaan berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Receipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<FinReceiptDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var receipt = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<FinReceiptDetailResponse>.Ok(Map(receipt), "Detail penerimaan berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost("{id:guid}/allocations")]
    [AccessAction("Allocate", "Allocate Receipt", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("Receipt", "Allocate")]
    [ProducesResponseType(typeof(ApiResponse<FinReceiptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Allocate(Guid id, [FromBody] AllocateReceiptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var lines = (request.Lines ?? []).Select(l => new AllocationLineRequest(l.ReceivableId, l.Amount)).ToList();
            var receipt = await _service.AllocateAsync(id, lines, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<FinReceiptResponse>.Ok(MapReceipt(receipt), "Alokasi penerimaan berhasil disimpan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/allocations/{allocationId:guid}/reverse")]
    [AccessAction("Allocate", "Allocate Receipt", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("Receipt", "Allocate")]
    [ProducesResponseType(typeof(ApiResponse<FinReceiptAllocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReverseAllocation(Guid id, Guid allocationId, CancellationToken cancellationToken)
    {
        try
        {
            var reversal = await _service.ReverseAllocationAsync(allocationId, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<FinReceiptAllocationResponse>.Ok(MapAllocation(reversal), "Alokasi berhasil dibalik."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static FinReceiptResponse MapReceipt(Areas.Corporate.FinanceManagement.Collection.Models.FinReceipt receipt) => new()
    {
        Id = receipt.Id,
        ReceiptNumber = receipt.ReceiptNumber,
        SourceType = receipt.SourceType,
        SourceTenderId = receipt.SourceTenderId,
        InvoiceId = receipt.InvoiceId,
        PaymentMethodId = receipt.PaymentMethodId,
        CashierShiftId = receipt.CashierShiftId,
        Amount = receipt.Amount,
        AllocatedAmount = receipt.AllocatedAmount,
        UnallocatedAmount = receipt.UnallocatedAmount,
        OccurredAt = receipt.OccurredAt,
        Status = receipt.Status,
        ReversalOfReceiptId = receipt.ReversalOfReceiptId,
        RowVersion = receipt.RowVersion
    };

    private static FinReceiptDetailResponse Map(Areas.Corporate.FinanceManagement.Collection.Models.FinReceipt receipt)
    {
        var mapped = MapReceipt(receipt);
        return new FinReceiptDetailResponse
        {
            Id = mapped.Id,
            ReceiptNumber = mapped.ReceiptNumber,
            SourceType = mapped.SourceType,
            SourceTenderId = mapped.SourceTenderId,
            InvoiceId = mapped.InvoiceId,
            PaymentMethodId = mapped.PaymentMethodId,
            CashierShiftId = mapped.CashierShiftId,
            Amount = mapped.Amount,
            AllocatedAmount = mapped.AllocatedAmount,
            UnallocatedAmount = mapped.UnallocatedAmount,
            OccurredAt = mapped.OccurredAt,
            Status = mapped.Status,
            ReversalOfReceiptId = mapped.ReversalOfReceiptId,
            RowVersion = mapped.RowVersion,
            Allocations = receipt.Allocations.Select(MapAllocation).ToList()
        };
    }

    private static FinReceiptAllocationResponse MapAllocation(Areas.Corporate.FinanceManagement.Collection.Models.FinReceiptAllocation allocation) => new()
    {
        Id = allocation.Id,
        ReceiptId = allocation.ReceiptId,
        ReceivableId = allocation.ReceivableId,
        TargetType = allocation.TargetType,
        Amount = allocation.Amount,
        IsReversal = allocation.IsReversal,
        ReversalOfAllocationId = allocation.ReversalOfAllocationId,
        AllocatedBy = allocation.AllocatedBy,
        AllocatedAt = allocation.AllocatedAt
    };

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        ReceivableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        ReceivableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or ReceivableValidationException or ReceivableBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
