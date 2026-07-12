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
        public int CashEncounter { get; set; }
        public int InsuranceEncounter { get; set; }
        public int ReferralEncounter { get; set; }
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

        public Guid? RoomId { get; set; }

        public string? RoomCode { get; set; }

        public string? RoomName { get; set; }

        public string? RoomNumber { get; set; }

        public string? RoomLocationName { get; set; }

        public string? RoomFloorName { get; set; }

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

        public string? PaymentSourceNameSnapshot { get; set; }

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

        public string? ReferralNumber { get; set; }

        public Guid? NoShowByUserId { get; set; }

        public string? NoShowByUserName { get; set; }

        public string? NoShowReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public string? CancelReason { get; set; }

        public string? Notes { get; set; }

        public PatientEncounterPaymentResponse? Payment { get; set; }
    }

    public class PatientEncounterPaymentResponse
    {
        public Guid Id { get; set; }

        public string PaymentSourceNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public EncounterPaymentType PaymentType { get; set; }

        public string PaymentTypeName { get; set; } = string.Empty;

        public Guid? PaymentMethodId { get; set; }

        public string? PaymentMethodName { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public string? InsuranceProviderName { get; set; }

        public string? PolicyNumberSnapshot { get; set; }

        public string? CardNumberSnapshot { get; set; }

        public string? MemberNumberSnapshot { get; set; }

        public string? PlanNameSnapshot { get; set; }

        public string? ClassNameSnapshot { get; set; }

        public string? BenefitPlanCodeSnapshot { get; set; }

        public DateTime? EffectiveStartDateSnapshot { get; set; }

        public DateTime? EffectiveEndDateSnapshot { get; set; }

        public bool IsEligible { get; set; }

        public bool IsPolicyActive { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
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

        public Guid? RoomId { get; set; }

        public string? RoomName { get; set; }

        public string? RoomNumber { get; set; }

        public Guid? DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public Guid? AgeCategoryId { get; set; }

        public string? AgeCategoryName { get; set; }

        public string? AgeTextAtEncounter { get; set; }

        public EncounterStatus EncounterStatus { get; set; }

        public string EncounterStatusName { get; set; } = string.Empty;

        public EncounterPaymentType PaymentType { get; set; }

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

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class PatientEncounterDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public EncounterStatus? EncounterStatus { get; set; }

        public EncounterType? EncounterType { get; set; }

        public EncounterPaymentType? PaymentType { get; set; }

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

        /// <summary>
        /// Ruangan pelayanan encounter. Jika tidak diisi dan DoctorScheduleId memiliki
        /// RoomId, backend akan menggunakan ruangan dari jadwal dokter.
        /// </summary>
        public Guid? RoomId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        public Guid? DoctorServiceRuleId { get; set; }

        public Guid? PatientClassId { get; set; }

        /// <summary>
        /// Diisi hanya ketika PaymentType = Tunai.
        /// </summary>
        public Guid? PaymentMethodId { get; set; }

        /// <summary>
        /// Diisi hanya ketika PaymentType = Asuransi dan harus merupakan
        /// MstPatientInsurance aktif milik PatientId yang sama.
        /// </summary>
        public Guid? PatientInsuranceId { get; set; }

        public Guid? KioskScanSessionId { get; set; }

        /// <summary>
        /// Jika kosong backend memakai tanggal operasional hari ini.
        /// </summary>
        public DateTime? VisitDate { get; set; }

        public EncounterType EncounterType { get; set; } = EncounterType.Outpatient;

        public VisitType VisitType { get; set; } = VisitType.NewVisit;

        public EncounterRegistrationSource RegistrationSource { get; set; } =
            EncounterRegistrationSource.FrontDesk;

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        public bool IsReferral { get; set; } = false;

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        public bool IsReferralRequired { get; set; } = false;

        public bool IsReferralVerified { get; set; } = false;

        public bool IsNewPatient { get; set; } = false;

        public bool IsFromKiosk { get; set; } = false;

        public bool IsWalkIn { get; set; } = true;

        public bool IsAppointment { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }
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

        public Guid? RoomId { get; set; }

        public string? RoomCode { get; set; }

        public string? RoomName { get; set; }

        public string? RoomNumber { get; set; }

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

        public PatientEncounterPaymentResponse Payment { get; set; } = new();
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

    public class DeletePatientEncounterRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}
