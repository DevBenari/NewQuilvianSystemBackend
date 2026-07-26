using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstLegalEntity", Schema = "public")]
    public class MstLegalEntity : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string LegalEntityCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string LegalEntityName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? TaxIdentificationNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessRegistrationNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public ICollection<MstHospitalSite> HospitalSites { get; set; }
            = new List<MstHospitalSite>();

        public ICollection<MstOrganizationUnit> OrganizationUnits { get; set; }
            = new List<MstOrganizationUnit>();

        public ICollection<MstCostCenter> CostCenters { get; set; }
            = new List<MstCostCenter>();

        public ICollection<MstWorkLocation> WorkLocations { get; set; }
            = new List<MstWorkLocation>();
    }
}
