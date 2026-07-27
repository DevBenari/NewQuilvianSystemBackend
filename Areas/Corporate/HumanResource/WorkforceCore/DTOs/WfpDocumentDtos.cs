using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpDocumentSummaryResponse
    {
        public int TotalDocument { get; set; }
        public int ActiveDocument { get; set; }
        public int InactiveDocument { get; set; }
        public int VerifiedDocument { get; set; }
        public int UnverifiedDocument { get; set; }
        public int ConfidentialDocument { get; set; }
        public int DocumentWithFile { get; set; }
        public int ExpiredDocument { get; set; }
        public int ExpiringWithin30Days { get; set; }
    }

    public class WfpDocumentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string? RequirementCode { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public string? FileContentType { get; set; }
        public string? OriginalFileName { get; set; }
        public string? StoredFileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? FileChecksum { get; set; }
        public bool HasFile { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringWithin30Days { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByName { get; set; }
        public string? VerificationNote { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpDocumentDetailResponse : WfpDocumentResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpDocumentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;
        public string MaximumFileSizeLabel { get; set; } = "10 MB";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpDocumentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpDocumentStringOptionResponse> DocumentTypeOptions { get; set; } = new();
        public List<WfpDocumentStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpDocumentStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> AllowedFileExtensions { get; set; } = new();
        public List<string> AllowedContentTypes { get; set; } = new();
    }

    public class WfpDocumentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? DocumentType { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsConfidential { get; set; }
        public bool? HasFile { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpDocumentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpDocumentRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(200)]
        public string? IssuingAuthority { get; set; }

        public IFormFile? File { get; set; }
        public bool IsConfidential { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpDocumentRequest : CreateWfpDocumentRequest
    {
        public bool ReplaceExistingFile { get; set; }
    }

    public class UpdateWfpDocumentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWfpDocumentRequest
    {
        public bool IsVerified { get; set; } = true;

        [MaxLength(500)]
        public string? VerificationNote { get; set; }
    }

    public class DeleteWfpDocumentFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
