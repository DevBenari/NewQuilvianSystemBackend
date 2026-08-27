using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdAttendance", Schema = "public")]
    public class HrdAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public Guid? WorkScheduleId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? AttendancePolicyId { get; set; }
        public Guid? GracePeriodPolicyId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public Guid? CheckInDeviceId { get; set; }
        public Guid? CheckOutDeviceId { get; set; }
        public Guid? AttendanceDailyId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public DateTime CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }

        public TimeOnly? WorkStartTime { get; set; }
        public TimeOnly? WorkEndTime { get; set; }

        public bool IsOvernightSchedule { get; set; } = false;

        public DateTime? ScheduledCheckInAt { get; set; }
        public DateTime? ScheduledCheckOutAt { get; set; }

        public int CheckInToleranceMinutes { get; set; } = 0;
        public int CheckOutToleranceMinutes { get; set; } = 0;

        public bool IsLate { get; set; } = false;
        public int LateMinutes { get; set; } = 0;
        public bool IsEarlyLeave { get; set; } = false;
        public int EarlyLeaveMinutes { get; set; } = 0;

        [Required]
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
        public int? BreakDurationMinutes { get; set; }
        public int? PayableWorkMinutes { get; set; }

        public bool IsGeofenceBypassed { get; set; } = false;

        [MaxLength(250)]
        public string? GeofenceBypassReason { get; set; }

        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string CheckInSource { get; set; } = "Login";

        [MaxLength(50)]
        public string? CheckOutSource { get; set; }

        [Required]
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

        public bool IsHoliday { get; set; } = false;
        public bool IsRestDay { get; set; } = false;
        public bool IsCorrected { get; set; } = false;
        public bool IsProcessed { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }

        [MaxLength(30)]
        public string ProcessingStatus { get; set; } = "Pending";

        [MaxLength(500)]
        public string? ProcessingMessage { get; set; }

        public ApplicationUser? User { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstDoctor? Doctor { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }
        public MstShift? Shift { get; set; }
        public MstAttendancePolicy? AttendancePolicy { get; set; }
        public MstGracePeriodPolicy? GracePeriodPolicy { get; set; }
        public MstAttendanceLocation? AttendanceLocation { get; set; }
        public MstAttendanceDevice? CheckInDevice { get; set; }
        public MstAttendanceDevice? CheckOutDevice { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
    }
}
