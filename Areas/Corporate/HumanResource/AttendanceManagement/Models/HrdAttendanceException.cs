using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdAttendanceException", Schema = "public")]
    public class HrdAttendanceException : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendanceDailyId { get; set; }

        public Guid? WorkforceProfileId { get; set; }
        public Guid? CorrectionRequestId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ExceptionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ExceptionType { get; set; } = "Unknown";
        // Late, EarlyLeave, MissingCheckIn, MissingCheckOut, Absent, OutsideGeofence, DuplicatePunch, ScheduleMismatch.

        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "Warning";
        // Info, Warning, High, Critical.

        [Required]
        [MaxLength(30)]
        public string ExceptionStatus { get; set; } = "Open";
        // Open, UnderReview, Corrected, Waived, Rejected, Closed.

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedAt { get; set; }
        public DateTime? ActualAt { get; set; }
        public int? DifferenceMinutes { get; set; }

        public bool IsAutoDetected { get; set; } = true;
        public bool IsPayrollBlocking { get; set; } = false;

        [MaxLength(100)]
        public string? DetectionRule { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        public Guid? ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }

        [MaxLength(1000)]
        public string? ResolutionNote { get; set; }

        public bool IsActive { get; set; } = true;

        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public HrdAttendanceCorrectionRequest? CorrectionRequest { get; set; }
        public ApplicationUser? ResolvedByUser { get; set; }
    }
}
