using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstExpenseCategory", Schema = "public")]
    public class MstExpenseCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ParentExpenseCategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ExpenseCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ExpenseCategoryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CategoryType { get; set; } = "General";
        // General, Travel, Medical, Training, Communication, Transportation, Other.

        public bool IsTravelRelated { get; set; } = false;

        public bool IsMedicalBenefitRelated { get; set; } = false;

        public bool IsTrainingRelated { get; set; } = false;

        public bool RequiresReceipt { get; set; } = true;

        public bool AllowWithoutReceipt { get; set; } = false;

        public bool IsReimbursable { get; set; } = true;

        public bool IsTaxable { get; set; } = false;

        public bool RequireCostCenter { get; set; } = true;

        public bool AllowSplitAllocation { get; set; } = false;

        public decimal? DefaultMaximumAmount { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstExpenseCategory? ParentExpenseCategory { get; set; }

        public ICollection<MstExpenseCategory> ChildExpenseCategories { get; set; }
            = new List<MstExpenseCategory>();

        public ICollection<MstTravelExpenseCategory> TravelExpenseCategories { get; set; }
            = new List<MstTravelExpenseCategory>();

        public ICollection<MstReimbursementPolicy> ReimbursementPolicies { get; set; }
            = new List<MstReimbursementPolicy>();
    }
}
