using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class CreateOvertimePlanRequest
    {
        [Required, MaxLength(200)]
        public string PlanTitle { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        [Required]
        public DateOnly PlanStartDate { get; set; }
        [Required]
        public DateOnly PlanEndDate { get; set; }
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Notes { get; set; }
        public List<CreateOvertimePlanDetailRequest> Details { get; set; } = new();
    }

    public class UpdateOvertimePlanRequest
    {
        [Required, MaxLength(200)]
        public string PlanTitle { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        [Required]
        public DateOnly PlanStartDate { get; set; }
        [Required]
        public DateOnly PlanEndDate { get; set; }
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateOvertimePlanStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class CancelOvertimePlanRequest
    {
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class GenerateOvertimeRequestsRequest
    {
        public List<Guid> DetailIds { get; set; } = new();
        public bool SkipExisting { get; set; } = true;
    }

    public class OvertimePlanResponse : OvertimePlanListResponse
    {
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? ValidatedByUserId { get; set; }
        public string? ValidatedByUserName { get; set; }
        public Guid? PublishedByUserId { get; set; }
        public string? PublishedByUserName { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public string? ClosedByUserName { get; set; }
        public List<OvertimePlanDetailResponse> Details { get; set; } = new();
    }

    public class OvertimePlanValidationResponse
    {
        public Guid OvertimePlanId { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public bool CanPublish { get; set; }
        public int TotalDetail { get; set; }
        public int ValidDetail { get; set; }
        public int ConflictDetail { get; set; }
        public int TotalPlannedMinutes { get; set; }
        public DateTime ValidatedAt { get; set; }
        public List<OvertimeValidationIssueResponse> Issues { get; set; } = new();
        public List<OvertimePlanDetailValidationResponse> DetailValidations { get; set; } = new();
    }

    public class OvertimePlanActionResponse
    {
        public Guid OvertimePlanId { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int AffectedDetailCount { get; set; }
        public int AffectedRequestCount { get; set; }
        public DateTime ActionAt { get; set; }
    }

    public class OvertimeGeneratedRequestItemResponse
    {
        public Guid PlanDetailId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public bool WasCreated { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class GenerateOvertimeRequestsResponse
    {
        public Guid OvertimePlanId { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int RequestedDetailCount { get; set; }
        public int CreatedRequestCount { get; set; }
        public int ExistingRequestCount { get; set; }
        public int SkippedDetailCount { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<OvertimeGeneratedRequestItemResponse> Items { get; set; } = new();
    }
}
