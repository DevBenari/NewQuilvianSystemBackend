using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public enum WorkforceCredentialLicenseType
    {
        STR = 1,
        SIP = 2,
        SIK = 3,
        SIPP = 4,
        SIPA = 5,
        SIPB = 6,
        Other = 99
    }

    public class WorkforceCredentialLicenseSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalCredentialLicense { get; set; }
        public int ActiveCredentialLicense { get; set; }
        public int InactiveCredentialLicense { get; set; }
        public int VerifiedCredentialLicense { get; set; }
        public int PendingVerificationCredentialLicense { get; set; }
        public int RejectedCredentialLicense { get; set; }
        public int RevokedCredentialLicense { get; set; }
        public int ExpiredCredentialLicense { get; set; }
        public int CurrentlyValidCredentialLicense { get; set; }
        public int PrimaryCredentialLicense { get; set; }
        public int CredentialLicenseWithFile { get; set; }
        public int CredentialLicenseWithoutFile { get; set; }
    }

    public class WorkforceCredentialLicenseResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? RequirementCode { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string? PracticeLocation { get; set; }
        public bool HasFile { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FilePreviewUrl { get; set; }
        public string? FileDownloadUrl { get; set; }
        public CredentialVerificationStatus VerificationStatus { get; set; }
        public bool IsVerified { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerificationNote { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedReason { get; set; }
        public Guid? RevokedByUserId { get; set; }
        public string? RevokedByUserName { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCredentialLicenseDetailResponse : WorkforceCredentialLicenseResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceCredentialLicenseOptionResponse
    {
        public Guid Id { get; set; }
        public string? RequirementCode { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string? PracticeLocation { get; set; }
        public CredentialVerificationStatus VerificationStatus { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public bool HasFile { get; set; }
    }

    public class WorkforceCredentialLicenseOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceCredentialLicenseOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceCredentialLicenseFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai CreateDateTime dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public string CodeInfo { get; set; } = "RequirementCode dibuat otomatis oleh backend dengan format CRL-RSMMC-00001. Frontend tidak perlu mengirim RequirementCode pada POST/PUT.";
        public WorkforceCredentialLicenseDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceCredentialLicenseCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceCredentialLicenseSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceCredentialLicenseTypeOptionResponse> LicenseTypeOptions { get; set; } = new();
        public List<WorkforceCredentialLicenseVerificationStatusOptionResponse> VerificationStatusOptions { get; set; } = new();
        public List<string> PreviewSupportedContentTypes { get; set; } = new()
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };
        public WorkforceCredentialLicenseFileUploadInfoResponse FileUploadInfo { get; set; } = new();
        public List<string> FrontendGuide { get; set; } = new();
    }

    public class WorkforceCredentialLicenseDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public WorkforceCredentialLicenseType? LicenseType { get; set; }
        public CredentialVerificationStatus? VerificationStatus { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceCredentialLicenseCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceCredentialLicenseSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceCredentialLicenseTypeOptionResponse
    {
        public WorkforceCredentialLicenseType Value { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> CommonFor { get; set; } = new();
    }

    public class WorkforceCredentialLicenseVerificationStatusOptionResponse
    {
        public CredentialVerificationStatus Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceCredentialLicenseFileUploadInfoResponse
    {
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
        public string MaxFileSizeLabel { get; set; } = "10 MB";
        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx"
        };
    }

    public class CreateWorkforceCredentialLicenseRequest
    {
        [Required]
        public WorkforceCredentialLicenseType LicenseType { get; set; } = WorkforceCredentialLicenseType.STR;

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime ExpiredDate { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        public IFormFile? File { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCredentialLicenseRequest : CreateWorkforceCredentialLicenseRequest
    {
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceCredentialLicenseStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class SetWorkforceCredentialLicensePrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class VerifyWorkforceCredentialLicenseRequest
    {
        [MaxLength(250)]
        public string? VerificationNote { get; set; }
    }

    public class RejectWorkforceCredentialLicenseRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class RevokeWorkforceCredentialLicenseRequest
    {
        [Required]
        [MaxLength(250)]
        public string RevokedReason { get; set; } = string.Empty;
    }

    public class DeleteWorkforceCredentialLicenseFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
