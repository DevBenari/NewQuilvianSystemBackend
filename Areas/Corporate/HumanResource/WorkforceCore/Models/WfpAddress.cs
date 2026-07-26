using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpAddress", Schema = "public")]
    public class WfpAddress : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AddressType { get; set; } = "Current";
        // Identity, Current, Domicile, Mailing, Emergency.

        [Required]
        [MaxLength(500)]
        public string AddressLine { get; set; } = string.Empty;

        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? PostalCodeId { get; set; }

        [MaxLength(150)]
        public string? VillageName { get; set; }

        [MaxLength(30)]
        public string? Latitude { get; set; }

        [MaxLength(30)]
        public string? Longitude { get; set; }

        public bool IsPrimary { get; set; } = false;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCountry? Country { get; set; }
        public MstProvince? Province { get; set; }
        public MstCity? City { get; set; }
        public MstDistrict? District { get; set; }
        public MstPostalCode? PostalCode { get; set; }
    }
}
