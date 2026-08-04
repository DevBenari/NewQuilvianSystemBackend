using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveAdjustment", Schema = "public")]
    public class TrxLeaveAdjustment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string AdjustmentNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveBalanceId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        [Required]
        public Guid LeaveEntitlementPeriodId { get; set; }

        [Required]
        public Guid LeaveAdjustmentReasonId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? OriginalAdjustmentId { get; set; }

        [Required]
        [MaxLength(30)]
        public string AdjustmentType { get; set; }
            = LeaveValueConstants.AdjustmentType.ManualAdjustment;

        [Required]
        [MaxLength(10)]
        public string Direction { get; set; }
            = LeaveValueConstants.TransactionDirection.Credit;

        public decimal RequestedDays { get; set; }

        public decimal? ApprovedDays { get; set; }

        public decimal PostedDays { get; set; } = 0;

        public DateOnly EffectiveDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string AdjustmentStatus { get; set; }
            = LeaveValueConstants.AdjustmentStatus.Draft;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? RequestNote { get; set; }

        [MaxLength(50)]
        public string SourceType { get; set; }
            = LeaveValueConstants.AdjustmentSourceType.HrManual;

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(100)]
        public string? SourceReferenceNumber { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid RequestedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNote { get; set; }

        public DateTime? RejectedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public DateTime? PostedAt { get; set; }

        public Guid? PostedByUserId { get; set; }

        public DateTime? ReversedAt { get; set; }

        public Guid? ReversedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ReversalReason { get; set; }

        public string? RequestSnapshotJson { get; set; }

        public string? ApprovalSnapshotJson { get; set; }

        public string? PostingSnapshotJson { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public WfpLeaveBalance? LeaveBalance { get; set; }

        public MstLeaveType? LeaveType { get; set; }

        public TrxLeaveEntitlementPeriod? LeaveEntitlementPeriod { get; set; }

        public MstLeaveAdjustmentReason? LeaveAdjustmentReason { get; set; }

        public TrxWorkflowInstance? WorkflowInstance { get; set; }

        public TrxLeaveAdjustment? OriginalAdjustment { get; set; }

        public TrxLeaveAdjustment? ReversalAdjustment { get; set; }

        public ApplicationUser? RequestedByUser { get; set; }

        public ApplicationUser? SubmittedByUser { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }

        public ApplicationUser? PostedByUser { get; set; }

        public ApplicationUser? ReversedByUser { get; set; }

        public ICollection<TrxLeaveBalanceTransaction> BalanceTransactions { get; set; }
            = new List<TrxLeaveBalanceTransaction>();
    }
}
