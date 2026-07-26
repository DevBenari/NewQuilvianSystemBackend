using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models
{
    [Table("TrxExpensePayment", Schema = "public")]
    public class TrxExpensePayment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseClaimId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentNumber { get; set; } = string.Empty;

        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? PayrollPeriodId { get; set; }

        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Draft";
        // Draft, PendingVerification, Approved, Processing, Paid,
        // Failed, Cancelled, PartiallyReversed, Reversed.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal PaymentAmount { get; set; } = 0m;
        public decimal ReversedAmount { get; set; } = 0m;
        public decimal NetPaidAmount { get; set; } = 0m;

        [MaxLength(100)]
        public string? PaymentReferenceNumber { get; set; }

        [MaxLength(50)]
        public string? PaymentMethodSnapshot { get; set; }

        [MaxLength(200)]
        public string? PayeeNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? BankNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumberSnapshot { get; set; }

        public DateTime? ScheduledPaymentAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public Guid? PaidByUserId { get; set; }

        public bool IsPostedToPayroll { get; set; } = false;
        public DateTime? PostedToPayrollAt { get; set; }
        public bool IsPostedToFinance { get; set; } = false;
        public DateTime? PostedToFinanceAt { get; set; }
        public bool IsPostedToGl { get; set; } = false;
        public DateTime? PostedToGlAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxExpenseClaim? ExpenseClaim { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public ApplicationUser? PaidByUser { get; set; }

        public ICollection<TrxExpenseReversal> Reversals { get; set; } = new List<TrxExpenseReversal>();
    }
}
