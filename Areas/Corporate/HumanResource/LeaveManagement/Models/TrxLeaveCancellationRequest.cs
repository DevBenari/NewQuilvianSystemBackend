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
    [Table("TrxLeaveCancellationRequest", Schema = "public")]
    public class TrxLeaveCancellationRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string CancellationNumber { get; set; } = string.Empty;

        [Required]
        public Guid LeaveRequestId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? BalanceTransactionId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateOnly? EffectiveCancellationDate { get; set; }

        public decimal RestoredDays { get; set; } = 0;

        [Required, MaxLength(2000)]
        public string CancellationReason { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string CancellationStatus { get; set; } = "Draft";
        // Draft, Submitted, WaitingApproval, Approved, Rejected, Cancelled, Applied

        public Guid? RequestedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpLeaveRequest? LeaveRequest { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public TrxLeaveBalanceTransaction? BalanceTransaction { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
    }
}
