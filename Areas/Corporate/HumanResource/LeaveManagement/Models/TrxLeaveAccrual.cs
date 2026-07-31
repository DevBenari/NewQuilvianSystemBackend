using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveAccrual", Schema = "public")]
    public class TrxLeaveAccrual : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string AccrualNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeaveEntitlementId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? LeaveAccrualRunId { get; set; }
        public Guid? BalanceTransactionId { get; set; }

        public DateOnly AccrualDate { get; set; }
        public DateOnly? ScheduledAccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }

        public int AccrualSequence { get; set; } = 1;
        public decimal AccrualAmountDays { get; set; } = 0;
        public decimal BalanceBeforeAccrual { get; set; } = 0;
        public decimal BalanceAfterAccrual { get; set; } = 0;

        public bool IsProrated { get; set; } = false;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        [Required, MaxLength(30)]
        public string AccrualStatus { get; set; }
            = LeaveValueConstants.AccrualStatus.Draft;

        [MaxLength(50)]
        public string AccrualFrequency { get; set; } = "Monthly";

        [MaxLength(50)]
        public string SourceType { get; set; } = "ScheduledAccrual";

        public Guid? SourceReferenceId { get; set; }

        public DateTime? CalculatedAt { get; set; }
        public Guid? CalculatedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public DateTime? ReversedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }

        public string? CalculationDetailJson { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public WfpLeaveBalance? LeaveBalance { get; set; }
        public TrxLeaveEntitlement? LeaveEntitlement { get; set; }
        public MstLeaveEntitlementPolicy? LeaveEntitlementPolicy { get; set; }
        public TrxLeaveAccrualRun? LeaveAccrualRun { get; set; }
        public TrxLeaveBalanceTransaction? BalanceTransaction { get; set; }
        public ApplicationUser? CalculatedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }

        public ICollection<TrxLeaveBalanceTransaction> BalanceTransactions { get; set; }
            = new List<TrxLeaveBalanceTransaction>();
    }
}
