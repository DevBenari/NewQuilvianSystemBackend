using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxWorkflowStepInstance", Schema = "public")]
    public class TrxWorkflowStepInstance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkflowInstanceId { get; set; }

        public Guid WorkflowStepId { get; set; }

        public Guid? ApprovalMatrixId { get; set; }

        public int StepOrder { get; set; }

        [Required]
        [MaxLength(50)]
        public string StepCodeSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StepNameSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StepTypeSnapshot { get; set; }
            = WorkflowValueConstants.StepType.Approval;

        [Required]
        [MaxLength(50)]
        public string ApprovalModeSnapshot { get; set; }
            = WorkflowValueConstants.ApprovalMode.Any;

        [Required]
        [MaxLength(50)]
        public string ApproverSourceSnapshot { get; set; }
            = WorkflowValueConstants.ApproverSource.RequesterManager;

        public int RequiredApprovalCount { get; set; } = 1;

        public decimal? RequiredApprovalPercentage { get; set; }

        public int TotalAssignmentCount { get; set; } = 0;

        public int ApprovedActionCount { get; set; } = 0;

        public int RejectedActionCount { get; set; } = 0;

        [Required]
        [MaxLength(40)]
        public string StepStatus { get; set; }
            = WorkflowValueConstants.StepStatus.Pending;

        public DateTime? AvailableAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? SkippedAt { get; set; }

        public bool IsCurrentStep { get; set; } = false;

        public bool IsDelegationAllowed { get; set; } = true;

        public bool IsAutoAction { get; set; } = false;

        [MaxLength(1000)]
        public string? InstructionsSnapshot { get; set; }

        public string? AssignmentResolutionJson { get; set; }

        public string? StepConditionSnapshotJson { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxWorkflowInstance? WorkflowInstance { get; set; }

        public MstWorkflowStep? WorkflowStep { get; set; }

        public MstApprovalMatrix? ApprovalMatrix { get; set; }

        public ICollection<TrxWorkflowApproverAssignment> ApproverAssignments { get; set; }
            = new List<TrxWorkflowApproverAssignment>();

        public ICollection<TrxApprovalAction> ApprovalActions { get; set; }
            = new List<TrxApprovalAction>();

        public ICollection<TrxWorkflowComment> Comments { get; set; }
            = new List<TrxWorkflowComment>();

        public ICollection<TrxWorkflowAttachment> Attachments { get; set; }
            = new List<TrxWorkflowAttachment>();

        public ICollection<TrxWorkflowStatusHistory> StatusHistories { get; set; }
            = new List<TrxWorkflowStatusHistory>();
    }
}
