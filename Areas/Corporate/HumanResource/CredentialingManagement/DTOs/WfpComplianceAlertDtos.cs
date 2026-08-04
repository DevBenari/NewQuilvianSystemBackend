using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    public class WfpComplianceAlertSummaryResponse
    {
        public int TotalAlert { get; set; }
        public int OpenAlert { get; set; }
        public int InProgressAlert { get; set; }
        public int ResolvedAlert { get; set; }
        public int IgnoredAlert { get; set; }
        public int ExpiredAlert { get; set; }
        public int CriticalAlert { get; set; }
        public int HighAlert { get; set; }
        public int OverdueAlert { get; set; }
        public int SchedulingBlockedAlert { get; set; }
        public int ClinicalServiceBlockedAlert { get; set; }
    }

    public class WfpComplianceAlertResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string SourceEntityName { get; set; } = string.Empty;
        public Guid SourceEntityId { get; set; }
        public ComplianceAlertType AlertType { get; set; }
        public string AlertTypeName { get; set; } = string.Empty;
        public string AlertTitle { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public ComplianceAlertStatus AlertStatus { get; set; }
        public string AlertStatusName { get; set; } = string.Empty;
        public ComplianceAlertSeverityLevel SeverityLevel { get; set; }
        public string SeverityLevelName { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public bool BlocksScheduling { get; set; }
        public bool BlocksClinicalService { get; set; }
        public bool IsOverdue { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public int LogCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpComplianceAlertDetailResponse : WfpComplianceAlertResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpComplianceAlertFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpComplianceAlertDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpComplianceAlertEnumOptionResponse> AlertTypeOptions { get; set; } = new();
        public List<WfpComplianceAlertEnumOptionResponse> AlertStatusOptions { get; set; } = new();
        public List<WfpComplianceAlertEnumOptionResponse> SeverityLevelOptions { get; set; } = new();
        public List<WfpComplianceAlertEnumOptionResponse> LogTypeOptions { get; set; } = new();
        public List<WfpComplianceAlertSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpComplianceAlertDefaultFilterResponse
    {
        public ComplianceAlertType? AlertType { get; set; }
        public ComplianceAlertStatus? AlertStatus { get; set; }
        public ComplianceAlertSeverityLevel? SeverityLevel { get; set; }
        public bool? IsResolved { get; set; }
        public bool? IsOverdue { get; set; }
        public bool? BlocksScheduling { get; set; }
        public bool? BlocksClinicalService { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "dueDate";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpComplianceAlertEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpComplianceAlertSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpComplianceAlertRequest
    {
        [Required]
        [MaxLength(100)]
        public string SourceEntityName { get; set; } = string.Empty;

        [Required]
        public Guid SourceEntityId { get; set; }

        [Required]
        public ComplianceAlertType AlertType { get; set; }

        [Required]
        [MaxLength(250)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string AlertMessage { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public ComplianceAlertSeverityLevel SeverityLevel { get; set; }

        public bool BlocksScheduling { get; set; }
        public bool BlocksClinicalService { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpComplianceAlertRequest : CreateWfpComplianceAlertRequest
    {
    }

    public class UpdateWfpComplianceAlertStatusRequest
    {
        [Required]
        public ComplianceAlertStatus AlertStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class ResolveWfpComplianceAlertRequest
    {
        [MaxLength(2000)]
        public string? ResolutionNotes { get; set; }
    }

    public class ReopenWfpComplianceAlertRequest
    {
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
