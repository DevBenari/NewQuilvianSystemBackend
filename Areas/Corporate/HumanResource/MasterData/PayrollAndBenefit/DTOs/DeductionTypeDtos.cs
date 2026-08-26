using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class DeductionTypeSummaryResponse
    {
        public int TotalDeductionType { get; set; }
        public int ActiveDeductionType { get; set; }
        public int InactiveDeductionType { get; set; }
        public int RecurringDeductionType { get; set; }
        public int StatutoryDeductionType { get; set; }
        public int PreTaxDeductionType { get; set; }
        public int ApprovalRequiredDeductionType { get; set; }
    }

    public class DeductionTypeResponse
    {
        public Guid Id { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? PayrollComponentCode { get; set; }
        public string? PayrollComponentName { get; set; }
        public string DeductionTypeCode { get; set; } = string.Empty;
        public string DeductionTypeName { get; set; } = string.Empty;
        public string DeductionCategory { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal DefaultAmount { get; set; }
        public decimal DefaultPercentage { get; set; }
        public decimal? MaximumAmount { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsStatutory { get; set; }
        public bool IsPreTax { get; set; }
        public bool RequiresApproval { get; set; }
        public bool AllowPartialDeduction { get; set; }
        public int Priority { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DeductionTypeDetailResponse : DeductionTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DeductionTypeOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? PayrollComponentCode { get; set; }
        public string? PayrollComponentName { get; set; }
        public string DeductionTypeCode { get; set; } = string.Empty;
        public string DeductionTypeName { get; set; } = string.Empty;
        public string DeductionCategory { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
    }

    public class DeductionTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DeductionTypeOptionResponse> Items { get; set; } = new();
    }

    public class DeductionTypeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DeductionTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DeductionTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DeductionTypeStringOptionResponse> PrimaryOptions { get; set; } = new();
        public List<DeductionTypeStringOptionResponse> SecondaryOptions { get; set; } = new();
        public List<DeductionTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class DeductionTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DeductionTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DeductionTypeStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DeductionTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDeductionTypeRequest
    {
        public Guid? PayrollComponentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string DeductionTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DeductionCategory { get; set; } = "Other";

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Fixed";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal DefaultAmount { get; set; }

        [Range(0, 100)]
        public decimal DefaultPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumAmount { get; set; }

        public bool IsRecurring { get; set; } = true;

        public bool IsStatutory { get; set; }

        public bool IsPreTax { get; set; }

        public bool RequiresApproval { get; set; }

        public bool AllowPartialDeduction { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int Priority { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

    }

    public class UpdateDeductionTypeRequest : CreateDeductionTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDeductionTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeductionTypeCreateResponse
    {
        public Guid Id { get; set; }
        public string DeductionTypeCode { get; set; } = string.Empty;
        public string DeductionTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}