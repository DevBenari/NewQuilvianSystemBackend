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
        public int WarningConflicts { get; set; }
        public List<string> ConflictMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();
        public List<RosterValidationConflictItemDto> ConflictItems { get; set; } = new();
    }

    public class RosterValidationConflictItemDto
    {
        public Guid ShiftAssignmentId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public DateOnly ShiftDate { get; set; }
        public string RuleCode { get; set; } = string.Empty; // B1..B7, W1..W7
        public string Severity { get; set; } = "BLOCKING"; // BLOCKING, WARNING
        public string Message { get; set; } = string.Empty;
    }

    public class CopyPreviousRosterPeriodRequest
    {
        [Required(ErrorMessage = "SourceRosterPeriodId wajib diisi.")]
        public Guid SourceRosterPeriodId { get; set; }
    }

    public class CopyPreviousRosterPeriodResponse
    {
        public bool Success { get; set; }
        public int CopiedAssignmentsCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PublishRosterPeriodRequest
    {
        public List<WarningReasonItemRequest>? WarningReasons { get; set; }
    }

    public class WarningReasonItemRequest
    {
        public string WarningCode { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public DateOnly? ShiftDate { get; set; }
        public string? ShiftCode { get; set; }
        public string ReasonCategory { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class PublishRosterPeriodResponse
    {
        public Guid RosterPeriodId { get; set; }
        public string Status { get; set; } = "Published";
        public DateTime PublishedAt { get; set; }
        public string PublishedBy { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PopulateEligibleStaffResponse
    {
        public Guid RosterPeriodId { get; set; }
        public int TotalStaffCount { get; set; }
        public int AccompaniedStaffCount { get; set; }
        public List<string> StaffNames { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class ApplyShiftPatternRequest
    {
        [Required(ErrorMessage = "ShiftPatternId wajib dipilih.")]
        public Guid ShiftPatternId { get; set; }

        public List<Guid>? TargetWorkforceProfileIds { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class ApplyShiftPatternResponse
    {
        public bool Success { get; set; }
        public int TotalAssignedShifts { get; set; }
        public int AffectedStaffCount { get; set; }
        public string Message { get; set; } = string.Empty;
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

    /// <summary>
    /// Request penunjukan pengganti darurat langsung pasca-terbit (BE-HRD-095, HRD-DEC-101).
    /// </summary>
    public class DirectShiftReplacementRequest
    {
        [Required(ErrorMessage = "ReplacementEmployeeId wajib diisi.")]
        public Guid ReplacementEmployeeId { get; set; }

        [Required(ErrorMessage = "ReasonCategory wajib diisi."), MaxLength(100)]
        public string ReasonCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notes wajib diisi."), MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response konfirmasi ganti petugas langsung pasca-terbit (BE-HRD-095, BE-HRD-096).
    /// </summary>
    public class DirectShiftReplacementResponse
    {
        public Guid ShiftAssignmentId { get; set; }
        public string RosterPeriodStatus { get; set; } = "Published";
        public bool IsManualOverride { get; set; } = true;
        public Guid AssignedToEmployeeId { get; set; }
        public string AssignedToEmployeeName { get; set; } = string.Empty;
        public Guid OriginalEmployeeId { get; set; }
        public string OriginalEmployeeName { get; set; } = string.Empty;
        public bool EffectiveImmediately { get; set; } = true;
        public List<string> NotificationsDispatched { get; set; } = new();
    }

    /// <summary>
    /// Request koreksi mundur jadwal dinas staf (BE-HRD-097, HRD-DEC-102, VR-SCH-17).
    /// </summary>
    public class RetroactiveShiftCorrectionRequest
    {
        [Required(ErrorMessage = "NewShiftId wajib diisi.")]
        public Guid NewShiftId { get; set; }

        [Required(ErrorMessage = "ReasonCategory wajib diisi."), MaxLength(100)]
        public string ReasonCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notes wajib diisi."), MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response hasil koreksi mundur jadwal dinas staf (BE-HRD-097).
    /// </summary>
    public class RetroactiveShiftCorrectionResponse
    {
        public Guid ShiftAssignmentId { get; set; }
        public bool IsRetroactive { get; set; } = true;
        public int DaysBack { get; set; }
        public bool AttendanceReevaluationTriggered { get; set; } = true;
        public string Message { get; set; } = string.Empty;
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
        public DateTime? CompletedAt { get; set; }
        public bool IsCalledIn { get; set; }
        public DateTime? CallInTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int? ResponseMinutes { get; set; }
        public bool? IsSlaMet { get; set; }
        public Guid? CalledByUnitId { get; set; }
        public string? CalledByUnitName { get; set; }
        public string? CallNotes { get; set; }
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

        public Guid? UnitId { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required, MaxLength(30)]
        public string OnCallRole { get; set; } = "Primary";

        public int? ExpectedResponseMinutes { get; set; } = 30;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request pencatatan pemanggilan darurat cito dokter/staf siaga aktif (BE-HRD-100, HRD-DEC-103).
    /// </summary>
    public class RecordOnCallCallInRequest
    {
        [Required(ErrorMessage = "CallInTime wajib diisi.")]
        public DateTime CallInTime { get; set; }

        [Required(ErrorMessage = "ArrivalTime wajib diisi.")]
        public DateTime ArrivalTime { get; set; }

        public Guid? CalledByUnitId { get; set; }

        [MaxLength(500)]
        public string? CallNotes { get; set; }
    }

    /// <summary>
    /// Response hasil pencatatan pemanggilan cito & evaluasi indikator SLA respon (BE-HRD-100).
    /// </summary>
    public class OnCallCallInResponse
    {
        public Guid OnCallAssignmentId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public DateTime CallInTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int ResponseMinutes { get; set; }
        public int ExpectedResponseMinutes { get; set; }
        public bool IsSlaMet { get; set; }
        public Guid? CalledByUnitId { get; set; }
        public string? CalledByUnitName { get; set; }
        public string? CallNotes { get; set; }
        public string Status { get; set; } = "Active";
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response rekapitulasi data kompensasi siaga standby dan tiket panggil cito untuk modul Payroll & Lembur (BE-HRD-101, INT-HR-38).
    /// </summary>
    public class OnCallCompensationSummaryResponse
    {
        public Guid? RosterPeriodId { get; set; }
        public int TotalOnCallShifts { get; set; }
        public double TotalStandbyHours { get; set; }
        public int TotalCallInCount { get; set; }
        public int TotalSlaMetCount { get; set; }
        public List<OnCallCompensationStaffItemDto> StaffItems { get; set; } = new();
    }

    public class OnCallCompensationStaffItemDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public int OnCallShiftCount { get; set; }
        public double StandbyHours { get; set; }
        public int CallInCount { get; set; }
        public int SlaMetCount { get; set; }
        public int SlaBreachedCount { get; set; }
        public int OvertimeMinutes { get; set; }
    }
}
