using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models
{
    [Table("TrxExpenseVerification", Schema = "public")]
    public class TrxExpenseVerification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseClaimId { get; set; }

        public Guid? ExpenseClaimItemId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        [Required, MaxLength(40)]
        public string VerificationType { get; set; } = "Finance";
        // Policy, Receipt, Benefit, Budget, HR, Finance, Final.

        [Required, MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";
        // Pending, Verified, PartiallyVerified, Rejected, NeedRevision.

        public decimal ClaimedAmountSnapshot { get; set; } = 0m;
        public decimal EligibleAmount { get; set; } = 0m;
        public decimal NonEligibleAmount { get; set; } = 0m;
        public decimal VerifiedAmount { get; set; } = 0m;

        public bool IsPolicyValid { get; set; } = false;
        public bool IsReceiptValid { get; set; } = false;
        public bool IsEmployeeEligible { get; set; } = false;
        public bool IsBudgetValid { get; set; } = false;
        public bool IsBenefitLimitValid { get; set; } = false;
        public bool IsFinalVerification { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string? ChecklistResultJson { get; set; }

        [MaxLength(2000)]
        public string? VerificationNotes { get; set; }

        public DateTime? VerifiedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxExpenseClaim? ExpenseClaim { get; set; }
        public TrxExpenseClaimItem? ExpenseClaimItem { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
