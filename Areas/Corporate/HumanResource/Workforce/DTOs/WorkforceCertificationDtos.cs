using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceCertificationSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalCertification { get; set; }
        public int ActiveCertification { get; set; }
        public int InactiveCertification { get; set; }
        public int VerifiedCertification { get; set; }
        public int UnverifiedCertification { get; set; }
        public int ExpiredCertification { get; set; }
        public int LifetimeCertification { get; set; }
        public int CertificationWithFile { get; set; }
        public int CertificationWithoutFile { get; set; }
    }

    public class WorkforceCertificationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? RequirementCode { get; set; }
        public string CertificationType { get; set; } = string.Empty;
        public string CertificationName { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsLifetime { get; set; }
        public bool HasFile { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FilePreviewUrl { get; set; }
        public string? FileDownloadUrl { get; set; }
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WorkforceCertificationDetailResponse : WorkforceCertificationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WorkforceCertificationOptionResponse
    {
        public Guid Id { get; set; }
        public string? RequirementCode { get; set; }
        public string CertificationType { get; set; } = string.Empty;
        public string CertificationName { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsLifetime { get; set; }
        public bool HasFile { get; set; }
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
    }

    public class WorkforceCertificationOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceCertificationOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceCertificationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai CreateDateTime dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WorkforceCertificationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceCertificationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceCertificationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> CertificationTypes { get; set; } = new()
        {
            "Clinical",
            "NonClinical",
            "Safety",
            "Quality",
            "IT",
            "Other"
        };
        public List<string> PreviewSupportedContentTypes { get; set; } = new()
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };
    }

    public class WorkforceCertificationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? CertificationType { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceCertificationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceCertificationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWorkforceCertificationRequest
    {
        [Required]
        [MaxLength(100)]
        public string CertificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CertificationName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; } = false;

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCertificationRequest : CreateWorkforceCertificationRequest
    {
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceCertificationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceCertificationRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class DeleteWorkforceCertificationFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
