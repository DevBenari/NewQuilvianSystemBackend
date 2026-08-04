using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class PayrollComponentSummaryResponse
    {
        public int TotalComponent { get; set; }
        public int ActiveComponent { get; set; }
        public int InactiveComponent { get; set; }
        public int EarningComponent { get; set; }
        public int DeductionComponent { get; set; }
        public int EmployerContributionComponent { get; set; }
        public int RecurringComponent { get; set; }
        public int TaxableComponent { get; set; }
    }

    public class PayrollComponentResponse
    {
        public Guid Id { get; set; }
        public Guid PayrollComponentCategoryId { get; set; }
        public string PayrollComponentCategoryCode { get; set; } = string.Empty;
        public string PayrollComponentCategoryName { get; set; } = string.Empty;
        public Guid? BaseComponentId { get; set; }
        public string? BaseComponentCode { get; set; }
        public string? BaseComponentName { get; set; }
        public string PayrollComponentCode { get; set; } = string.Empty;
        public string PayrollComponentName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string? FormulaExpression { get; set; }
        public decimal DefaultAmount { get; set; }
        public decimal DefaultPercentage { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsProrated { get; set; }
        public bool IsAttendanceBased { get; set; }
        public bool IsOvertimeBased { get; set; }
        public bool IsBenefitBased { get; set; }
        public bool IsEmployerContribution { get; set; }
        public bool IsEmployeeContribution { get; set; }
        public bool IsDisplayedOnPayslip { get; set; }
        public bool IsEditableDuringPayroll { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int DerivedComponentCount { get; set; }
        public int AllowanceTypeCount { get; set; }
        public int DeductionTypeCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PayrollComponentDetailResponse : PayrollComponentResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PayrollComponentOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PayrollComponentCategoryId { get; set; }
        public string PayrollComponentCode { get; set; } = string.Empty;
        public string PayrollComponentName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
    }

    public class PayrollComponentOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PayrollComponentOptionResponse> Items { get; set; } = new();
    }

    public class PayrollComponentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PayrollComponentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PayrollMasterStringOptionResponse> ComponentTypeOptions { get; set; } = new();
        public List<PayrollMasterStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<PayrollMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PayrollMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PayrollComponentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? PayrollComponentCategoryId { get; set; }
        public string? ComponentType { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsRecurring { get; set; }
        public bool? IsTaxable { get; set; }
        public bool? IsProrated { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "payrollComponentName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePayrollComponentRequest
    {
        [Required]
        public Guid PayrollComponentCategoryId { get; set; }

        public Guid? BaseComponentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PayrollComponentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ComponentType { get; set; } = "Earning";

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Fixed";

        [MaxLength(1000)]
        public string? FormulaExpression { get; set; }

        public decimal DefaultAmount { get; set; }
        public decimal DefaultPercentage { get; set; }
        public bool IsRecurring { get; set; } = true;
        public bool IsTaxable { get; set; } = true;
        public bool IsProrated { get; set; } = true;
        public bool IsAttendanceBased { get; set; }
        public bool IsOvertimeBased { get; set; }
        public bool IsBenefitBased { get; set; }
        public bool IsEmployerContribution { get; set; }
        public bool IsEmployeeContribution { get; set; }
        public bool IsDisplayedOnPayslip { get; set; } = true;
        public bool IsEditableDuringPayroll { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdatePayrollComponentRequest : CreatePayrollComponentRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePayrollComponentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class PayrollComponentCreateResponse
    {
        public Guid Id { get; set; }
        public string PayrollComponentCode { get; set; } = string.Empty;
        public string PayrollComponentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
