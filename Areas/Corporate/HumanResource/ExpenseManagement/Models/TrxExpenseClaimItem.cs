using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models
{
    [Table("TrxExpenseClaimItem", Schema = "public")]
    public class TrxExpenseClaimItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseClaimId { get; set; }

        [Required]
        public Guid ExpenseCategoryId { get; set; }

        public Guid? ReimbursementPolicyId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public Guid? CostCenterId { get; set; }

        public int LineNumber { get; set; } = 1;
        public DateOnly TransactionDate { get; set; }

        [MaxLength(250)]
        public string? MerchantName { get; set; }

        [MaxLength(100)]
        public string? MerchantTaxNumber { get; set; }

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public decimal Quantity { get; set; } = 1m;
        public decimal UnitAmount { get; set; } = 0m;
        public decimal ClaimedAmount { get; set; } = 0m;
        public decimal EligibleAmount { get; set; } = 0m;
        public decimal NonEligibleAmount { get; set; } = 0m;
        public decimal ApprovedAmount { get; set; } = 0m;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal ExchangeRate { get; set; } = 1m;
        public decimal OriginalCurrencyAmount { get; set; } = 0m;
        public decimal BaseCurrencyAmount { get; set; } = 0m;

        public decimal? MaximumAmountPerTransactionSnapshot { get; set; }
        public decimal? MaximumAmountPerPeriodSnapshot { get; set; }
        public decimal? BenefitLimitSnapshot { get; set; }
        public decimal PeriodUsedBeforeAmount { get; set; } = 0m;
        public decimal PeriodUsedAfterAmount { get; set; } = 0m;

        public bool RequiresReceipt { get; set; } = true;
        public bool HasReceipt { get; set; } = false;
        public bool IsPolicyActive { get; set; } = false;
        public bool IsEmployeeGradeEligible { get; set; } = false;
        public bool IsTransactionDateValid { get; set; } = false;
        public bool IsCostCenterValid { get; set; } = false;
        public bool IsWithinTransactionLimit { get; set; } = false;
        public bool IsWithinPeriodLimit { get; set; } = false;
        public bool IsWithinBenefitLimit { get; set; } = false;
        public bool HasDuplicateReceipt { get; set; } = false;
        public bool IsEligible { get; set; } = false;

        [MaxLength(1000)]
        public string? NonEligibleReason { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [Required, MaxLength(30)]
        public string ItemStatus { get; set; } = "Draft";
        // Draft, Submitted, Eligible, PartiallyEligible, NonEligible,
        // Approved, Rejected, Paid, Reversed.

        public bool IsActive { get; set; } = true;

        public TrxExpenseClaim? ExpenseClaim { get; set; }
        public MstExpenseCategory? ExpenseCategory { get; set; }
        public MstReimbursementPolicy? ReimbursementPolicy { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public MstCostCenter? CostCenter { get; set; }

        public ICollection<TrxExpenseReceipt> Receipts { get; set; } = new List<TrxExpenseReceipt>();
        public ICollection<TrxExpenseVerification> Verifications { get; set; } = new List<TrxExpenseVerification>();
    }
}
