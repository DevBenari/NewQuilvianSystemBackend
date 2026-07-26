using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxRosterApproval", Schema = "public")]
    public class TrxRosterApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RosterPeriodId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public int StepOrder { get; set; } = 1;

        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? AssignedApproverUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [Required]
        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, RevisionRequested, Skipped, Cancelled

        [MaxLength(30)]
        public string? ActionType { get; set; }

        public DateTime? ActionAt { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public bool IsDelegated { get; set; } = false;
        public Guid? DelegatedFromUserId { get; set; }
        public Guid? DelegatedToUserId { get; set; }
        public DateTime? DueAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxRosterPeriod? RosterPeriod { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
        public ApplicationUser? DelegatedToUser { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
    }
}
