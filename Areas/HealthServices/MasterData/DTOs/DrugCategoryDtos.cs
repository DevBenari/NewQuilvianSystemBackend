using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DrugCategorySummaryResponse
    {
        public int TotalDrugCategory { get; set; }
        public int ActiveDrugCategory { get; set; }
        public int InactiveDrugCategory { get; set; }
        public int AntibioticDrugCategory { get; set; }
        public int NarcoticDrugCategory { get; set; }
        public int PsychotropicDrugCategory { get; set; }
        public int HighAlertDrugCategory { get; set; }
        public int ChronicDiseaseDrugCategory { get; set; }
        public int VaccineDrugCategory { get; set; }
        public int ConsumableDrugCategory { get; set; }
    }

    public class DrugCategoryResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public string? DrugGroupName { get; set; }
        public string DrugCategoryType { get; set; } = string.Empty;
        public string DrugCategoryTypeName { get; set; } = string.Empty;
        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DrugCategoryDetailResponse : DrugCategoryResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DrugCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public string? DrugGroupName { get; set; }
        public string DrugCategoryType { get; set; } = string.Empty;
        public string DrugCategoryTypeName { get; set; } = string.Empty;
        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public int SortOrder { get; set; }
    }

    public class DrugCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DrugCategoryOptionResponse> Items { get; set; } = new();
    }

    public class DrugCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public DrugCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugCategoryCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DrugCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DrugCategoryTypeOptionResponse> DrugCategoryTypeOptions { get; set; } = new();
        public List<DrugCategoryQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DrugCategoryFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DrugCategoryFormFieldMetadataResponse> UpdateFields { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class DrugCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? DrugCategoryType { get; set; }
        public bool? IsAntibiotic { get; set; }
        public bool? IsNarcotic { get; set; }
        public bool? IsPsychotropic { get; set; }
        public bool? IsHighAlert { get; set; }
        public bool? IsChronicDiseaseDrug { get; set; }
        public bool? IsVaccine { get; set; }
        public bool? IsConsumable { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugCategoryCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class DrugCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugCategoryTypeOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugCategoryQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class DrugCategoryFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool IsReadonly { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }
    }

    public class CreateDrugCategoryRequest
    {
        [Required]
        [MaxLength(150)]
        public string DrugCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DrugGroupName { get; set; }

        [Required]
        [MaxLength(50)]
        public string DrugCategoryType { get; set; } = "General";

        public bool IsAntibiotic { get; set; } = false;
        public bool IsNarcotic { get; set; } = false;
        public bool IsPsychotropic { get; set; } = false;
        public bool IsHighAlert { get; set; } = false;
        public bool IsChronicDiseaseDrug { get; set; } = false;
        public bool IsVaccine { get; set; } = false;
        public bool IsConsumable { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDrugCategoryRequest : CreateDrugCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDrugCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteDrugCategoryRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DrugCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public string? DrugGroupName { get; set; }
        public string DrugCategoryType { get; set; } = string.Empty;
        public string DrugCategoryTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class DrugCategoryUpdateResponse : DrugCategoryCreateResponse
    {
    }

    public class DrugCategoryDeleteResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}
