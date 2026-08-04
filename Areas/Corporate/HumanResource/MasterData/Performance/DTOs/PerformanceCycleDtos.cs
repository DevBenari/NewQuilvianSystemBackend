using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs
{

    public class PerformanceCycleSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int CurrentData { get; set; }
        public int LockedData { get; set; }
        public int OpenData { get; set; }
        public int CompletedData { get; set; }
    }

    public class PerformanceCycleResponse
    {
        public Guid Id { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string CycleCode { get; set; } = string.Empty;
        public string CycleName { get; set; } = string.Empty;
        public string CycleType { get; set; } = string.Empty;
        public int? PeriodYear { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string CycleStatus { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
        public int TemplateCount { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PerformanceCycleDetailResponse : PerformanceCycleResponse
    {
        public DateTime? GoalSettingStartDate { get; set; }
        public DateTime? GoalSettingEndDate { get; set; }
        public DateTime? MidReviewStartDate { get; set; }
        public DateTime? MidReviewEndDate { get; set; }
        public DateTime? FinalReviewStartDate { get; set; }
        public DateTime? FinalReviewEndDate { get; set; }
        public DateTime? CalibrationStartDate { get; set; }
        public DateTime? CalibrationEndDate { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PerformanceCycleOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CycleType { get; set; } = string.Empty;
        public string CycleStatus { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
    }

    public class PerformanceCycleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PerformanceCycleOptionResponse> Items { get; set; } = new();
    }

    public class PerformanceCycleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PerformanceCycleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PerformanceStringOptionResponse> CycleTypeOptions { get; set; } = new();
        public List<PerformanceStringOptionResponse> CycleStatusOptions { get; set; } = new();
        public List<PerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PerformanceCycleDefaultFilterResponse
    {
        public string? CycleType { get; set; }
        public string? CycleStatus { get; set; }
        public bool? IsCurrent { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "periodStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePerformanceCycleRequest
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        [Required, MaxLength(200)]
        public string CycleName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string CycleType { get; set; } = "Annual";
        public int? PeriodYear { get; set; }
        [Required]
        public DateTime PeriodStartDate { get; set; }
        [Required]
        public DateTime PeriodEndDate { get; set; }
        public DateTime? GoalSettingStartDate { get; set; }
        public DateTime? GoalSettingEndDate { get; set; }
        public DateTime? MidReviewStartDate { get; set; }
        public DateTime? MidReviewEndDate { get; set; }
        public DateTime? FinalReviewStartDate { get; set; }
        public DateTime? FinalReviewEndDate { get; set; }
        public DateTime? CalibrationStartDate { get; set; }
        public DateTime? CalibrationEndDate { get; set; }
        [Required, MaxLength(50)]
        public string CycleStatus { get; set; } = "Draft";
        public bool IsCurrent { get; set; }
        public bool IsLocked { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdatePerformanceCycleRequest : CreatePerformanceCycleRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePerformanceCycleStatusRequest
    {
        [Required, MaxLength(50)]
        public string CycleStatus { get; set; } = string.Empty;
        public bool? IsCurrent { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PerformanceStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PerformanceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
