using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdAttendanceDailySegment", Schema = "public")]
    public class HrdAttendanceDailySegment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendanceDailyId { get; set; }

        public Guid? ShiftAssignmentId { get; set; }

        public int SegmentOrder { get; set; } = 1;

        [Required]
        [MaxLength(30)]
        public string SegmentType { get; set; }
            = AttendanceValueConstants.AttendanceSegmentType.Work;

        [MaxLength(30)]
        public string SegmentSource { get; set; }
            = AttendanceValueConstants.AttendanceSegmentSource.Processor;

        public DateTime? ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }

        public Guid? StartRawLogId { get; set; }
        public Guid? EndRawLogId { get; set; }

        public int ScheduledMinutes { get; set; } = 0;
        public int ActualMinutes { get; set; } = 0;
        public int BreakMinutes { get; set; } = 0;
        public int PayableMinutes { get; set; } = 0;
        public int LateMinutes { get; set; } = 0;
        public int EarlyLeaveMinutes { get; set; } = 0;
        public int OvertimeMinutes { get; set; } = 0;

        public bool IsOvernight { get; set; } = false;
        public bool IsCorrected { get; set; } = false;

        [MaxLength(30)]
        public string SegmentStatus { get; set; }
            = AttendanceValueConstants.AttendanceSegmentStatus.Calculated;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public HrdAttendanceRawLog? StartRawLog { get; set; }
        public HrdAttendanceRawLog? EndRawLog { get; set; }
    }
}
