using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstClinicalPrivilegeCatalog", Schema = "public")]
    public class MstClinicalPrivilegeCatalog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ProfessionId { get; set; }

        public Guid? SpecializationId { get; set; }

        public Guid? RequiredCompetencyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string PrivilegeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PrivilegeCategory { get; set; } = "ClinicalProcedure";

        [MaxLength(100)]
        public string? ReferenceProcedureCode { get; set; }

        public CompetencyLevel? MinimumCompetencyLevel { get; set; }

        public int MinimumExperienceMonths { get; set; } = 0;

        public bool RequiresSupervision { get; set; } = false;

        public bool AllowsIndependentPractice { get; set; } = true;

        public bool IsHighRisk { get; set; } = false;

        public int? DefaultValidityMonths { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProfession? Profession { get; set; }

        public MstSpecialization? Specialization { get; set; }

        public MstCompetency? RequiredCompetency { get; set; }
    }
}
