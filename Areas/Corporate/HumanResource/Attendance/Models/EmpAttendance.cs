using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models
{
    [Table("EmpAttendance", Schema = "public")]
    public class EmpAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }
        public Guid? WorkforceProfileId { get; set; }

        public Guid? WorkScheduleId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public DateTime CheckInAt { get; set; }

        public DateTime? CheckOutAt { get; set; }

        public TimeOnly? WorkStartTime { get; set; }

        public TimeOnly? WorkEndTime { get; set; }

        public int CheckInToleranceMinutes { get; set; } = 0;

        public bool IsLate { get; set; } = false;

        public int LateMinutes { get; set; } = 0;

        [MaxLength(50)]
        public string AttendanceStatus { get; set; } = "Present";

        public double CheckInLatitude { get; set; }

        public double CheckInLongitude { get; set; }

        public double? CheckInAccuracyMeters { get; set; }

        public double CheckInDistanceMeters { get; set; }

        public double? CheckOutLatitude { get; set; }

        public double? CheckOutLongitude { get; set; }

        public double? CheckOutAccuracyMeters { get; set; }

        public double? CheckOutDistanceMeters { get; set; }

        public int? WorkDurationMinutes { get; set; }

        public bool IsGeofenceBypassed { get; set; } = false;

        [MaxLength(250)]
        public string? GeofenceBypassReason { get; set; }

        [MaxLength(50)]
        public UserType UserType { get; set; }

        [MaxLength(50)]
        public string CheckInSource { get; set; } = "Login";

        [MaxLength(50)]
        public string? CheckOutSource { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "CheckedIn";

        [MaxLength(100)]
        public string? CheckInIpAddress { get; set; }

        [MaxLength(100)]
        public string? CheckOutIpAddress { get; set; }

        [MaxLength(500)]
        public string? CheckInUserAgent { get; set; }

        [MaxLength(500)]
        public string? CheckOutUserAgent { get; set; }

        //Navigation
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? User { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
    }
}
