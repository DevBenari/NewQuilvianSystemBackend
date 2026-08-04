using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class PayrollComponentCategorySummaryResponse
    {
        public int TotalCategory { get; set; }
        public int ActiveCategory { get; set; }
        public int InactiveCategory { get; set; }
        public int EarningCategory { get; set; }
        public int DeductionCategory { get; set; }
        public int EmployerContributionCategory { get; set; }
        public int InformationCategory { get; set; }
    }

    public class PayrollComponentCategoryResponse
    {
        public Guid Id { get; set; }
        public string ComponentCategoryCode { get; set; } = string.Empty;
        public string ComponentCategoryName { get; set; } = string.Empty;
        public string ComponentGroup { get; set; } = string.Empty;
        public bool AffectsGrossPay { get; set; }
        public bool AffectsTaxableIncome { get; set; }
        public bool AffectsTakeHomePay { get; set; }
        public bool IsEmployerCost { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int PayrollComponentCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PayrollComponentCategoryDetailResponse : PayrollComponentCategoryResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PayrollComponentCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string ComponentCategoryCode { get; set; } = string.Empty;
        public string ComponentCategoryName { get; set; } = string.Empty;
        public string ComponentGroup { get; set; } = string.Empty;
    }

    public class PayrollComponentCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PayrollComponentCategoryOptionResponse> Items { get; set; } = new();
    }

    public class PayrollComponentCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PayrollComponentCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PayrollMasterStringOptionResponse> ComponentGroupOptions { get; set; } = new();
        public List<PayrollMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PayrollMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PayrollComponentCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? ComponentGroup { get; set; }
        public bool? AffectsGrossPay { get; set; }
        public bool? AffectsTaxableIncome { get; set; }
        public bool? AffectsTakeHomePay { get; set; }
        public bool? IsEmployerCost { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "componentCategoryName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePayrollComponentCategoryRequest
    {
        [Required]
        [MaxLength(150)]
        public string ComponentCategoryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ComponentGroup { get; set; } = "Earning";

        public bool AffectsGrossPay { get; set; } = true;
        public bool AffectsTaxableIncome { get; set; } = true;
        public bool AffectsTakeHomePay { get; set; } = true;
        public bool IsEmployerCost { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdatePayrollComponentCategoryRequest : CreatePayrollComponentCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePayrollComponentCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class PayrollComponentCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string ComponentCategoryCode { get; set; } = string.Empty;
        public string ComponentCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PayrollMasterStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PayrollMasterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PayrollMasterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
