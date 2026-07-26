using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxWorkflowAttachment", Schema = "public")]
    public class TrxWorkflowAttachment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public Guid? WorkflowStepInstanceId { get; set; }
        public Guid? ApprovalActionId { get; set; }
        public Guid? WorkflowCommentId { get; set; }
        public Guid? UploadedByUserId { get; set; }
        public Guid? UploadedByWorkforceProfileId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        [MaxLength(100)]
        public string? AttachmentCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsRequesterVisible { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public TrxWorkflowInstance? WorkflowInstance { get; set; }
        public TrxWorkflowStepInstance? WorkflowStepInstance { get; set; }
        public TrxApprovalAction? ApprovalAction { get; set; }
        public TrxWorkflowComment? WorkflowComment { get; set; }
        public ApplicationUser? UploadedByUser { get; set; }
        public MstWorkforceProfile? UploadedByWorkforceProfile { get; set; }
    }
}
