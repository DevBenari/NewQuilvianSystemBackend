using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("WfpLeaveBalance", Schema = "public")]
    public class WfpLeaveBalance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeavePolicyId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? LeaveEntitlementPeriodId { get; set; }

        public int Year { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }

        public decimal OpeningBalanceDays { get; set; } = 0;
        public decimal EntitlementDays { get; set; } = 0;
        public decimal AccruedDays { get; set; } = 0;
        public decimal CarriedForwardDays { get; set; } = 0;
        public decimal AdjustmentDays { get; set; } = 0;
        public decimal CompensatoryDays { get; set; } = 0;
        public decimal ReservedDays { get; set; } = 0;
        public decimal PendingDays { get; set; } = 0;
        public decimal UsedDays { get; set; } = 0;
        public decimal RecalledDays { get; set; } = 0;
        public decimal ExpiredDays { get; set; } = 0;
        public decimal EncashmentDays { get; set; } = 0;
        public decimal RemainingDays { get; set; } = 0;
        public decimal AvailableDays { get; set; } = 0;

        [Required]
        [MaxLength(30)]
        public string BalanceStatus { get; set; }
            = LeaveValueConstants.BalanceStatus.Active;

        public Guid? LastTransactionId { get; set; }
        public long LastTransactionSequence { get; set; } = 0;
        public long BalanceVersion { get; set; } = 0;

        public DateTime? LastCalculatedAt { get; set; }
        public DateTime? LastReconciledAt { get; set; }
        public DateOnly? CarryForwardExpiryDate { get; set; }

        public bool IsLocked { get; set; } = false;
        public DateTime? LockedAt { get; set; }
        public Guid? LockedByUserId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public MstLeavePolicy? LeavePolicy { get; set; }
        public MstLeaveEntitlementPolicy? LeaveEntitlementPolicy { get; set; }
        public TrxLeaveEntitlementPeriod? LeaveEntitlementPeriod { get; set; }
        public TrxLeaveBalanceTransaction? LastTransaction { get; set; }
        public ApplicationUser? LockedByUser { get; set; }

        public ICollection<TrxLeaveEntitlement> Entitlements { get; set; }
            = new List<TrxLeaveEntitlement>();

        public ICollection<TrxLeaveAccrual> Accruals { get; set; }
            = new List<TrxLeaveAccrual>();

        public ICollection<TrxLeaveBalanceTransaction> Transactions { get; set; }
            = new List<TrxLeaveBalanceTransaction>();

        public ICollection<TrxLeaveAdjustment> Adjustments { get; set; }
            = new List<TrxLeaveAdjustment>();

        public ICollection<WfpLeaveRequest> LeaveRequests { get; set; }
            = new List<WfpLeaveRequest>();
    }
}
