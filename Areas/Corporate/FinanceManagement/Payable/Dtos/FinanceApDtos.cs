using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;

public sealed class SupplierPayableQuery
{
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }
    public DateOnly? InvoiceDateFrom { get; set; }
    public DateOnly? InvoiceDateTo { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class RecordSupplierPaymentRequest
{
    [Required]
    public Guid SupplierPayableId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Nominal pembayaran utang harus lebih dari 0.")]
    public decimal Amount { get; set; }

    [Required]
    public Guid BankAccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "TRANSFER";

    [Required]
    [MaxLength(150)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class SupplierPayablePaymentResponse
{
    public Guid PayableId { get; set; }
    public string PayableNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public decimal PreviousOutstanding { get; set; }
    public decimal CurrentOutstanding { get; set; }
    public decimal TotalPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}

public sealed class SupplierPayableAgingQuery
{
    public DateOnly? AsOfDate { get; set; }
}

public sealed class SupplierPayableAgingBucketResult
{
    public string BucketLabel { get; set; } = string.Empty;
    public int DaysMin { get; set; }
    public int? DaysMax { get; set; }
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class SupplierPayableReportResponse
{
    public DateOnly AsOfDate { get; set; }
    public decimal TotalOriginalAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalAdjustedAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public int TotalPayableCount { get; set; }
    public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    public Dictionary<string, decimal> SupplierBreakdown { get; set; } = new();
    public List<SupplierPayableAgingBucketResult> AgingBuckets { get; set; } = new();
}
