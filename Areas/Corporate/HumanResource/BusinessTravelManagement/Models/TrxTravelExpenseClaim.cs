using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelExpenseClaim", Schema = "public")]
    public class TrxTravelExpenseClaim : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? ReimbursementPolicyId { get; set; }
        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(50)]
        public string ExpenseClaimNumber { get; set; } = string.Empty;

        public DateOnly ClaimDate { get; set; }
        public DateOnly? SettlementDueDate { get; set; }
        public decimal ClaimedAmount { get; set; } = 0m;
        public decimal EligibleAmount { get; set; } = 0m;
        public decimal ApprovedAmount { get; set; } = 0m;
        public decimal RejectedAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(30)]
        public string ClaimStatus { get; set; } = "Draft";
        // Draft, Submitted, HrVerified, FinanceVerified, Approved, Rejected,
        // NeedRevision, WaitingSettlement, Settled, PostedToFinance, Cancelled.

        public DateTime? SubmittedAt { get; set; }
        public DateTime? HrVerifiedAt { get; set; }
        public DateTime? FinanceVerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? PostedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? HrVerifiedByUserId { get; set; }
        public Guid? FinanceVerifiedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public Guid? PostedByUserId { get; set; }

        [MaxLength(2000)]
        public string? ClaimNotes { get; set; }

        [MaxLength(2000)]
        public string? VerificationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstReimbursementPolicy? ReimbursementPolicy { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? HrVerifiedByUser { get; set; }
        public ApplicationUser? FinanceVerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }

        public ICollection<TrxTravelExpenseItem> Items { get; set; } = new List<TrxTravelExpenseItem>();
        public ICollection<TrxTravelSettlement> Settlements { get; set; } = new List<TrxTravelSettlement>();
        public ICollection<TrxTravelDocument> Documents { get; set; } = new List<TrxTravelDocument>();
    }
}
