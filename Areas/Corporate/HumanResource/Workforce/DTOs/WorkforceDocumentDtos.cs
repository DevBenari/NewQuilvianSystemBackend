using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
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

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceDocumentListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int DocumentWithFileData { get; set; }

        public List<WorkforceDocumentResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceDocumentRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

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

    public class UpdateWorkforceDocumentRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceDocumentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceDocumentRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
