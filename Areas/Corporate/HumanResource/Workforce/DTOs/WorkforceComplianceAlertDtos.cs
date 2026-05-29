using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceComplianceAlertResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string SourceEntityName { get; set; } = string.Empty;

        public Guid SourceEntityId { get; set; }

        public string SourceDisplayName { get; set; } = string.Empty;

        public ComplianceAlertType AlertType { get; set; }

        public string AlertTitle { get; set; } = string.Empty;

        public string AlertMessage { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public int DaysRemaining { get; set; }

        public bool IsOverdue { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; }

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; }

        public bool IsResolved { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        public string? ResolvedByUserName { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int LogCount { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforceComplianceAlertLogResponse> Logs { get; set; } = new();
    }

    public class WorkforceComplianceAlertLogResponse
    {
        public Guid Id { get; set; }

        public Guid ComplianceAlertId { get; set; }

        public ComplianceAlertLogType LogType { get; set; }

        public ComplianceAlertStatus? OldStatus { get; set; }

        public ComplianceAlertStatus? NewStatus { get; set; }

        public string? LogMessage { get; set; }

        public Guid? PerformedByUserId { get; set; }

        public string? PerformedByUserName { get; set; }

        public DateTime PerformedAt { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceComplianceAlertListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int OpenData { get; set; }

        public int InProgressData { get; set; }

        public int ResolvedData { get; set; }

        public int IgnoredData { get; set; }

        public int CancelledData { get; set; }

        public int ExpiredData { get; set; }

        public int OverdueData { get; set; }

        public int DueTodayData { get; set; }

        public int DueInSevenDaysData { get; set; }

        public int DueInThirtyDaysData { get; set; }

        public int CriticalData { get; set; }

        public int HighData { get; set; }

        public List<WorkforceComplianceAlertResponse> Items { get; set; } = new();
    }

    public class WorkforceComplianceAlertSummaryResponse
    {
        public int TotalAlert { get; set; }

        public int OpenAlert { get; set; }

        public int InProgressAlert { get; set; }

        public int ResolvedAlert { get; set; }

        public int OverdueAlert { get; set; }

        public int DueTodayAlert { get; set; }

        public int DueInSevenDaysAlert { get; set; }

        public int DueInThirtyDaysAlert { get; set; }

        public int CriticalAlert { get; set; }

        public int HighAlert { get; set; }

        public int DocumentAlert { get; set; }

        public int LicenseAlert { get; set; }

        public int CertificationAlert { get; set; }

        public int HealthRecordAlert { get; set; }

        public int ContractAlert { get; set; }

        public int ClinicalPrivilegeAlert { get; set; }

        public int ExternalAccessAlert { get; set; }
    }

    public class CreateWorkforceComplianceAlertRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SourceEntityName { get; set; } = string.Empty;

        [Required]
        public Guid SourceEntityId { get; set; }

        public ComplianceAlertType AlertType { get; set; } = ComplianceAlertType.Unknown;

        [Required]
        [MaxLength(200)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string AlertMessage { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; } = ComplianceAlertSeverityLevel.Low;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceComplianceAlertRequest
    {
        public ComplianceAlertType AlertType { get; set; } = ComplianceAlertType.Unknown;

        [Required]
        [MaxLength(200)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string AlertMessage { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; } = ComplianceAlertSeverityLevel.Low;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceComplianceAlertStatusRequest
    {
        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ResolveWorkforceComplianceAlertRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class ReopenWorkforceComplianceAlertRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class AddWorkforceComplianceAlertLogRequest
    {
        public ComplianceAlertLogType LogType { get; set; } = ComplianceAlertLogType.NoteAdded;

        [MaxLength(1000)]
        public string? LogMessage { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class GenerateWorkforceComplianceAlertRequest
    {
        public Guid? WorkforceProfileId { get; set; }

        [Range(0, 365)]
        public int DaysBeforeDue { get; set; } = 30;

        public bool IncludeExpired { get; set; } = true;

        public bool IncludeWillExpire { get; set; } = true;

        public bool IncludeDocument { get; set; } = true;

        public bool IncludeCertification { get; set; } = true;

        public bool IncludeCredentialLicense { get; set; } = true;

        public bool IncludeClinicalPrivilege { get; set; } = true;

        public bool IncludeHealthRecord { get; set; } = true;

        public bool IncludeEmployeeContract { get; set; } = true;

        public bool IncludeDoctorContract { get; set; } = true;

        public bool IncludeExternalAccess { get; set; } = true;

        public bool IncludeContractHistory { get; set; } = true;
    }

    public class GenerateWorkforceComplianceAlertResponse
    {
        public int DaysBeforeDue { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public int CandidateData { get; set; }

        public int CreatedData { get; set; }

        public int SkippedDuplicateData { get; set; }

        public int SkippedResolvedData { get; set; }

        public int DocumentCreatedData { get; set; }

        public int CertificationCreatedData { get; set; }

        public int CredentialLicenseCreatedData { get; set; }

        public int ClinicalPrivilegeCreatedData { get; set; }

        public int HealthRecordCreatedData { get; set; }

        public int EmployeeContractCreatedData { get; set; }

        public int DoctorContractCreatedData { get; set; }

        public int ExternalAccessCreatedData { get; set; }

        public int ContractHistoryCreatedData { get; set; }

        public List<WorkforceComplianceAlertResponse> CreatedItems { get; set; } = new();
    }
}
