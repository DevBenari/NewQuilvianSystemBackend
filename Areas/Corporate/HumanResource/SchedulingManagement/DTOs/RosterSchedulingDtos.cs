using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs
{
    public class RosterPeriodSummaryResponse
    {
        public int TotalPeriods { get; set; }
        public int DraftPeriods { get; set; }
        public int PublishedPeriods { get; set; }
        public int LockedPeriods { get; set; }
        public int ClosedPeriods { get; set; }
    }

    public class RosterPeriodResponse
    {
        public Guid Id { get; set; }
        public string RosterPeriodCode { get; set; } = string.Empty;
        public string RosterPeriodName { get; set; } = string.Empty;
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string RosterStatus { get; set; } = "Draft";
        public int VersionNumber { get; set; }
        public int TotalAssignmentCount { get; set; }
        public int TotalConflictCount { get; set; }
        public int BlockingConflictCount { get; set; }
        public bool IsValidationPassed { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class RosterPeriodDetailResponse : RosterPeriodResponse
    {
        public string? Description { get; set; }
        public string? ValidationSummaryJson { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public List<RosterAssignmentResponse> Assignments { get; set; } = new();
    }

    public class CreateRosterPeriodRequest
    {
        [Required, MaxLength(200)]
        public string RosterPeriodName { get; set; } = string.Empty;

        public Guid? DepartmentId { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        [Required]
        public DateOnly PeriodStartDate { get; set; }

        [Required]
        public DateOnly PeriodEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateRosterPeriodRequest
    {
        [Required, MaxLength(200)]
        public string RosterPeriodName { get; set; } = string.Empty;

        public Guid? DepartmentId { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        [Required]
        public DateOnly PeriodStartDate { get; set; }

        [Required]
        public DateOnly PeriodEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class RosterShiftItemRequest
    {
        [Required]
        public DateOnly ShiftDate { get; set; }

        [Required]
        public Guid ShiftId { get; set; }

        public bool IsNightShift { get; set; } = false;
        public bool IsOnCall { get; set; } = false;
    }

    public class CreateRosterAssignmentRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PositionId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<RosterShiftItemRequest> Shifts { get; set; } = new();
    }

    public class RosterAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid RosterPeriodId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string AssignmentStatus { get; set; } = "Draft";
        public int PlannedShiftCount { get; set; }
        public bool HasConflict { get; set; }
        public int ConflictCount { get; set; }
        public bool IsValidationPassed { get; set; }
        public string? ValidationResultJson { get; set; }
        public string? Notes { get; set; }
        public List<ShiftAssignmentItemResponse> ShiftAssignments { get; set; } = new();
    }

    public class ShiftAssignmentItemResponse
    {
        public Guid Id { get; set; }
        public DateOnly ShiftDate { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public string AssignmentType { get; set; } = "Regular";
        public string AssignmentStatus { get; set; } = "Draft";
        public bool IsNightShift { get; set; }
        public bool IsOnCall { get; set; }
        public bool HasDoubleShiftConflict { get; set; }
        public bool HasMinimumRestConflict { get; set; }
        public bool HasBlockingConflict { get; set; }
    }

    public class RosterValidationResultResponse
    {
        public Guid RosterPeriodId { get; set; }
        public bool IsValidationPassed { get; set; }
        public int TotalConflicts { get; set; }
        public int BlockingConflicts { get; set; }
        public List<string> ConflictMessages { get; set; } = new();
    }

    public class ShiftReplacementResponse
    {
        public Guid Id { get; set; }
        public Guid ShiftAssignmentId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public Guid OriginalWorkforceProfileId { get; set; }
        public string OriginalWorkforceDisplayName { get; set; } = string.Empty;
        public Guid ReplacementWorkforceProfileId { get; set; }
        public string ReplacementWorkforceDisplayName { get; set; } = string.Empty;
        public string ReplacementType { get; set; } = "Emergency";
        public string Reason { get; set; } = string.Empty;
        public string ReplacementStatus { get; set; } = "Draft";
        public DateTime CreateDateTime { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    public class CreateShiftReplacementRequest
    {
        [Required]
        public Guid ShiftAssignmentId { get; set; }

        [Required]
        public Guid OriginalWorkforceProfileId { get; set; }

        [Required]
        public Guid ReplacementWorkforceProfileId { get; set; }

        [Required, MaxLength(40)]
        public string ReplacementType { get; set; } = "Emergency";

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class OnCallAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid RosterPeriodId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid OnCallTypeId { get; set; }
        public string OnCallTypeName { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string OnCallRole { get; set; } = "Primary";
        public string AssignmentStatus { get; set; } = "Scheduled";
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateOnCallAssignmentRequest
    {
        [Required]
        public Guid RosterPeriodId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid OnCallTypeId { get; set; }

        public Guid? ShiftId { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required, MaxLength(30)]
        public string OnCallRole { get; set; } = "Primary";

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
