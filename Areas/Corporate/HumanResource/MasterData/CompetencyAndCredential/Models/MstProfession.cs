using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstProfession", Schema = "public")]
    public class MstProfession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ProfessionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ProfessionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ProfessionGroup { get; set; } = "General";
        // Medical, Nursing, AlliedHealth, Pharmacy, Administration, Technical, General.

        public bool IsClinicalProfession { get; set; } = false;

        public bool RequiresCredentialing { get; set; } = false;

        public bool RequiresLicense { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstSpecialization> Specializations { get; set; }
            = new List<MstSpecialization>();

        public ICollection<MstCertificationType> CertificationTypes { get; set; }
            = new List<MstCertificationType>();

        public ICollection<MstLicenseType> LicenseTypes { get; set; }
            = new List<MstLicenseType>();
    }
}
