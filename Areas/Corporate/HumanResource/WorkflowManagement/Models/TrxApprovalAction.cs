using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxApprovalAction", Schema = "public")]
    public class TrxApprovalAction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public Guid? WorkflowStepInstanceId { get; set; }
        public Guid? ApprovalDelegationId { get; set; }
        public Guid? AssignedApproverUserId { get; set; }
        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid ActualActionByUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? DelegatedFromUserId { get; set; }

        [Required]
        [MaxLength(40)]
        public string ActionType { get; set; } = "Approve";

        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        [MaxLength(4000)]
        public string? Comment { get; set; }

        public bool IsDelegated { get; set; } = false;
        public bool IsSystemAction { get; set; } = false;

        [MaxLength(50)]
        public string ActionSource { get; set; } = "Web";

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(40)]
        public string? PreviousWorkflowStatus { get; set; }

        [MaxLength(40)]
        public string? ResultingWorkflowStatus { get; set; }

        [MaxLength(40)]
        public string? PreviousStepStatus { get; set; }

        [MaxLength(40)]
        public string? ResultingStepStatus { get; set; }

        public string? ActionContextJson { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxWorkflowInstance? WorkflowInstance { get; set; }
        public TrxWorkflowStepInstance? WorkflowStepInstance { get; set; }
        public TrxApprovalDelegation? ApprovalDelegation { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
        public ICollection<TrxWorkflowAttachment> Attachments { get; set; } = new List<TrxWorkflowAttachment>();
    }
}
