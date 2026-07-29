using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs
{

    public class PerformanceRatingScaleSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int NumericData { get; set; }
    }

    public class PerformanceRatingScaleResponse
    {
        public Guid Id { get; set; }
        public string ScaleCode { get; set; } = string.Empty;
        public string ScaleName { get; set; } = string.Empty;
        public string ScaleType { get; set; } = string.Empty;
        public decimal MinimumScore { get; set; }
        public decimal MaximumScore { get; set; }
        public decimal? PassingScore { get; set; }
        public int DecimalPlaces { get; set; }
        public bool IsHigherScoreBetter { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int TemplateCount { get; set; }
        public int TemplateDetailCount { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PerformanceRatingScaleDetailResponse : PerformanceRatingScaleResponse
    {
        public string? RatingDefinitionJson { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PerformanceRatingScaleOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ScaleType { get; set; } = string.Empty;
        public decimal MinimumScore { get; set; }
        public decimal MaximumScore { get; set; }
        public decimal? PassingScore { get; set; }
    }

    public class PerformanceRatingScaleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PerformanceRatingScaleOptionResponse> Items { get; set; } = new();
    }

    public class PerformanceRatingScaleFilterMetadataResponse
    {
        public PerformanceRatingScaleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PerformanceStringOptionResponse> ScaleTypeOptions { get; set; } = new();
        public List<PerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PerformanceRatingScaleDefaultFilterResponse
    {
        public string? ScaleType { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "scaleName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePerformanceRatingScaleRequest
    {
        [Required, MaxLength(200)]
        public string ScaleName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string ScaleType { get; set; } = "Numeric";
        public decimal MinimumScore { get; set; } = 1m;
        public decimal MaximumScore { get; set; } = 5m;
        public decimal? PassingScore { get; set; }
        public int DecimalPlaces { get; set; } = 2;
        public bool IsHigherScoreBetter { get; set; } = true;
        public string? RatingDefinitionJson { get; set; }
        public bool IsDefault { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdatePerformanceRatingScaleRequest : CreatePerformanceRatingScaleRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePerformanceRatingScaleStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }
}
