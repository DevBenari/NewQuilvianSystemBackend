using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxBusinessTravelApproval", Schema = "public")]
    public class TrxBusinessTravelApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public Guid? AssignedApproverUserId { get; set; }
        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        public int StepOrder { get; set; } = 1;

        [Required, MaxLength(30)]
        public string ApprovalRole { get; set; } = "Manager";
        // Supervisor, Manager, BudgetOwner, HR, Finance, Director.

        [Required, MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, NeedRevision, Skipped, Cancelled.

        [MaxLength(30)]
        public string? ActionType { get; set; }

        public DateTime? AssignedAt { get; set; }
        public DateTime? ActionAt { get; set; }
        public DateTime? DueAt { get; set; }

        public bool IsDelegated { get; set; } = false;
        public Guid? DelegatedFromUserId { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
    }
}
