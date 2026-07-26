using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstOnCallAllowancePolicy", Schema = "public")]
    public class MstOnCallAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AllowanceTypeId { get; set; }

        public Guid? OnCallTypeId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OnCallAllowancePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string OnCallAllowancePolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "FixedPerAssignment";
        // FixedPerAssignment, PerHour, PerDay, PerActualCall, PercentageOfBaseSalary.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal BaseRateAmount { get; set; } = 0m;

        public decimal ActualCallRateAmount { get; set; } = 0m;

        public decimal HourlyRateAmount { get; set; } = 0m;

        public decimal PercentageOfBaseSalary { get; set; } = 0m;

        public int MinimumOnCallHours { get; set; } = 0;

        public decimal? MaximumAmountPerPeriod { get; set; }

        public decimal WeekendMultiplier { get; set; } = 1m;

        public decimal HolidayMultiplier { get; set; } = 1m;

        public bool RequireAttendanceEvidence { get; set; } = false;

        public bool RequireSupervisorVerification { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Priority { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstAllowanceType? AllowanceType { get; set; }

        public MstOnCallType? OnCallType { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }
    }
}
