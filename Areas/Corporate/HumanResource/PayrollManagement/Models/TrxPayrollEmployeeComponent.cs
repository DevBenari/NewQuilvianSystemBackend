using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollEmployeeComponent", Schema = "public")]
    public class TrxPayrollEmployeeComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunEmployeeId { get; set; }

        public Guid? PayrollComponentId { get; set; }
        public Guid? SourceId { get; set; }

        [Required, MaxLength(50)]
        public string SourceType { get; set; } = "Master";
        // Master, SalaryAssignment, Attendance, Overtime, VariableInput,
        // Adjustment, Benefit, Tax, Insurance, MedicalServiceFee.

        [Required, MaxLength(50)]
        public string ComponentCodeSnapshot { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ComponentNameSnapshot { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string ComponentTypeSnapshot { get; set; } = "Earning";

        [Required, MaxLength(30)]
        public string CalculationMethodSnapshot { get; set; } = "Fixed";

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal Quantity { get; set; } = 1m;
        public decimal Rate { get; set; } = 0m;
        public decimal Percentage { get; set; } = 0m;
        public decimal BaseAmount { get; set; } = 0m;
        public decimal Amount { get; set; } = 0m;
        public decimal EmployerAmount { get; set; } = 0m;
        public decimal TaxableAmount { get; set; } = 0m;

        public bool IsTaxable { get; set; } = false;
        public bool IsProrated { get; set; } = false;
        public bool IsManual { get; set; } = false;
        public bool IsDisplayedOnPayslip { get; set; } = true;

        [MaxLength(2000)]
        public string? FormulaSnapshot { get; set; }

        public string? CalculationDetailJson { get; set; }
        public int SortOrder { get; set; } = 0;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
    }
}
