using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    public class WfpCertificationSummaryResponse
    {
        public int TotalCertification { get; set; }
        public int ActiveCertification { get; set; }
        public int InactiveCertification { get; set; }
        public int VerifiedCertification { get; set; }
        public int UnverifiedCertification { get; set; }
        public int LifetimeCertification { get; set; }
        public int ExpiredCertification { get; set; }
        public int ExpiringSoonCertification { get; set; }
    }

    public class WfpCertificationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? CertificationTypeId { get; set; }
        public string? CertificationTypeCode { get; set; }
        public string? CertificationTypeMasterName { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionCode { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? CredentialingRequirementId { get; set; }
        public string? RequirementCode { get; set; }
        public string CertificationType { get; set; } = string.Empty;
        public string CertificationName { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsLifetime { get; set; }
        public bool IsExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public bool HasFile { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public bool BlocksSchedulingWhenInvalid { get; set; }
        public bool BlocksClinicalServiceWhenInvalid { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpCertificationDetailResponse : WfpCertificationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpCertificationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpCertificationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpCertificationTypeOptionResponse> CertificationTypeOptions { get; set; } = new();
        public List<WfpCertificationStringOptionResponse> VerificationStatusOptions { get; set; } = new();
        public List<WfpCertificationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpCertificationDefaultFilterResponse
    {
        public Guid? CertificationTypeId { get; set; }
        public string? VerificationStatus { get; set; }
        public bool? IsLifetime { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsActive { get; set; }
        public int? ExpiringWithinDays { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "issueDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpCertificationTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public string? IssuingAuthority { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool IsRenewable { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCertificationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCertificationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpCertificationRequest
    {
        public Guid? CertificationTypeId { get; set; }
        public Guid? CredentialingRequirementId { get; set; }

        [MaxLength(50)]
        public string? RequirementCode { get; set; }

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

        public DateTime IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsLifetime { get; set; }

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; }

        [Required]
        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public bool BlocksSchedulingWhenInvalid { get; set; } = true;
        public bool BlocksClinicalServiceWhenInvalid { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpCertificationRequest : CreateWfpCertificationRequest
    {
    }

    public class UpdateWfpCertificationStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class VerifyWfpCertificationRequest
    {
        public bool IsVerified { get; set; } = true;

        [Required]
        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Verified";
    }
}
