using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public enum WorkforceDocumentType
    {
        Unknown = 0,
        KTP = 1,
        NPWP = 2,
        KK = 3,
        CONTRACT = 4,
        NDA = 5,
        PASSPORT = 6,
        IJAZAH = 7,
        SKCK = 8,
        BPJS_KESEHATAN = 9,
        BPJS_KETENAGAKERJAAN = 10,
        OTHER = 99
    }

    public class WorkforceDocumentSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalDocument { get; set; }
        public int ActiveDocument { get; set; }
        public int InactiveDocument { get; set; }
        public int VerifiedDocument { get; set; }
        public int UnverifiedDocument { get; set; }
        public int ExpiredDocument { get; set; }
        public int DocumentWithFile { get; set; }
        public int DocumentWithoutFile { get; set; }
        public int KtpDocument { get; set; }
        public int NpwpDocument { get; set; }
        public int ContractDocument { get; set; }
        public int PassportDocument { get; set; }
    }

    public class WorkforceDocumentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? RequirementCode { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
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

    public class WorkforceDocumentDetailResponse : WorkforceDocumentResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WorkforceDocumentOptionResponse
    {
        public Guid Id { get; set; }
        public string? RequirementCode { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool HasFile { get; set; }
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
    }

    public class WorkforceDocumentOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceDocumentOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceDocumentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai CreateDateTime dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public string CodeInfo { get; set; } = "RequirementCode dibuat otomatis oleh backend dengan format DOC-RSMMC-00001 dan tidak perlu dikirim dari frontend.";
        public WorkforceDocumentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceDocumentCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceDocumentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceDocumentTypeOptionResponse> DocumentTypeOptions { get; set; } = new();
        public List<string> PreviewSupportedContentTypes { get; set; } = new()
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };
        public WorkforceDocumentFileUploadInfoResponse FileUploadInfo { get; set; } = new();
        public List<string> FrontendGuide { get; set; } = new();
    }

    public class WorkforceDocumentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public WorkforceDocumentType? DocumentType { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceDocumentCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceDocumentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceDocumentTypeOptionResponse
    {
        public WorkforceDocumentType Value { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsuallyRequiresDocumentNumber { get; set; }
        public bool UsuallyHasExpiryDate { get; set; }
    }

    public class WorkforceDocumentFileUploadInfoResponse
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

    public class CreateWorkforceDocumentRequest
    {
        [Required]
        public WorkforceDocumentType DocumentType { get; set; } = WorkforceDocumentType.Unknown;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceDocumentRequest : CreateWorkforceDocumentRequest
    {
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceDocumentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceDocumentRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class DeleteWorkforceDocumentFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
