namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;

public sealed class FinReceiptAllocationResponse
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public Guid? ReceivableId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsReversal { get; set; }
    public Guid? ReversalOfAllocationId { get; set; }
    public Guid AllocatedBy { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
}

public class FinReceiptResponse
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceTenderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? CashierShiftId { get; set; }
    public decimal Amount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal UnallocatedAmount { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ReversalOfReceiptId { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class FinReceiptDetailResponse : FinReceiptResponse
{
    public List<FinReceiptAllocationResponse> Allocations { get; set; } = [];
}

/// <summary>Satu baris potongan sisi penerimaan (BE-FIN-040, FIN-DES-048). Hanya sah pada baris
/// alokasi ber-ReceivableId terisi (FIN-VAL-128).</summary>
public sealed class ReceiptDeductionLineRequestDto
{
    public string DeductionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>Satu baris permintaan alokasi. ReceivableId kosong = INVOICE_DIRECT (FIN-DES-011).</summary>
public sealed class AllocationLineRequestDto
{
    public Guid? ReceivableId { get; set; }
    public decimal Amount { get; set; }
    public List<ReceiptDeductionLineRequestDto>? Deductions { get; set; }
}

public sealed class AllocateReceiptRequest
{
    public List<AllocationLineRequestDto> Lines { get; set; } = [];
}

/// <summary>Baris GET /receipts/{id}/deductions (FIN-API-1.2 B.8).</summary>
public sealed class ReceiptDeductionResponse
{
    public Guid Id { get; set; }
    public string DeductionNumber { get; set; } = string.Empty;
    public Guid ReceiptId { get; set; }
    public Guid ReceiptAllocationId { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
    public bool IsReversal { get; set; }
    public Guid? ReversalOfDeductionId { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class FinReceiptQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? SourceType { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string SortBy { get; set; } = "OccurredAt";
    public string SortDirection { get; set; } = "desc";
}

/// <summary>BE-FIN-050, `GET /receipts/register` (FIN-API-1.0). Buku penerimaan kasir per tanggal.</summary>
public sealed class ReceiptRegisterQuery
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public Guid? CashierShiftId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu baris buku penerimaan kasir — menelusur ke tender dan kwitansi asalnya
/// (KwitansiNumber/SourceTenderId tidak ada pada FinReceiptResponse umum).</summary>
public sealed class ReceiptRegisterResponse
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? KwitansiNumber { get; set; }
    public Guid? SourceTenderId { get; set; }
    public Guid? CashierShiftId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}

/// <summary>BE-FIN-050, `GET /receipts/shift-reconciliation` (FIN-API-1.0). Satu shift per
/// pemanggilan — kontrak menyatakan response tunggal, bukan daftar berpaging.</summary>
public sealed class ShiftReconciliationQuery
{
    [System.ComponentModel.DataAnnotations.Required] public Guid CashierShiftId { get; set; }
}

/// <summary>Perbandingan total penerimaan tunai Finance dengan SystemCash Billing untuk satu
/// shift kasir. Baca saja — tidak pernah menulis FinReceipt maupun BilCashierShift.</summary>
public sealed class ShiftReconciliationResponse
{
    public Guid CashierShiftId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public string CashierShiftStatus { get; set; } = string.Empty;
    public decimal SystemCashAmount { get; set; }
    public decimal FinanceNetCashReceiptAmount { get; set; }
    public decimal Variance { get; set; }
    public int CashReceiptCount { get; set; }
}

