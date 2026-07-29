using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{

    public class WfpPerformanceReviewDetailSummaryResponse
    {
        public int TotalDetail { get; set; }
        public int ActiveDetail { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal AverageFinalScore { get; set; }
    }

    public class WfpPerformanceReviewItemResponse
    {
        public Guid Id { get; set; }
        public Guid PerformanceReviewId { get; set; }
        public Guid? KpiCatalogId { get; set; }
        public Guid? PerformanceTemplateDetailId { get; set; }
        public string DetailType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? IndicatorCode { get; set; }
        public string IndicatorName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? Score { get; set; }
        public string? Rating { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class WfpPerformanceReviewItemDetailResponse : WfpPerformanceReviewItemResponse
    {
        public string? Description { get; set; }
        public string? EvidencePath { get; set; }
        public string? Comments { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class WfpPerformanceReviewDetailFilterMetadataResponse
    {
        public WfpPerformanceReviewDetailDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpPerformanceStringOptionResponse> DetailTypeOptions { get; set; } = new();
        public List<WfpPerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpPerformanceReviewDetailDefaultFilterResponse
    {
        public string? DetailType { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sequence";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateWfpPerformanceReviewDetailRequest
    {
        public Guid? KpiCatalogId { get; set; }
        public Guid? PerformanceTemplateDetailId { get; set; }
        [Required, MaxLength(40)]
        public string DetailType { get; set; } = "KPI";
        [MaxLength(150)]
        public string? Category { get; set; }
        [MaxLength(100)]
        public string? IndicatorCode { get; set; }
        [Required, MaxLength(250)]
        public string IndicatorName { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public decimal Weight { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? Score { get; set; }
        [MaxLength(100)]
        public string? Rating { get; set; }
        [MaxLength(1000)]
        public string? EvidencePath { get; set; }
        [MaxLength(3000)]
        public string? Comments { get; set; }
        public int Sequence { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpPerformanceReviewDetailRequest : CreateWfpPerformanceReviewDetailRequest
    {
    }

    public class UpdateWfpPerformanceReviewDetailStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
