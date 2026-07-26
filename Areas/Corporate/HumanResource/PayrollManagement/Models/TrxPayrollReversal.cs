using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollReversal", Schema = "public")]
    public class TrxPayrollReversal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollPaymentId { get; set; }
        public Guid? OriginalGlHeaderId { get; set; }
        public Guid? ReversalGlHeaderId { get; set; }
        public Guid? FinanceReversalId { get; set; }

        [Required, MaxLength(50)]
        public string ReversalNumber { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string ReversalType { get; set; } = "Full";
        // Full, Partial, Employee, Component, PaymentOnly.

        [Required, MaxLength(30)]
        public string ReversalStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Posted, Rejected, Cancelled.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal ReversalAmount { get; set; } = 0m;

        [Required, MaxLength(1000)]
        public string ReversalReason { get; set; } = string.Empty;

        public DateTime? ReversedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRun? PayrollRun { get; set; }
        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public TrxPayrollPayment? PayrollPayment { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }
    }
}
