using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxShiftReplacement", Schema = "public")]
    public class TrxShiftReplacement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ShiftAssignmentId { get; set; }

        [Required]
        public Guid OriginalWorkforceProfileId { get; set; }

        [Required]
        public Guid ReplacementWorkforceProfileId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        [Required]
        [MaxLength(40)]
        public string ReplacementType { get; set; } = "Replacement";
        // Replacement, AbsenceCover, Emergency, LeaveCover, TrainingCover

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string ReplacementStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Rejected, Applied, Cancelled

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? AppliedAt { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public MstWorkforceProfile? OriginalWorkforceProfile { get; set; }
        public MstWorkforceProfile? ReplacementWorkforceProfile { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
