using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs
{

    public class PerformanceTemplateDetailSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int RequiredData { get; set; }
        public decimal TotalWeight { get; set; }
    }

    public class PerformanceTemplateDetailResponse
    {
        public Guid Id { get; set; }
        public Guid PerformanceTemplateId { get; set; }
        public Guid? ParentDetailId { get; set; }
        public Guid? KpiCatalogId { get; set; }
        public Guid? CompetencyId { get; set; }
        public Guid? RatingScaleId { get; set; }
        public string DetailCode { get; set; } = string.Empty;
        public string DetailName { get; set; } = string.Empty;
        public string DetailType { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? MinimumTargetValue { get; set; }
        public decimal? MaximumTargetValue { get; set; }
        public string? MeasurementUnit { get; set; }
        public string ScoreMethod { get; set; } = string.Empty;
        public string? TargetDirection { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class PerformanceTemplateDetailDetailResponse : PerformanceTemplateDetailResponse
    {
        public string? EvidenceRequirement { get; set; }
        public bool AllowEmployeeComment { get; set; }
        public bool AllowReviewerComment { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class PerformanceTemplateDetailFilterMetadataResponse
    {
        public PerformanceTemplateDetailDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PerformanceStringOptionResponse> DetailTypeOptions { get; set; } = new();
        public List<PerformanceStringOptionResponse> ScoreMethodOptions { get; set; } = new();
        public List<PerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PerformanceTemplateDetailDefaultFilterResponse
    {
        public string? DetailType { get; set; }
        public string? ScoreMethod { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePerformanceTemplateDetailRequest
    {
        public Guid? ParentDetailId { get; set; }
        public Guid? KpiCatalogId { get; set; }
        public Guid? CompetencyId { get; set; }
        public Guid? RatingScaleId { get; set; }
        [Required, MaxLength(50)]
        public string DetailCode { get; set; } = string.Empty;
        [Required, MaxLength(250)]
        public string DetailName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string DetailType { get; set; } = "KPI";
        [MaxLength(1000)]
        public string? Description { get; set; }
        public decimal Weight { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? MinimumTargetValue { get; set; }
        public decimal? MaximumTargetValue { get; set; }
        [MaxLength(100)]
        public string? MeasurementUnit { get; set; }
        [Required, MaxLength(50)]
        public string ScoreMethod { get; set; } = "RatingScale";
        [MaxLength(50)]
        public string? TargetDirection { get; set; }
        [MaxLength(500)]
        public string? EvidenceRequirement { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool AllowEmployeeComment { get; set; } = true;
        public bool AllowReviewerComment { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class UpdatePerformanceTemplateDetailRequest : CreatePerformanceTemplateDetailRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePerformanceTemplateDetailStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
