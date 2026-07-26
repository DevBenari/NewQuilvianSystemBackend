using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("WfpLeaveRequest", Schema = "public")]
    public class WfpLeaveRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeavePolicyId { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ReplacementWorkforceProfileId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public bool IsHalfDay { get; set; } = false;

        [MaxLength(20)]
        public string? HalfDayPeriod { get; set; }
        // FirstHalf, SecondHalf

        public bool IsHourly { get; set; } = false;
        public int? RequestedMinutes { get; set; }

        public decimal RequestedDays { get; set; } = 0;
        public decimal CalculatedWorkingDays { get; set; } = 0;
        public decimal ExcludedHolidayDays { get; set; } = 0;
        public decimal ExcludedWeeklyOffDays { get; set; } = 0;

        public decimal BalanceBeforeRequest { get; set; } = 0;
        public decimal EstimatedBalanceDeduction { get; set; } = 0;
        public decimal EstimatedBalanceAfterRequest { get; set; } = 0;
        public decimal ActualBalanceDeduction { get; set; } = 0;

        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ContactAddressDuringLeave { get; set; }

        [MaxLength(50)]
        public string? ContactNumberDuringLeave { get; set; }

        [MaxLength(2000)]
        public string? HandoverNotes { get; set; }

        public bool RequiresReplacement { get; set; } = false;
        public bool HasRosterConflict { get; set; } = false;
        public bool HasTrainingConflict { get; set; } = false;
        public bool HasCriticalStaffingImpact { get; set; } = false;

        public string? BalanceSimulationJson { get; set; }
        public string? RosterImpactJson { get; set; }
        public string? ValidationResultJson { get; set; }

        [Required, MaxLength(40)]
        public string LeaveRequestStatus { get; set; } = "Draft";
        // Draft, Submitted, WaitingSupervisorApproval, WaitingManagerApproval,
        // WaitingHrVerification, Approved, Rejected, NeedRevision, Cancelled,
        // Taken, Completed, Recalled, Expired

        public int CurrentApprovalStep { get; set; } = 0;

        public DateTime? SubmittedAt { get; set; }
        public DateTime? SupervisorApprovedAt { get; set; }
        public DateTime? ManagerApprovedAt { get; set; }
        public DateTime? HrVerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? RecalledAt { get; set; }
        public DateTime? ExpiredAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public MstLeavePolicy? LeavePolicy { get; set; }
        public WfpLeaveBalance? LeaveBalance { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkforceProfile? ReplacementWorkforceProfile { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxLeaveRequestApproval> Approvals { get; set; } = new List<TrxLeaveRequestApproval>();
        public ICollection<TrxLeaveRequestAttachment> Attachments { get; set; } = new List<TrxLeaveRequestAttachment>();
        public ICollection<TrxLeaveCancellationRequest> CancellationRequests { get; set; } = new List<TrxLeaveCancellationRequest>();
        public ICollection<TrxLeaveRecall> Recalls { get; set; } = new List<TrxLeaveRecall>();
        public ICollection<TrxLeaveBalanceTransaction> BalanceTransactions { get; set; } = new List<TrxLeaveBalanceTransaction>();
    }
}
