using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollRun", Schema = "public")]
    public class TrxPayrollRun : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollPeriodId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        [Required, MaxLength(50)]
        public string RunNumber { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string RunType { get; set; } = "Regular";
        // Regular, OffCycle, Adjustment, Bonus, FinalPayroll, MedicalServiceFee.

        [Required, MaxLength(30)]
        public string RunStatus { get; set; } = "Draft";
        // Draft, CollectingInput, Calculating, Review, WaitingApproval,
        // Approved, PaymentProcessing, Paid, Posted, Closed, Cancelled, Reversed.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public DateTime PeriodStartDateSnapshot { get; set; }
        public DateTime PeriodEndDateSnapshot { get; set; }
        public DateTime? AttendanceCutoffDateSnapshot { get; set; }
        public DateTime? VariableInputCutoffDateSnapshot { get; set; }
        public DateTime? PaymentDateSnapshot { get; set; }

        public int TotalEmployeeCount { get; set; } = 0;
        public int ProcessedEmployeeCount { get; set; } = 0;
        public int ErrorEmployeeCount { get; set; } = 0;
        public decimal TotalBaseSalary { get; set; } = 0m;
        public decimal TotalEarning { get; set; } = 0m;
        public decimal TotalDeduction { get; set; } = 0m;
        public decimal TotalTax { get; set; } = 0m;
        public decimal TotalEmployeeContribution { get; set; } = 0m;
        public decimal TotalEmployerContribution { get; set; } = 0m;
        public decimal TotalGrossPay { get; set; } = 0m;
        public decimal TotalNetPay { get; set; } = 0m;
        public decimal TotalPaidAmount { get; set; } = 0m;

        public bool IsLocked { get; set; } = false;
        public DateTime? LockedAt { get; set; }
        public Guid? LockedByUserId { get; set; }

        public DateTime? CalculationStartedAt { get; set; }
        public DateTime? CalculatedAt { get; set; }
        public Guid? CalculatedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }

        public Guid? FinancePaymentBatchId { get; set; }
        public Guid? GlHeaderId { get; set; }

        public string? PolicySnapshotJson { get; set; }
        public string? ConfigurationSnapshotJson { get; set; }
        public string? ValidationSummaryJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? LockedByUser { get; set; }
        public ApplicationUser? CalculatedByUser { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<TrxPayrollRunEmployee> Employees { get; set; }
            = new List<TrxPayrollRunEmployee>();
        public ICollection<TrxPayrollAdjustment> Adjustments { get; set; }
            = new List<TrxPayrollAdjustment>();
        public ICollection<TrxPayrollApproval> Approvals { get; set; }
            = new List<TrxPayrollApproval>();
        public ICollection<TrxPayrollPayment> Payments { get; set; }
            = new List<TrxPayrollPayment>();
        public ICollection<TrxPayrollReversal> Reversals { get; set; }
            = new List<TrxPayrollReversal>();
    }
}
