using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimeRequestApproval", Schema = "public")]
    public class TrxOvertimeRequestApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OvertimeRequestId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public int StepOrder { get; set; } = 1;

        [Required, MaxLength(40)]
        public string ApprovalLevel { get; set; } = "Supervisor";
        // Supervisor, Manager, HR, Finance.

        public Guid? AssignedApproverUserId { get; set; }
        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? DelegatedFromUserId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [Required, MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, NeedRevision, Skipped, Cancelled.

        [MaxLength(30)]
        public string? ActionType { get; set; }
        // Approve, Reject, RequestRevision, Skip, Delegate.

        public DateTime? ActionAt { get; set; }
        public int ApprovedMinutes { get; set; } = 0;
        public decimal ApprovedEstimatedCost { get; set; } = 0;

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public bool IsCurrentStep { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public WfpOvertimeRequest? OvertimeRequest { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
    }
}
