using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimeRealizationDetail", Schema = "public")]
    public class TrxOvertimeRealizationDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OvertimeRealizationId { get; set; }

        public Guid? OvertimeRequestDetailId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? AttendanceId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? OvertimeRateId { get; set; }

        public int SequenceNumber { get; set; } = 1;
        public DateOnly OvertimeDate { get; set; }

        public DateTime? AttendanceCheckInAt { get; set; }
        public DateTime? AttendanceCheckOutAt { get; set; }
        public DateTime ActualStartAt { get; set; }
        public DateTime ActualEndAt { get; set; }

        public int ActualMinutes { get; set; } = 0;
        public int BreakMinutes { get; set; } = 0;
        public int EligibleMinutes { get; set; } = 0;
        public int VerifiedMinutes { get; set; } = 0;
        public int VarianceFromApprovedMinutes { get; set; } = 0;

        [Required, MaxLength(30)]
        public string DayType { get; set; } = "Workday";

        [MaxLength(50)]
        public string? RateBandSnapshot { get; set; }

        [MaxLength(50)]
        public string? CalculationMethodSnapshot { get; set; }

        public decimal RateMultiplierSnapshot { get; set; } = 1;
        public decimal? FixedAmountSnapshot { get; set; }
        public decimal BaseHourlyRateSnapshot { get; set; } = 0;
        public decimal CalculatedAmount { get; set; } = 0;
        public decimal VerifiedAmount { get; set; } = 0;

        [MaxLength(500)]
        public string? EvidenceFilePath { get; set; }

        [MaxLength(255)]
        public string? EvidenceFileName { get; set; }

        [MaxLength(150)]
        public string? EvidenceContentType { get; set; }

        public long? EvidenceFileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? EvidenceChecksum { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required, MaxLength(30)]
        public string DetailStatus { get; set; } = "Draft";
        // Draft, Submitted, Verified, NeedRevision, Rejected, Posted.

        public bool IsActive { get; set; } = true;

        public TrxOvertimeRealization? OvertimeRealization { get; set; }
        public TrxOvertimeRequestDetail? OvertimeRequestDetail { get; set; }
        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public HrdAttendance? Attendance { get; set; }
        public TrxAttendanceDaily? AttendanceDaily { get; set; }
        public MstOvertimeRate? OvertimeRate { get; set; }
    }
}
