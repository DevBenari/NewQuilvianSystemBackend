using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpEducationSummaryResponse
    {
        public int TotalEducation { get; set; }
        public int ActiveEducation { get; set; }
        public int InactiveEducation { get; set; }
        public int HighestEducation { get; set; }
        public int VerifiedEducation { get; set; }
        public int UnverifiedEducation { get; set; }
        public int EducationWithCertificate { get; set; }
        public int EducationWithFile { get; set; }
    }

    public class WfpEducationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string? RequirementCode { get; set; }
        public string EducationLevel { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string? Major { get; set; }
        public int? GraduationYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CountryId { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? CertificateNumber { get; set; }
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public string? FileContentType { get; set; }
        public bool HasFile { get; set; }
        public bool IsHighestEducation { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpEducationDetailResponse : WfpEducationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpEducationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;
        public string MaximumFileSizeLabel { get; set; } = "10 MB";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpEducationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpEducationStringOptionResponse> EducationLevelOptions { get; set; } = new();
        public List<WfpEducationCountryOptionResponse> CountryOptions { get; set; } = new();
        public List<WfpEducationStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> AllowedFileExtensions { get; set; } = new();
        public List<string> AllowedContentTypes { get; set; } = new();
    }

    public class WfpEducationDefaultFilterResponse
    {
        public string? EducationLevel { get; set; }
        public Guid? CountryId { get; set; }
        public bool? IsHighestEducation { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "isHighestEducation";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpEducationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpEducationCountryOptionResponse
    {
        public Guid Id { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpEducationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Major { get; set; }

        [Range(1900, 2200)]
        public int? GraduationYear { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CountryId { get; set; }

        [MaxLength(150)]
        public string? CertificateNumber { get; set; }

        public IFormFile? File { get; set; }
        public bool IsHighestEducation { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpEducationRequest : CreateWfpEducationRequest
    {
        public bool ReplaceExistingFile { get; set; }
    }

    public class UpdateWfpEducationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWfpEducationHighestRequest
    {
        public bool IsHighestEducation { get; set; } = true;
    }

    public class VerifyWfpEducationRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class DeleteWfpEducationFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
