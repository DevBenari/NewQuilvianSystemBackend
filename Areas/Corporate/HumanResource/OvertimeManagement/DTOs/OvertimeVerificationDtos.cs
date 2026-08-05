using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class StartOvertimeVerificationRequest
    {
        [MaxLength(40)]
        public string VerificationType { get; set; } = "HR";

        [MaxLength(2000)]
        public string? Comments { get; set; }

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class OvertimeVerificationDetailAdjustmentRequest
    {
        [Required]
        public Guid RealizationDetailId { get; set; }

        [Range(0, int.MaxValue)]
        public int VerifiedMinutes { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class ApproveOvertimeVerificationRequest
    {
        [MaxLength(2000)]
        public string? Comments { get; set; }

        [MaxLength(2000)]
        public string? AdjustmentReason { get; set; }

        public List<OvertimeVerificationDetailAdjustmentRequest> DetailAdjustments { get; set; }
            = new();

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class RequestOvertimeVerificationRevisionRequest
    {
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Comments { get; set; }

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class RejectOvertimeVerificationRequest
    {
        [Required]
        public Guid RejectionReasonId { get; set; }

        [Required, MaxLength(2000)]
        public string Comments { get; set; } = string.Empty;

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class OvertimeVerificationMutationResponse
    {
        public Guid OvertimeVerificationId { get; set; }
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public int RealizationVersion { get; set; }
        public string RealizationStatus { get; set; } = string.Empty;
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string VerificationType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public int SubmittedMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VerifiedMinutes { get; set; }
        public int AdjustmentMinutes { get; set; }
        public bool HasVariance { get; set; }
        public bool RequiresRevision { get; set; }
        public bool IsFinalVerification { get; set; }
        public bool IsIdempotentResult { get; set; }
        public DateTime? ActionAt { get; set; }
    }
}
