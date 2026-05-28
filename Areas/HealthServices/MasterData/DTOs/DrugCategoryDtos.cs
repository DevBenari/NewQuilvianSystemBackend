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
        public int CoveredByInsuranceDefaultDrugCategory { get; set; }
    }

    public class DrugCategoryResponse
    {
        public Guid Id { get; set; }

        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;

        public string? DrugGroupName { get; set; }
        public string DrugCategoryType { get; set; } = string.Empty;

        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DrugCategoryDetailResponse : DrugCategoryResponse
    {
        public string? Description { get; set; }
    }

    public class DrugCategoryOptionResponse
    {
        public Guid Id { get; set; }

        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;

        public string? DrugGroupName { get; set; }
        public string DrugCategoryType { get; set; } = string.Empty;

        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
    }

    public class DrugCategoryFilterMetadataResponse
    {
        public DrugCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> DrugCategoryTypeOptions { get; set; } = new();
    }

    public class DrugCategoryDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        public string? DrugGroupName { get; set; }
        public string? DrugCategoryType { get; set; }

        public bool? IsAntibiotic { get; set; }
        public bool? IsNarcotic { get; set; }
        public bool? IsPsychotropic { get; set; }
        public bool? IsHighAlert { get; set; }
        public bool? IsChronicDiseaseDrug { get; set; }
        public bool? IsVaccine { get; set; }
        public bool? IsConsumable { get; set; }
        public bool? IsCoveredByInsuranceDefault { get; set; }

        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string DrugCategoryCode { get; set; } = string.Empty;

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
        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDrugCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string DrugCategoryCode { get; set; } = string.Empty;

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
        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DrugCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
    }

    public class DrugCategoryUpdateResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DrugCategoryStatusResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DrugCategoryDeleteResponse
    {
        public Guid Id { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}
