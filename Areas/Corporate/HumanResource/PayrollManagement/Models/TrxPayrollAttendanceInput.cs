using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollAttendanceInput", Schema = "public")]
    public class TrxPayrollAttendanceInput : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunEmployeeId { get; set; }

        public Guid? AttendanceDailyId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        [Required, MaxLength(30)]
        public string AttendanceStatusSnapshot { get; set; } = "Present";

        public int ScheduledWorkMinutes { get; set; } = 0;
        public int ActualWorkMinutes { get; set; } = 0;
        public int PayableWorkMinutes { get; set; } = 0;
        public int LateMinutes { get; set; } = 0;
        public int EarlyLeaveMinutes { get; set; } = 0;
        public int OvertimeMinutes { get; set; } = 0;
        public decimal PaidLeaveDays { get; set; } = 0m;
        public decimal UnpaidLeaveDays { get; set; } = 0m;
        public decimal AbsentDays { get; set; } = 0m;
        public decimal AttendanceAllowanceAmount { get; set; } = 0m;
        public decimal AttendanceDeductionAmount { get; set; } = 0m;

        public bool IsHoliday { get; set; } = false;
        public bool IsRestDay { get; set; } = false;
        public bool IsBusinessTravel { get; set; } = false;
        public bool IsCorrectionApplied { get; set; } = false;

        public string? AttendanceSnapshotJson { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public Guid? ImportedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public ApplicationUser? ImportedByUser { get; set; }
    }
}
