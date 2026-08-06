using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("WfpShiftSwapRequest", Schema = "public")]
    public class WfpShiftSwapRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid RequesterWorkforceProfileId { get; set; }

        [Required]
        public Guid TargetWorkforceProfileId { get; set; }

        public Guid? RosterPeriodId { get; set; }

        [Required]
        public Guid RequesterShiftAssignmentId { get; set; }

        [Required]
        public Guid TargetShiftAssignmentId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateOnly RequesterShiftDate { get; set; }
        public DateOnly TargetShiftDate { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [Required]
        [MaxLength(30)]
        public string RequestStatus { get; set; } = "Draft";
        // Draft, PendingTarget, TargetAccepted, TargetRejected, PendingApproval, NeedRevision, Approved, Rejected, Cancelled, Applied

        public DateTime? RequestedAt { get; set; }
        public DateTime? TargetRespondedAt { get; set; }
        public bool? IsAcceptedByTarget { get; set; }

        [MaxLength(1000)]
        public string? TargetResponseNotes { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public bool IsAppliedToRoster { get; set; } = false;
        public DateTime? AppliedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? RequesterWorkforceProfile { get; set; }
        public MstWorkforceProfile? TargetWorkforceProfile { get; set; }
        public TrxRosterPeriod? RosterPeriod { get; set; }
        public TrxShiftAssignment? RequesterShiftAssignment { get; set; }
        public TrxShiftAssignment? TargetShiftAssignment { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
    }
}
