using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models
{
    [Table("MstWorkflowStep", Schema = "public")]
    public class MstWorkflowStep : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StepCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StepName { get; set; } = string.Empty;

        public int StepOrder { get; set; } = 1;

        [Required]
        [MaxLength(50)]
        public string StepType { get; set; } = "Approval";
        // Approval, Review, Verification, Notification, SystemAction.

        [Required]
        [MaxLength(50)]
        public string ApprovalMode { get; set; } = "Any";
        // Any, All, Sequential, Percentage.

        public int RequiredApprovalCount { get; set; } = 1;

        public decimal? RequiredApprovalPercentage { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApproverSourceType { get; set; } = "RequesterManager";
        // RequesterManager, ManagerLevel, Position, OrganizationUnit,
        // Role, SpecificUser, ApprovalMatrix, RequesterSelected.

        public Guid? ApproverPositionId { get; set; }

        public Guid? ApproverOrganizationUnitId { get; set; }

        public Guid? SpecificApproverUserId { get; set; }

        [MaxLength(100)]
        public string? ApproverRoleCode { get; set; }

        public int? ManagerLevel { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsParallel { get; set; } = false;

        public bool AllowDelegation { get; set; } = true;

        public bool AllowSelfApproval { get; set; } = false;

        public int? ReminderAfterHours { get; set; }

        public int? EscalationAfterHours { get; set; }

        public int? AutoApproveAfterHours { get; set; }

        public int? AutoRejectAfterHours { get; set; }

        [MaxLength(50)]
        public string? OnApproveNextStepCode { get; set; }

        [MaxLength(50)]
        public string? OnRejectStepCode { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public MstPosition? ApproverPosition { get; set; }

        public MstOrganizationUnit? ApproverOrganizationUnit { get; set; }

        public ICollection<MstApprovalMatrix> ApprovalMatrices { get; set; }
            = new List<MstApprovalMatrix>();

        public ICollection<MstApprovalDelegationPolicy> DelegationPolicies { get; set; }
            = new List<MstApprovalDelegationPolicy>();

        public ICollection<MstRejectionReason> RejectionReasons { get; set; }
            = new List<MstRejectionReason>();
    }
}
