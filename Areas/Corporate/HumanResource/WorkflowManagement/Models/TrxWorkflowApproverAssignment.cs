using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxWorkflowApproverAssignment", Schema = "public")]
    public class TrxWorkflowApproverAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkflowInstanceId { get; set; }

        public Guid WorkflowStepInstanceId { get; set; }

        public Guid? ApprovalMatrixId { get; set; }

        public Guid? ApprovalDelegationId { get; set; }

        public Guid AssignedApproverUserId { get; set; }

        public Guid? AssignedApproverWorkforceProfileId { get; set; }

        public Guid? OriginalApproverUserId { get; set; }

        public Guid? OriginalApproverWorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? AssignedApproverRoleCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApproverSourceSnapshot { get; set; }
            = WorkflowValueConstants.ApproverSource.RequesterManager;

        public int AssignmentOrder { get; set; } = 1;

        [Required]
        [MaxLength(40)]
        public string AssignmentStatus { get; set; }
            = WorkflowValueConstants.AssignmentStatus.Pending;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AvailableAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? DelegatedAt { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsCurrentAssignment { get; set; } = false;

        public bool IsDelegated { get; set; } = false;

        public string? ResolutionSnapshotJson { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxWorkflowInstance? WorkflowInstance { get; set; }

        public TrxWorkflowStepInstance? WorkflowStepInstance { get; set; }

        public MstApprovalMatrix? ApprovalMatrix { get; set; }

        public TrxApprovalDelegation? ApprovalDelegation { get; set; }

        public ApplicationUser? AssignedApproverUser { get; set; }

        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }

        public ApplicationUser? OriginalApproverUser { get; set; }

        public MstWorkforceProfile? OriginalApproverWorkforceProfile { get; set; }

        public ICollection<TrxApprovalAction> ApprovalActions { get; set; }
            = new List<TrxApprovalAction>();
    }
}
