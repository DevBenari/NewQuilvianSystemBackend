using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstSalaryStructure", Schema = "public")]
    public class MstSalaryStructure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SalaryGradeId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SalaryStructureCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SalaryStructureName { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required]
        [MaxLength(50)]
        public string PaymentFrequency { get; set; } = "Monthly";
        // Monthly, BiWeekly, Weekly, Daily.

        public decimal DefaultBaseSalary { get; set; } = 0m;

        public decimal? MinimumBaseSalary { get; set; }

        public decimal? MaximumBaseSalary { get; set; }

        public decimal StandardWorkingDaysPerMonth { get; set; } = 22m;

        public decimal StandardWorkingHoursPerMonth { get; set; } = 173m;

        public bool IsProrated { get; set; } = true;

        public bool IncludeOvertime { get; set; } = true;

        public bool IncludeShiftAllowance { get; set; } = true;

        public bool IncludeOnCallAllowance { get; set; } = true;

        public bool IncludeHazardAllowance { get; set; } = true;

        public bool IncludeBenefitDeduction { get; set; } = true;

        public string? ComponentConfigurationJson { get; set; }
        // Snapshot konfigurasi komponen untuk tahap awal. Pada pengembangan lanjut
        // dapat dipecah menjadi MstSalaryStructureComponent.

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstSalaryGrade? SalaryGrade { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }
    }
}
