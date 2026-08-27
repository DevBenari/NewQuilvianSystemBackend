using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdMissingAttendance", Schema = "public")]
    public class HrdMissingAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? UserId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? AttendanceExceptionId { get; set; }
        public Guid? AttendanceCorrectionRequestId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string MissingType { get; set; } = "MissingCheckOut";
        // MissingCheckIn, MissingCheckOut, MissingBoth, IncompleteSegment.

        public DateTime? ExpectedCheckInAt { get; set; }
        public DateTime? ExpectedCheckOutAt { get; set; }
        public DateTime? ActualCheckInAt { get; set; }
        public DateTime? ActualCheckOutAt { get; set; }

        [Required]
        [MaxLength(30)]
        public string MissingStatus { get; set; } = "Open";
        // Open, Notified, CorrectionSubmitted, Resolved, Waived, Closed.

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? NotifiedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ResolutionNote { get; set; }

        public bool IsPayrollBlocking { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? User { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShift? Shift { get; set; }
        public HrdAttendanceException? AttendanceException { get; set; }
        public HrdAttendanceCorrectionRequest? AttendanceCorrectionRequest { get; set; }
        public ApplicationUser? ResolvedByUser { get; set; }
    }
}
