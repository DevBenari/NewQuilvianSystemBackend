using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstTravelDestinationZone", Schema = "public")]
    public class MstTravelDestinationZone : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DestinationZoneCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DestinationZoneName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ZoneType { get; set; } = "Domestic";
        // Domestic, International, Remote, HighCost, SpecialRisk.

        public decimal? DistanceFromBaseKilometers { get; set; }

        [MaxLength(50)]
        public string? RiskLevel { get; set; }
        // Low, Medium, High, Critical.

        public bool IsDomestic { get; set; } = true;

        public bool RequiresSpecialApproval { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstCountry? Country { get; set; }

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        public ICollection<MstTravelAllowanceRate> AllowanceRates { get; set; }
            = new List<MstTravelAllowanceRate>();
    }
}
