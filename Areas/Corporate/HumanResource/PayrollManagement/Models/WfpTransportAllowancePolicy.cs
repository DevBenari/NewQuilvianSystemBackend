using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("WfpTransportAllowancePolicy", Schema = "public")]
    public class WfpTransportAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollComponentId { get; set; }

        [Required, MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string CalculationMethod { get; set; } = "FixedMonthly";
        // FixedMonthly, PerAttendance, PerDay, Reimbursement.

        public decimal FixedMonthlyAmount { get; set; } = 0m;
        public decimal PerAttendanceAmount { get; set; } = 0m;
        public decimal DailyLimitAmount { get; set; } = 0m;
        public decimal MonthlyLimitAmount { get; set; } = 0m;
        public int MinimumAttendanceMinutes { get; set; } = 0;

        public bool IsAttendanceBased { get; set; } = false;
        public bool IsProrated { get; set; } = true;
        public bool IsTaxable { get; set; } = true;
        public bool IsIncludedInPayroll { get; set; } = true;
        public bool IncludeBusinessTravelDay { get; set; } = false;
        public bool IncludePaidLeaveDay { get; set; } = false;
        public bool IncludeHoliday { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }

        public ICollection<WfpTransportAllowance> TransportAllowances { get; set; }
            = new List<WfpTransportAllowance>();
    }
}
