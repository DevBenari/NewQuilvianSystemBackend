using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    [Table("PrlPayrollPayment", Schema = "public")]
    public class PrlPayrollPayment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public Guid PayrollRunEmployeeId { get; set; }

        public Guid? BankAccountId { get; set; }
        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentNumber { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Draft";
        // Draft, Scheduled, Processing, Paid, Failed, Cancelled,
        // PartiallyReversed, Reversed.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal PaymentAmount { get; set; } = 0m;
        public decimal ReversedAmount { get; set; } = 0m;
        public decimal NetPaidAmount { get; set; } = 0m;

        [MaxLength(200)]
        public string? PayeeNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? BankNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumberSnapshot { get; set; }

        [MaxLength(100)]
        public string? PaymentReferenceNumber { get; set; }

        public DateTime? ScheduledPaymentAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public Guid? PaidByUserId { get; set; }
        public DateTime? PostedToFinanceAt { get; set; }
        public DateTime? PostedToGlAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public PrlPayrollRun? PayrollRun { get; set; }
        public PrlPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public ApplicationUser? PaidByUser { get; set; }

        public ICollection<PrlPayrollReversal> Reversals { get; set; }
            = new List<PrlPayrollReversal>();
        public ICollection<PrlMedicalServiceFeePayment> MedicalServiceFeePayments { get; set; }
            = new List<PrlMedicalServiceFeePayment>();
    }
}
