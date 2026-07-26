using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("WfpScheduleChangeRequest", Schema = "public")]
    public class WfpScheduleChangeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? CurrentShiftAssignmentId { get; set; }
        public Guid? RequestedShiftAssignmentId { get; set; }

        public Guid? CurrentWorkScheduleId { get; set; }
        public Guid? RequestedWorkScheduleId { get; set; }
        public Guid? CurrentShiftId { get; set; }
        public Guid? RequestedShiftId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        [Required]
        [MaxLength(40)]
        public string RequestType { get; set; } = "ScheduleChange";
        // ScheduleChange, ShiftChange, DayOffChange, TemporarySchedule

        public DateOnly RequestedDate { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [Required]
        [MaxLength(30)]
        public string RequestStatus { get; set; } = "Draft";
        // Draft, Submitted, UnderReview, Approved, Rejected, Cancelled, Applied

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public bool IsAppliedToRoster { get; set; } = false;
        public DateTime? AppliedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }
        public TrxRosterPeriod? RosterPeriod { get; set; }
        public TrxShiftAssignment? CurrentShiftAssignment { get; set; }
        public TrxShiftAssignment? RequestedShiftAssignment { get; set; }
        public MstWorkSchedule? CurrentWorkSchedule { get; set; }
        public MstWorkSchedule? RequestedWorkSchedule { get; set; }
        public MstShift? CurrentShift { get; set; }
        public MstShift? RequestedShift { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
    }
}
