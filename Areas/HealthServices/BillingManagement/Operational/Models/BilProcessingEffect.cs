using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    public class BilProcessingEffect : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Consumer { get; set; } = string.Empty;

        public string OperationType { get; set; } = string.Empty;

        public string IdempotencyKey { get; set; } = string.Empty;

        public string RequestFingerprint { get; set; } = string.Empty;

        public string SourceContext { get; set; } = string.Empty;

        public Guid MilestoneFactId { get; set; }

        public int MilestoneFactVersion { get; set; }

        public string EffectType { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public BillingProcessingOutcome Outcome { get; set; } = BillingProcessingOutcome.Received;

        public Guid? FolioId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public BillingChargeCalculationStatus? CalculationStatus { get; set; }

        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? CausationId { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
