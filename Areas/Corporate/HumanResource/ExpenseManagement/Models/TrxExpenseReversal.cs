using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models
{
    [Table("TrxExpenseReversal", Schema = "public")]
    public class TrxExpenseReversal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseClaimId { get; set; }

        [Required]
        public Guid ExpensePaymentId { get; set; }

        [Required, MaxLength(50)]
        public string ReversalNumber { get; set; } = string.Empty;

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? ReversedByUserId { get; set; }

        public Guid? FinanceReversalId { get; set; }
        public Guid? GlReversalHeaderId { get; set; }

        [Required, MaxLength(30)]
        public string ReversalStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Rejected, Posted, Cancelled.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal ReversalAmount { get; set; } = 0m;
        public DateTime? ReversedAt { get; set; }
        public DateTime? PostedAt { get; set; }

        [Required, MaxLength(2000)]
        public string ReversalReason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxExpenseClaim? ExpenseClaim { get; set; }
        public TrxExpensePayment? ExpensePayment { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }
    }
}
