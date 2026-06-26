using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class NurseStationQueueSummaryResponse
    {
        public int TotalQueue { get; set; }
        public int WaitingForNurseQueue { get; set; }
        public int CalledByNurseQueue { get; set; }
        public int InNurseScreeningQueue { get; set; }
        public int WaitingForDoctorQueue { get; set; }
        public int CompletedQueue { get; set; }
        public int SkippedQueue { get; set; }
        public int NoShowQueue { get; set; }
        public int PriorityQueue { get; set; }
    }

    public class NurseStationQueueResponse
    {
        public Guid Id { get; set; }
        public Guid EncounterId { get; set; }
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
        public Guid? NurseStationClusterId { get; set; }
        public string? NurseStationClusterName { get; set; }
        public DateTime QueueDate { get; set; }
        public int QueueNumber { get; set; }
        public string QueueCode { get; set; } = string.Empty;
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public int NurseCallAttemptCount { get; set; }
        public DateTime? LastNurseCalledAt { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public DateTime? ScreeningStartedAt { get; set; }
        public DateTime? ScreeningCompletedAt { get; set; }
        public int SkipCount { get; set; }
        public int RequeueCount { get; set; }
        public DateTime? NoShowAt { get; set; }
        public bool IsPriorityQueue { get; set; }
        public bool IsScreeningRequired { get; set; }
        public bool IsDoctorRequired { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }

        public string? ChiefComplaint { get; set; }
        public DateTime EncounterDate { get; set; }
        public DateTime RegisteredAt { get; set; }
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
        public string? ReferralNumber { get; set; }
        public string? EligibilityReferenceNumber { get; set; }
        public List<NurseStationQueueGuarantorResponse> Guarantors { get; set; } = new();

        public string PatientCode { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? GenderName { get; set; }
        public string? ReligionName { get; set; }
        public string? MaritalStatusName { get; set; }
        public string? BloodTypeName { get; set; }
        public string? IdentityTypeName { get; set; }
        public string? IdentityNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CountryName { get; set; }
        public string? ProvinceName { get; set; }
        public string? CityName { get; set; }
        public string? DistrictName { get; set; }
        public string? PostalCode { get; set; }
        public bool IsMember { get; set; }
        public Guid? ActivePatientMembershipId { get; set; }
        public Guid? DefaultMembershipTierId { get; set; }
        public string? DefaultMembershipTierName { get; set; }
        public bool IsNewborn { get; set; }
        public Guid? MotherPatientId { get; set; }
        public string? MotherMedicalRecordNumber { get; set; }
        public string? MotherPatientName { get; set; }
    }

    public class NurseStationQueueGuarantorResponse
    {
        public Guid Id { get; set; }
        public string EncounterGuarantorNumber { get; set; } = string.Empty;
        public PatientEncounterGuarantorType GuarantorType { get; set; }
        public string GuarantorTypeName { get; set; } = string.Empty;
        public PatientEncounterGuarantorRole GuarantorRole { get; set; }
        public string GuarantorRoleName { get; set; } = string.Empty;
        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; }
        public string GuarantorStatusName { get; set; } = string.Empty;
        public int CoveragePriority { get; set; }
        public bool IsPrimary { get; set; }
        public string? GuarantorNameSnapshot { get; set; }
        public string? PolicyNumberSnapshot { get; set; }
        public string? CardNumberSnapshot { get; set; }
        public string? MemberNumberSnapshot { get; set; }
        public string? PlanNameSnapshot { get; set; }
        public string? ClassNameSnapshot { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? InsuranceProviderName { get; set; }
        public string? CompanyGuarantorName { get; set; }
        public bool IsEligibilityRequired { get; set; }
        public bool IsEligible { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
    }

    public class NurseStationQueueActionRequest
    {
        [MaxLength(250)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class NurseStationQueueActionResponse
    {
        public Guid QueueId { get; set; }
        public Guid EncounterId { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public EncounterStatus EncounterStatus { get; set; }
        public string EncounterStatusName { get; set; } = string.Empty;
        public int NurseCallAttemptCount { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public DateTime? ScreeningStartedAt { get; set; }
        public DateTime? ScreeningCompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class NurseStationQueueFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public NurseStationQueueDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<NurseStationQueueSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<NurseStationQueueStatusOptionResponse> QueueStatusOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class NurseStationQueueDefaultFilterResponse
    {
        public DateTime? QueueDate { get; set; }
        public Guid? NurseStationClusterId { get; set; }
        public QueueStatus? QueueStatus { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "queueNumber";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NurseStationQueueSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class NurseStationQueueStatusOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
