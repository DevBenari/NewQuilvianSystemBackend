using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionCompoundItem", Schema = "public")]
    public class TrxPrescriptionCompoundItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionCompoundId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? InsuranceTariffId { get; set; }

        public Guid? InsuranceCoverageRuleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DrugCodeSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DrugNameSnapshot { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? GenericNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? DrugCategoryNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? DrugFormSnapshot { get; set; }

        [MaxLength(100)]
        public string? StrengthSnapshot { get; set; }

        [MaxLength(50)]
        public string? RouteSnapshot { get; set; }

        public bool IsFormularySnapshot { get; set; }

        public bool IsGenericSnapshot { get; set; }

        public bool IsAntibioticSnapshot { get; set; }

        public bool IsNarcoticSnapshot { get; set; }

        public bool IsPsychotropicSnapshot { get; set; }

        public bool IsHighAlertSnapshot { get; set; }

        public bool IsAllowFractionalSourceSnapshot { get; set; }

        public CompoundIngredientCalculationMode CalculationMode { get; set; }
            = CompoundIngredientCalculationMode.LegacySourceQuantity;

        public CompoundIngredientRole IngredientRole { get; set; }
            = CompoundIngredientRole.ActiveIngredient;

        /// <summary>
        /// Target klinis. Maknanya mengikuti CalculationMode:
        /// dosis per unit, persen, atau konsentrasi per satuan akhir.
        /// </summary>
        public decimal? TargetValue { get; set; }

        public Guid? TargetUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? TargetUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? TargetUnitSymbolSnapshot { get; set; }

        [MaxLength(50)]
        public string? TargetConcentrationUnit { get; set; }

        public decimal? CalculatedActiveAmount { get; set; }

        public Guid? CalculatedActiveUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? CalculatedActiveUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? CalculatedActiveUnitSymbolSnapshot { get; set; }

        public decimal? SourceStrengthValue { get; set; }

        public Guid? SourceStrengthMeasurementId { get; set; }

        [MaxLength(100)]
        public string? SourceStrengthUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? SourceStrengthUnitSymbolSnapshot { get; set; }

        /// <summary>
        /// Denominator strength sumber. Contoh 125 mg / 5 mL berarti nilai 5.
        /// Untuk tablet/kapsul umumnya 1 satuan sumber.
        /// </summary>
        public decimal SourceContentQuantity { get; set; } = 1;

        public Guid? SourceContentUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? SourceContentUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? SourceContentUnitSymbolSnapshot { get; set; }

        public decimal? TheoreticalSourceQuantity { get; set; }

        /// <summary>
        /// Quantity sumber hasil verifikasi farmasi. Jika null, pricing memakai
        /// quantity teoritis yang sudah menerapkan aturan pembulatan sediaan.
        /// </summary>
        public decimal? VerifiedSourceQuantity { get; set; }

        /// <summary>
        /// Quantity yang benar-benar dikirim ke perhitungan tarif/coverage.
        /// </summary>
        public decimal PricingQuantity { get; set; } = 1;

        public bool IsQuantitySufficientToFinal { get; set; }

        [MaxLength(50)]
        public string CalculationStatus { get; set; } = "Legacy";

        [MaxLength(1000)]
        public string? CalculationNote { get; set; }

        /// <summary>
        /// Field legacy tetap dipertahankan agar data dan integrasi lama tidak rusak.
        /// Pada mode baru berisi jumlah sumber teoritis per unit akhir.
        /// </summary>
        public decimal AmountPerPackage { get; set; } = 1;

        /// <summary>
        /// Field legacy tetap dipertahankan. Pada mode baru berisi total sumber teoritis.
        /// </summary>
        public decimal TotalQuantity { get; set; } = 1;

        public Guid? QuantityUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? QuantityUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? QuantityUnitSymbolSnapshot { get; set; }

        [MaxLength(500)]
        public string? IngredientInstruction { get; set; }

        public decimal HospitalUnitPrice { get; set; }

        public decimal? ContractUnitPrice { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        [MaxLength(50)]
        public string PricingSource { get; set; } = "HospitalTariff";

        public bool IsCoverageApplicable { get; set; }

        public bool IsCoveredByInsurance { get; set; }

        [MaxLength(50)]
        public string CoverageStatus { get; set; } = "NotApplicable";

        public decimal CoveragePercent { get; set; }

        public decimal CoveredAmount { get; set; }

        public decimal PatientPayAmount { get; set; }

        public decimal CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; }

        public bool IsApproved { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(250)]
        public string? ApprovalNote { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; }

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(1000)]
        public string? CoverageNote { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPrescriptionCompound? PrescriptionCompound { get; set; }

        public MstDrug? Drug { get; set; }

        public MstTariff? Tariff { get; set; }

        public MstInsuranceTariff? InsuranceTariff { get; set; }

        public MstInsuranceCoverageRule? InsuranceCoverageRule { get; set; }

        public MstMeasurement? QuantityUnitMeasurement { get; set; }

        public MstMeasurement? TargetUnitMeasurement { get; set; }

        public MstMeasurement? CalculatedActiveUnitMeasurement { get; set; }

        public MstMeasurement? SourceStrengthMeasurement { get; set; }

        public MstMeasurement? SourceContentUnitMeasurement { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
