using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class AgeCategorySummaryResponse
    {
        public int TotalAgeCategory { get; set; }
        public int ActiveAgeCategory { get; set; }
        public int InactiveAgeCategory { get; set; }
        public int DefaultAgeCategory { get; set; }
        public int SelectableInKiosk { get; set; }
        public int SelectableInRegistration { get; set; }
        public int UsedForClinicalRule { get; set; }
        public int WithOpenEndedRange { get; set; }
    }

    public class AgeCategoryResponse
    {
        public Guid Id { get; set; }
        public string AgeCategoryCode { get; set; } = string.Empty;
        public string AgeCategoryName { get; set; } = string.Empty;
        public string? AgeCategoryShortName { get; set; }
        public int MinAgeDays { get; set; }
        public int? MaxAgeDays { get; set; }
        public string AgeRangeLabel { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsSelectableInKiosk { get; set; }
        public bool IsSelectableInRegistration { get; set; }
        public bool IsUsedForClinicalRule { get; set; }
        public string? StandardReference { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class AgeCategoryDetailResponse : AgeCategoryResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class AgeCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string AgeCategoryCode { get; set; } = string.Empty;
        public string AgeCategoryName { get; set; } = string.Empty;
        public string? AgeCategoryShortName { get; set; }
        public int MinAgeDays { get; set; }
        public int? MaxAgeDays { get; set; }
        public string AgeRangeLabel { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
    }

    public class AgeCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AgeCategoryOptionResponse> Items { get; set; } = new();
    }

    public class AgeCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public AgeCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AgeCategoryCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<AgeCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<AgeCategoryQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<AgeCategoryFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<AgeCategoryFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class AgeCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsSelectableInKiosk { get; set; }
        public bool? IsSelectableInRegistration { get; set; }
        public bool? IsUsedForClinicalRule { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AgeCategoryCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class AgeCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AgeCategoryQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class AgeCategoryFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateAgeCategoryRequest
    {
        [Required]
        [MaxLength(150)]
        public string AgeCategoryName { get; set; } = string.Empty;

        [MaxLength(75)]
        public string? AgeCategoryShortName { get; set; }

        [Range(0, int.MaxValue)]
        public int MinAgeDays { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int? MaxAgeDays { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsSelectableInKiosk { get; set; } = true;

        public bool IsSelectableInRegistration { get; set; } = true;

        public bool IsUsedForClinicalRule { get; set; } = true;

        [MaxLength(250)]
        public string? StandardReference { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateAgeCategoryRequest : CreateAgeCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAgeCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteAgeCategoryRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class AgeCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string AgeCategoryCode { get; set; } = string.Empty;
        public string AgeCategoryName { get; set; } = string.Empty;
        public int MinAgeDays { get; set; }
        public int? MaxAgeDays { get; set; }
        public string AgeRangeLabel { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class AgeCategoryUpdateResponse
    {
        public Guid Id { get; set; }
        public string AgeCategoryCode { get; set; } = string.Empty;
        public string AgeCategoryName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class AgeCategoryStatusResponse
    {
        public Guid Id { get; set; }
        public string AgeCategoryCode { get; set; } = string.Empty;
        public string AgeCategoryName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class AgeCategoryDeleteResponse
    {
        public Guid Id { get; set; }
        public string AgeCategoryCode { get; set; } = string.Empty;
        public string AgeCategoryName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
