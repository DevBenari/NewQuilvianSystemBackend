using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstTravelClass", Schema = "public")]
    public class MstTravelClass : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TravelClassCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TravelClassName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TransportMode { get; set; } = "Air";
        // Air, Rail, Sea, Road, Accommodation, Other.

        [MaxLength(50)]
        public string? ClassLevel { get; set; }
        // Economy, PremiumEconomy, Business, Executive, Standard, Deluxe.

        public bool IsDomesticAllowed { get; set; } = true;

        public bool IsInternationalAllowed { get; set; } = false;

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstTravelAllowanceRate> AllowanceRates { get; set; }
            = new List<MstTravelAllowanceRate>();
    }
}
