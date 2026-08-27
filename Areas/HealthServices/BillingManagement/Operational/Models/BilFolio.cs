using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    public class BilFolio : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EncounterId { get; set; }

        public BillingFolioStatus Status { get; set; } = BillingFolioStatus.Open;

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public ICollection<BilChargeLine> ChargeLines { get; set; } = new List<BilChargeLine>();
    }
}
