using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("WfpOvertimeRequest", Schema = "public")]
    public class WfpOvertimeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CostCenterId { get; set; }

        public Guid? OvertimePolicyId { get; set; }

        public Guid? SourceOvertimePlanDetailId { get; set; }

        [Required, MaxLength(40)]
        public string RequestSource { get; set; } = "EmployeeSelfService";

        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }

        // Compatibility with the previous single-day overtime model.
        public Guid? AttendanceId { get; set; }
        public Guid? AttendanceDailyId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateOnly OvertimeDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        public DateTime? PlannedStartAt { get; set; }
        public DateTime? PlannedEndAt { get; set; }
        public TimeOnly? RequestedStartTime { get; set; }
        public TimeOnly? RequestedEndTime { get; set; }

        public int RequestedMinutes { get; set; } = 0;
        public int ApprovedMinutes { get; set; } = 0;
        public int EstimatedBreakMinutes { get; set; } = 0;

        public decimal EstimatedBaseHourlyRate { get; set; } = 0;
        public decimal EstimatedOvertimeCost { get; set; } = 0;
        public decimal ApprovedEstimatedCost { get; set; } = 0;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? WorkDescription { get; set; }

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(255)]
        public string? AttachmentFileName { get; set; }

        [MaxLength(150)]
        public string? AttachmentContentType { get; set; }

        public bool IsUrgent { get; set; } = false;
        public bool IsBeforeShift { get; set; } = false;
        public bool IsAfterShift { get; set; } = true;
        public bool IsRestDay { get; set; } = false;
        public bool IsHoliday { get; set; } = false;

        public bool HasScheduleConflict { get; set; } = false;
        public bool HasLeaveConflict { get; set; } = false;
        public bool HasTrainingConflict { get; set; } = false;
        public bool HasMinimumRestConflict { get; set; } = false;
        public bool HasWorkHourLimitConflict { get; set; } = false;
        public bool IsPolicyCompliant { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [Required, MaxLength(40)]
        public string OvertimeRequestStatus { get; set; } = "Draft";
        // Draft, Submitted, ApprovedForWork, Rejected, InProgress,
        // WaitingRealization, WaitingVerification, Realized,
        // PostedToPayroll, Cancelled.

        public int CurrentApprovalStep { get; set; } = 0;

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? WaitingRealizationAt { get; set; }
        public DateTime? RealizedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNotes { get; set; }

        // Compatibility and payroll integration fields.
        public bool IsPayrollProcessed { get; set; } = false;
        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public DateTime? PayrollProcessedAt { get; set; }
        public Guid? ProcessedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstOvertimePolicy? OvertimePolicy { get; set; }
        public TrxOvertimePlanDetail? SourceOvertimePlanDetail { get; set; }
        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }
        public TrxRosterPeriod? RosterPeriod { get; set; }
        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShift? Shift { get; set; }
        public HrdAttendance? Attendance { get; set; }
        public TrxAttendanceDaily? AttendanceDaily { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }
        public ApplicationUser? ProcessedByUser { get; set; }

        public ICollection<TrxOvertimeRequestDetail> Details { get; set; }
            = new List<TrxOvertimeRequestDetail>();

        public ICollection<TrxOvertimeRequestApproval> Approvals { get; set; }
            = new List<TrxOvertimeRequestApproval>();

        public ICollection<TrxOvertimeRealization> Realizations { get; set; }
            = new List<TrxOvertimeRealization>();

        public ICollection<TrxCompensatoryTimeOff> CompensatoryTimeOffs { get; set; }
            = new List<TrxCompensatoryTimeOff>();
    }
}
