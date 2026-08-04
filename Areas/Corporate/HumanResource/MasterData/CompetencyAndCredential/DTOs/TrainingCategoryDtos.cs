using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class TrainingCategorySummaryResponse
    {
        public int TotalTrainingCategory { get; set; }
        public int ActiveTrainingCategory { get; set; }
        public int InactiveTrainingCategory { get; set; }
        public int MandatoryCategory { get; set; }
        public int NonMandatoryCategory { get; set; }
    }

    public class TrainingCategoryResponse
    {
        public Guid Id { get; set; }
        public string TrainingCategoryCode { get; set; } = string.Empty;
        public string TrainingCategoryName { get; set; } = string.Empty;
        public bool IsMandatoryCategory { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TrainingCatalogCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class TrainingCategoryDetailResponse : TrainingCategoryResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class TrainingCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string TrainingCategoryCode { get; set; } = string.Empty;
        public string TrainingCategoryName { get; set; } = string.Empty;
        public bool IsMandatoryCategory { get; set; }
    }

    public class TrainingCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<TrainingCategoryOptionResponse> Items { get; set; } = new();
    }

    public class TrainingCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public TrainingCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<TrainingCategoryCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<TrainingCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class TrainingCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsMandatoryCategory { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "trainingCategoryName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class TrainingCategoryCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class TrainingCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateTrainingCategoryRequest
    {
        [Required, MaxLength(150)]
        public string TrainingCategoryName { get; set; } = string.Empty;
        public bool IsMandatoryCategory { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
    }

    public class UpdateTrainingCategoryRequest : CreateTrainingCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTrainingCategoryStatusRequest { public bool IsActive { get; set; } }

    public class TrainingCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string TrainingCategoryCode { get; set; } = string.Empty;
        public string TrainingCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
