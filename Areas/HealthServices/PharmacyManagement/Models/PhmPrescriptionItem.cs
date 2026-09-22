using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("PhmPrescriptionItem", Schema = "public")]
    public class PhmPrescriptionItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionId { get; set; }

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

        public decimal Dose { get; set; } = 1;

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? DoseUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? DoseUnitSymbolSnapshot { get; set; }

        [MaxLength(50)]
        public string? FrequencyCode { get; set; }

        [MaxLength(150)]
        public string? FrequencyText { get; set; }

        public decimal? FrequencyPerDay { get; set; }

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

        public decimal Quantity { get; set; } = 1;

        public Guid? DispenseUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? DispenseUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? DispenseUnitSymbolSnapshot { get; set; }

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

        // =====================================================================
        // BE-RWI-099 / migration R4 — kamus data 0.5 bagian 13.1, RWI-DEC-121.
        //
        // Penghentian butir dari Resep Harian. Butir yang dihentikan TIDAK dihapus dan TIDAK
        // dibatalkan: ia tetap terbaca pada daftar beserta siapa, kapan, dan kenapa ia
        // dihentikan. Aksi penghentiannya sendiri milik BE-RWI-100, karena ia wajib membatalkan
        // dosis MAR dalam transaksi yang sama.
        // =====================================================================

        /// <summary>
        /// <c>true</c> bila dokter menghentikan butir ini dari Resep Harian. Tidak dapat kembali
        /// menjadi <c>false</c> — terapi baru ditulis sebagai butir resep baru.
        /// </summary>
        public bool IsStopped { get; set; } = false;

        /// <summary>Waktu penghentian. Wajib terisi bila <see cref="IsStopped"/> bernilai <c>true</c>.</summary>
        public DateTime? StoppedAt { get; set; }

        /// <summary>Dokter penghenti, diambil dari akun login.</summary>
        public Guid? StoppedByUserId { get; set; }

        /// <summary>
        /// Alasan penghentian. SENSITIF — tidak boleh masuk payload logger
        /// (permission-audit-matrix 0.6.0 bagian 9.2).
        /// </summary>
        [MaxLength(500)]
        public string? StopReason { get; set; }

        /// <summary>
        /// Jenis dosis butir. <c>SlidingScale</c> mewajibkan tepat satu order sliding scale.
        /// </summary>
        public PrescriptionDoseKind DoseKind { get; set; } = PrescriptionDoseKind.Fixed;

        public PhmPrescription? Prescription { get; set; }

        public MstDrug? Drug { get; set; }

        public MstTariff? Tariff { get; set; }

        public MstInsuranceTariff? InsuranceTariff { get; set; }

        public MstInsuranceCoverageRule? InsuranceCoverageRule { get; set; }

        public MstMeasurement? DoseUnitMeasurement { get; set; }

        public MstMeasurement? DispenseUnitMeasurement { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? StoppedByUser { get; set; }
    }
}
