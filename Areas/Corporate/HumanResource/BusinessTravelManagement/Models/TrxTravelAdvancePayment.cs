using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelAdvancePayment", Schema = "public")]
    public class TrxTravelAdvancePayment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelAdvanceRequestId { get; set; }

        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(50)]
        public string PaymentNumber { get; set; } = string.Empty;

        public DateOnly PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(100)]
        public string? BankReferenceNumber { get; set; }

        [MaxLength(100)]
        public string? FinanceReferenceNumber { get; set; }

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Pending";
        // Pending, Processing, Paid, Failed, Reversed, Cancelled.

        public DateTime? ProcessedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ReversedAt { get; set; }
        public Guid? ProcessedByUserId { get; set; }
        public Guid? ReversedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTravelAdvanceRequest? TravelAdvanceRequest { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public WfpBankAccount? BankAccount { get; set; }
        public ApplicationUser? ProcessedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }

        public ICollection<TrxTravelSettlement> Settlements { get; set; } = new List<TrxTravelSettlement>();
    }
}
