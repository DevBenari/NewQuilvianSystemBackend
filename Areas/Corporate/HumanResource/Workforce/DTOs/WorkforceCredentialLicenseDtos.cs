using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
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

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

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

    public class WorkforceCredentialLicenseListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int PendingVerificationData { get; set; }

        public int RejectedData { get; set; }

        public int RevokedData { get; set; }

        public int ExpiredData { get; set; }

        public int CurrentlyValidData { get; set; }

        public int CredentialLicenseWithFileData { get; set; }

        public List<WorkforceCredentialLicenseResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceCredentialLicenseRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;

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

    public class UpdateWorkforceCredentialLicenseRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;

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

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCredentialLicenseStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
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
}
