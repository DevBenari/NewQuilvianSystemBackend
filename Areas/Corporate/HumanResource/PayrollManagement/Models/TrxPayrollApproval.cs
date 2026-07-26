using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollApproval", Schema = "public")]
    public class TrxPayrollApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public Guid? AssignedApproverUserId { get; set; }
        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? DelegatedFromUserId { get; set; }

        public int StepOrder { get; set; } = 0;

        [Required, MaxLength(50)]
        public string ApprovalLevel { get; set; } = "Payroll";

        [Required, MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, NeedRevision, Skipped, Cancelled.

        [MaxLength(30)]
        public string? ActionType { get; set; }

        public DateTime? ActionAt { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public bool IsDelegated { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public TrxPayrollRun? PayrollRun { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
    }
}
