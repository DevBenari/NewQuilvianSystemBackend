using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DrugSummaryResponse
    {
        public int TotalDrug { get; set; }
        public int ActiveDrug { get; set; }
        public int InactiveDrug { get; set; }
        public int FormularyDrug { get; set; }
        public int GenericDrug { get; set; }
        public int AntibioticDrug { get; set; }
        public int NarcoticDrug { get; set; }
        public int PsychotropicDrug { get; set; }
        public int HighAlertDrug { get; set; }
        public int ChronicDiseaseDrug { get; set; }
        public int VaccineDrug { get; set; }
        public int ConsumableDrug { get; set; }
        public int CompoundIngredientAllowedDrug { get; set; }
        public int StockManagedDrug { get; set; }
        public int BatchTrackedDrug { get; set; }
        public int ExpiryDateTrackedDrug { get; set; }
        public int FractionalDispenseAllowedDrug { get; set; }
        public int NeedPrescriptionDrug { get; set; }
        public int PrescribableDrug { get; set; }
        public int NeedApprovalDrug { get; set; }
    }

    public class DrugResponse
    {
        public Guid Id { get; set; }
        public Guid DrugCategoryId { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public string? DrugCategoryType { get; set; }
        public string? DrugGroupName { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }
        public string? ManufacturerName { get; set; }
        public string? DrugForm { get; set; }
        public string? DrugFormName { get; set; }
        public string? Strength { get; set; }
        public decimal? StrengthValue { get; set; }
        public Guid? StrengthMeasurementId { get; set; }
        public string? StrengthMeasurementCode { get; set; }
        public string? StrengthMeasurementName { get; set; }
        public string? StrengthMeasurementSymbol { get; set; }
        public Guid? BaseUnitMeasurementId { get; set; }
        public string? BaseUnitMeasurementCode { get; set; }
        public string? BaseUnitMeasurementName { get; set; }
        public string? BaseUnitMeasurementSymbol { get; set; }
        public Guid? DispenseUnitMeasurementId { get; set; }
        public string? DispenseUnitMeasurementCode { get; set; }
        public string? DispenseUnitMeasurementName { get; set; }
        public string? DispenseUnitMeasurementSymbol { get; set; }
        public Guid? PurchaseUnitMeasurementId { get; set; }
        public string? PurchaseUnitMeasurementCode { get; set; }
        public string? PurchaseUnitMeasurementName { get; set; }
        public string? PurchaseUnitMeasurementSymbol { get; set; }
        public Guid? StockUnitMeasurementId { get; set; }
        public string? StockUnitMeasurementCode { get; set; }
        public string? StockUnitMeasurementName { get; set; }
        public string? StockUnitMeasurementSymbol { get; set; }
        public Guid? DefaultDoseUnitMeasurementId { get; set; }
        public string? DefaultDoseUnitMeasurementCode { get; set; }
        public string? DefaultDoseUnitMeasurementName { get; set; }
        public string? DefaultDoseUnitMeasurementSymbol { get; set; }
        public string? Route { get; set; }
        public string? RouteName { get; set; }
        public bool IsFormulary { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsCompoundIngredientAllowed { get; set; }
        public bool IsStockManaged { get; set; }
        public bool IsBatchTracked { get; set; }
        public bool IsExpiryDateTracked { get; set; }
        public bool IsAllowFractionalDispense { get; set; }
        public bool IsNeedPrescription { get; set; }
        public bool IsPrescribable { get; set; }
        public bool IsNeedApproval { get; set; }
        public string? ExternalDrugCode { get; set; }
        public string? IntegrationCode { get; set; }
        public string? BpomRegistrationNumber { get; set; }
        public string? NationalDrugCode { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DrugDetailResponse : DrugResponse
    {
        public string? Indication { get; set; }
        public string? Contraindication { get; set; }
        public string? SideEffect { get; set; }
        public string? WarningPrecaution { get; set; }
        public string? DosageInformation { get; set; }
        public string? DrugInteraction { get; set; }
        public string? AdministrationInstruction { get; set; }
        public string? StorageInstruction { get; set; }
        public string? PregnancyCategory { get; set; }
        public string? LactationNote { get; set; }
        public string? PediatricNote { get; set; }
        public string? GeriatricNote { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DrugOptionResponse
    {
        public Guid Id { get; set; }
        public Guid DrugCategoryId { get; set; }
        public string DrugCategoryName { get; set; } = string.Empty;
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }
        public string? DrugForm { get; set; }
        public string? DrugFormName { get; set; }
        public string? Strength { get; set; }
        public decimal? StrengthValue { get; set; }
        public Guid? StrengthMeasurementId { get; set; }
        public string? StrengthMeasurementName { get; set; }
        public string? StrengthMeasurementSymbol { get; set; }
        public Guid? BaseUnitMeasurementId { get; set; }
        public string? BaseUnitMeasurementName { get; set; }
        public string? BaseUnitMeasurementSymbol { get; set; }
        public Guid? DispenseUnitMeasurementId { get; set; }
        public string? DispenseUnitMeasurementName { get; set; }
        public string? DispenseUnitMeasurementSymbol { get; set; }
        public Guid? PurchaseUnitMeasurementId { get; set; }
        public string? PurchaseUnitMeasurementName { get; set; }
        public string? PurchaseUnitMeasurementSymbol { get; set; }
        public Guid? StockUnitMeasurementId { get; set; }
        public string? StockUnitMeasurementName { get; set; }
        public string? StockUnitMeasurementSymbol { get; set; }
        public Guid? DefaultDoseUnitMeasurementId { get; set; }
        public string? DefaultDoseUnitMeasurementName { get; set; }
        public string? DefaultDoseUnitMeasurementSymbol { get; set; }
        public string? Route { get; set; }
        public string? RouteName { get; set; }
        public bool IsFormulary { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsCompoundIngredientAllowed { get; set; }
        public bool IsStockManaged { get; set; }
        public bool IsAllowFractionalDispense { get; set; }
        public bool IsNeedPrescription { get; set; }
        public bool IsPrescribable { get; set; }
        public bool IsNeedApproval { get; set; }
    }

    public class DrugClinicalInformationResponse
    {
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }
        public string? DrugForm { get; set; }
        public string? Strength { get; set; }
        public string? Route { get; set; }
        public string? Indication { get; set; }
        public string? Contraindication { get; set; }
        public string? SideEffect { get; set; }
        public string? WarningPrecaution { get; set; }
        public string? DosageInformation { get; set; }
        public string? DrugInteraction { get; set; }
        public string? AdministrationInstruction { get; set; }
        public string? StorageInstruction { get; set; }
        public string? PregnancyCategory { get; set; }
        public string? LactationNote { get; set; }
        public string? PediatricNote { get; set; }
        public string? GeriatricNote { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsAntibiotic { get; set; }
    }

    public class DrugOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DrugOptionResponse> Items { get; set; } = new();
    }

    public class DrugFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DrugDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DrugSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DrugStringOptionResponse> DrugFormOptions { get; set; } = new();
        public List<DrugStringOptionResponse> RouteOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class DrugDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DrugCategoryId { get; set; }
        public Guid? BaseUnitMeasurementId { get; set; }
        public Guid? StrengthMeasurementId { get; set; }
        public string? DrugForm { get; set; }
        public string? Route { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFormulary { get; set; }
        public bool? IsGeneric { get; set; }
        public bool? IsAntibiotic { get; set; }
        public bool? IsNarcotic { get; set; }
        public bool? IsPsychotropic { get; set; }
        public bool? IsHighAlert { get; set; }
        public bool? IsChronicDiseaseDrug { get; set; }
        public bool? IsVaccine { get; set; }
        public bool? IsConsumable { get; set; }
        public bool? IsCompoundIngredientAllowed { get; set; }
        public bool? IsStockManaged { get; set; }
        public bool? IsBatchTracked { get; set; }
        public bool? IsExpiryDateTracked { get; set; }
        public bool? IsAllowFractionalDispense { get; set; }
        public bool? IsNeedPrescription { get; set; }
        public bool? IsPrescribable { get; set; }
        public bool? IsNeedApproval { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugRequest
    {
        [Required]
        public Guid DrugCategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DrugName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? GenericName { get; set; }

        [MaxLength(200)]
        public string? BrandName { get; set; }

        [MaxLength(100)]
        public string? ManufacturerName { get; set; }

        [MaxLength(100)]
        public string? DrugForm { get; set; }

        [MaxLength(100)]
        public string? Strength { get; set; }

        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? StrengthValue { get; set; }

        public Guid? StrengthMeasurementId { get; set; }

        [MaxLength(50)]
        public Guid? BaseUnitMeasurementId { get; set; }
        public Guid? DispenseUnitMeasurementId { get; set; }
        public Guid? PurchaseUnitMeasurementId { get; set; }
        public Guid? StockUnitMeasurementId { get; set; }
        public Guid? DefaultDoseUnitMeasurementId { get; set; }

        [MaxLength(50)]
        public string? Route { get; set; }

        public bool IsFormulary { get; set; } = true;
        public bool IsGeneric { get; set; } = false;
        public bool IsAntibiotic { get; set; } = false;
        public bool IsNarcotic { get; set; } = false;
        public bool IsPsychotropic { get; set; } = false;
        public bool IsHighAlert { get; set; } = false;
        public bool IsChronicDiseaseDrug { get; set; } = false;
        public bool IsVaccine { get; set; } = false;
        public bool IsConsumable { get; set; } = false;
        public bool IsCompoundIngredientAllowed { get; set; } = true;
        public bool IsStockManaged { get; set; } = true;
        public bool IsBatchTracked { get; set; } = true;
        public bool IsExpiryDateTracked { get; set; } = true;
        public bool IsAllowFractionalDispense { get; set; } = false;
        public bool IsNeedPrescription { get; set; } = true;
        public bool IsPrescribable { get; set; } = true;
        public bool IsNeedApproval { get; set; } = false;

        [MaxLength(1000)]
        public string? Indication { get; set; }

        [MaxLength(1000)]
        public string? Contraindication { get; set; }

        [MaxLength(1000)]
        public string? SideEffect { get; set; }

        [MaxLength(1000)]
        public string? WarningPrecaution { get; set; }

        [MaxLength(1000)]
        public string? DosageInformation { get; set; }

        [MaxLength(1000)]
        public string? DrugInteraction { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? StorageInstruction { get; set; }

        [MaxLength(100)]
        public string? PregnancyCategory { get; set; }

        [MaxLength(250)]
        public string? LactationNote { get; set; }

        [MaxLength(250)]
        public string? PediatricNote { get; set; }

        [MaxLength(250)]
        public string? GeriatricNote { get; set; }

        [MaxLength(50)]
        public string? ExternalDrugCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        [MaxLength(50)]
        public string? BpomRegistrationNumber { get; set; }

        [MaxLength(50)]
        public string? NationalDrugCode { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDrugRequest : CreateDrugRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDrugStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteDrugRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DrugCreateResponse
    {
        public Guid Id { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public Guid DrugCategoryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DrugUpdateResponse : DrugCreateResponse
    {
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DrugDeleteResponse
    {
        public Guid Id { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}
