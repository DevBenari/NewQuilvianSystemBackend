using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimeRequestDetail", Schema = "public")]
    public class TrxOvertimeRequestDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OvertimeRequestId { get; set; }

        public int SequenceNumber { get; set; } = 1;
        public DateOnly OvertimeDate { get; set; }

        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? AttendanceId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? OvertimeRateId { get; set; }

        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }
        public DateTime? ApprovedStartAt { get; set; }
        public DateTime? ApprovedEndAt { get; set; }

        public int RequestedMinutes { get; set; } = 0;
        public int ApprovedMinutes { get; set; } = 0;
        public int BreakMinutes { get; set; } = 0;

        [Required, MaxLength(30)]
        public string DayType { get; set; } = "Workday";
        // Workday, RestDay, Holiday, SpecialHoliday.

        [Required, MaxLength(40)]
        public string OvertimeCategory { get; set; } = "AfterShift";
        // BeforeShift, AfterShift, RestDay, Holiday, Emergency, OnCall.

        [MaxLength(50)]
        public string? RateCodeSnapshot { get; set; }

        public decimal RateMultiplierSnapshot { get; set; } = 1;
        public decimal BaseHourlyRateSnapshot { get; set; } = 0;
        public decimal EstimatedCost { get; set; } = 0;
        public decimal ApprovedCost { get; set; } = 0;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(2000)]
        public string WorkDescription { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required, MaxLength(30)]
        public string DetailStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Rejected, Cancelled, Realized.

        public bool IsActive { get; set; } = true;

        public WfpOvertimeRequest? OvertimeRequest { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShift? Shift { get; set; }
        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public HrdAttendance? Attendance { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public MstOvertimeRate? OvertimeRate { get; set; }

        public ICollection<TrxOvertimeRealizationDetail> RealizationDetails { get; set; }
            = new List<TrxOvertimeRealizationDetail>();
    }
}
