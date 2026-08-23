using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("WfpTransportAllowanceTransaction", Schema = "public")]
    public class WfpTransportAllowanceTransaction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TransportAllowanceId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? AttendanceDailyId { get; set; }

        [Required, MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        public DateOnly TransactionDate { get; set; }

        [Required, MaxLength(30)]
        public string TransactionType { get; set; } = "Accrual";
        // Accrual, Adjustment, Reservation, Payment, Reversal, Expiry.

        [Required, MaxLength(30)]
        public string TransactionStatus { get; set; } = "Draft";
        // Draft, Calculated, Approved, Posted, Reversed, Cancelled.

        public decimal Quantity { get; set; } = 0m;
        public decimal Rate { get; set; } = 0m;
        public decimal Amount { get; set; } = 0m;
        public decimal BalanceAfterTransaction { get; set; } = 0m;

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public Guid? SourceId { get; set; }

        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpTransportAllowance? TransportAllowance { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
    }
}
