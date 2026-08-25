using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceSelfServiceCaptureRequest
    {
        [Required]
        public Guid AttendanceLocationId { get; set; }

        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Range(0, 100000)]
        public decimal? AccuracyMeters { get; set; }

        [Required]
        public Guid ClientRequestId { get; set; }
    }

    public class AttendanceSelfServiceLocationResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string LocationType { get; set; } = string.Empty;
        public int RadiusMeters { get; set; }
        public bool RequiresGeolocation { get; set; }
        public bool AllowMobileAttendance { get; set; }
    }

    public class AttendanceSelfServiceCaptureStatusResponse
    {
        public Guid UserId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public DateTime ServerNowUtc { get; set; }
        public DateTime LocalNow { get; set; }
        public bool IsCheckedIn { get; set; }
        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public DateTime? LastCheckInAt { get; set; }
        public DateTime? LastCheckOutAt { get; set; }
        public Guid? CurrentAttendanceDailyId { get; set; }
        public DateOnly? CurrentAttendanceDate { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? AttendanceProcessingStatus { get; set; }
        public bool GpsRequired { get; set; } = true;
        public List<AttendanceSelfServiceLocationResponse> AllowedLocations { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class AttendanceSelfServiceCaptureResponse
    {
        public Guid RawLogId { get; set; }
        public bool IsDuplicate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public DateTime EventAt { get; set; }
        public Guid AttendanceLocationId { get; set; }
        public string AttendanceLocationName { get; set; } = string.Empty;
        public decimal DistanceMeters { get; set; }
        public int RadiusMeters { get; set; }
        public bool IsInsideGeofence { get; set; }
        public DateOnly? WorkDate { get; set; }
        public bool ProcessingTriggered { get; set; }
        public bool ProcessingSucceeded { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? AttendanceProcessingStatus { get; set; }
        public string RawLogProcessingStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
