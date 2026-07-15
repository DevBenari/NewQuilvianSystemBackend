using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionWorkspaceResponse
    {
        public Guid PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public Guid ConsultationId { get; set; }
        public string ConsultationNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public EncounterPaymentType PaymentType { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public string? PaymentSourceName { get; set; }
        public string? InsuranceProviderName { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? PatientClassName { get; set; }
        public PrescriptionStatus PrescriptionStatus { get; set; }
        public PrescriptionPaymentStatus PaymentStatus { get; set; }
        public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; }
        public DateTime PrescriptionDateTime { get; set; }
        public string? ClinicalNote { get; set; }
        public string? DoctorInstruction { get; set; }
        public PrescriptionConsultationContextResponse Consultation { get; set; } = new();
        public PrescriptionWorkspaceSummaryResponse Summary { get; set; } = new();
        public List<PrescriptionWorkspaceItemResponse> Items { get; set; } = new();
        public List<PrescriptionWorkspaceCompoundResponse> Compounds { get; set; } = new();
        public bool CanEdit { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }

    public class PrescriptionConsultationContextResponse
    {
        public string? ChiefComplaint { get; set; }
        public string? DiagnosisText { get; set; }
        public string? PrimaryDiagnosisText { get; set; }
        public string? SecondaryDiagnosisText { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }
    }

    public class PrescriptionWorkspaceSummaryResponse
    {
        public int RegularItemCount { get; set; }
        public int CompoundCount { get; set; }
        public int CompoundIngredientCount { get; set; }
        public int TotalItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
    }

    public class PrescriptionWorkspaceItemResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? DrugForm { get; set; }
        public string? Strength { get; set; }
        public decimal Dose { get; set; }
        public Guid? DoseUnitMeasurementId { get; set; }
        public string? DoseUnitName { get; set; }
        public string? DoseUnitSymbol { get; set; }
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
        public string? DispenseUnitName { get; set; }
        public string? DispenseUnitSymbol { get; set; }
        public Guid? TariffId { get; set; }
        public Guid? InsuranceTariffId { get; set; }
        public Guid? InsuranceCoverageRuleId { get; set; }
        public decimal PricingQuantity { get; set; }
        public decimal HospitalUnitPrice { get; set; }
        public decimal? ContractUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal HospitalTotalPrice { get; set; }
        public string PricingSource { get; set; } = string.Empty;
        public bool IsCoverageApplicable { get; set; }
        public bool IsCovered { get; set; }
        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public decimal CoPaymentAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public string? CoverageNote { get; set; }
        public int SortOrder { get; set; }
    }

    public class PrescriptionWorkspaceCompoundResponse
    {
        public Guid Id { get; set; }
        public string CompoundName { get; set; } = string.Empty;
        public string? CompoundForm { get; set; }
        public CompoundCalculationMode CalculationMode { get; set; }
        public string CalculationModeName { get; set; } = string.Empty;
        public decimal TotalPackage { get; set; }
        public Guid? PackageUnitMeasurementId { get; set; }
        public string? PackageUnitName { get; set; }
        public string? PackageUnitSymbol { get; set; }
        public decimal? FinalQuantity { get; set; }
        public Guid? FinalQuantityMeasurementId { get; set; }
        public string? FinalQuantityUnitName { get; set; }
        public string? FinalQuantityUnitSymbol { get; set; }
        public decimal DosePerUse { get; set; }
        public Guid? DoseUnitMeasurementId { get; set; }
        public string? DoseUnitName { get; set; }
        public string? DoseUnitSymbol { get; set; }
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
        public List<PrescriptionWorkspaceCompoundItemResponse> Items { get; set; } = new();
    }

    public class PrescriptionWorkspaceCompoundItemResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? DrugForm { get; set; }
        public string? Strength { get; set; }
        public bool IsAllowFractionalSource { get; set; }
        public CompoundIngredientCalculationMode CalculationMode { get; set; }
        public string CalculationModeName { get; set; } = string.Empty;
        public CompoundIngredientRole IngredientRole { get; set; }
        public string IngredientRoleName { get; set; } = string.Empty;
        public decimal? TargetValue { get; set; }
        public Guid? TargetUnitMeasurementId { get; set; }
        public string? TargetUnitName { get; set; }
        public string? TargetUnitSymbol { get; set; }
        public string? TargetConcentrationUnit { get; set; }
        public decimal? CalculatedActiveAmount { get; set; }
        public Guid? CalculatedActiveUnitMeasurementId { get; set; }
        public string? CalculatedActiveUnitName { get; set; }
        public string? CalculatedActiveUnitSymbol { get; set; }
        public decimal? SourceStrengthValue { get; set; }
        public Guid? SourceStrengthMeasurementId { get; set; }
        public string? SourceStrengthUnitName { get; set; }
        public string? SourceStrengthUnitSymbol { get; set; }
        public decimal SourceContentQuantity { get; set; }
        public Guid? SourceContentUnitMeasurementId { get; set; }
        public string? SourceContentUnitName { get; set; }
        public string? SourceContentUnitSymbol { get; set; }
        public decimal? TheoreticalSourceQuantity { get; set; }
        public decimal? VerifiedSourceQuantity { get; set; }
        public bool IsQuantitySufficientToFinal { get; set; }
        public string CalculationStatus { get; set; } = string.Empty;
        public string? CalculationNote { get; set; }
        public decimal AmountPerPackage { get; set; }
        public decimal TotalQuantity { get; set; }
        public Guid? QuantityUnitMeasurementId { get; set; }
        public string? QuantityUnitName { get; set; }
        public string? QuantityUnitSymbol { get; set; }
        public string? IngredientInstruction { get; set; }
        public Guid? TariffId { get; set; }
        public Guid? InsuranceTariffId { get; set; }
        public Guid? InsuranceCoverageRuleId { get; set; }
        public decimal PricingQuantity { get; set; }
        public decimal HospitalUnitPrice { get; set; }
        public decimal? ContractUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal HospitalTotalPrice { get; set; }
        public string PricingSource { get; set; } = string.Empty;
        public bool IsCoverageApplicable { get; set; }
        public bool IsCovered { get; set; }
        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public decimal CoPaymentAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public string? CoverageNote { get; set; }
        public int SortOrder { get; set; }
    }

    public class AutosavePrescriptionWorkspaceRequest
    {
        public DateTime? ExpectedUpdatedAt { get; set; }
        public DateTime? PrescriptionDateTime { get; set; }
        [MaxLength(1000)] public string? ClinicalNote { get; set; }
        [MaxLength(1000)] public string? DoctorInstruction { get; set; }
        public List<AutosavePrescriptionItemRequest> Items { get; set; } = new();
        public List<AutosavePrescriptionCompoundRequest> Compounds { get; set; } = new();
        public List<Guid> RemovedItemIds { get; set; } = new();
        public List<Guid> RemovedCompoundIds { get; set; } = new();
        public List<Guid> RemovedCompoundItemIds { get; set; } = new();
    }

    public class AutosavePrescriptionItemRequest
    {
        public Guid? Id { get; set; }
        [Required] public Guid DrugId { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal Dose { get; set; } = 1;
        public Guid? DoseUnitMeasurementId { get; set; }
        [MaxLength(50)] public string? FrequencyCode { get; set; }
        [MaxLength(150)] public string? FrequencyText { get; set; }
        [Range(typeof(decimal), "0", "999")] public decimal? FrequencyPerDay { get; set; }
        [Range(typeof(decimal), "0", "99999")] public decimal? DurationValue { get; set; }
        [MaxLength(30)] public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        [MaxLength(250)] public string? AdministrationTime { get; set; }
        [MaxLength(500)] public string? Signa { get; set; }
        [MaxLength(500)] public string? AdministrationInstruction { get; set; }
        [MaxLength(500)] public string? DoctorNote { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal Quantity { get; set; } = 1;
        public Guid? DispenseUnitMeasurementId { get; set; }
        public int SortOrder { get; set; }
    }

    public class AutosavePrescriptionCompoundRequest
    {
        public Guid? Id { get; set; }
        [Required, MaxLength(200)] public string CompoundName { get; set; } = string.Empty;
        [MaxLength(100)] public string? CompoundForm { get; set; }
        public CompoundCalculationMode CalculationMode { get; set; } = CompoundCalculationMode.LegacySourceUnit;
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal TotalPackage { get; set; } = 1;
        public Guid? PackageUnitMeasurementId { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal? FinalQuantity { get; set; }
        public Guid? FinalQuantityMeasurementId { get; set; }
        [MaxLength(100)] public string? FinalQuantityUnitName { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal DosePerUse { get; set; } = 1;
        public Guid? DoseUnitMeasurementId { get; set; }
        [MaxLength(50)] public string? FrequencyCode { get; set; }
        [MaxLength(150)] public string? FrequencyText { get; set; }
        [Range(typeof(decimal), "0", "999")] public decimal? FrequencyPerDay { get; set; }
        [Range(typeof(decimal), "0", "99999")] public decimal? DurationValue { get; set; }
        [MaxLength(30)] public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        [MaxLength(250)] public string? AdministrationTime { get; set; }
        [MaxLength(500)] public string? Signa { get; set; }
        [MaxLength(1000)] public string? CompoundingInstruction { get; set; }
        [MaxLength(500)] public string? AdministrationInstruction { get; set; }
        [MaxLength(500)] public string? DoctorNote { get; set; }
        public int SortOrder { get; set; }
        public List<AutosavePrescriptionCompoundItemRequest> Items { get; set; } = new();
    }

    public class AutosavePrescriptionCompoundItemRequest
    {
        public Guid? Id { get; set; }
        [Required] public Guid DrugId { get; set; }
        public CompoundIngredientCalculationMode CalculationMode { get; set; } = CompoundIngredientCalculationMode.LegacySourceQuantity;
        public CompoundIngredientRole IngredientRole { get; set; } = CompoundIngredientRole.ActiveIngredient;
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal? TargetValue { get; set; }
        public Guid? TargetUnitMeasurementId { get; set; }
        [MaxLength(100)] public string? TargetUnitName { get; set; }
        [MaxLength(50)] public string? TargetConcentrationUnit { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal? SourceStrengthValue { get; set; }
        public Guid? SourceStrengthMeasurementId { get; set; }
        [MaxLength(100)] public string? SourceStrengthUnitName { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal? SourceContentQuantity { get; set; }
        public Guid? SourceContentUnitMeasurementId { get; set; }
        [MaxLength(100)] public string? SourceContentUnitName { get; set; }
        public bool IsQuantitySufficientToFinal { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal AmountPerPackage { get; set; } = 1;
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal TotalQuantity { get; set; } = 1;
        public Guid? QuantityUnitMeasurementId { get; set; }
        [MaxLength(500)] public string? IngredientInstruction { get; set; }
        public int SortOrder { get; set; }
    }

    public class AutosavePrescriptionWorkspaceResponse
    {
        public Guid PrescriptionId { get; set; }
        public DateTime SavedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public PrescriptionWorkspaceSummaryResponse Summary { get; set; } = new();
        public List<AutosaveEntityIdResponse> ItemIds { get; set; } = new();
        public List<AutosaveCompoundIdResponse> CompoundIds { get; set; } = new();
    }

    public class AutosaveEntityIdResponse
    {
        public Guid? RequestId { get; set; }
        public Guid SavedId { get; set; }
    }

    public class AutosaveCompoundIdResponse : AutosaveEntityIdResponse
    {
        public List<AutosaveEntityIdResponse> ItemIds { get; set; } = new();
    }
}
