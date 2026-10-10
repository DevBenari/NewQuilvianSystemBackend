using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class CreateInstallmentPlanRequest
{
    [Range(2, 60)] public int InstallmentCount { get; set; }
    [Range(0.01, double.MaxValue)] public decimal InstallmentAmount { get; set; }
    [Required, RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$")] public string FirstDeductionPeriod { get; set; } = string.Empty;
    [MaxLength(512)] public string? AgreementDocumentPath { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class ApproveInstallmentPlanRequest
{
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class RejectInstallmentPlanRequest
{
    [Required, MaxLength(500)] public string RejectionReason { get; set; } = string.Empty;
}

public sealed class CancelInstallmentPlanRequest
{
    [Required, MaxLength(500)] public string CancelReason { get; set; } = string.Empty;
}

public sealed class InstallmentPlanQuery
{
    public string? Status { get; set; }
    public Guid? BenefitOwnerId { get; set; }
    public string? DeductionPeriod { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "requestedAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class InstallmentPlanResponse
{
    public Guid Id { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public Guid ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public int InstallmentCount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal TotalAgreedAmount { get; set; }
    public string FirstDeductionPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RequestedByName { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancelReason { get; set; }
    public string? Notes { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class InstallmentPlanListResponse : InstallmentPlanResponse
{
    /// <summary>Sensitif (FIN-DEC-172) — tetap dikembalikan ke pengguna berhak, MUST NOT masuk custom logger.</summary>
    public Guid? BenefitOwnerId { get; set; }
    /// <summary>Sensitif.</summary>
    public string? BenefitRelationship { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public sealed class InstallmentPlanDetailResponse : InstallmentPlanResponse
{
    public List<InstallmentResponse> Installments { get; set; } = new();
}

public sealed class InstallmentResponse
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public string DeductionPeriod { get; set; } = string.Empty;
    public decimal ScheduledAmount { get; set; }
    public decimal CarriedOverAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? LastResultAt { get; set; }
}
