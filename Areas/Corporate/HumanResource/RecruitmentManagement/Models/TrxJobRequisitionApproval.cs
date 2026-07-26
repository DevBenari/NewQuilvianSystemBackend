using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxJobRequisitionApproval", Schema = "public")]
    public class TrxJobRequisitionApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobRequisitionId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public int StepOrder { get; set; } = 0;
        public Guid? AssignedApproverUserId { get; set; }
        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }

        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, RevisionRequested, Skipped, Cancelled.

        [MaxLength(30)]
        public string? ActionType { get; set; }

        public DateTime? AssignedAt { get; set; }
        public DateTime? ActionAt { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool IsDelegated { get; set; } = false;
        public Guid? DelegatedFromUserId { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxJobRequisition? JobRequisition { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
    }
}
