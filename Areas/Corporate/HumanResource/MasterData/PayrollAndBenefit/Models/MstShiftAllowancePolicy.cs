using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstShiftAllowancePolicy", Schema = "public")]
    public class MstShiftAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AllowanceTypeId { get; set; }

        public Guid? ShiftId { get; set; }

        public Guid? ShiftGroupId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShiftAllowancePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ShiftAllowancePolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "FixedPerShift";
        // FixedPerShift, PerHour, PercentageOfBaseSalary.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal RateAmount { get; set; } = 0m;

        public decimal PercentageOfBaseSalary { get; set; } = 0m;

        public int MinimumEligibleMinutes { get; set; } = 0;

        public decimal? MaximumAmountPerPeriod { get; set; }

        public bool ApplyOnWorkday { get; set; } = true;

        public bool ApplyOnWeekend { get; set; } = true;

        public bool ApplyOnHoliday { get; set; } = true;

        public bool ApplyOnlyNightShift { get; set; } = false;

        public bool RequireAttendanceMatch { get; set; } = true;

        public bool RequireCompletedShift { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Priority { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstAllowanceType? AllowanceType { get; set; }

        public MstShift? Shift { get; set; }

        public MstShiftGroup? ShiftGroup { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }
    }
}
