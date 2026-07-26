using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstPayrollComponent", Schema = "public")]
    public class MstPayrollComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollComponentCategoryId { get; set; }

        public Guid? BaseComponentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PayrollComponentCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PayrollComponentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ComponentType { get; set; } = "Earning";
        // Earning, Deduction, EmployerContribution, Information.

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Fixed";
        // Fixed, Percentage, Formula, ManualInput, Attendance, Overtime, Benefit.

        [MaxLength(1000)]
        public string? FormulaExpression { get; set; }

        public decimal DefaultAmount { get; set; } = 0m;

        public decimal DefaultPercentage { get; set; } = 0m;

        public bool IsRecurring { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsProrated { get; set; } = true;

        public bool IsAttendanceBased { get; set; } = false;

        public bool IsOvertimeBased { get; set; } = false;

        public bool IsBenefitBased { get; set; } = false;

        public bool IsEmployerContribution { get; set; } = false;

        public bool IsEmployeeContribution { get; set; } = false;

        public bool IsDisplayedOnPayslip { get; set; } = true;

        public bool IsEditableDuringPayroll { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstPayrollComponentCategory? PayrollComponentCategory { get; set; }

        public MstPayrollComponent? BaseComponent { get; set; }

        public ICollection<MstPayrollComponent> DerivedComponents { get; set; }
            = new List<MstPayrollComponent>();

        public ICollection<MstAllowanceType> AllowanceTypes { get; set; }
            = new List<MstAllowanceType>();

        public ICollection<MstDeductionType> DeductionTypes { get; set; }
            = new List<MstDeductionType>();
    }
}
