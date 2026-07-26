using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstHazardAllowancePolicy", Schema = "public")]
    public class MstHazardAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AllowanceTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? WorkLocationId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string HazardAllowancePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string HazardAllowancePolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HazardLevel { get; set; } = "Low";
        // Low, Medium, High, Critical.

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "FixedMonthly";
        // FixedMonthly, PerDay, PerShift, PercentageOfBaseSalary.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal RateAmount { get; set; } = 0m;

        public decimal PercentageOfBaseSalary { get; set; } = 0m;

        public int MinimumExposureDays { get; set; } = 0;

        public decimal? MaximumAmountPerPeriod { get; set; }

        public bool RequireOccupationalHealthClearance { get; set; } = true;

        public bool RequireActiveAssignment { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Priority { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstAllowanceType? AllowanceType { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstWorkLocation? WorkLocation { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }
    }
}
