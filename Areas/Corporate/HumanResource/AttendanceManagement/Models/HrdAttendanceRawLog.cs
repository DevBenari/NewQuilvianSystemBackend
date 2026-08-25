using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdAttendanceRawLog", Schema = "public")]
    public class HrdAttendanceRawLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public UserType? UserType { get; set; }

        public Guid? AttendanceDeviceId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public Guid? HospitalSiteId { get; set; }

        [MaxLength(100)]
        public string? ExternalLogId { get; set; }

        [MaxLength(100)]
        public string? ExternalDeviceId { get; set; }

        [MaxLength(100)]
        public string? DeviceUserKey { get; set; }

        [Required]
        public DateTime EventAt { get; set; }

        [Required]
        [MaxLength(30)]
        public string EventType { get; set; }
            = AttendanceValueConstants.RawLogEventType.Unknown;

        [Required]
        [MaxLength(30)]
        public string SourceType { get; set; }
            = AttendanceValueConstants.RawLogSourceType.Device;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public decimal? DistanceMeters { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(128)]
        public string? EventHash { get; set; }

        public string? RawPayloadJson { get; set; }

        [Required]
        [MaxLength(30)]
        public string ProcessingStatus { get; set; }
            = AttendanceValueConstants.RawLogProcessingStatus.Pending;

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        public Guid? ProcessedAttendanceId { get; set; }
        public Guid? ProcessedAttendanceDailyId { get; set; }

        [MaxLength(1000)]
        public string? ProcessingMessage { get; set; }

        public bool IsActive { get; set; } = true;

        public ApplicationUser? User { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstDoctor? Doctor { get; set; }
        public MstAttendanceDevice? AttendanceDevice { get; set; }
        public MstAttendanceLocation? AttendanceLocation { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public HrdAttendance? ProcessedAttendance { get; set; }
        public HrdAttendanceDaily? ProcessedAttendanceDaily { get; set; }
    }
}
