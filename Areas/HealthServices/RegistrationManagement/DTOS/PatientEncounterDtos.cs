using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    // =========================================================
    // PATIENT ENCOUNTER GUARANTOR DTOs
    // =========================================================

    public class PatientEncounterGuarantorRequest
    {
        public PatientEncounterGuarantorType GuarantorType { get; set; } = PatientEncounterGuarantorType.PatientCash;

        public PatientEncounterGuarantorRole GuarantorRole { get; set; } = PatientEncounterGuarantorRole.Primary;

        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; } = PatientEncounterGuarantorStatus.Draft;

        public PatientEncounterGuarantorCheckMethod CheckMethod { get; set; } = PatientEncounterGuarantorCheckMethod.None;

        [Range(1, 99)]
        public int CoveragePriority { get; set; } = 1;

        public bool IsPrimary { get; set; } = true;

        public Guid? PaymentMethodId { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public Guid? CompanyGuarantorId { get; set; }

        public Guid? PatientCompanyGuarantorId { get; set; }

        public Guid? PatientMembershipId { get; set; }

        [MaxLength(250)]
        public string? GuarantorNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? PolicyNumberSnapshot { get; set; }

        [MaxLength(100)]
        public string? CardNumberSnapshot { get; set; }

        [MaxLength(100)]
        public string? MemberNumberSnapshot { get; set; }

        [MaxLength(150)]
        public string? PlanNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? ClassNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCodeSnapshot { get; set; }

        public DateTime? EffectiveStartDateSnapshot { get; set; }

        public DateTime? EffectiveEndDateSnapshot { get; set; }

        public bool IsEligibilityRequired { get; set; } = false;

        public bool IsEligible { get; set; } = false;

        [MaxLength(250)]
        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        [MaxLength(250)]
        public string? VerificationReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? VerificationOfficerName { get; set; }

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        public bool IsNeedApproval { get; set; } = false;

        public bool IsNeedGuaranteeLetter { get; set; } = false;

        public bool IsNeedReferralLetter { get; set; } = false;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        public decimal? CoveragePercent { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? UsedLimitAmount { get; set; }

        public decimal? RoomLimitPerDayAmount { get; set; }

        public decimal? DeductibleAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public decimal? EstimatedCoveredAmount { get; set; }

        public decimal? EstimatedPatientPayAmount { get; set; }

        public bool IsPolicyActive { get; set; } = false;

        public bool? IsPremiumPaid { get; set; }

        public bool? IsCardActive { get; set; }

        public bool? IsInWaitingPeriod { get; set; }

        public DateTime? WaitingPeriodUntilDate { get; set; }

        public bool? HasSpecialExclusion { get; set; }

        [MaxLength(500)]
        public string? SpecialExclusionNote { get; set; }

        public bool? HasPreviousClaim { get; set; }

        [MaxLength(500)]
        public string? PreviousClaimNote { get; set; }

        [MaxLength(4000)]
        public string? BenefitSnapshotJson { get; set; }

        [MaxLength(4000)]
        public string? ManualCheckResultJson { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class PatientEncounterGuarantorResponse
    {
        public Guid Id { get; set; }

        public string EncounterGuarantorNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public PatientEncounterGuarantorType GuarantorType { get; set; }

        public PatientEncounterGuarantorRole GuarantorRole { get; set; }

        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; }

        public PatientEncounterGuarantorCheckMethod CheckMethod { get; set; }

        public int CoveragePriority { get; set; }

        public bool IsPrimary { get; set; }

        public Guid? PaymentMethodId { get; set; }

        public string? PaymentMethodName { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public string? InsuranceProviderName { get; set; }

        public Guid? CompanyGuarantorId { get; set; }

        public string? CompanyGuarantorName { get; set; }

        public Guid? PatientCompanyGuarantorId { get; set; }

        public Guid? PatientMembershipId { get; set; }

        public string? GuarantorNameSnapshot { get; set; }

        public string? PolicyNumberSnapshot { get; set; }

        public string? CardNumberSnapshot { get; set; }

        public string? MemberNumberSnapshot { get; set; }

        public string? PlanNameSnapshot { get; set; }

        public string? ClassNameSnapshot { get; set; }

        public string? BenefitPlanCodeSnapshot { get; set; }

        public bool IsEligibilityRequired { get; set; }

        public bool IsEligible { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public bool IsNeedApproval { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; }

        public bool IsNeedReferralLetter { get; set; }

        public bool IsAllowExcessPaymentByPatient { get; set; }

        public decimal? CoveragePercent { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? EstimatedCoveredAmount { get; set; }

        public decimal? EstimatedPatientPayAmount { get; set; }

        public bool IsPolicyActive { get; set; }

        public bool? IsPremiumPaid { get; set; }

        public bool? IsCardActive { get; set; }

        public bool? IsInWaitingPeriod { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class PatientEncounterGuarantorDetailResponse : PatientEncounterGuarantorResponse
    {
        public DateTime? EffectiveStartDateSnapshot { get; set; }

        public DateTime? EffectiveEndDateSnapshot { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public string? VerifiedByUserName { get; set; }

        public string? VerificationReferenceNumber { get; set; }

        public string? VerificationOfficerName { get; set; }

        public string? VerificationNote { get; set; }

        public decimal? UsedLimitAmount { get; set; }

        public decimal? RoomLimitPerDayAmount { get; set; }

        public decimal? DeductibleAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public DateTime? WaitingPeriodUntilDate { get; set; }

        public bool? HasSpecialExclusion { get; set; }

        public string? SpecialExclusionNote { get; set; }

        public bool? HasPreviousClaim { get; set; }

        public string? PreviousClaimNote { get; set; }

        public string? BenefitSnapshotJson { get; set; }

        public string? ManualCheckResultJson { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public string? CancelReason { get; set; }

        public string? Notes { get; set; }
    }

    public class PatientEncounterGuarantorCreateResponse
    {
        public Guid Id { get; set; }

        public string EncounterGuarantorNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public PatientEncounterGuarantorType GuarantorType { get; set; }

        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; }

        public int CoveragePriority { get; set; }

        public bool IsPrimary { get; set; }

        public string? GuarantorNameSnapshot { get; set; }
    }

    public class PatientEncounterGuarantorUpdateResponse : PatientEncounterGuarantorCreateResponse
    {
        public bool IsEligible { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }
    }

    public class VerifyPatientEncounterGuarantorRequest
    {
        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; } = PatientEncounterGuarantorStatus.Eligible;

        public PatientEncounterGuarantorCheckMethod CheckMethod { get; set; } = PatientEncounterGuarantorCheckMethod.Phone;

        public bool IsEligible { get; set; } = true;

        public bool IsPolicyActive { get; set; } = true;

        public bool? IsPremiumPaid { get; set; }

        public bool? IsCardActive { get; set; }

        [MaxLength(250)]
        public string? EligibilityReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? VerificationReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? VerificationOfficerName { get; set; }

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? UsedLimitAmount { get; set; }

        public bool? IsInWaitingPeriod { get; set; }

        public DateTime? WaitingPeriodUntilDate { get; set; }

        public bool? HasSpecialExclusion { get; set; }

        [MaxLength(500)]
        public string? SpecialExclusionNote { get; set; }

        public bool IsNeedApproval { get; set; } = false;

        public bool IsNeedGuaranteeLetter { get; set; } = false;

        public bool IsNeedReferralLetter { get; set; } = false;

        [MaxLength(4000)]
        public string? ManualCheckResultJson { get; set; }
    }

    public class CancelPatientEncounterGuarantorRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }

    // =========================================================
    // PATIENT ENCOUNTER DTOs
    // =========================================================

    public class PatientEncounterCreateRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        public Guid? DoctorServiceRuleId { get; set; }

        public Guid? PatientClassId { get; set; }

        public Guid? PaymentMethodId { get; set; }

        public Guid? KioskScanSessionId { get; set; }

        public EncounterType EncounterType { get; set; } = EncounterType.Outpatient;

        public VisitType VisitType { get; set; } = VisitType.NewVisit;

        public EncounterRegistrationSource RegistrationSource { get; set; } = EncounterRegistrationSource.FrontDesk;

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        public bool IsReferral { get; set; } = false;

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        public bool IsReferralRequired { get; set; } = false;

        public bool IsReferralVerified { get; set; } = false;

        [MaxLength(250)]
        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public bool IsEligibilityRequired { get; set; } = false;

        public bool IsEligibilityCompleted { get; set; } = false;

        public bool IsNewPatient { get; set; } = false;

        public bool IsFromKiosk { get; set; } = false;

        public bool IsWalkIn { get; set; } = true;

        public bool IsAppointment { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<PatientEncounterGuarantorRequest> Guarantors { get; set; } = new();
    }

    public class PatientEncounterCreateResponse
    {
        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public EncounterStatus EncounterStatus { get; set; }

        public Guid? QueueId { get; set; }

        public string? QueueCode { get; set; }

        public int? QueueNumber { get; set; }

        public QueueStatus? QueueStatus { get; set; }

        public bool IsQueueCreated { get; set; }

        public bool IsScreeningRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public bool IsQueueRequired { get; set; }

        public int GuarantorCount { get; set; }

        public List<PatientEncounterGuarantorCreateResponse> Guarantors { get; set; } = new();
    }

    public class PatientEncounterResponse
    {
        public Guid Id { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid ServiceUnitId { get; set; }

        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid? ClinicId { get; set; }

        public string? ClinicName { get; set; }

        public Guid? DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        public Guid? DoctorServiceRuleId { get; set; }

        public Guid? PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public Guid? PaymentMethodId { get; set; }

        public string? PaymentMethodName { get; set; }

        public EncounterType EncounterType { get; set; }

        public VisitType VisitType { get; set; }

        public EncounterRegistrationSource RegistrationSource { get; set; }

        public EncounterPaymentType PaymentType { get; set; }

        public EncounterStatus EncounterStatus { get; set; }

        public DateTime EncounterDate { get; set; }

        public DateTime RegisteredAt { get; set; }

        public bool IsInsurancePatient { get; set; }

        public bool IsCompanyPatient { get; set; }

        public bool IsMembershipPatient { get; set; }

        public bool IsMixedPayment { get; set; }

        public string? PrimaryGuarantorNameSnapshot { get; set; }

        public string? PrimaryGuarantorTypeSnapshot { get; set; }

        public bool IsEligibilityRequired { get; set; }

        public bool IsEligibilityCompleted { get; set; }

        public bool IsReferral { get; set; }

        public bool IsScreeningRequired { get; set; }

        public bool IsQueueRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class PatientEncounterDetailResponse : PatientEncounterResponse
    {
        public Guid? KioskScanSessionId { get; set; }

        public string? ChiefComplaint { get; set; }

        public string? ReferralNumber { get; set; }

        public bool IsReferralRequired { get; set; }

        public bool IsReferralVerified { get; set; }

        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? NoShowAt { get; set; }

        public string? NoShowReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }

        public string? Notes { get; set; }

        public List<PatientEncounterGuarantorResponse> Guarantors { get; set; } = new();
    }
}
