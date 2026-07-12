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
        public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; }
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

    public class MarkPrescriptionBillingGeneratedRequest
    {
        public Guid? BillingId { get; set; }
    }

    public class MarkPrescriptionPaymentCompletedRequest
    {
        public DateTime? CompletedAt { get; set; }
    }

    public class CancelPrescriptionRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
