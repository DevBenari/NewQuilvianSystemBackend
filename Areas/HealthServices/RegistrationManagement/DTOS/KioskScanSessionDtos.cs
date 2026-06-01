using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class KioskScanSessionResponse
    {
        public Guid Id { get; set; }
        public string SessionCode { get; set; } = string.Empty;
        public KioskScanSource ScanSource { get; set; }
        public KioskScanSessionStatus ScanStatus { get; set; }
        public Guid? KioskDeviceId { get; set; }
        public string? KioskDeviceName { get; set; }
        public Guid? IdentityScannerProfileId { get; set; }
        public string? IdentityScannerProfileName { get; set; }
        public Guid? PatientId { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string? PatientName { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? CardNumber { get; set; }
        public string? MemberNumber { get; set; }
        public string? InsuranceCardNumber { get; set; }
        public bool IsPatientFound { get; set; }
        public bool IsManualInput { get; set; }
        public bool IsUsedForRegistration { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
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

    public class CreateKioskScanSessionRequest
    {
        public Guid? KioskDeviceId { get; set; }
        public Guid? IdentityScannerProfileId { get; set; }
        public KioskScanSource ScanSource { get; set; } = KioskScanSource.Unknown;

        [MaxLength(2000)]
        public string? RawScanText { get; set; }

        [MaxLength(2000)]
        public string? ParsedJson { get; set; }

        [MaxLength(100)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

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

        public bool IsManualInput { get; set; } = false;
    }

    public class KioskScanSessionCreateResponse
    {
        public Guid Id { get; set; }
        public string SessionCode { get; set; } = string.Empty;
        public KioskScanSessionStatus ScanStatus { get; set; }
        public bool IsPatientFound { get; set; }
        public Guid? PatientId { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string? PatientName { get; set; }
    }

    public class KioskScanSessionSummaryResponse
    {
        public int TotalSession { get; set; }
        public int SuccessSession { get; set; }
        public int FailedSession { get; set; }
        public int ManualInputSession { get; set; }
        public int PatientFoundSession { get; set; }
        public int UsedForRegistrationSession { get; set; }
    }
}