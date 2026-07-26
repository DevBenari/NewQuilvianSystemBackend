using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstSpecialization", Schema = "public")]
    public class MstSpecialization : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProfessionId { get; set; }

        public Guid? ParentSpecializationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SpecializationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string SpecializationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SpecializationType { get; set; } = "Specialization";
        // Specialization, SubSpecialization, Expertise, PracticeArea.

        public bool IsClinicalSpecialization { get; set; } = true;

        public bool RequiresCredentialing { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProfession? Profession { get; set; }

        public MstSpecialization? ParentSpecialization { get; set; }

        public ICollection<MstSpecialization> ChildSpecializations { get; set; }
            = new List<MstSpecialization>();
    }
}
