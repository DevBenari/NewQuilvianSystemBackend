using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class PreviewBenefitSettlementRequest
{
    [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$")] public string? AccountingPeriodCode { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientType { get; set; }
    public string? ResolutionStatus { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Period { get; set; }
    public string? Search { get; set; }
    public List<Guid>? ReceivableIds { get; set; }
}

public sealed class CreateBenefitSettlementRequest
{
    [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$")] public string? AccountingPeriodCode { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientType { get; set; }
    public string? ResolutionStatus { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Period { get; set; }
    public string? Search { get; set; }
    public List<Guid>? ReceivableIds { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class PostBenefitSettlementRequest
{
    [Required] public Guid RowVersion { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class CancelBenefitSettlementRequest
{
    [Required] public Guid RowVersion { get; set; }
    [Required, MaxLength(500)] public string CancelReason { get; set; } = string.Empty;
}

public sealed class BenefitSettlementQuery
{
    public string? AccountingPeriodCode { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "accountingPeriodCode";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class BenefitSettlementItemResponse
{
    public Guid ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? PatientName { get; set; }
    public decimal Amount { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? PatientType { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
}

public sealed class BenefitSettlementPreviewResponse
{
    public string AccountingPeriodCode { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public List<BenefitSettlementItemResponse> Items { get; set; } = new();
}

public class BenefitSettlementResponse
{
    public Guid Id { get; set; }
    public string SettlementNumber { get; set; } = string.Empty;
    public string AccountingPeriodCode { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = "MENUNGGU_PERSETUJUAN";
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? PostedByName { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? CancelReason { get; set; }
    public Guid? AccountingEventId { get; set; }
    public string? Notes { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class BenefitSettlementListResponse : BenefitSettlementResponse
{
}

public sealed class BenefitSettlementDetailResponse : BenefitSettlementResponse
{
    public List<BenefitSettlementItemResponse> Items { get; set; } = new();
}
