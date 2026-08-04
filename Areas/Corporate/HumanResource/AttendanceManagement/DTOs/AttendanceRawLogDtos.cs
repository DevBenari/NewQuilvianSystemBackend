using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceRawLogSummaryResponse
    {
        public int TotalRawLog { get; set; }
        public int ReceivedToday { get; set; }
        public int Pending { get; set; }
        public int Matched { get; set; }
        public int Processed { get; set; }
        public int Rejected { get; set; }
        public int Error { get; set; }
        public int UnmatchedWorkforce { get; set; }
        public int DeviceSource { get; set; }
        public int MobileSource { get; set; }
    }

    public class AttendanceRawLogResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorCode { get; set; }
        public UserType? UserType { get; set; }
        public Guid? AttendanceDeviceId { get; set; }
        public string? AttendanceDeviceCode { get; set; }
        public string? AttendanceDeviceName { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public string? AttendanceLocationCode { get; set; }
        public string? AttendanceLocationName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteCode { get; set; }
        public string? HospitalSiteName { get; set; }
        public string? ExternalLogId { get; set; }
        public string? ExternalDeviceId { get; set; }
        public string? DeviceUserKey { get; set; }
        public DateTime EventAt { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public decimal? DistanceMeters { get; set; }
        public string ProcessingStatus { get; set; } = string.Empty;
        public string? ProcessingMessage { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? ProcessedAttendanceId { get; set; }
        public Guid? ProcessedAttendanceDailyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class AttendanceRawLogDetailResponse : AttendanceRawLogResponse
    {
        public string? EventHash { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RawPayloadJson { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class AttendanceRawLogCreateResponse
    {
        public Guid Id { get; set; }
        public bool IsDuplicate { get; set; }
        public Guid? ExistingRawLogId { get; set; }
        public string ProcessingStatus { get; set; } = string.Empty;
        public string? ProcessingMessage { get; set; }
        public Guid? UserId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? AttendanceDeviceId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public DateTime EventAt { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string EventHash { get; set; } = string.Empty;
    }

    public class AttendanceRawLogBatchItemResponse
    {
        public int Index { get; set; }
        public bool Success { get; set; }
        public Guid? Id { get; set; }
        public bool IsDuplicate { get; set; }
        public Guid? ExistingRawLogId { get; set; }
        public string? ExternalLogId { get; set; }
        public string? DeviceUserKey { get; set; }
        public string? ProcessingStatus { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AttendanceRawLogBatchResponse
    {
        public int TotalItem { get; set; }
        public int SuccessCount { get; set; }
        public int DuplicateCount { get; set; }
        public int FailedCount { get; set; }
        public List<AttendanceRawLogBatchItemResponse> Items { get; set; } = new();
    }

    public class AttendanceRawLogRetryResponse
    {
        public Guid Id { get; set; }
        public string PreviousProcessingStatus { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public string? ProcessingMessage { get; set; }
        public Guid? UserId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
    }

    public class AttendanceRawLogFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ssK";
        public string ResetButtonLabel { get; set; } = "Reset";
        public int MaximumBatchItem { get; set; } = 500;
        public AttendanceRawLogDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AttendanceRawLogStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<AttendanceRawLogStringOptionResponse> EventTypeOptions { get; set; } = new();
        public List<AttendanceRawLogStringOptionResponse> SourceTypeOptions { get; set; } = new();
        public List<AttendanceRawLogStringOptionResponse> ProcessingStatusOptions { get; set; } = new();
        public List<AttendanceRawLogGuidOptionResponse> AttendanceDeviceOptions { get; set; } = new();
        public List<AttendanceRawLogGuidOptionResponse> AttendanceLocationOptions { get; set; } = new();
        public List<AttendanceRawLogSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceRawLogDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? AttendanceDeviceId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? EventType { get; set; }
        public string? SourceType { get; set; }
        public string? ProcessingStatus { get; set; }
        public bool? IsMatched { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "eventAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceRawLogStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceRawLogGuidOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class AttendanceRawLogSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateAttendanceRawLogRequest
    {
        public Guid? UserId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
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
        public DateTimeOffset EventAt { get; set; }

        [Required, MaxLength(30)]
        public string EventType { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string SourceType { get; set; } = string.Empty;

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        [Range(0, 100000)]
        public decimal? AccuracyMeters { get; set; }

        public JsonElement? RawPayload { get; set; }
    }

    public class CreateAttendanceRawLogBatchRequest
    {
        [Required, MinLength(1), MaxLength(500)]
        public List<CreateAttendanceRawLogRequest> Items { get; set; } = new();
    }

    public class AttendanceRawLogQueryRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? AttendanceDeviceId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? EventType { get; set; }
        public string? SourceType { get; set; }
        public string? ProcessingStatus { get; set; }
        public bool? IsMatched { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "eventAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
