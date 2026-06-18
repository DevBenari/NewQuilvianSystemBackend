using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class KioskScanSessionSummaryResponse
    {
        public int TotalSession { get; set; }
        public int StartedSession { get; set; }
        public int SuccessSession { get; set; }
        public int FailedSession { get; set; }
        public int CancelledSession { get; set; }
        public int ManualInputSession { get; set; }
        public int PatientFoundSession { get; set; }
        public int PatientNotFoundSession { get; set; }
        public int UsedForRegistrationSession { get; set; }
        public int UnusedForRegistrationSession { get; set; }
    }

    public class KioskScanSessionResponse
    {
        public Guid Id { get; set; }

        public string SessionCode { get; set; } = string.Empty;

        public KioskScanSource ScanSource { get; set; }

        public string ScanSourceName { get; set; } = string.Empty;

        public KioskScanSessionStatus ScanStatus { get; set; }

        public string ScanStatusName { get; set; } = string.Empty;

        public Guid? KioskDeviceId { get; set; }

        public string? KioskDeviceName { get; set; }

        public Guid? IdentityScannerProfileId { get; set; }

        public string? IdentityScannerProfileName { get; set; }

        public Guid? PatientId { get; set; }

        public string? PatientCode { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? ScanImagePath { get; set; }

        public string? ScanImageContentType { get; set; }

        public string? CardNumber { get; set; }

        public string? MemberNumber { get; set; }

        public string? InsuranceCardNumber { get; set; }

        public bool IsPatientFound { get; set; }

        public bool IsManualInput { get; set; }

        public bool IsUsedForRegistration { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }
    }

    public class KioskScanSessionDetailResponse : KioskScanSessionResponse
    {
        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? GenderText { get; set; }

        public string? Address { get; set; }

        public string? RawScanText { get; set; }

        public string? ParsedJson { get; set; }

        public string? FailureReason { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }
    }

    public class KioskScanSessionOptionResponse
    {
        public Guid Id { get; set; }

        public string SessionCode { get; set; } = string.Empty;

        public KioskScanSource ScanSource { get; set; }

        public string ScanSourceName { get; set; } = string.Empty;

        public KioskScanSessionStatus ScanStatus { get; set; }

        public string ScanStatusName { get; set; } = string.Empty;

        public Guid? PatientId { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        public string? IdentityNumber { get; set; }

        public string? CardNumber { get; set; }

        public string? MemberNumber { get; set; }

        public bool IsPatientFound { get; set; }

        public bool IsUsedForRegistration { get; set; }

        public DateTime StartedAt { get; set; }
    }

    public class KioskScanSessionOptionPagedResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<KioskScanSessionOptionResponse> Items { get; set; } = new();
    }

    public class KioskScanSessionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public KioskScanSessionDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<KioskScanSessionCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<KioskScanSessionSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<KioskScanSessionRelationFilterResponse> RelationFilters { get; set; } = new();

        public List<KioskScanSessionEnumOptionResponse> ScanSourceOptions { get; set; } = new();

        public List<KioskScanSessionEnumOptionResponse> ScanStatusOptions { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class KioskScanSessionDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public Guid? KioskDeviceId { get; set; }

        public Guid? PatientId { get; set; }

        public KioskScanSource? ScanSource { get; set; }

        public KioskScanSessionStatus? ScanStatus { get; set; }

        public bool? IsPatientFound { get; set; }

        public bool? IsManualInput { get; set; }

        public bool? IsUsedForRegistration { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "startedAt";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class KioskScanSessionRelationFilterResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;
    }

    public class KioskScanSessionEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class KioskScanSessionCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class KioskScanSessionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreateKioskScanSessionRequest
    {
        public Guid? KioskDeviceId { get; set; }

        public Guid? IdentityScannerProfileId { get; set; }

        public KioskScanSource ScanSource { get; set; } = KioskScanSource.Unknown;

        [MaxLength(12000)]
        public string? RawScanText { get; set; }

        [MaxLength(12000)]
        public string? ParsedJson { get; set; }

        [MaxLength(100)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(500)]
        public string? ScanImagePath { get; set; }

        [MaxLength(100)]
        public string? ScanImageContentType { get; set; }

        [MaxLength(100)]
        public string? CardNumber { get; set; }

        [MaxLength(100)]
        public string? MemberNumber { get; set; }

        [MaxLength(100)]
        public string? InsuranceCardNumber { get; set; }

        [MaxLength(200)]
        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? GenderText { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(250)]
        public string? FailureReason { get; set; }

        public bool IsManualInput { get; set; } = false;
    }

    public class KioskScanSessionCreateResponse
    {
        public Guid Id { get; set; }

        public string SessionCode { get; set; } = string.Empty;

        public KioskScanSessionStatus ScanStatus { get; set; }

        public string ScanStatusName { get; set; } = string.Empty;

        public bool IsPatientFound { get; set; }

        public Guid? PatientId { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }
    }

    public class MarkKioskScanSessionUsedForRegistrationRequest
    {
        public Guid? PatientId { get; set; }
    }

    public class CancelKioskScanSessionRequest
    {
        [MaxLength(250)]
        public string? CancelReason { get; set; }
    }

    public class DeleteKioskScanSessionRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}
