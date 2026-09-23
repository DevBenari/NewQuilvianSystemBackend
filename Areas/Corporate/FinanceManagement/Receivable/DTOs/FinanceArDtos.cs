using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;

public sealed class RecordReceivablePaymentRequest
{
    [Required]
    public Guid ReceivableId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Nominal pembayaran harus lebih dari 0.")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "TRANSFER";

    public Guid? BankAccountId { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class DirectReceivableWriteOffRequest
{
    [Required]
    public Guid ReceivableId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Nominal penghapusan piutang harus lebih dari 0.")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class ReceivablePaymentResponse
{
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public decimal PreviousOutstanding { get; set; }
    public decimal CurrentOutstanding { get; set; }
    public decimal TotalAllocated { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class ReceivableReportResponse
{
    public DateOnly AsOfDate { get; set; }
    public decimal TotalOriginalAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal TotalAllocatedAmount { get; set; }
    public decimal TotalAdjustedAmount { get; set; }
    public decimal TotalWrittenOffAmount { get; set; }
    public int TotalReceivableCount { get; set; }
    public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    public Dictionary<string, decimal> DebtorTypeBreakdown { get; set; } = new();
    public List<ReceivableAgingBucketResult> AgingBuckets { get; set; } = new();
}
