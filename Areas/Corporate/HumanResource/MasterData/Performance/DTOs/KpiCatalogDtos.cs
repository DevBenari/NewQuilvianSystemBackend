using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs
{

    public class KpiCatalogSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int QuantitativeData { get; set; }
        public int CascadableData { get; set; }
    }

    public class KpiCatalogResponse
    {
        public Guid Id { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public string KpiCode { get; set; } = string.Empty;
        public string KpiName { get; set; } = string.Empty;
        public string KpiCategory { get; set; } = string.Empty;
        public string? MeasurementUnit { get; set; }
        public string TargetDirection { get; set; } = string.Empty;
        public string MeasurementFrequency { get; set; } = string.Empty;
        public decimal? DefaultTargetValue { get; set; }
        public decimal? MinimumTargetValue { get; set; }
        public decimal? MaximumTargetValue { get; set; }
        public decimal DefaultWeight { get; set; }
        public bool IsQuantitative { get; set; }
        public bool IsCascadable { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int TemplateDetailCount { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class KpiCatalogDetailResponse : KpiCatalogResponse
    {
        public string? DataSource { get; set; }
        public string? CalculationFormula { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class KpiCatalogOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string TargetDirection { get; set; } = string.Empty;
        public string MeasurementFrequency { get; set; } = string.Empty;
        public string? MeasurementUnit { get; set; }
        public decimal? DefaultTargetValue { get; set; }
        public decimal DefaultWeight { get; set; }
    }

    public class KpiCatalogOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<KpiCatalogOptionResponse> Items { get; set; } = new();
    }

    public class KpiCatalogFilterMetadataResponse
    {
        public KpiCatalogDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PerformanceStringOptionResponse> TargetDirectionOptions { get; set; } = new();
        public List<PerformanceStringOptionResponse> MeasurementFrequencyOptions { get; set; } = new();
        public List<PerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class KpiCatalogDefaultFilterResponse
    {
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public string? KpiCategory { get; set; }
        public string? TargetDirection { get; set; }
        public string? MeasurementFrequency { get; set; }
        public bool? IsQuantitative { get; set; }
        public bool? IsCascadable { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateKpiCatalogRequest
    {
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        [Required, MaxLength(250)]
        public string KpiName { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string KpiCategory { get; set; } = "General";
        [MaxLength(1000)]
        public string? Description { get; set; }
        [MaxLength(100)]
        public string? MeasurementUnit { get; set; }
        [Required, MaxLength(50)]
        public string TargetDirection { get; set; } = "HigherIsBetter";
        [Required, MaxLength(50)]
        public string MeasurementFrequency { get; set; } = "Annual";
        [MaxLength(250)]
        public string? DataSource { get; set; }
        [MaxLength(2000)]
        public string? CalculationFormula { get; set; }
        public decimal? DefaultTargetValue { get; set; }
        public decimal? MinimumTargetValue { get; set; }
        public decimal? MaximumTargetValue { get; set; }
        public decimal DefaultWeight { get; set; }
        public bool IsQuantitative { get; set; } = true;
        public bool IsCascadable { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateKpiCatalogRequest : CreateKpiCatalogRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateKpiCatalogStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
