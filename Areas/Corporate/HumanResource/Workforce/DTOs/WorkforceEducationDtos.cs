using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceEducationResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string EducationLevel { get; set; } = string.Empty;

        public string InstitutionName { get; set; } = string.Empty;

        public string? Major { get; set; }

        public int? GraduationYear { get; set; }

        public string? CertificateNumber { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceEducationListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int EducationWithFileData { get; set; }

        public List<WorkforceEducationResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceEducationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Major { get; set; }

        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceEducationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Major { get; set; }

        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceEducationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceEducationRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
