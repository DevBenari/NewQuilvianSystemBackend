using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstLicenseType", Schema = "public")]
    public class MstLicenseType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ProfessionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string LicenseTypeName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? IssuingAuthority { get; set; }

        [MaxLength(200)]
        public string? RegulatoryBody { get; set; }

        public int? DefaultValidityMonths { get; set; }

        public bool RequiresExpiryDate { get; set; } = true;

        public bool IsRenewable { get; set; } = true;

        public bool RequiresDocument { get; set; } = true;

        public bool RequiresVerification { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProfession? Profession { get; set; }
    }
}
