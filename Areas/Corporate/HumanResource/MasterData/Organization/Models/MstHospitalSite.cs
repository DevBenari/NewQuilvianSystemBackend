using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstHospitalSite", Schema = "public")]
    public class MstHospitalSite : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SiteCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string SiteName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SiteType { get; set; } = "Hospital";
        // Hospital, Clinic, Laboratory, Office, Warehouse, TrainingCenter, Other.

        [MaxLength(100)]
        public string? AccreditationNumber { get; set; }

        [MaxLength(100)]
        public string? TimeZoneId { get; set; } = "Asia/Jakarta";

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsMainSite { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstCountry? Country { get; set; }

        public MstProvince? Province { get; set; }

        public MstCity? City { get; set; }

        public MstDistrict? District { get; set; }

        public MstPostalCode? PostalCode { get; set; }

        public ICollection<MstOrganizationUnit> OrganizationUnits { get; set; }
            = new List<MstOrganizationUnit>();

        public ICollection<MstCostCenter> CostCenters { get; set; }
            = new List<MstCostCenter>();

        public ICollection<MstWorkLocation> WorkLocations { get; set; }
            = new List<MstWorkLocation>();
    }
}
