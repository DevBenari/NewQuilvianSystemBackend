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
        public int NeedPrescriptionDrug { get; set; }
        public int CoveredByInsuranceDefaultDrug { get; set; }
        public int NeedApprovalDrug { get; set; }
        public int HasDefaultTariffDrug { get; set; }
    }

    public class DrugResponse
    {
        public Guid Id { get; set; }

        public Guid DrugCategoryId { get; set; }
        public string DrugCategoryCode { get; set; } = string.Empty;
        public string DrugCategoryName { get; set; } = string.Empty;
        public string? DrugCategoryType { get; set; }
        public string? DrugGroupName { get; set; }

        public Guid? DefaultTariffId { get; set; }
        public string? DefaultTariffCode { get; set; }
        public string? DefaultTariffName { get; set; }
        public decimal? DefaultTariffNormalPrice { get; set; }

        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }
        public string? ManufacturerName { get; set; }
        public string? DrugForm { get; set; }
        public string? Strength { get; set; }
        public string? BaseUnit { get; set; }
        public string? DispenseUnit { get; set; }
        public string? Route { get; set; }

        public bool IsFormulary { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsNeedPrescription { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsNeedApproval { get; set; }

        public decimal DefaultPrice { get; set; }
        public decimal? InsurancePrice { get; set; }
        public decimal? MemberPrice { get; set; }
        public decimal? CompanyPrice { get; set; }

        public string? ExternalDrugCode { get; set; }
        public string? IntegrationCode { get; set; }
        public string? BpomRegistrationNumber { get; set; }
        public string? NationalDrugCode { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
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
    }

    public class DrugOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DrugCategoryId { get; set; }
        public string DrugCategoryName { get; set; } = string.Empty;

        public Guid? DefaultTariffId { get; set; }
        public string? DefaultTariffName { get; set; }
        public decimal? DefaultTariffNormalPrice { get; set; }

        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }
        public string? DrugForm { get; set; }
        public string? Strength { get; set; }
        public string? BaseUnit { get; set; }
        public string? DispenseUnit { get; set; }
        public string? Route { get; set; }

        public bool IsFormulary { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsAntibiotic { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsPsychotropic { get; set; }
        public bool IsHighAlert { get; set; }
        public bool IsChronicDiseaseDrug { get; set; }
        public bool IsVaccine { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsNeedPrescription { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsNeedApproval { get; set; }

        public decimal DefaultPrice { get; set; }
        public decimal? InsurancePrice { get; set; }
        public decimal? MemberPrice { get; set; }
        public decimal? CompanyPrice { get; set; }
    }

    public class DrugFilterMetadataResponse
    {
        public DrugDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> DrugFormOptions { get; set; } = new();
        public List<string> RouteOptions { get; set; } = new();
    }

    public class DrugDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        public Guid? DrugCategoryId { get; set; }
        public Guid? DefaultTariffId { get; set; }
        public bool? HasDefaultTariff { get; set; }

        public string? GenericName { get; set; }
        public string? BrandName { get; set; }
        public string? ManufacturerName { get; set; }
        public string? DrugForm { get; set; }
        public string? BaseUnit { get; set; }
        public string? DispenseUnit { get; set; }
        public string? Route { get; set; }

        public bool? IsFormulary { get; set; }
        public bool? IsGeneric { get; set; }
        public bool? IsAntibiotic { get; set; }
        public bool? IsNarcotic { get; set; }
        public bool? IsPsychotropic { get; set; }
        public bool? IsHighAlert { get; set; }
        public bool? IsChronicDiseaseDrug { get; set; }
        public bool? IsVaccine { get; set; }
        public bool? IsConsumable { get; set; }
        public bool? IsNeedPrescription { get; set; }
        public bool? IsCoveredByInsuranceDefault { get; set; }
        public bool? IsNeedApproval { get; set; }

        public decimal? MinimumPrice { get; set; }
        public decimal? MaximumPrice { get; set; }

        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugRequest
    {
        [Required]
        public Guid DrugCategoryId { get; set; }

        public Guid? DefaultTariffId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DrugCode { get; set; } = string.Empty;

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

        [MaxLength(50)]
        public string? BaseUnit { get; set; }

        [MaxLength(50)]
        public string? DispenseUnit { get; set; }

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
        public bool IsNeedPrescription { get; set; } = true;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;
        public bool IsNeedApproval { get; set; } = false;

        [Range(0, 999999999999)]
        public decimal DefaultPrice { get; set; } = 0;

        [Range(0, 999999999999)]
        public decimal? InsurancePrice { get; set; }

        [Range(0, 999999999999)]
        public decimal? MemberPrice { get; set; }

        [Range(0, 999999999999)]
        public decimal? CompanyPrice { get; set; }

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

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDrugRequest
    {
        [Required]
        public Guid DrugCategoryId { get; set; }

        public Guid? DefaultTariffId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DrugCode { get; set; } = string.Empty;

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

        [MaxLength(50)]
        public string? BaseUnit { get; set; }

        [MaxLength(50)]
        public string? DispenseUnit { get; set; }

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
        public bool IsNeedPrescription { get; set; } = true;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;
        public bool IsNeedApproval { get; set; } = false;

        [Range(0, 999999999999)]
        public decimal DefaultPrice { get; set; } = 0;

        [Range(0, 999999999999)]
        public decimal? InsurancePrice { get; set; }

        [Range(0, 999999999999)]
        public decimal? MemberPrice { get; set; }

        [Range(0, 999999999999)]
        public decimal? CompanyPrice { get; set; }

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

        public bool IsActive { get; set; } = true;
    }

    public class DrugCreateResponse
    {
        public Guid Id { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public Guid DrugCategoryId { get; set; }
    }

    public class DrugUpdateResponse
    {
        public Guid Id { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DrugStatusResponse
    {
        public Guid Id { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
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
