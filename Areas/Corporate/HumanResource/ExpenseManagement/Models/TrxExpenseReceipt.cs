using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models
{
    [Table("TrxExpenseReceipt", Schema = "public")]
    public class TrxExpenseReceipt : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseClaimId { get; set; }

        [Required]
        public Guid ExpenseClaimItemId { get; set; }

        [MaxLength(100)]
        public string? ReceiptNumber { get; set; }

        public DateOnly? ReceiptDate { get; set; }

        [MaxLength(250)]
        public string? MerchantName { get; set; }

        public decimal? ReceiptAmount { get; set; }

        [MaxLength(3)]
        public string? CurrencyCode { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? ReceiptChecksum { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        [Column(TypeName = "jsonb")]
        public string? OcrResultJson { get; set; }

        public bool IsOriginalReceipt { get; set; } = true;
        public bool IsDuplicate { get; set; } = false;
        public Guid? DuplicateOfReceiptId { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxExpenseClaim? ExpenseClaim { get; set; }
        public TrxExpenseClaimItem? ExpenseClaimItem { get; set; }
        public TrxExpenseReceipt? DuplicateOfReceipt { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
