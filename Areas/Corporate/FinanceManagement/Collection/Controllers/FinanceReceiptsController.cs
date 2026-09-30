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
/// Resource permission `FinanceReceipt` — nama kanonikal penuh sejak `BE-FIN-042`
/// (`FIN-DEC-078`, `permission-audit-matrix.md` §D.5). Sebelumnya memakai nama pendek `Receipt`.
/// `GET /receipts` (daftar), `GET /receipts/{id}`, alokasi, dan pembalikan alokasi dibangun
/// BE-FIN-018 (pembaruan 23 September 2026). `GET /receipts/register` dan
/// `GET /receipts/shift-reconciliation` dibangun BE-FIN-050 — keduanya eksplisit dikecualikan
/// BE-FIN-018 ("di luar cakupan literal roadmap task ini"). `POST /receipts` (pembuatan manual
/// non-kasir) dan `POST /receipts/{id}/reverse` (pembalikan penerimaan penuh secara manual) pada
/// kontrak `FIN-API-1.0`/`FIN-PERM-1.0` MASIH belum ada service-nya — tetap gap terbuka, tidak
/// dikarang di sini.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/receipts")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIPT", "Corporate Finance Management Receipt", "Receipt",
    AreaName = "Corporate", ControllerName = "FinanceReceipt", Description = "Penerimaan Finance dari tender Billing — alokasi ke piutang dan pembaliknya", SortOrder = 31)]
[Tags("Corporate / Finance Management / Receipt")]
public sealed class FinanceReceiptsController : ControllerBase
{
    private readonly FinanceReceiptService _service;
    public FinanceReceiptsController(FinanceReceiptService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FinReceiptResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] FinReceiptQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<FinReceiptResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar penerimaan berhasil diambil."));

    [HttpGet("register")]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceiptRegisterResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegister([FromQuery] ReceiptRegisterQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ReceiptRegisterResponse>>.Ok(
            await _service.GetRegisterAsync(request, cancellationToken), "Buku penerimaan kasir berhasil diambil."));

    [HttpGet("shift-reconciliation")]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ShiftReconciliationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShiftReconciliation([FromQuery] ShiftReconciliationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetShiftReconciliationAsync(request.CashierShiftId, cancellationToken);
            return Ok(ApiResponse<ShiftReconciliationResponse>.Ok(result, "Rekonsiliasi shift berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceipt", "Read")]
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
    [AccessPermission("FinanceReceipt", "Allocate")]
    [ProducesResponseType(typeof(ApiResponse<FinReceiptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Allocate(Guid id, [FromBody] AllocateReceiptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var lines = (request.Lines ?? []).Select(l => new AllocationLineRequest(
                l.ReceivableId,
                l.Amount,
                (l.Deductions ?? []).Select(d => new ReceiptDeductionLineRequest(d.DeductionType, d.Amount, d.Reason, d.ReferenceNumber)).ToList()))
                .ToList();
            var receipt = await _service.AllocateAsync(id, lines, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<FinReceiptResponse>.Ok(MapReceipt(receipt), "Alokasi penerimaan berhasil disimpan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpGet("{id:guid}/deductions")]
    [AccessAction("Read", "Read Receipt", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceipt", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<ReceiptDeductionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeductions(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deductions = await _service.GetDeductionsAsync(id, cancellationToken);
            return Ok(ApiResponse<List<ReceiptDeductionResponse>>.Ok(
                deductions.Select(MapDeduction).ToList(), "Daftar potongan penerimaan berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost("{id:guid}/allocations/{allocationId:guid}/reverse")]
    [AccessAction("Allocate", "Allocate Receipt", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("FinanceReceipt", "Allocate")]
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

    private static ReceiptDeductionResponse MapDeduction(Areas.Corporate.FinanceManagement.Collection.Models.FinReceiptDeduction deduction) => new()
    {
        Id = deduction.Id,
        DeductionNumber = deduction.DeductionNumber,
        ReceiptId = deduction.ReceiptId,
        ReceiptAllocationId = deduction.ReceiptAllocationId,
        DeductionType = deduction.DeductionType,
        Amount = deduction.Amount,
        Reason = deduction.Reason,
        ReferenceNumber = deduction.ReferenceNumber,
        IsReversal = deduction.IsReversal,
        ReversalOfDeductionId = deduction.ReversalOfDeductionId,
        RowVersion = deduction.RowVersion
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
