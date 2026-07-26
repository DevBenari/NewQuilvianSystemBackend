using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollRunEmployee", Schema = "public")]
    public class TrxPayrollRunEmployee : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? PayrollProfileId { get; set; }
        public Guid? TaxProfileId { get; set; }
        public Guid? InsuranceProfileId { get; set; }
        public Guid? SalaryAssignmentId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public Guid? SalaryGradeId { get; set; }

        [Required, MaxLength(30)]
        public string EmployeePayrollStatus { get; set; } = "Pending";
        // Pending, InputCollected, Calculated, Review, Error, Approved, Paid, Posted, Reversed.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(50)]
        public string? EmployeeNumberSnapshot { get; set; }

        [Required, MaxLength(200)]
        public string EmployeeNameSnapshot { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DepartmentNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? PositionNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? EmployeeGradeSnapshot { get; set; }

        [MaxLength(100)]
        public string? CostCenterCodeSnapshot { get; set; }

        [MaxLength(200)]
        public string? CostCenterNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? BankNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumberSnapshot { get; set; }

        [MaxLength(200)]
        public string? BankAccountHolderSnapshot { get; set; }

        [MaxLength(50)]
        public string? TaxStatusSnapshot { get; set; }

        [MaxLength(50)]
        public string? NpwpNumberSnapshot { get; set; }

        public decimal BaseSalary { get; set; } = 0m;
        public decimal TotalRecurringEarning { get; set; } = 0m;
        public decimal TotalVariableEarning { get; set; } = 0m;
        public decimal TotalOvertimeAmount { get; set; } = 0m;
        public decimal TotalAttendanceAllowance { get; set; } = 0m;
        public decimal TotalTransportAllowance { get; set; } = 0m;
        public decimal TotalBenefit { get; set; } = 0m;
        public decimal TotalDeduction { get; set; } = 0m;
        public decimal TotalTax { get; set; } = 0m;
        public decimal TotalEmployeeInsuranceContribution { get; set; } = 0m;
        public decimal TotalEmployerInsuranceContribution { get; set; } = 0m;
        public decimal GrossPay { get; set; } = 0m;
        public decimal NetPay { get; set; } = 0m;
        public decimal PaymentAmount { get; set; } = 0m;

        public int ScheduledWorkMinutes { get; set; } = 0;
        public int ActualWorkMinutes { get; set; } = 0;
        public int LateMinutes { get; set; } = 0;
        public int EarlyLeaveMinutes { get; set; } = 0;
        public int OvertimeMinutes { get; set; } = 0;
        public decimal PaidLeaveDays { get; set; } = 0m;
        public decimal UnpaidLeaveDays { get; set; } = 0m;
        public decimal AbsentDays { get; set; } = 0m;

        public bool IsSnapshotFrozen { get; set; } = false;
        public DateTime? SnapshotFrozenAt { get; set; }
        public Guid? SnapshotFrozenByUserId { get; set; }
        public bool IsFinalized { get; set; } = false;
        public DateTime? FinalizedAt { get; set; }
        public Guid? FinalizedByUserId { get; set; }

        public string? EmployeeSnapshotJson { get; set; }
        public string? SalarySnapshotJson { get; set; }
        public string? TaxSnapshotJson { get; set; }
        public string? InsuranceSnapshotJson { get; set; }
        public string? CalculationResultJson { get; set; }
        public string? ValidationResultJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRun? PayrollRun { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public WfpPayroll? PayrollProfile { get; set; }
        public WfpTax? TaxProfile { get; set; }
        public WfpInsurance? InsuranceProfile { get; set; }
        public WfpSalaryAssignment? SalaryAssignment { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstSalaryStructure? SalaryStructure { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public ApplicationUser? SnapshotFrozenByUser { get; set; }
        public ApplicationUser? FinalizedByUser { get; set; }

        public ICollection<TrxPayrollEmployeeComponent> Components { get; set; }
            = new List<TrxPayrollEmployeeComponent>();
        public ICollection<TrxPayrollAttendanceInput> AttendanceInputs { get; set; }
            = new List<TrxPayrollAttendanceInput>();
        public ICollection<TrxPayrollOvertimeInput> OvertimeInputs { get; set; }
            = new List<TrxPayrollOvertimeInput>();
        public ICollection<TrxPayrollVariableInput> VariableInputs { get; set; }
            = new List<TrxPayrollVariableInput>();
        public ICollection<TrxPayrollAdjustment> Adjustments { get; set; }
            = new List<TrxPayrollAdjustment>();
        public ICollection<TrxPayrollPayment> Payments { get; set; }
            = new List<TrxPayrollPayment>();
        public TrxPayrollPayslip? Payslip { get; set; }
    }
}
