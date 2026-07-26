using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveRequestApproval", Schema = "public")]
    public class TrxLeaveRequestApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveRequestId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public int StepOrder { get; set; } = 1;

        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? AssignedApproverUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [Required, MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, NeedRevision, Skipped, Cancelled

        [MaxLength(30)]
        public string? ActionType { get; set; }
        // Approve, Reject, RequestRevision, Skip, Cancel

        public DateTime? ActionAt { get; set; }
        public DateTime? DueAt { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public bool IsDelegated { get; set; } = false;
        public Guid? DelegatedFromUserId { get; set; }
        public Guid? DelegatedToUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpLeaveRequest? LeaveRequest { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
        public ApplicationUser? DelegatedToUser { get; set; }
    }
}
