using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class PatientEncounterSummaryResponse
    {
        public int TotalEncounter { get; set; }
        public int RegisteredEncounter { get; set; }
        public int WaitingForNurseEncounter { get; set; }
        public int WaitingForDoctorEncounter { get; set; }
        public int CompletedEncounter { get; set; }
        public int CancelledEncounter { get; set; }
        public int NoShowEncounter { get; set; }
        public int InsuranceEncounter { get; set; }
        public int CompanyEncounter { get; set; }
        public int MembershipEncounter { get; set; }
        public int MixedPaymentEncounter { get; set; }
        public int ReferralEncounter { get; set; }
        public int EligibilityRequiredEncounter { get; set; }
        public int EligibilityCompletedEncounter { get; set; }
        public int FromKioskEncounter { get; set; }
    }

    public class PatientEncounterResponse
    {
        public Guid Id { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

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

        public Guid? AgeCategoryId { get; set; }

        public string? AgeCategoryCode { get; set; }

        public string? AgeCategoryName { get; set; }

        public int? AgeYearAtEncounter { get; set; }

        public int? AgeMonthAtEncounter { get; set; }

        public int? AgeDayAtEncounter { get; set; }

        public int? TotalAgeDaysAtEncounter { get; set; }

        public string? AgeTextAtEncounter { get; set; }

        public DateTime? AgeReferenceDate { get; set; }

        public DateTime? AgeCalculatedAt { get; set; }

        public Guid? PaymentMethodId { get; set; }

        public string? PaymentMethodName { get; set; }

        public DateTime EncounterDate { get; set; }

        public EncounterType EncounterType { get; set; }

        public string EncounterTypeName { get; set; } = string.Empty;

        public VisitType VisitType { get; set; }

        public string VisitTypeName { get; set; } = string.Empty;

        public EncounterRegistrationSource RegistrationSource { get; set; }

        public string RegistrationSourceName { get; set; } = string.Empty;

        public EncounterStatus EncounterStatus { get; set; }

        public string EncounterStatusName { get; set; } = string.Empty;

        public EncounterPaymentType PaymentType { get; set; }

        public string PaymentTypeName { get; set; } = string.Empty;

        public bool IsInsurancePatient { get; set; }

        public bool IsCompanyPatient { get; set; }

        public bool IsMembershipPatient { get; set; }

        public bool IsMixedPayment { get; set; }

        public string? PrimaryGuarantorNameSnapshot { get; set; }

        public string? PrimaryGuarantorTypeSnapshot { get; set; }

        public bool IsEligibilityRequired { get; set; }

        public bool IsEligibilityCompleted { get; set; }

        public bool IsReferral { get; set; }

        public bool IsReferralRequired { get; set; }

        public bool IsReferralVerified { get; set; }

        public bool IsNewPatient { get; set; }

        public bool IsFromKiosk { get; set; }

        public bool IsWalkIn { get; set; }

        public bool IsAppointment { get; set; }

        public bool IsScreeningRequired { get; set; }

        public bool IsQueueRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public DateTime RegisteredAt { get; set; }

        public Guid RegisteredByUserId { get; set; }

        public string? RegisteredByUserName { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime? NoShowAt { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }
    }

    public class PatientEncounterDetailResponse : PatientEncounterResponse
    {
        public Guid? KioskScanSessionId { get; set; }

        public string? ChiefComplaint { get; set; }

        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public string? ReferralNumber { get; set; }

        public Guid? NoShowByUserId { get; set; }

        public string? NoShowByUserName { get; set; }

        public string? NoShowReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public string? CancelReason { get; set; }

        public string? Notes { get; set; }

        public List<PatientEncounterGuarantorResponse> Guarantors { get; set; } = new();
    }

    public class PatientEncounterOptionResponse
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

        public Guid? AgeCategoryId { get; set; }

        public string? AgeCategoryName { get; set; }

        public string? AgeTextAtEncounter { get; set; }

        public EncounterStatus EncounterStatus { get; set; }

        public string EncounterStatusName { get; set; } = string.Empty;

        public DateTime EncounterDate { get; set; }

        public DateTime RegisteredAt { get; set; }
    }

    public class PatientEncounterOptionPagedResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<PatientEncounterOptionResponse> Items { get; set; } = new();
    }

    public class PatientEncounterFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public PatientEncounterDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<PatientEncounterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<PatientEncounterRelationFilterResponse> RelationFilters { get; set; } = new();

        public List<PatientEncounterSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> EncounterTypeOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> VisitTypeOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> RegistrationSourceOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> EncounterStatusOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> PaymentTypeOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> GuarantorTypeOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> GuarantorStatusOptions { get; set; } = new();

        public List<PatientEncounterEnumOptionResponse> GuarantorRoleOptions { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class PatientEncounterDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public EncounterStatus? EncounterStatus { get; set; }

        public EncounterType? EncounterType { get; set; }

        public EncounterPaymentType? PaymentType { get; set; }

        public bool? IsInsurancePatient { get; set; }

        public bool? IsCompanyPatient { get; set; }

        public bool? IsEligibilityRequired { get; set; }

        public bool? IsEligibilityCompleted { get; set; }

        public bool? IsReferral { get; set; }

        public bool? IsActive { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "registeredAt";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PatientEncounterRelationFilterResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;
    }

    public class PatientEncounterEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PatientEncounterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PatientEncounterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

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

        /// <summary>
        /// Tanggal kunjungan yang diminta.
        /// Jika kosong, backend memakai tanggal operasional hari ini.
        /// Untuk booking/appointment jadwal mingguan, field ini wajib dikirim.
        /// </summary>
        public DateTime? VisitDate { get; set; }

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

        public string EncounterStatusName { get; set; } = string.Empty;

        public Guid? QueueId { get; set; }

        public string? QueueCode { get; set; }

        public int? QueueNumber { get; set; }

        public QueueStatus? QueueStatus { get; set; }

        public string? QueueStatusName { get; set; }

        public DateTime EncounterDate { get; set; }

        public DateTime? QueueDate { get; set; }

        public bool IsFutureVisit { get; set; }

        public bool IsQueueCreated { get; set; }

        public bool IsScreeningRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public bool IsQueueRequired { get; set; }

        public Guid? AgeCategoryId { get; set; }

        public string? AgeCategoryCode { get; set; }

        public string? AgeCategoryName { get; set; }

        public int? AgeYearAtEncounter { get; set; }

        public int? AgeMonthAtEncounter { get; set; }

        public int? AgeDayAtEncounter { get; set; }

        public int? TotalAgeDaysAtEncounter { get; set; }

        public string? AgeTextAtEncounter { get; set; }

        public DateTime? AgeReferenceDate { get; set; }

        public DateTime? AgeCalculatedAt { get; set; }

        public int GuarantorCount { get; set; }

        public List<PatientEncounterGuarantorCreateResponse> Guarantors { get; set; } = new();
    }

    public class PatientEncounterStatusRequest
    {
        public EncounterStatus EncounterStatus { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }
    }

    public class PatientEncounterCancelRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }

    public class PatientEncounterNoShowRequest
    {
        [Required]
        [MaxLength(250)]
        public string NoShowReason { get; set; } = string.Empty;
    }

    public class DeletePatientEncounterRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

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

        public string GuarantorTypeName { get; set; } = string.Empty;

        public PatientEncounterGuarantorRole GuarantorRole { get; set; }

        public string GuarantorRoleName { get; set; } = string.Empty;

        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; }

        public string GuarantorStatusName { get; set; } = string.Empty;

        public PatientEncounterGuarantorCheckMethod CheckMethod { get; set; }

        public string CheckMethodName { get; set; } = string.Empty;

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

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }
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

        public string GuarantorTypeName { get; set; } = string.Empty;

        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; }

        public string GuarantorStatusName { get; set; } = string.Empty;

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

    public class DeletePatientEncounterGuarantorRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}
