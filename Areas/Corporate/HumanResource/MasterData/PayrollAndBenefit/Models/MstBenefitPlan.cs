using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstBenefitPlan", Schema = "public")]
    public class MstBenefitPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BenefitTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string BenefitPlanCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BenefitPlanName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(100)]
        public string? ExternalPlanCode { get; set; }

        [MaxLength(100)]
        public string? PolicyNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string CoverageType { get; set; } = "Individual";
        // Individual, Family, EmployeeAndSpouse, EmployeeAndChildren.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal? CoverageLimitAmount { get; set; }

        public decimal EmployerContributionAmount { get; set; } = 0m;

        public decimal EmployerContributionPercentage { get; set; } = 0m;

        public decimal EmployeeContributionAmount { get; set; } = 0m;

        public decimal EmployeeContributionPercentage { get; set; } = 0m;

        public int WaitingPeriodMonths { get; set; } = 0;

        public int MaximumDependents { get; set; } = 0;

        public DateTime? EnrollmentStartDate { get; set; }

        public DateTime? EnrollmentEndDate { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstBenefitType? BenefitType { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public ICollection<MstBenefitEligibilityRule> EligibilityRules { get; set; }
            = new List<MstBenefitEligibilityRule>();
    }
}
