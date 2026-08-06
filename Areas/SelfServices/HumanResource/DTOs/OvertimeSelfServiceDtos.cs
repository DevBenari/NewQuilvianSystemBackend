using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs
{
    public class CreateMyOvertimeRequest
    {
        [Required]
        public DateOnly OvertimeDate { get; set; }

        [Required]
        public DateTime PlannedStartAt { get; set; }

        [Required]
        public DateTime PlannedEndAt { get; set; }

        [Range(0, 1440)]
        public int EstimatedBreakMinutes { get; set; }

        [Required, MaxLength(40)]
        public string OvertimeCategory { get; set; } = "AfterShift";

        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string WorkDescription { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsUrgent { get; set; }
    }

    public class UpdateMyOvertimeRequest : CreateMyOvertimeRequest
    {
    }

    public class PreviewMyOvertimeRequest : CreateMyOvertimeRequest
    {
        public Guid? ExcludeRequestId { get; set; }
    }

    public class SubmitMyOvertimeRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class CancelMyOvertimeRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class MyOvertimeValidationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
        public Guid? ReferenceId { get; set; }
        public bool IsBlocking { get; set; }
    }

    public class MyOvertimePreviewResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public DateOnly OvertimeDate { get; set; }
        public DateOnly PlannedEndDate { get; set; }
        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }
        public int RawMinutes { get; set; }
        public int EstimatedBreakMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int RoundedMinutes { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;

        public bool IsScheduleResolved { get; set; }
        public string ScheduleSource { get; set; } = string.Empty;
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleCode { get; set; }
        public string? WorkScheduleName { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public bool IsRestDay { get; set; }
        public bool IsHoliday { get; set; }

        public Guid? OvertimePolicyId { get; set; }
        public string? OvertimePolicyCode { get; set; }
        public string? OvertimePolicyName { get; set; }
        public bool RequirePreApproval { get; set; }
        public bool RequirePostVerification { get; set; }
        public bool RequireAttendanceMatch { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? PreviewOvertimeRateId { get; set; }
        public string? PreviewOvertimeRateCode { get; set; }
        public string? PreviewOvertimeRateName { get; set; }
        public decimal PreviewRateMultiplier { get; set; }

        public bool HasScheduleConflict { get; set; }
        public bool HasRequestOverlap { get; set; }
        public bool HasPlanOverlap { get; set; }
        public bool HasWorkHourLimitConflict { get; set; }
        public bool IsPolicyCompliant { get; set; }
        public bool CanSaveDraft { get; set; }
        public bool CanSubmit { get; set; }

        public List<string> EvaluatedChecks { get; set; } = new();
        public List<string> DeferredChecks { get; set; } = new();
        public List<MyOvertimeValidationIssueResponse> Issues { get; set; } = new();
    }

    public class MyOvertimeActionResponse
    {
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime ActionAt { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowStatus { get; set; }
        public int CurrentApprovalStep { get; set; }
        public string? CurrentWorkflowStepCode { get; set; }
        public bool WorkflowCreated { get; set; }
        public bool WorkflowSubmitted { get; set; }
        public bool LifecycleSynchronized { get; set; }
    }
}
