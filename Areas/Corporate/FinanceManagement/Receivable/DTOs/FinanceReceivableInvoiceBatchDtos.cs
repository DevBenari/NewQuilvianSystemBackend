using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class ReceivableInvoiceBatchQuery
{
    public Guid? DebtorReferenceId { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class EligibleReceivableQuery
{
    [Required] public Guid DebtorReferenceId { get; set; }
}

public sealed class EligibleReceivableResponse
{
    public Guid Id { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class CreateReceivableInvoiceBatchRequest
{
    [Required] public DateOnly PeriodStart { get; set; }
    [Required] public DateOnly PeriodEnd { get; set; }
    [Required, MinLength(1)] public List<Guid> ReceivableIds { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchRowVersionRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}

public class ReceivableInvoiceBatchResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string DebtorType { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? IssuedAt { get; set; }
    public Guid RowVersion { get; set; }

    // Sumbu klaim penjamin (BE-FIN-052, FIN-DEC-097) — aditif, lihat FIN-API-1.3 §D.3.
    public string? ClaimStatus { get; set; }
    public decimal? ApprovedAmount { get; set; }

    /// <summary>Dihitung (TotalAmount - ApprovedAmount), TIDAK disimpan. Null selama belum APPROVED (FIN-DES-071).</summary>
    public decimal? ClaimVarianceAmount { get; set; }

    public string? PayerClaimReference { get; set; }
    public string? ClaimNote { get; set; }
    public DateTimeOffset? PayerVerifiedAt { get; set; }
    public DateTimeOffset? ClaimApprovedAt { get; set; }
    public DateTimeOffset? ClaimClosedAt { get; set; }
}

public sealed class ClaimVerifyRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(100)] public string? PayerClaimReference { get; set; }
    [MaxLength(500)] public string? ClaimNote { get; set; }
}

public sealed class ClaimApproveRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Nominal yang disetujui penjamin wajib diisi.")]
    [Range(0, double.MaxValue, ErrorMessage = "Nominal yang disetujui tidak boleh kurang dari nol.")]
    public decimal? ApprovedAmount { get; set; }

    [MaxLength(100)] public string? PayerClaimReference { get; set; }

    /// <summary>Wajib diisi bila ApprovedAmount lebih kecil dari TotalAmount (FIN-VAL-151) — diperiksa di service, bukan di sini, karena TotalAmount bukan bagian request.</summary>
    [MaxLength(500)] public string? ClaimNote { get; set; }
}

public sealed class ClaimCloseRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string? ClaimNote { get; set; }
}

public sealed class ReceivableInvoiceBatchMemberResponse
{
    public Guid Id { get; set; }
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string ReceivableStatus { get; set; } = string.Empty;
}

public sealed class ReceivableInvoiceBatchDetailResponse : ReceivableInvoiceBatchResponse
{
    public List<ReceivableInvoiceBatchMemberResponse> Members { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchDocumentResponse
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal GrandTotalCoveredAmount { get; set; }
    public List<CompanyGuarantorInvoiceDocumentResponse> Invoices { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
