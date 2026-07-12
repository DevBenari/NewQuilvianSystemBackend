using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionItemResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public Guid DrugId { get; set; }
        public Guid? TariffId { get; set; }
        public Guid? InsuranceTariffId { get; set; }
        public Guid? InsuranceCoverageRuleId { get; set; }
        public string DrugCodeSnapshot { get; set; } = string.Empty;
        public string DrugNameSnapshot { get; set; } = string.Empty;
        public string? GenericNameSnapshot { get; set; }
        public string? DrugCategoryNameSnapshot { get; set; }
        public string? DrugFormSnapshot { get; set; }
        public string? StrengthSnapshot { get; set; }
        public string? RouteSnapshot { get; set; }
        public bool IsFormularySnapshot { get; set; }
        public bool IsGenericSnapshot { get; set; }
        public bool IsAntibioticSnapshot { get; set; }
        public bool IsNarcoticSnapshot { get; set; }
        public bool IsPsychotropicSnapshot { get; set; }
        public bool IsHighAlertSnapshot { get; set; }
        public decimal Dose { get; set; }
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
        public string? AdministrationInstruction { get; set; }
        public string? DoctorNote { get; set; }
        public decimal Quantity { get; set; }
        public Guid? DispenseUnitMeasurementId { get; set; }
        public string? DispenseUnitNameSnapshot { get; set; }
        public string? DispenseUnitSymbolSnapshot { get; set; }
        public decimal HospitalUnitPrice { get; set; }
        public decimal? ContractUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string PricingSource { get; set; } = string.Empty;
        public bool IsCoverageApplicable { get; set; }
        public bool IsCoveredByInsurance { get; set; }
        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public decimal CoPaymentAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
        public string? CoverageNote { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PrescriptionItemDetailResponse : PrescriptionItemResponse
    {
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovalNote { get; set; }
    }

    public class PrescriptionItemFilterMetadataResponse
    {
        public PrescriptionItemDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PrescriptionItemSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PrescriptionItemDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PrescriptionId { get; set; }
        public Guid? DrugId { get; set; }
        public bool? IsCoveredByInsurance { get; set; }
        public bool? IsNeedApproval { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsAntibiotic { get; set; }
        public bool? IsHighAlert { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PrescriptionItemSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePrescriptionItemRequest
    {
        [Required]
        public Guid PrescriptionId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal Dose { get; set; } = 1;

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(50)]
        public string? FrequencyCode { get; set; }

        [MaxLength(150)]
        public string? FrequencyText { get; set; }

        [Range(typeof(decimal), "0", "999")]
        public decimal? FrequencyPerDay { get; set; }

        [Range(typeof(decimal), "0", "99999")]
        public decimal? DurationValue { get; set; }

        [MaxLength(30)]
        public string? DurationUnit { get; set; }

        public bool IsAsNeeded { get; set; }

        [MaxLength(250)]
        public string? AdministrationTime { get; set; }

        [MaxLength(500)]
        public string? Signa { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? DoctorNote { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal Quantity { get; set; } = 1;

        public Guid? DispenseUnitMeasurementId { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdatePrescriptionItemRequest
    {
        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal Dose { get; set; } = 1;

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(50)]
        public string? FrequencyCode { get; set; }

        [MaxLength(150)]
        public string? FrequencyText { get; set; }

        [Range(typeof(decimal), "0", "999")]
        public decimal? FrequencyPerDay { get; set; }

        [Range(typeof(decimal), "0", "99999")]
        public decimal? DurationValue { get; set; }

        [MaxLength(30)]
        public string? DurationUnit { get; set; }

        public bool IsAsNeeded { get; set; }

        [MaxLength(250)]
        public string? AdministrationTime { get; set; }

        [MaxLength(500)]
        public string? Signa { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? DoctorNote { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal Quantity { get; set; } = 1;

        public Guid? DispenseUnitMeasurementId { get; set; }

        public int SortOrder { get; set; }
    }

    public class ApprovePrescriptionItemRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class PrescriptionItemMutationResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid DrugId { get; set; }
        public string DrugNameSnapshot { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public string CoverageStatus { get; set; } = string.Empty;
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
        public int RegularItemCount { get; set; }
        public int TotalItemCount { get; set; }
        public decimal PrescriptionTotalPrice { get; set; }
        public decimal PrescriptionCoveredAmount { get; set; }
        public decimal PrescriptionPatientPayAmount { get; set; }
        public bool PrescriptionNeedApproval { get; set; }
    }
}
