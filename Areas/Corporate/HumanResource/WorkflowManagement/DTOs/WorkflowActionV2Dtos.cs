using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs
{
    public class WorkflowRejectRequest
    {
        public Guid RejectionReasonId { get; set; }

        [MaxLength(4000)]
        public string? Comment { get; set; }

        public List<Guid> AttachmentIds { get; set; } = new();

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowRequestRevisionRequest
    {
        public Guid? RejectionReasonId { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Comment { get; set; } = string.Empty;

        public List<Guid> AttachmentIds { get; set; } = new();

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowReturnRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReturnToStepCode { get; set; } = string.Empty;

        public Guid? RejectionReasonId { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Comment { get; set; } = string.Empty;

        public List<Guid> AttachmentIds { get; set; } = new();

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowCancelRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowWithdrawRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowVerifyRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowAcknowledgeRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
