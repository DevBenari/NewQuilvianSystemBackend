using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs
{
    public class WorkflowCommentFilterMetadataResponse
    {
        public List<WorkflowStringOptionResponse> CommentTypeOptions { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WorkflowCommentListResponse
    {
        public Guid Id { get; set; }

        public Guid WorkflowInstanceId { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public string? WorkflowStepCode { get; set; }

        public string? WorkflowStepName { get; set; }

        public Guid? ParentCommentId { get; set; }

        public string CommentType { get; set; } = string.Empty;

        public string CommentText { get; set; } = string.Empty;

        public DateTime CommentedAt { get; set; }

        public Guid? CommentByUserId { get; set; }

        public Guid? CommentByWorkforceProfileId { get; set; }

        public string? CommentByName { get; set; }

        public bool IsRequesterVisible { get; set; }

        public bool IsInternalComment { get; set; }

        public bool IsSystemGenerated { get; set; }

        public int ReplyCount { get; set; }

        public int AttachmentCount { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }
    }

    public class CreateWorkflowCommentRequest
    {
        public Guid? WorkflowStepInstanceId { get; set; }

        public Guid? ParentCommentId { get; set; }

        [MaxLength(40)]
        public string CommentType { get; set; } = "General";

        [Required]
        [MaxLength(5000)]
        public string CommentText { get; set; } = string.Empty;

        public bool IsRequesterVisible { get; set; } = true;

        public bool IsInternalComment { get; set; } = false;
    }

    public class UpdateWorkflowCommentRequest
    {
        [Required]
        [MaxLength(5000)]
        public string CommentText { get; set; } = string.Empty;

        public bool IsRequesterVisible { get; set; } = true;

        public bool IsInternalComment { get; set; } = false;
    }

    public class WorkflowAttachmentFilterMetadataResponse
    {
        public long MaximumFileSizeBytes { get; set; }

        public List<string> AllowedExtensions { get; set; } = new();

        public List<WorkflowStringOptionResponse> AttachmentCategoryOptions { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WorkflowAttachmentListResponse
    {
        public Guid Id { get; set; }

        public Guid WorkflowInstanceId { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public string? WorkflowStepCode { get; set; }

        public string? WorkflowStepName { get; set; }

        public Guid? ApprovalActionId { get; set; }

        public string? ApprovalActionType { get; set; }

        public Guid? WorkflowCommentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        public string? FileChecksum { get; set; }

        public string? AttachmentCategory { get; set; }

        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; }

        public Guid? UploadedByUserId { get; set; }

        public Guid? UploadedByWorkforceProfileId { get; set; }

        public string? UploadedByName { get; set; }

        public bool IsRequesterVisible { get; set; }

        public bool IsConfidential { get; set; }

        public string DownloadUrl { get; set; } = string.Empty;

        public bool CanDelete { get; set; }
    }

    public class UploadWorkflowAttachmentRequest
    {
        [Required]
        public IFormFile? File { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public Guid? ApprovalActionId { get; set; }

        public Guid? WorkflowCommentId { get; set; }

        [MaxLength(100)]
        public string? AttachmentCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsRequesterVisible { get; set; } = true;

        public bool IsConfidential { get; set; } = false;
    }

    public class WorkflowAttachmentDownloadResponse
    {
        public string PhysicalPath { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = "application/octet-stream";
    }

    public class WorkflowAttachmentStoredFileResponse
    {
        public string RelativePath { get; set; } = string.Empty;

        public string Checksum { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public string ContentType { get; set; } = "application/octet-stream";
    }
}
