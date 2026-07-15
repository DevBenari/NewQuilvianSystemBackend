using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionCompoundItemResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionCompoundId { get; set; }
        public string CompoundName { get; set; } = string.Empty;
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
        public bool IsAllowFractionalSource { get; set; }

        public CompoundIngredientCalculationMode CalculationMode { get; set; }
        public string CalculationModeName { get; set; } = string.Empty;
        public CompoundIngredientRole IngredientRole { get; set; }
        public string IngredientRoleName { get; set; } = string.Empty;
        public decimal? TargetValue { get; set; }
        public Guid? TargetUnitMeasurementId { get; set; }
        public string? TargetUnitNameSnapshot { get; set; }
        public string? TargetUnitSymbolSnapshot { get; set; }
        public string? TargetConcentrationUnit { get; set; }
        public decimal? CalculatedActiveAmount { get; set; }
        public Guid? CalculatedActiveUnitMeasurementId { get; set; }
        public string? CalculatedActiveUnitNameSnapshot { get; set; }
        public string? CalculatedActiveUnitSymbolSnapshot { get; set; }
        public decimal? SourceStrengthValue { get; set; }
        public Guid? SourceStrengthMeasurementId { get; set; }
        public string? SourceStrengthUnitNameSnapshot { get; set; }
        public string? SourceStrengthUnitSymbolSnapshot { get; set; }
        public decimal SourceContentQuantity { get; set; }
        public Guid? SourceContentUnitMeasurementId { get; set; }
        public string? SourceContentUnitNameSnapshot { get; set; }
        public string? SourceContentUnitSymbolSnapshot { get; set; }
        public decimal? TheoreticalSourceQuantity { get; set; }
        public decimal? VerifiedSourceQuantity { get; set; }
        public decimal PricingQuantity { get; set; }
        public bool IsQuantitySufficientToFinal { get; set; }
        public string CalculationStatus { get; set; } = string.Empty;
        public string? CalculationNote { get; set; }

        public decimal AmountPerPackage { get; set; }
        public decimal TotalQuantity { get; set; }
        public Guid? QuantityUnitMeasurementId { get; set; }
        public string? QuantityUnitNameSnapshot { get; set; }
        public string? QuantityUnitSymbolSnapshot { get; set; }
        public string? IngredientInstruction { get; set; }
        public decimal HospitalUnitPrice { get; set; }
        public decimal? ContractUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal HospitalTotalPrice { get; set; }
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

    public class PrescriptionCompoundItemDetailResponse : PrescriptionCompoundItemResponse
    {
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovalNote { get; set; }
    }

    public abstract class PrescriptionCompoundItemMutationRequestBase
    {
        public CompoundIngredientCalculationMode CalculationMode { get; set; }
            = CompoundIngredientCalculationMode.LegacySourceQuantity;

        public CompoundIngredientRole IngredientRole { get; set; }
            = CompoundIngredientRole.ActiveIngredient;

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? TargetValue { get; set; }

        public Guid? TargetUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? TargetUnitName { get; set; }

        [MaxLength(50)]
        public string? TargetConcentrationUnit { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? SourceStrengthValue { get; set; }

        public Guid? SourceStrengthMeasurementId { get; set; }

        [MaxLength(100)]
        public string? SourceStrengthUnitName { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? SourceContentQuantity { get; set; }

        public Guid? SourceContentUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? SourceContentUnitName { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? VerifiedSourceQuantity { get; set; }

        public bool IsQuantitySufficientToFinal { get; set; }

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal AmountPerPackage { get; set; } = 1;

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal TotalQuantity { get; set; } = 1;

        public Guid? QuantityUnitMeasurementId { get; set; }

        [MaxLength(500)]
        public string? IngredientInstruction { get; set; }

        public int SortOrder { get; set; }
    }

    public class CreatePrescriptionCompoundItemRequest : PrescriptionCompoundItemMutationRequestBase
    {
        [Required]
        public Guid PrescriptionCompoundId { get; set; }

        [Required]
        public Guid DrugId { get; set; }
    }

    public class UpdatePrescriptionCompoundItemRequest : PrescriptionCompoundItemMutationRequestBase
    {
    }

    public class ApprovePrescriptionCompoundItemRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class PrescriptionCompoundItemMutationResponse : PrescriptionCompoundItemResponse
    {
        public int IngredientCount { get; set; }
        public decimal CompoundTotalPrice { get; set; }
        public decimal CompoundCoveredAmount { get; set; }
        public decimal CompoundPatientPayAmount { get; set; }
        public int RegularItemCount { get; set; }
        public int CompoundCount { get; set; }
        public int CompoundIngredientCount { get; set; }
        public int TotalItemCount { get; set; }
        public decimal PrescriptionTotalPrice { get; set; }
        public decimal PrescriptionCoveredAmount { get; set; }
        public decimal PrescriptionPatientPayAmount { get; set; }
    }
}
