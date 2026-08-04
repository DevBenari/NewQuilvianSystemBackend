using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class SalaryStructureSummaryResponse
    {
        public int TotalSalaryStructure { get; set; }
        public int ActiveSalaryStructure { get; set; }
        public int InactiveSalaryStructure { get; set; }
        public int DefaultSalaryStructure { get; set; }
        public int ProratedSalaryStructure { get; set; }
        public int OvertimeIncludedStructure { get; set; }
    }

    public class SalaryStructureResponse
    {
        public Guid Id { get; set; }
        public Guid SalaryGradeId { get; set; }
        public string? SalaryGradeCode { get; set; }
        public string? SalaryGradeName { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string SalaryStructureCode { get; set; } = string.Empty;
        public string SalaryStructureName { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string PaymentFrequency { get; set; } = string.Empty;
        public decimal DefaultBaseSalary { get; set; }
        public decimal? MinimumBaseSalary { get; set; }
        public decimal? MaximumBaseSalary { get; set; }
        public decimal StandardWorkingDaysPerMonth { get; set; }
        public decimal StandardWorkingHoursPerMonth { get; set; }
        public bool IsProrated { get; set; }
        public bool IncludeOvertime { get; set; }
        public bool IncludeShiftAllowance { get; set; }
        public bool IncludeOnCallAllowance { get; set; }
        public bool IncludeHazardAllowance { get; set; }
        public bool IncludeBenefitDeduction { get; set; }
        public string? ComponentConfigurationJson { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int WorkforcePayrollCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class SalaryStructureDetailResponse : SalaryStructureResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class SalaryStructureOptionResponse
    {
        public Guid Id { get; set; }
        public Guid SalaryGradeId { get; set; }
        public string? SalaryGradeName { get; set; }
        public string SalaryStructureCode { get; set; } = string.Empty;
        public string SalaryStructureName { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string PaymentFrequency { get; set; } = string.Empty;
        public decimal DefaultBaseSalary { get; set; }
        public bool IsDefault { get; set; }
    }

    public class SalaryStructureOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<SalaryStructureOptionResponse> Items { get; set; } = new();
    }

    public class SalaryStructureFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public SalaryStructureDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<SalaryStructureCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<SalaryStructureStringOptionResponse> PaymentFrequencyOptions { get; set; } = new();
        public List<SalaryStructureSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class SalaryStructureDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public string? PaymentFrequency { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsProrated { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "salaryStructureName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class SalaryStructureCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SalaryStructureStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SalaryStructureSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateSalaryStructureRequest
    {
        [Required]
        public Guid SalaryGradeId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string SalaryStructureName { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required]
        [MaxLength(50)]
        public string PaymentFrequency { get; set; } = "Monthly";

        [Range(0, double.MaxValue)]
        public decimal DefaultBaseSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinimumBaseSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumBaseSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StandardWorkingDaysPerMonth { get; set; } = 22m;

        [Range(0, double.MaxValue)]
        public decimal StandardWorkingHoursPerMonth { get; set; } = 173m;

        public bool IsProrated { get; set; } = true;
        public bool IncludeOvertime { get; set; } = true;
        public bool IncludeShiftAllowance { get; set; } = true;
        public bool IncludeOnCallAllowance { get; set; } = true;
        public bool IncludeHazardAllowance { get; set; } = true;
        public bool IncludeBenefitDeduction { get; set; } = true;
        public string? ComponentConfigurationJson { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateSalaryStructureRequest : CreateSalaryStructureRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSalaryStructureStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class SalaryStructureCreateResponse
    {
        public Guid Id { get; set; }
        public string SalaryStructureCode { get; set; } = string.Empty;
        public string SalaryStructureName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
