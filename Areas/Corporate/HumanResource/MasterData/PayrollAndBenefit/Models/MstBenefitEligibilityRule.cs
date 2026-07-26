using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstBenefitEligibilityRule", Schema = "public")]
    public class MstBenefitEligibilityRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BenefitPlanId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        public Guid? EmployeeGradeId { get; set; }

        public Guid? SalaryGradeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EligibilityRuleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EligibilityRuleName { get; set; } = string.Empty;

        public int MinimumServiceMonths { get; set; } = 0;

        public int? MinimumAge { get; set; }

        public int? MaximumAge { get; set; }

        public bool AllowProbationEmployee { get; set; } = false;

        public bool AllowContractEmployee { get; set; } = true;

        public bool RequireFullTimeEmployment { get; set; } = false;

        public decimal MinimumWeeklyHours { get; set; } = 0m;

        public int CoverageStartOffsetDays { get; set; } = 0;

        public int CoverageEndAfterTerminationDays { get; set; } = 0;

        public bool RequireManagerApproval { get; set; } = false;

        public bool RequireHrVerification { get; set; } = true;

        public int Priority { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstBenefitPlan? BenefitPlan { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public MstEmployeeGrade? EmployeeGrade { get; set; }

        public MstSalaryGrade? SalaryGrade { get; set; }
    }
}
