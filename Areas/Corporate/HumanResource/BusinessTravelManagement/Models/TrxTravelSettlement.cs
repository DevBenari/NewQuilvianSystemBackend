using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelSettlement", Schema = "public")]
    public class TrxTravelSettlement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? TravelExpenseClaimId { get; set; }
        public Guid? TravelAdvanceRequestId { get; set; }
        public Guid? TravelAdvancePaymentId { get; set; }
        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(50)]
        public string SettlementNumber { get; set; } = string.Empty;

        public DateOnly SettlementDate { get; set; }
        public decimal AdvancePaidAmount { get; set; } = 0m;
        public decimal ApprovedExpenseAmount { get; set; } = 0m;
        public decimal SettlementDifferenceAmount { get; set; } = 0m;
        public decimal EmployeeRefundAmount { get; set; } = 0m;
        public decimal CompanyPayableAmount { get; set; } = 0m;
        public decimal SettledAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(30)]
        public string SettlementStatus { get; set; } = "Draft";
        // Draft, Submitted, Verified, Approved, WaitingPayment, Paid,
        // WaitingEmployeeRefund, Refunded, PostedToFinance, Completed, Rejected, Cancelled.

        public DateTime? SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? PostedByUserId { get; set; }

        [MaxLength(2000)]
        public string? SettlementNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxTravelExpenseClaim? TravelExpenseClaim { get; set; }
        public TrxTravelAdvanceRequest? TravelAdvanceRequest { get; set; }
        public TrxTravelAdvancePayment? TravelAdvancePayment { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
    }
}
