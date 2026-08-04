using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxApprovalDelegation", Schema = "public")]
    public class TrxApprovalDelegation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DelegatorUserId { get; set; }

        public Guid? DelegatorWorkforceProfileId { get; set; }

        public Guid DelegateUserId { get; set; }

        public Guid? DelegateWorkforceProfileId { get; set; }

        public Guid? ApprovalDelegationPolicyId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowStepId { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public Guid? RevokedByUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string DelegationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string DelegationStatus { get; set; }
            = WorkflowValueConstants.DelegationStatus.Draft;

        public DateTime EffectiveStartAt { get; set; }

        public DateTime EffectiveEndAt { get; set; }

        [MaxLength(1000)]
        public string? DelegationReason { get; set; }

        public bool AppliesToAllWorkflows { get; set; } = false;

        public bool AllowSubDelegation { get; set; } = false;

        public bool PreserveDelegatorAccountability { get; set; } = true;

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        [MaxLength(1000)]
        public string? RevocationReason { get; set; }

        public string? ScopeDefinitionJson { get; set; }

        public bool IsActive { get; set; } = true;

        public ApplicationUser? DelegatorUser { get; set; }

        public MstWorkforceProfile? DelegatorWorkforceProfile { get; set; }

        public ApplicationUser? DelegateUser { get; set; }

        public MstWorkforceProfile? DelegateWorkforceProfile { get; set; }

        public MstApprovalDelegationPolicy? ApprovalDelegationPolicy { get; set; }

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public MstWorkflowStep? WorkflowStep { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RevokedByUser { get; set; }

        public ICollection<TrxWorkflowApproverAssignment> ApproverAssignments { get; set; }
            = new List<TrxWorkflowApproverAssignment>();

        public ICollection<TrxApprovalAction> ApprovalActions { get; set; }
            = new List<TrxApprovalAction>();
    }
}
