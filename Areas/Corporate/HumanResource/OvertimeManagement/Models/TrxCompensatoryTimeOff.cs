using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxCompensatoryTimeOff", Schema = "public")]
    public class TrxCompensatoryTimeOff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string CreditNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OvertimeRequestId { get; set; }
        public Guid? OvertimeRealizationId { get; set; }
        public Guid? OvertimeVerificationId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveBalanceTransactionId { get; set; }

        public DateOnly EarnedDate { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        public int SourceOvertimeMinutes { get; set; } = 0;
        public decimal ConversionRate { get; set; } = 1;
        public int EarnedMinutes { get; set; } = 0;
        public int ReservedMinutes { get; set; } = 0;
        public int UsedMinutes { get; set; } = 0;
        public int ExpiredMinutes { get; set; } = 0;
        public int RemainingMinutes { get; set; } = 0;

        [Required, MaxLength(30)]
        public string CompensatoryStatus { get; set; } = "Pending";
        // Pending, Available, PartiallyUsed, Used, Expired, Cancelled.

        public DateTime? GeneratedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOvertimeRequest? OvertimeRequest { get; set; }
        public TrxOvertimeRealization? OvertimeRealization { get; set; }
        public TrxOvertimeVerification? OvertimeVerification { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public TrxLeaveBalanceTransaction? LeaveBalanceTransaction { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
