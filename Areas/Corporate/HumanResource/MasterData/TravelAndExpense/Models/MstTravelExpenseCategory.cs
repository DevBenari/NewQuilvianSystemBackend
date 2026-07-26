using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstTravelExpenseCategory", Schema = "public")]
    public class MstTravelExpenseCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ExpenseCategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TravelExpenseCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TravelExpenseCategoryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ExpenseType { get; set; } = "Other";
        // Transportation, Accommodation, Meal, DailyAllowance, LocalTransport, Other.

        [Required]
        [MaxLength(50)]
        public string UnitType { get; set; } = "Actual";
        // Actual, PerDay, PerNight, PerTrip, PerKilometer.

        public bool RequiresReceipt { get; set; } = true;

        public bool AllowWithoutReceipt { get; set; } = false;

        public bool IsAdvanceEligible { get; set; } = true;

        public bool IsReimbursable { get; set; } = true;

        public bool IsTaxable { get; set; } = false;

        public decimal? DefaultDailyLimit { get; set; }

        public decimal? DefaultTransactionLimit { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstExpenseCategory? ExpenseCategory { get; set; }

        public ICollection<MstTravelAllowanceRate> AllowanceRates { get; set; }
            = new List<MstTravelAllowanceRate>();
    }
}
