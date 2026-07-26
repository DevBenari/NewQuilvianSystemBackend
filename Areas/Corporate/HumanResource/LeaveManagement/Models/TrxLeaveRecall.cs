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
    [Table("TrxLeaveRecall", Schema = "public")]
    public class TrxLeaveRecall : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string RecallNumber { get; set; } = string.Empty;

        [Required]
        public Guid LeaveRequestId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? ReplacementWorkforceProfileId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? BalanceTransactionId { get; set; }

        public DateOnly OriginalLeaveEndDate { get; set; }
        public DateOnly RecallEffectiveDate { get; set; }
        public DateOnly? ActualReturnToWorkDate { get; set; }

        public decimal RecalledLeaveDays { get; set; } = 0;
        public decimal RestoredBalanceDays { get; set; } = 0;

        [Required, MaxLength(2000)]
        public string RecallReason { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string RecallStatus { get; set; } = "Draft";
        // Draft, Submitted, Acknowledged, Approved, Rejected, Applied, Cancelled

        public Guid? InitiatedByUserId { get; set; }
        public Guid? AcknowledgedByUserId { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? AppliedAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpLeaveRequest? LeaveRequest { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkforceProfile? ReplacementWorkforceProfile { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public TrxLeaveBalanceTransaction? BalanceTransaction { get; set; }
        public ApplicationUser? InitiatedByUser { get; set; }
        public ApplicationUser? AcknowledgedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
