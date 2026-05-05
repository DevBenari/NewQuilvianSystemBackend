using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models
{
    [Table("EmpAttendance", Schema = "public")]
    public class EmpAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public DateTime CheckInAt { get; set; }

        public DateTime? CheckOutAt { get; set; }

        public double CheckInLatitude { get; set; }

        public double CheckInLongitude { get; set; }

        public double? CheckInAccuracyMeters { get; set; }

        public double CheckInDistanceMeters { get; set; }

        public double? CheckOutLatitude { get; set; }

        public double? CheckOutLongitude { get; set; }

        public double? CheckOutAccuracyMeters { get; set; }

        public double? CheckOutDistanceMeters { get; set; }

        public int? WorkDurationMinutes { get; set; }

        [MaxLength(50)]
        public string PersonType { get; set; } = string.Empty;

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
    }
}
