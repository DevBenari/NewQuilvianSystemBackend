using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
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

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCertificationListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int CertificationWithFileData { get; set; }

        public List<WorkforceCertificationResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceCertificationRequest
    {
        [MaxLength(100)]
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

    public class UpdateWorkforceCertificationRequest
    {
        [MaxLength(100)]
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

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; } = false;

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCertificationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceCertificationRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
