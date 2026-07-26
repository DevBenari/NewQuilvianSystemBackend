using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstMandatoryTrainingRule", Schema = "public")]
    public class MstMandatoryTrainingRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TrainingCatalogId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? ProfessionId { get; set; }

        public Guid? SpecializationId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RuleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } = string.Empty;

        public int CompletionDueDaysFromJoin { get; set; } = 0;

        public int? RecurrenceMonths { get; set; }

        public int GracePeriodDays { get; set; } = 0;

        public bool IsRequiredBeforeAssignment { get; set; } = false;

        public bool IsRequiredForCredentialing { get; set; } = false;

        public bool IsRequiredBeforeIndependentPractice { get; set; } = false;

        public bool RequiresPassingResult { get; set; } = false;

        public decimal? MinimumPassingScore { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int Priority { get; set; } = 0;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstTrainingCatalog? TrainingCatalog { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstPosition? Position { get; set; }

        public MstProfession? Profession { get; set; }

        public MstSpecialization? Specialization { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }
    }
}
