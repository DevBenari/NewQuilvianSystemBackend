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

        public Guid? LeaveRequestId { get; set; }
        public Guid? LeaveEntitlementId { get; set; }
        public Guid? LeaveAccrualId { get; set; }

        public Guid? ReversedTransactionId { get; set; }

        public DateTime TransactionDateTime { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(50)]
        public string TransactionType { get; set; } = "Adjustment";
        // Opening, Entitlement, Accrual, CarryForward, Reservation, Deduction,
        // CancellationRestore, RecallAdjustment, Expiry, ManualAdjustment,
        // CompensatoryCredit, Encashment, Reversal

        [Required, MaxLength(10)]
        public string Direction { get; set; } = "Credit";
        // Credit, Debit

        public decimal TransactionDays { get; set; } = 0;
        public decimal PreviousOpeningBalanceDays { get; set; } = 0;
        public decimal PreviousAvailableDays { get; set; } = 0;
        public decimal PreviousReservedDays { get; set; } = 0;
        public decimal NewAvailableDays { get; set; } = 0;
        public decimal NewReservedDays { get; set; } = 0;
        public decimal NewUsedDays { get; set; } = 0;

        [MaxLength(50)]
        public string SourceType { get; set; } = "System";

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(100)]
        public string? SourceReferenceNumber { get; set; }

        [Required, MaxLength(30)]
        public string TransactionStatus { get; set; } = "Posted";
        // Draft, Posted, Reversed, Cancelled

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
        public WfpLeaveRequest? LeaveRequest { get; set; }
        public TrxLeaveEntitlement? LeaveEntitlement { get; set; }
        public TrxLeaveAccrual? LeaveAccrual { get; set; }
        public TrxLeaveBalanceTransaction? ReversedTransaction { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }
    }
}
