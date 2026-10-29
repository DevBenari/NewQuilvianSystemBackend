using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;

public sealed class HrdBenefitSettlementApprovalQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? PeriodCode { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string SortBy { get; set; } = "createdDate";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class HrdApproveSettlementRequest
{
    [Required] public Guid RowVersion { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class HrdRejectSettlementRequest
{
    [Required] public Guid RowVersion { get; set; }
    [Required, MaxLength(500)] public string RejectReason { get; set; } = string.Empty;
}

public class HrdBenefitSettlementApprovalItemResponse
{
    public Guid Id { get; set; }
    public string SettlementNumber { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; }
    public string AccountingPeriodCode { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public int OwnerCount { get; set; }
    public string DocumentStatus { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class HrdBenefitSettlementApprovalDetailResponse : HrdBenefitSettlementApprovalItemResponse
{
    public string? Notes { get; set; }
    public string? ApprovalNotes { get; set; }
    public List<HrdBenefitSettlementOwnerRowResponse> Items { get; set; } = new();
}

public sealed class HrdBenefitSettlementOwnerRowResponse
{
    public Guid ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? PatientName { get; set; }
    public string? PatientType { get; set; }
    public DateTimeOffset? ServiceDate { get; set; }
    public decimal Amount { get; set; }

    // Rincian Pemilik Benefit (Karyawan / Dokter)
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }
    public string? BenefitOwnerName { get; set; }
    public string? BenefitOwnerStaffNumber { get; set; }
    public string? BenefitOwnerKind { get; set; } // "Karyawan", "Dokter", atau "Pegawai"
    public string? BenefitPlanName { get; set; }
    public string? RelationshipCode { get; set; } // "SELF", "SPOUSE", "CHILD"

    // Informasi Plafon Periode Terkait
    public decimal CoverageLimitAmount { get; set; }
    public decimal UsedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal ProposedAmount { get; set; }
    public decimal ProjectedRemainingAmount { get; set; }
    public bool IsSufficient { get; set; }
}
