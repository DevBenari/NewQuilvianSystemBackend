using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public enum WorkforceEducationLevel
    {
        Unknown = 0,
        SMA = 1,
        SMK = 2,
        D1 = 3,
        D2 = 4,
        D3 = 5,
        D4 = 6,
        S1 = 7,
        PROFESSION = 8,
        S2 = 9,
        S3 = 10,
        SPECIALIST_1 = 11,
        SPECIALIST_2 = 12,
        OTHER = 99
    }

    public class WorkforceEducationSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalEducation { get; set; }
        public int ActiveEducation { get; set; }
        public int InactiveEducation { get; set; }
        public int VerifiedEducation { get; set; }
        public int UnverifiedEducation { get; set; }
        public int EducationWithFile { get; set; }
        public int EducationWithoutFile { get; set; }
        public int DiplomaEducation { get; set; }
        public int BachelorEducation { get; set; }
        public int ProfessionEducation { get; set; }
        public int MasterEducation { get; set; }
        public int DoctoralEducation { get; set; }
        public int SpecialistEducation { get; set; }
    }

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
        public bool HasFile { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FilePreviewUrl { get; set; }
        public string? FileDownloadUrl { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceEducationDetailResponse : WorkforceEducationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceEducationOptionResponse
    {
        public Guid Id { get; set; }
        public string? RequirementCode { get; set; }
        public string EducationLevel { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string? Major { get; set; }
        public int? GraduationYear { get; set; }
        public string? CertificateNumber { get; set; }
        public bool HasFile { get; set; }
        public bool IsVerified { get; set; }
    }

    public class WorkforceEducationOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceEducationOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceEducationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai CreateDateTime dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public string CodeInfo { get; set; } = "RequirementCode dibuat otomatis oleh backend dengan format EDU-RSMMC-00001 dan tidak perlu dikirim dari frontend.";
        public WorkforceEducationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceEducationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceEducationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceEducationLevelOptionResponse> EducationLevelOptions { get; set; } = new();
        public List<string> PreviewSupportedContentTypes { get; set; } = new()
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };
        public WorkforceEducationFileUploadInfoResponse FileUploadInfo { get; set; } = new();
        public List<string> FrontendGuide { get; set; } = new();
    }

    public class WorkforceEducationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public WorkforceEducationLevel? EducationLevel { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceEducationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceEducationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceEducationLevelOptionResponse
    {
        public WorkforceEducationLevel Value { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsuallyRequiresCertificateNumber { get; set; }
    }

    public class WorkforceEducationFileUploadInfoResponse
    {
        public int MaxFileSizeMb { get; set; } = 10;
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
        public string PreviewInfo { get; set; } = "PDF dan image bisa dipreview langsung. DOC/DOCX/XLS/XLSX biasanya perlu viewer khusus atau fallback download.";
    }

    public class CreateWorkforceEducationRequest
    {
        [Required]
        public WorkforceEducationLevel EducationLevel { get; set; } = WorkforceEducationLevel.Unknown;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Major { get; set; }

        [Range(1900, 3000)]
        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceEducationRequest : CreateWorkforceEducationRequest
    {
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceEducationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceEducationRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class DeleteWorkforceEducationFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
