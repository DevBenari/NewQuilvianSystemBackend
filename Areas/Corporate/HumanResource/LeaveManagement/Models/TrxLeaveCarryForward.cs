using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveCarryForward", Schema = "public")]
    public class TrxLeaveCarryForward : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveCarryForwardRunId { get; set; }

        [Required]
        public Guid LeaveCarryForwardPolicyId { get; set; }

        [Required]
        public Guid SourceLeaveEntitlementPeriodId { get; set; }

        [Required]
        public Guid DestinationLeaveEntitlementPeriodId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid SourceLeaveTypeId { get; set; }

        [Required]
        public Guid DestinationLeaveTypeId { get; set; }

        [Required]
        public Guid SourceLeaveBalanceId { get; set; }

        public Guid? DestinationLeaveBalanceId { get; set; }

        [Required, MaxLength(50)]
        public string CarryForwardNumber { get; set; } = string.Empty;

        public DateOnly CalculationDate { get; set; }
        public DateOnly? CarryForwardExpiryDate { get; set; }

        public decimal SourceAvailableDays { get; set; } = 0;
        public decimal EligibleDays { get; set; } = 0;
        public decimal CarryForwardDays { get; set; } = 0;
        public decimal ExpiredDays { get; set; } = 0;
        public decimal ExcessDays { get; set; } = 0;
        public decimal PayoutDays { get; set; } = 0;
        public decimal RoundingAdjustmentDays { get; set; } = 0;

        [Required, MaxLength(30)]
        public string CarryForwardStatus { get; set; }
            = LeaveValueConstants.CarryForwardStatus.Draft;

        [MaxLength(100)]
        public string? SkipReasonCode { get; set; }

        [MaxLength(1000)]
        public string? SkipReason { get; set; }

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        public DateTime? CalculatedAt { get; set; }
        public Guid? CalculatedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public DateTime? ReversedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }

        public string? SourceBalanceSnapshotJson { get; set; }
        public string? CalculationDetailJson { get; set; }

        [MaxLength(4000)]
        public string? ErrorMessage { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxLeaveCarryForwardRun? LeaveCarryForwardRun { get; set; }
        public MstLeaveCarryForwardPolicy? LeaveCarryForwardPolicy { get; set; }
        public TrxLeaveEntitlementPeriod? SourceLeaveEntitlementPeriod { get; set; }
        public TrxLeaveEntitlementPeriod? DestinationLeaveEntitlementPeriod { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? SourceLeaveType { get; set; }
        public MstLeaveType? DestinationLeaveType { get; set; }
        public WfpLeaveBalance? SourceLeaveBalance { get; set; }
        public WfpLeaveBalance? DestinationLeaveBalance { get; set; }
        public ApplicationUser? CalculatedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }

        public ICollection<TrxLeaveBalanceTransaction> BalanceTransactions { get; set; }
            = new List<TrxLeaveBalanceTransaction>();
    }
}
