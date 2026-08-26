using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs
{
    public class RecognizeBillingMilestoneRequest
    {
        [Required]
        [MaxLength(128)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [Required]
        public Guid MilestoneFactId { get; set; }

        [Range(1, int.MaxValue)]
        public int MilestoneFactVersion { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SourceContext { get; set; } = string.Empty;

        [Required]
        public Guid SourceAggregateId { get; set; }

        public Guid? SourceItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EffectType { get; set; } = string.Empty;

        [Required]
        public DateTime OccurredAt { get; set; }

        [Range(typeof(decimal), "0.000001", "999999999999.999999")]
        public decimal? Quantity { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        public string? TariffSnapshot { get; set; }

        public string? RuleSnapshot { get; set; }

        public string? RoundingSnapshot { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? CausationId { get; set; }
    }

    public class BillingContractErrorResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    public class RecognizeBillingMilestoneResponse
    {
        public Guid ProcessingEffectId { get; set; }

        public bool IsReplay { get; set; }

        public Guid? FolioId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public BillingProcessingOutcome Outcome { get; set; }

        public BillingChargeCalculationStatus? CalculationStatus { get; set; }

        public List<BillingContractErrorResponse> Errors { get; set; } = new();

        public int Version { get; set; }
    }

    public class BillingChargeComponentResponse
    {
        public Guid Id { get; set; }

        public string ComponentKey { get; set; } = string.Empty;

        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }

        public string? TariffSnapshot { get; set; }

        public string? RuleSnapshot { get; set; }

        public string? RoundingSnapshot { get; set; }

        public decimal? CalculatedAmount { get; set; }

        public int CalculationVersion { get; set; }
    }

    public class BillingChargeLineResponse
    {
        public Guid Id { get; set; }

        public string SourceContext { get; set; } = string.Empty;

        public Guid SourceAggregateId { get; set; }

        public Guid? SourceItemId { get; set; }

        public Guid MilestoneFactId { get; set; }

        public int MilestoneFactVersion { get; set; }

        public string EffectType { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public BillingChargeCalculationStatus CalculationStatus { get; set; }

        public string? Currency { get; set; }

        public decimal? GrossAmount { get; set; }

        public decimal? EligibleAmount { get; set; }

        public string? ReviewReasonCode { get; set; }

        public int Version { get; set; }

        public List<BillingChargeComponentResponse> Components { get; set; } = new();
    }

    public class BillingFolioDetailResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public BillingFolioStatus Status { get; set; }

        public int Version { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public List<BillingChargeLineResponse> ChargeLines { get; set; } = new();
    }
}
