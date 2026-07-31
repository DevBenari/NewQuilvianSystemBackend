using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveBalanceTransaction", Schema = "public")]
    public class TrxLeaveBalanceTransaction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [Required]
        public Guid LeaveBalanceId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveRequestId { get; set; }
        public Guid? LeaveEntitlementId { get; set; }
        public Guid? LeaveAccrualId { get; set; }
        public Guid? LeaveCarryForwardId { get; set; }
        public Guid? LeaveAdjustmentId { get; set; }

        public Guid? ReversedTransactionId { get; set; }
        public Guid? OriginalTransactionId { get; set; }

        public DateTime TransactionDateTime { get; set; } = DateTime.UtcNow;
        public DateOnly? EffectiveDate { get; set; }
        public long TransactionSequence { get; set; } = 0;

        [Required, MaxLength(50)]
        public string TransactionType { get; set; }
            = LeaveValueConstants.TransactionType.ManualAdjustment;

        [Required, MaxLength(10)]
        public string Direction { get; set; }
            = LeaveValueConstants.TransactionDirection.Credit;

        public decimal TransactionDays { get; set; } = 0;

        public decimal OpeningBalanceDelta { get; set; } = 0;
        public decimal EntitlementDelta { get; set; } = 0;
        public decimal AccruedDelta { get; set; } = 0;
        public decimal CarryForwardDelta { get; set; } = 0;
        public decimal AdjustmentDelta { get; set; } = 0;
        public decimal CompensatoryDelta { get; set; } = 0;
        public decimal PendingDelta { get; set; } = 0;
        public decimal ReservedDelta { get; set; } = 0;
        public decimal UsedDelta { get; set; } = 0;
        public decimal RecalledDelta { get; set; } = 0;
        public decimal ExpiredDelta { get; set; } = 0;
        public decimal EncashmentDelta { get; set; } = 0;
        public decimal AvailableDelta { get; set; } = 0;

        public decimal PreviousOpeningBalanceDays { get; set; } = 0;
        public decimal PreviousAvailableDays { get; set; } = 0;
        public decimal PreviousReservedDays { get; set; } = 0;
        public decimal NewAvailableDays { get; set; } = 0;
        public decimal NewReservedDays { get; set; } = 0;
        public decimal NewUsedDays { get; set; } = 0;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(50)]
        public string? PostingBatchType { get; set; }

        public Guid? PostingBatchId { get; set; }

        [MaxLength(50)]
        public string SourceType { get; set; } = "System";

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(100)]
        public string? SourceReferenceNumber { get; set; }

        [Required, MaxLength(30)]
        public string TransactionStatus { get; set; }
            = LeaveValueConstants.TransactionStatus.Posted;

        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public DateTime? ReversedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpLeaveBalance? LeaveBalance { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public TrxLeaveEntitlementPeriod? LeaveEntitlementPeriod { get; set; }
        public WfpLeaveRequest? LeaveRequest { get; set; }
        public TrxLeaveEntitlement? LeaveEntitlement { get; set; }
        public TrxLeaveAccrual? LeaveAccrual { get; set; }
        public TrxLeaveCarryForward? LeaveCarryForward { get; set; }
        public TrxLeaveAdjustment? LeaveAdjustment { get; set; }
        public TrxLeaveBalanceTransaction? ReversedTransaction { get; set; }
        public TrxLeaveBalanceTransaction? OriginalTransaction { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }
    }
}
