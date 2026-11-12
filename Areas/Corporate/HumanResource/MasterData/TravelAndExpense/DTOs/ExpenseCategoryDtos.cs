using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.DTOs
{
    public class ExpenseCategoryRequest
    {
        /// <summary>Kosongkan supaya kode dibuat otomatis.</summary>
        [MaxLength(50)]
        public string? ExpenseCategoryCode { get; set; }

        [Required, MaxLength(200)]
        public string ExpenseCategoryName { get; set; } = string.Empty;

        public Guid? ParentExpenseCategoryId { get; set; }

        [Required, MaxLength(40)]
        public string CategoryType { get; set; } = "General";

        public bool IsTravelRelated { get; set; }
        public bool IsMedicalBenefitRelated { get; set; }
        public bool IsTrainingRelated { get; set; }
        public bool RequiresReceipt { get; set; } = true;
        public bool AllowWithoutReceipt { get; set; }
        public bool IsReimbursable { get; set; } = true;
        public bool IsTaxable { get; set; }
        public bool RequireCostCenter { get; set; } = true;
        public bool AllowSplitAllocation { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DefaultMaximumAmount { get; set; }

        public int SortOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class ExpenseCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ExpenseCategoryResponse : ExpenseCategoryRequest
    {
        public Guid Id { get; set; }
        public string? ParentExpenseCategoryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class ExpenseCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ExpenseCategorySummaryResponse
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int TravelRelated { get; set; }
    }

    public class ExpenseCategoryFilterMetadataResponse
    {
        public List<string> CategoryTypes { get; set; } = new();
        public List<string> SortOptions { get; set; } = new() { "expenseCategoryCode", "expenseCategoryName", "sortOrder", "createDateTime" };
    }

    /// <summary>Galat bentrok data master (dikembalikan sebagai 409).</summary>
    public class ExpenseCategoryConflictException : Exception
    {
        public ExpenseCategoryConflictException(string message) : base(message) { }
    }
}
