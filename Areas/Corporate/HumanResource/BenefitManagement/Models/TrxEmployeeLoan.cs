using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxEmployeeLoan", Schema = "public")]
    public class TrxEmployeeLoan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? BenefitPlanId { get; set; }

        public Guid? BankAccountId { get; set; }

        public Guid? PayrollComponentId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LoanNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LoanType { get; set; } = "EmployeeLoan";

        [Required]
        [MaxLength(30)]
        public string LoanStatus { get; set; } = "Draft";

        public DateTime ApplicationDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? DisbursementDate { get; set; }

        public DateTime? InstallmentStartDate { get; set; }

        public DateTime? MaturityDate { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal PrincipalAmount { get; set; } = 0m;

        public decimal InterestRate { get; set; } = 0m;

        public decimal InterestAmount { get; set; } = 0m;

        public decimal AdministrationFee { get; set; } = 0m;

        public decimal TotalPayableAmount { get; set; } = 0m;

        public int InstallmentCount { get; set; } = 0;

        public decimal InstallmentAmount { get; set; } = 0m;

        public decimal PaidAmount { get; set; } = 0m;

        public decimal OutstandingAmount { get; set; } = 0m;

        [MaxLength(1000)]
        public string? Purpose { get; set; }

        [MaxLength(100)]
        public string? FinanceReferenceNumber { get; set; }

        public Guid? FinanceTransactionId { get; set; }

        public Guid? GlHeaderId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? DisbursedAt { get; set; }

        public Guid? DisbursedByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? DisbursedByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxEmployeeLoanInstallment> Installments { get; set; } = new List<TrxEmployeeLoanInstallment>();
    }
}
