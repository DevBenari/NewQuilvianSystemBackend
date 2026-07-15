using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionCompoundResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string CompoundName { get; set; } = string.Empty;
        public string? CompoundForm { get; set; }
        public CompoundCalculationMode CalculationMode { get; set; }
        public string CalculationModeName { get; set; } = string.Empty;
        public decimal TotalPackage { get; set; }
        public Guid? PackageUnitMeasurementId { get; set; }
        public string? PackageUnitNameSnapshot { get; set; }
        public string? PackageUnitSymbolSnapshot { get; set; }
        public decimal? FinalQuantity { get; set; }
        public Guid? FinalQuantityMeasurementId { get; set; }
        public string? FinalQuantityUnitNameSnapshot { get; set; }
        public string? FinalQuantityUnitSymbolSnapshot { get; set; }
        public decimal DosePerUse { get; set; }
        public Guid? DoseUnitMeasurementId { get; set; }
        public string? DoseUnitNameSnapshot { get; set; }
        public string? DoseUnitSymbolSnapshot { get; set; }
        public string? FrequencyCode { get; set; }
        public string? FrequencyText { get; set; }
        public decimal? FrequencyPerDay { get; set; }
        public decimal? DurationValue { get; set; }
        public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        public string? AdministrationTime { get; set; }
        public string? Signa { get; set; }
        public string? CompoundingInstruction { get; set; }
        public string? AdministrationInstruction { get; set; }
        public string? DoctorNote { get; set; }
        public int IngredientCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PrescriptionCompoundDetailResponse : PrescriptionCompoundResponse
    {
        public List<PrescriptionCompoundItemResponse> Items { get; set; } = new();
    }

    public class PrescriptionCompoundFilterMetadataResponse
    {
        public PrescriptionCompoundDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PrescriptionCompoundSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PrescriptionCompoundCalculationModeOptionResponse> CalculationModes { get; set; } = new();
    }

    public class PrescriptionCompoundDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PrescriptionId { get; set; }
        public bool? IsNeedApproval { get; set; }
        public bool? IsApproved { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PrescriptionCompoundSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PrescriptionCompoundCalculationModeOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public abstract class PrescriptionCompoundMutationRequestBase
    {
        [Required]
        [MaxLength(200)]
        public string CompoundName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompoundForm { get; set; }

        public CompoundCalculationMode CalculationMode { get; set; }
            = CompoundCalculationMode.LegacySourceUnit;

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal TotalPackage { get; set; } = 1;

        public Guid? PackageUnitMeasurementId { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? FinalQuantity { get; set; }

        public Guid? FinalQuantityMeasurementId { get; set; }

        [MaxLength(100)]
        public string? FinalQuantityUnitName { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal DosePerUse { get; set; } = 1;

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(50)]
        public string? FrequencyCode { get; set; }

        [MaxLength(150)]
        public string? FrequencyText { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal? FrequencyPerDay { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal? DurationValue { get; set; }

        [MaxLength(30)]
        public string? DurationUnit { get; set; }

        public bool IsAsNeeded { get; set; }

        [MaxLength(250)]
        public string? AdministrationTime { get; set; }

        [MaxLength(500)]
        public string? Signa { get; set; }

        [MaxLength(1000)]
        public string? CompoundingInstruction { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? DoctorNote { get; set; }

        public int SortOrder { get; set; }
    }

    public class CreatePrescriptionCompoundRequest : PrescriptionCompoundMutationRequestBase
    {
        [Required]
        public Guid PrescriptionId { get; set; }
    }

    public class UpdatePrescriptionCompoundRequest : PrescriptionCompoundMutationRequestBase
    {
    }

    public class PrescriptionCompoundMutationResponse : PrescriptionCompoundResponse
    {
        public int RegularItemCount { get; set; }
        public int CompoundCount { get; set; }
        public int CompoundIngredientCount { get; set; }
        public int TotalItemCount { get; set; }
        public decimal PrescriptionTotalPrice { get; set; }
        public decimal PrescriptionCoveredAmount { get; set; }
        public decimal PrescriptionPatientPayAmount { get; set; }
    }
}
