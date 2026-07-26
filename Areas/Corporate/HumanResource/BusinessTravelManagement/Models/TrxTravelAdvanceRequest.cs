using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelAdvanceRequest", Schema = "public")]
    public class TrxTravelAdvanceRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(50)]
        public string AdvanceRequestNumber { get; set; } = string.Empty;

        public DateOnly RequestDate { get; set; }
        public DateOnly? RequiredPaymentDate { get; set; }
        public decimal RequestedAmount { get; set; } = 0m;
        public decimal ApprovedAmount { get; set; } = 0m;
        public decimal PaidAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Column(TypeName = "jsonb")]
        public string? AdvanceBreakdownJson { get; set; }

        [Required, MaxLength(30)]
        public string AdvanceStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Rejected, WaitingPayment, PartiallyPaid, Paid, Cancelled, Settled.

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? SettledAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? RejectedByUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }

        public ICollection<TrxTravelAdvancePayment> Payments { get; set; } = new List<TrxTravelAdvancePayment>();
        public ICollection<TrxTravelSettlement> Settlements { get; set; } = new List<TrxTravelSettlement>();
    }
}
