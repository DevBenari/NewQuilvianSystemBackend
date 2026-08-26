using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    public class BilChargeComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ChargeLineId { get; set; }

        public string ComponentKey { get; set; } = string.Empty;

        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }

        public string? TariffSnapshot { get; set; }

        public string? RuleSnapshot { get; set; }

        public string? RoundingSnapshot { get; set; }

        public decimal? CalculatedAmount { get; set; }

        public int CalculationVersion { get; set; } = 1;

        public BilChargeLine ChargeLine { get; set; } = null!;
    }
}
