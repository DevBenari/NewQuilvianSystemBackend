using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PrescribingDrugResponse
    {
        public Guid DrugId { get; set; }

        public string DrugCode { get; set; } = string.Empty;

        public string DrugName { get; set; } = string.Empty;

        public string? GenericName { get; set; }

        public string? BrandName { get; set; }

        public string? ManufacturerName { get; set; }

        public string? DrugForm { get; set; }

        public string? Strength { get; set; }

        public decimal? StrengthValue { get; set; }

        public string? Route { get; set; }

        public Guid DrugCategoryId { get; set; }

        public string DrugCategoryName { get; set; } = string.Empty;

        public Guid? BaseUnitMeasurementId { get; set; }

        public string? BaseUnitName { get; set; }

        public string? BaseUnitSymbol { get; set; }

        public Guid? DispenseUnitMeasurementId { get; set; }

        public string? DispenseUnitName { get; set; }

        public string? DispenseUnitSymbol { get; set; }

        public Guid? DefaultDoseUnitMeasurementId { get; set; }

        public string? DefaultDoseUnitName { get; set; }

        public string? DefaultDoseUnitSymbol { get; set; }

        public bool IsFormulary { get; set; }

        public bool IsGeneric { get; set; }

        public bool IsAntibiotic { get; set; }

        public bool IsNarcotic { get; set; }

        public bool IsPsychotropic { get; set; }

        public bool IsHighAlert { get; set; }

        public bool IsCompoundIngredientAllowed { get; set; }

        public bool IsAllowFractionalDispense { get; set; }

        public bool IsStockManaged { get; set; }

        public bool IsNeedPrescription { get; set; }

        public bool IsNeedApprovalFromDrug { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCode { get; set; }

        public string? TariffName { get; set; }

        public Guid? InsuranceTariffId { get; set; }

        public Guid? InsuranceCoverageRuleId { get; set; }

        public string PaymentTypeName { get; set; } = string.Empty;

        public string PricingSource { get; set; } = string.Empty;

        public bool IsCoverageApplicable { get; set; }

        public bool IsCovered { get; set; }

        public string CoverageStatus { get; set; } = string.Empty;

        public decimal CoveragePercent { get; set; }

        public decimal HospitalUnitPrice { get; set; }

        public decimal? ContractUnitPrice { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal CoveredAmount { get; set; }

        public decimal PatientPayAmount { get; set; }

        public bool IsNeedApproval { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; }

        public string? CoverageNote { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    public class PrescribingDrugDetailResponse : PrescribingDrugResponse
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
    }

    public class PrescribingDrugFilterMetadataResponse
    {
        public PrescribingDrugDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<PrescribingDrugSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<PrescribingDrugQueryParameterResponse> QueryParameters { get; set; } = new();
    }

    public class PrescribingDrugDefaultFilterResponse
    {
        public Guid EncounterId { get; set; }

        public Guid? DrugCategoryId { get; set; }

        public bool? IsFormulary { get; set; }

        public bool? IsGeneric { get; set; }

        public bool? IsAntibiotic { get; set; }

        public bool? IsHighAlert { get; set; }

        public bool? IsCompoundIngredientAllowed { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "drugName";

        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PrescribingDrugSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PrescribingDrugQueryParameterResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public string? Description { get; set; }
    }

    public class PrescribingDrugPagedResponse
    {
        public Guid EncounterId { get; set; }

        public string PaymentTypeName { get; set; } = string.Empty;

        public string? InsuranceProviderName { get; set; }

        public string? BenefitPlanName { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<PrescribingDrugResponse> Items { get; set; } = new();
    }
}
