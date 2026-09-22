using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionResponse
    {
        public Guid Id { get; set; }
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
        public EncounterPaymentType PaymentTypeSnapshot { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public string? PaymentSourceNameSnapshot { get; set; }
        public string? InsuranceProviderNameSnapshot { get; set; }
        public string? BenefitPlanNameSnapshot { get; set; }
        public string? PatientClassNameSnapshot { get; set; }
        public PrescriptionStatus PrescriptionStatus { get; set; }
        public PrescriptionPaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// Keadaan pemenuhan resep, milik <c>PharmacyManagement</c>. <b>Hanya dibaca</b> dari
        /// sub-modul Rawat Inap - <c>RUL-DOK-01</c>.
        /// </summary>
        public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; }

        /// <summary>Jenis resep: rutin, harian, atau obat pulang - <c>BE-RWI-050</c>.</summary>
        public PrescriptionOrderType PrescriptionOrderType { get; set; }

        /// <summary>Perawatan rawat inap yang menaungi resep, bila ada.</summary>
        public Guid? InpEpisodeId { get; set; }

        public DateTime PrescriptionDateTime { get; set; }
        public int RegularItemCount { get; set; }
        public int CompoundCount { get; set; }
        public int CompoundIngredientCount { get; set; }
        public int TotalItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
        public bool IsReadyForCashier => PrescriptionStatus == PrescriptionStatus.Submitted;
        public bool IsReadyForPharmacy => FulfillmentStatus == PrescriptionFulfillmentStatus.ReadyForPharmacy;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PrescriptionDetailResponse : PrescriptionResponse
    {
        public Guid? PaymentSourceId { get; set; }
        public Guid? PatientInsuranceId { get; set; }
        public Guid? InsuranceProviderId { get; set; }
        public string? PolicyNumberSnapshot { get; set; }
        public string? BenefitPlanCodeSnapshot { get; set; }
        public string? ClinicalNote { get; set; }
        public string? DoctorInstruction { get; set; }
        public string? PharmacyNote { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public string? SubmittedByUserName { get; set; }
        public Guid? BillingId { get; set; }
        public DateTime? BillingGeneratedAt { get; set; }
        public DateTime? PaymentCompletedAt { get; set; }
        public Guid? PaymentCompletedByUserId { get; set; }
        public string? PaymentCompletedByUserName { get; set; }
        public DateTime? ReadyForPharmacyAt { get; set; }
        public Guid? PharmacyQueueId { get; set; }
        public DateTime? PharmacyQueuedAt { get; set; }
        public DateTime? PharmacyVerifiedAt { get; set; }
        public Guid? PharmacyVerifiedByUserId { get; set; }
        public string? PharmacyVerifiedByUserName { get; set; }
        public DateTime? PreparationStartedAt { get; set; }
        public DateTime? ReadyToDispenseAt { get; set; }
        public DateTime? DispensedAt { get; set; }
        public Guid? DispensedByUserId { get; set; }
        public string? DispensedByUserName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
    }

    public class PrescriptionOptionResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public Guid ConsultationId { get; set; }
        public PrescriptionStatus PrescriptionStatus { get; set; }
        public PrescriptionPaymentStatus PaymentStatus { get; set; }
        public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; }
        public DateTime PrescriptionDateTime { get; set; }
        public int TotalItemCount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class PrescriptionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PrescriptionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PrescriptionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PrescriptionEnumOptionResponse> PrescriptionStatusOptions { get; set; } = new();
        public List<PrescriptionEnumOptionResponse> PaymentStatusOptions { get; set; } = new();
        public List<PrescriptionEnumOptionResponse> FulfillmentStatusOptions { get; set; } = new();
    }

    public class PrescriptionDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public PrescriptionStatus? PrescriptionStatus { get; set; }
        public PrescriptionPaymentStatus? PaymentStatus { get; set; }
        public PrescriptionFulfillmentStatus? FulfillmentStatus { get; set; }
        public bool? IsNeedApproval { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "prescriptionDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PrescriptionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PrescriptionEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePrescriptionRequest
    {
        [Required]
        public Guid EncounterId { get; set; }
        [Required]
        public Guid ConsultationId { get; set; }
        public DateTime? PrescriptionDateTime { get; set; }

        /// <summary>
        /// Jenis resep menurut peruntukannya - <c>BE-RWI-050</c>, <c>RWI-DEC-046</c>.
        /// </summary>
        /// <remarks>
        /// Obat pulang menjadi jenis yang <b>eksplisit</b>, bukan disimpulkan dari waktu
        /// penulisan maupun dari status perawatan. Petugas farmasi harus dapat menyaringnya di
        /// layar mereka sendiri, dan menebaknya dari waktu akan salah pada pasien yang
        /// pemulangannya tertunda.
        /// </remarks>
        public PrescriptionOrderType PrescriptionOrderType { get; set; } = PrescriptionOrderType.Routine;

        /// <summary>
        /// Kunci permintaan. Opsional, tetapi sangat dianjurkan: tanpa kunci, percobaan ulang
        /// karena jaringan terputus melahirkan resep kedua beserta obatnya.
        /// </summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
        [MaxLength(1000)]
        public string? DoctorInstruction { get; set; }
    }

    public class UpdatePrescriptionRequest
    {
        public DateTime? PrescriptionDateTime { get; set; }
        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
        [MaxLength(1000)]
        public string? DoctorInstruction { get; set; }
    }

    public class PrescriptionCreateResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public Guid ConsultationId { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public PrescriptionOrderType PrescriptionOrderType { get; set; }
        public PrescriptionStatus PrescriptionStatus { get; set; }
        public PrescriptionPaymentStatus PaymentStatus { get; set; }
        public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; }
        public DateTime PrescriptionDateTime { get; set; }
        public int TotalItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public bool HasPrescription { get; set; }
        public int PrescriptionCount { get; set; }
        public string? PrescriptionText { get; set; }
    }

    public class PrescriptionUpdateResponse : PrescriptionCreateResponse { }

    // RJ-BIL-BE-002 / RJ-BIL-CONFLICT-006 keputusan author 1A:
    // MarkPrescriptionBillingGeneratedRequest dan MarkPrescriptionPaymentCompletedRequest
    // dihapus bersama endpoint finansial klinis yang memakainya.

    public class CancelPrescriptionRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu resep pada Resep Harian episode rawat inap — BE-RWI-099, api-contract 0.6.0 bagian
    /// 12.6. Seluruh isian <see cref="PrescriptionResponse"/> tetap ada, sehingga pemanggil lama
    /// endpoint yang sama tidak kehilangan satu field pun; yang bertambah hanya butir dan racikan.
    /// </summary>
    public class InpatientPrescriptionListItem : PrescriptionResponse
    {
        public List<InpatientPrescriptionItemResponse> Items { get; set; } = new();
        public List<InpatientPrescriptionCompoundResponse> Compounds { get; set; } = new();
    }

    /// <summary>
    /// Permintaan penghentian satu butir obat dari Resep Harian — BE-RWI-100, api-contract 0.6.0 bagian 12.6.
    /// </summary>
    public class StopPrescriptionItemRequest
    {
        /// <summary>
        /// Alasan penghentian obat. Wajib diisi (VAL-DOK-51a).
        /// </summary>
        [Required(ErrorMessage = "Alasan penghentian wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan penghentian maksimal 500 karakter.")]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Butir obat non-racikan pada Resep Harian, termasuk keadaan penghentiannya — BE-RWI-099.
    /// </summary>
    public class InpatientPrescriptionItemResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCodeSnapshot { get; set; } = string.Empty;
        public string DrugNameSnapshot { get; set; } = string.Empty;
        public string? GenericNameSnapshot { get; set; }
        public string? DrugFormSnapshot { get; set; }
        public string? StrengthSnapshot { get; set; }
        public string? RouteSnapshot { get; set; }
        public bool IsFormularySnapshot { get; set; }
        public bool IsHighAlertSnapshot { get; set; }
        public decimal Dose { get; set; }
        public string? DoseUnitNameSnapshot { get; set; }
        public string? FrequencyCode { get; set; }
        public string? FrequencyText { get; set; }
        public bool IsAsNeeded { get; set; }
        public string? Signa { get; set; }
        public string? AdministrationInstruction { get; set; }
        public decimal Quantity { get; set; }
        public string? DispenseUnitNameSnapshot { get; set; }
        public PrescriptionDoseKind DoseKind { get; set; }
        public bool IsStopped { get; set; }
        public DateTime? StoppedAt { get; set; }
        public Guid? StoppedByUserId { get; set; }
        public string? StoppedByName { get; set; }
        public string? StopReason { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Racikan pada Resep Harian beserta bahannya — BE-RWI-099 kriteria 3.
    /// </summary>
    public class InpatientPrescriptionCompoundResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public string CompoundName { get; set; } = string.Empty;
        public string? CompoundForm { get; set; }
        public decimal TotalPackage { get; set; }
        public string? PackageUnitNameSnapshot { get; set; }
        public decimal DosePerUse { get; set; }
        public string? DoseUnitNameSnapshot { get; set; }
        public string? FrequencyText { get; set; }
        public bool IsAsNeeded { get; set; }
        public string? Signa { get; set; }
        public string? AdministrationInstruction { get; set; }
        public int SortOrder { get; set; }
        public List<InpatientPrescriptionCompoundIngredientResponse> Ingredients { get; set; } = new();
    }

    /// <summary>
    /// Satu bahan racikan pada Resep Harian — BE-RWI-099.
    /// </summary>
    public class InpatientPrescriptionCompoundIngredientResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugNameSnapshot { get; set; } = string.Empty;
        public string? StrengthSnapshot { get; set; }
        public decimal AmountPerPackage { get; set; }
        public decimal TotalQuantity { get; set; }
        public string? QuantityUnitNameSnapshot { get; set; }
    }
}
