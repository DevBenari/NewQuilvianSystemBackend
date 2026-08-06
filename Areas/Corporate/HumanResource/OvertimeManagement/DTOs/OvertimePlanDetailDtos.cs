using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class CreateOvertimePlanDetailRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? OvertimePolicyId { get; set; }
        [Required]
        public DateOnly OvertimeDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        [Required]
        public DateTime PlannedStartAt { get; set; }
        [Required]
        public DateTime PlannedEndAt { get; set; }
        [Range(0, 1440)]
        public int EstimatedBreakMinutes { get; set; }
        [Required, MaxLength(30)]
        public string DayType { get; set; } = "Workday";
        [Required, MaxLength(40)]
        public string OvertimeCategory { get; set; } = "AfterShift";
        [Required, MaxLength(2000)]
        public string WorkDescription { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateOvertimePlanDetailRequest : CreateOvertimePlanDetailRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class OvertimePlanDetailPreviewRequest : CreateOvertimePlanDetailRequest
    {
        public Guid? ExcludeDetailId { get; set; }
    }

    public class OvertimeValidationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsBlocking { get; set; }
        public string? Field { get; set; }
        public Guid? ReferenceId { get; set; }
    }

    public class OvertimePlanDetailValidationResponse
    {
        public Guid? DetailId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }
        public int RawPlannedMinutes { get; set; }
        public int EstimatedBreakMinutes { get; set; }
        public int EligiblePlannedMinutes { get; set; }
        public int RoundedPlannedMinutes { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? OvertimePolicyId { get; set; }
        public string? OvertimePolicyCode { get; set; }
        public string? OvertimePolicyName { get; set; }
        public Guid? PreviewOvertimeRateId { get; set; }
        public string? PreviewOvertimeRateCode { get; set; }
        public decimal? PreviewRateMultiplier { get; set; }
        public bool HasScheduleConflict { get; set; }
        public bool HasLeaveConflict { get; set; }
        public bool HasTrainingConflict { get; set; }
        public bool HasMinimumRestConflict { get; set; }
        public bool HasWorkHourLimitConflict { get; set; }
        public bool IsPolicyCompliant { get; set; }
        public bool CanPersist { get; set; }
        public bool CanPublish { get; set; }
        public List<string> EvaluatedChecks { get; set; } = new();
        public List<string> DeferredChecks { get; set; } = new();
        public List<OvertimeValidationIssueResponse> Issues { get; set; } = new();
    }

    public class OvertimePlanDetailResponse
    {
        public Guid Id { get; set; }
        public Guid OvertimePlanId { get; set; }
        public int SequenceNumber { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string? WorkLocationName { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleCode { get; set; }
        public string? WorkScheduleName { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }
        public Guid? OvertimePolicyId { get; set; }
        public string? OvertimePolicyCode { get; set; }
        public string? OvertimePolicyName { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }
        public int PlannedMinutes { get; set; }
        public int EstimatedBreakMinutes { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;
        public string WorkDescription { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool HasScheduleConflict { get; set; }
        public bool HasLeaveConflict { get; set; }
        public bool HasTrainingConflict { get; set; }
        public bool HasMinimumRestConflict { get; set; }
        public bool HasWorkHourLimitConflict { get; set; }
        public bool IsPolicyCompliant { get; set; }
        public string? ValidationResultJson { get; set; }
        public string DetailStatus { get; set; } = string.Empty;
        public Guid? GeneratedOvertimeRequestId { get; set; }
        public string? GeneratedOvertimeRequestNumber { get; set; }
        public string? GeneratedOvertimeRequestStatus { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }
}
