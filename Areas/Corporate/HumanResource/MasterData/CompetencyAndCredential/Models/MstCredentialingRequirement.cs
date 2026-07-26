using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstCredentialingRequirement", Schema = "public")]
    public class MstCredentialingRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ProfessionId { get; set; }

        public Guid? SpecializationId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? CompetencyId { get; set; }

        public Guid? TrainingCatalogId { get; set; }

        public Guid? CertificationTypeId { get; set; }

        public Guid? LicenseTypeId { get; set; }

        public Guid? ClinicalPrivilegeCatalogId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RequirementName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RequirementType { get; set; } = "Document";
        // Document, Competency, Training, Certification, License, Experience, ClinicalPrivilege.

        public CompetencyLevel? MinimumCompetencyLevel { get; set; }

        public int MinimumExperienceMonths { get; set; } = 0;

        public int RequiredQuantity { get; set; } = 1;

        public int? ValidityMonths { get; set; }

        public bool IsMandatory { get; set; } = true;

        public bool RequiresDocument { get; set; } = true;

        public bool RequiresVerification { get; set; } = true;

        public bool RequiresExpiryDate { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProfession? Profession { get; set; }

        public MstSpecialization? Specialization { get; set; }

        public MstPosition? Position { get; set; }

        public MstCompetency? Competency { get; set; }

        public MstTrainingCatalog? TrainingCatalog { get; set; }

        public MstCertificationType? CertificationType { get; set; }

        public MstLicenseType? LicenseType { get; set; }

        public MstClinicalPrivilegeCatalog? ClinicalPrivilegeCatalog { get; set; }
    }
}
