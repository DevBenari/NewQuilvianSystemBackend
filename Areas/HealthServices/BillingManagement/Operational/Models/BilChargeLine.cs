using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    public class BilChargeLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FolioId { get; set; }

        public string SourceContext { get; set; } = string.Empty;

        public Guid SourceAggregateId { get; set; }

        public Guid? SourceItemId { get; set; }

        public Guid MilestoneFactId { get; set; }

        public int MilestoneFactVersion { get; set; }

        public string EffectType { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public BillingChargeCalculationStatus CalculationStatus { get; set; } =
            BillingChargeCalculationStatus.Received;

        public string? Currency { get; set; }

        public decimal? GrossAmount { get; set; }

        public decimal? EligibleAmount { get; set; }

        public string? ReviewReasonCode { get; set; }

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public BilFolio Folio { get; set; } = null!;

        public ICollection<BilChargeComponent> Components { get; set; } =
            new List<BilChargeComponent>();
    }
}
