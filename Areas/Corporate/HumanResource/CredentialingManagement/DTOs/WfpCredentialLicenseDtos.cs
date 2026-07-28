using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    public class WfpCredentialLicenseSummaryResponse
    {
        public int TotalLicense { get; set; }
        public int ActiveLicense { get; set; }
        public int InactiveLicense { get; set; }
        public int PrimaryLicense { get; set; }
        public int VerifiedLicense { get; set; }
        public int UnverifiedLicense { get; set; }
        public int RevokedLicense { get; set; }
        public int ExpiredLicense { get; set; }
        public int ExpiringSoonLicense { get; set; }
    }

    public class WfpCredentialLicenseResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? LicenseTypeId { get; set; }
        public string? LicenseTypeCode { get; set; }
        public string? LicenseTypeMasterName { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionCode { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? CredentialingRequirementId { get; set; }
        public string? RequirementCode { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? PracticeLocation { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool IsExpired { get; set; }
        public int DaysUntilExpiry { get; set; }
        public CredentialVerificationStatus VerificationStatus { get; set; }
        public string VerificationStatusName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public string? VerificationNotes { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? RevokedByUserId { get; set; }
        public string? RevokedByUserName { get; set; }
        public string? RevocationReason { get; set; }
        public bool BlocksSchedulingWhenInvalid { get; set; }
        public bool BlocksClinicalServiceWhenInvalid { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public bool HasFile { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpCredentialLicenseDetailResponse : WfpCredentialLicenseResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpCredentialLicenseFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpCredentialLicenseDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpLicenseTypeOptionResponse> LicenseTypeOptions { get; set; } = new();
        public List<WfpCredentialLicenseEnumOptionResponse> VerificationStatusOptions { get; set; } = new();
        public List<WfpCredentialLicenseSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpCredentialLicenseDefaultFilterResponse
    {
        public Guid? LicenseTypeId { get; set; }
        public CredentialVerificationStatus? VerificationStatus { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsRevoked { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsActive { get; set; }
        public int? ExpiringWithinDays { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "expiredDate";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpLicenseTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? RegulatoryBody { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool IsRenewable { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCredentialLicenseEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCredentialLicenseSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpCredentialLicenseRequest
    {
        public Guid? LicenseTypeId { get; set; }
        public Guid? CredentialingRequirementId { get; set; }

        [MaxLength(50)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(250)]
        public string? PracticeLocation { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public CredentialVerificationStatus VerificationStatus { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool BlocksSchedulingWhenInvalid { get; set; } = true;
        public bool BlocksClinicalServiceWhenInvalid { get; set; } = true;

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpCredentialLicenseRequest : CreateWfpCredentialLicenseRequest
    {
    }

    public class UpdateWfpCredentialLicenseStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class SetWfpCredentialLicensePrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class VerifyWfpCredentialLicenseRequest
    {
        public bool IsVerified { get; set; } = true;
        public CredentialVerificationStatus? VerificationStatus { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }
    }

    public class RevokeWfpCredentialLicenseRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RevocationReason { get; set; } = string.Empty;
    }
}
