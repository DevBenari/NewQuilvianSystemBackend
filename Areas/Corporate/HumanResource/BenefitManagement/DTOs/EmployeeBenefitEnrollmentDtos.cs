using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // BE-FIN-103, FIN-API-1.10 R.1.1.

    public sealed class CreateEmployeeBenefitEnrollmentRequest
    {
        [Required] public Guid EmployeeId { get; set; }
        [Required] public Guid BenefitPlanId { get; set; }
        [Required] public DateTime EnrollmentDate { get; set; }
        [Required] public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public sealed class UpdateEmployeeBenefitEnrollmentRequest
    {
        [Required] public DateTime EnrollmentDate { get; set; }
        [Required] public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        [Required] public Guid RowVersion { get; set; }
    }

    public sealed class EnrollmentTransitionRequest
    {
        [Required] public Guid RowVersion { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public sealed class EnrollmentCancelRequest
    {
        [Required] public Guid RowVersion { get; set; }
        [Required, MaxLength(500)] public string CancellationReason { get; set; } = string.Empty;
    }

    public sealed class EmployeeBenefitEnrollmentPagedQuery
    {
        [MaxLength(100)] public string? Search { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        [MaxLength(30)] public string? EnrollmentStatus { get; set; }
        [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
        [Range(1, 50)] public int PageSize { get; set; } = 10;
    }

    public class EmployeeBenefitEnrollmentListResponse
    {
        public Guid Id { get; set; }
        public string EnrollmentNumber { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public Guid BenefitPlanId { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string EnrollmentStatus { get; set; } = string.Empty;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public decimal RemainingAmount { get; set; }
        public int ActiveDependentCount { get; set; }
    }

    public sealed class EmployeeBenefitEnrollmentResponse : EmployeeBenefitEnrollmentListResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string CoverageType { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public decimal CoverageLimitAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public int MaximumDependents { get; set; }
        public bool IsMedicalServiceBenefit { get; set; }
        public bool IsWithinEffectivePeriod { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string? ActivatedByName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelledByName { get; set; }
        public Guid RowVersion { get; set; }
        public List<string> AvailableActions { get; set; } = new();
        public List<EmployeeBenefitDependentResponse> Dependents { get; set; } = new();
    }

    public sealed class EmployeeBenefitSummaryResponse
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public bool HasActiveMedicalEnrollment { get; set; }
        public Guid? EnrollmentId { get; set; }
        public string? EnrollmentNumber { get; set; }
        public string? PlanName { get; set; }
        public string? CoverageType { get; set; }
        public List<BenefitPersonBalanceResponse> Persons { get; set; } = new();
    }

    public sealed class BenefitPersonBalanceResponse
    {
        public string BenefitPersonKind { get; set; } = string.Empty;
        public Guid BenefitPersonId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string RelationshipCode { get; set; } = string.Empty;
        public decimal LimitAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public sealed class EnrollmentStatusCountResponse
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class EmployeeBenefitEnrollmentSummaryResponse
    {
        public int Total { get; set; }
        public List<EnrollmentStatusCountResponse> ByStatus { get; set; } = new();
    }

    public sealed class EnrollmentFilterOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public sealed class EmployeeBenefitEnrollmentFilterMetadataResponse
    {
        public List<EnrollmentFilterOptionResponse> EnrollmentStatuses { get; set; } = new();
        public List<EnrollmentFilterOptionResponse> DependentStatuses { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50 };
        public int DefaultPageSize { get; set; } = 10;
    }

    public sealed class EnrollmentStatusHistoryResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string? ActorName { get; set; }
        public string? Reason { get; set; }
    }
}
