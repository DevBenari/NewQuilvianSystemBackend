using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("TrxAttendanceCorrectionApproval", Schema = "public")]
    public class TrxAttendanceCorrectionApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendanceCorrectionRequestId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public int StepOrder { get; set; } = 1;

        public Guid? AssignedApproverUserId { get; set; }
        public Guid? AssignedApproverWorkforceProfileId { get; set; }
        public Guid? ActualActionByUserId { get; set; }
        public Guid? ActualActionByWorkforceProfileId { get; set; }
        public Guid? DelegatedFromUserId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [Required]
        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, Returned, Skipped, Cancelled.

        [MaxLength(30)]
        public string? ActionType { get; set; }

        public DateTime? ActionAt { get; set; }

        [MaxLength(1500)]
        public string? Comments { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxAttendanceCorrectionRequest? AttendanceCorrectionRequest { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public ApplicationUser? AssignedApproverUser { get; set; }
        public MstWorkforceProfile? AssignedApproverWorkforceProfile { get; set; }
        public ApplicationUser? ActualActionByUser { get; set; }
        public MstWorkforceProfile? ActualActionByWorkforceProfile { get; set; }
        public ApplicationUser? DelegatedFromUser { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
    }
}
