using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("WfpPayroll", Schema = "public")]
    public class WfpPayroll : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? SalaryAssignmentId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public Guid? LastPayrollPeriodId { get; set; }

        [MaxLength(50)]
        public string? PayrollNumber { get; set; }

        [MaxLength(50)]
        public string? PayrollGroupCode { get; set; }

        [Required, MaxLength(30)]
        public string PayrollStatus { get; set; } = "Active";
        // Active, Suspended, OnHold, Terminated.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(30)]
        public string PaymentFrequency { get; set; } = "Monthly";

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public bool IsPayrollEligible { get; set; } = true;
        public bool IsConfidential { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // Kolom kompatibilitas data payroll lama/current setup.
        // Histori pembayaran resmi wajib menggunakan snapshot TrxPayrollRunEmployee.
        public decimal BaseSalary { get; set; } = 0m;
        public decimal TotalAllowance { get; set; } = 0m;
        public decimal TotalDeduction { get; set; } = 0m;
        public decimal GrossSalary { get; set; } = 0m;
        public decimal TaxAmount { get; set; } = 0m;
        public decimal InsuranceAmount { get; set; } = 0m;
        public decimal NetSalary { get; set; } = 0m;
        public DateTime? LastCalculatedAt { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public WfpSalaryAssignment? SalaryAssignment { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstSalaryStructure? SalaryStructure { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public MstPayrollPeriod? LastPayrollPeriod { get; set; }

        public ICollection<TrxPayrollRunEmployee> PayrollRunEmployees { get; set; }
            = new List<TrxPayrollRunEmployee>();
    }
}
