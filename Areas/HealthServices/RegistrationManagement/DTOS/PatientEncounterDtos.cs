using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
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

        public Guid? PatientInsuranceId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public Guid? CompanyGuarantorId { get; set; }

        public Guid? PatientCompanyGuarantorId { get; set; }

        public Guid? PatientMembershipId { get; set; }

        public Guid? KioskScanSessionId { get; set; }

        public EncounterType EncounterType { get; set; } = EncounterType.Outpatient;

        public VisitType VisitType { get; set; } = VisitType.NewVisit;

        public EncounterRegistrationSource RegistrationSource { get; set; } = EncounterRegistrationSource.FrontDesk;

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        [MaxLength(250)]
        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public bool IsNewPatient { get; set; } = false;

        public bool IsFromKiosk { get; set; } = false;

        public bool IsWalkIn { get; set; } = true;

        public bool IsAppointment { get; set; } = false;

        public bool IsReferral { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }
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

        public bool IsScreeningRequired { get; set; }

        public bool IsQueueRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class PatientEncounterDetailResponse : PatientEncounterResponse
    {
        public Guid? PatientInsuranceId { get; set; }

        public string? InsuranceProviderName { get; set; }

        public Guid? CompanyGuarantorId { get; set; }

        public string? CompanyGuarantorName { get; set; }

        public Guid? PatientMembershipId { get; set; }

        public string? MemberNumber { get; set; }

        public Guid? KioskScanSessionId { get; set; }

        public string? ChiefComplaint { get; set; }

        public string? ReferralNumber { get; set; }

        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? Notes { get; set; }
    }
}