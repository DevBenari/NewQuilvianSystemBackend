using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    [Table("TrxPatientEncounter", Schema = "public")]
    public class TrxPatientEncounter : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EncounterNumber { get; set; } = string.Empty;

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

        public DateTime EncounterDate { get; set; } = DateTime.UtcNow;

        public EncounterType EncounterType { get; set; } = EncounterType.Outpatient;

        public VisitType VisitType { get; set; } = VisitType.NewVisit;

        public EncounterRegistrationSource RegistrationSource { get; set; } = EncounterRegistrationSource.FrontDesk;

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        public EncounterStatus EncounterStatus { get; set; } = EncounterStatus.Registered;

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

        public bool IsScreeningRequired { get; set; } = false;

        public bool IsQueueRequired { get; set; } = true;

        public bool IsDoctorRequired { get; set; } = true;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public Guid RegisteredByUserId { get; set; } = Guid.Empty;

        public DateTime? CheckedInAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? NoShowAt { get; set; }

        public Guid? NoShowByUserId { get; set; }

        [MaxLength(250)]
        public string? NoShowReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstDoctorSchedule? DoctorSchedule { get; set; }

        public MstDoctorServiceRule? DoctorServiceRule { get; set; }

        public MstPatientClass? PatientClass { get; set; }

        public MstPaymentMethod? PaymentMethod { get; set; }

        public MstPatientInsurance? PatientInsurance { get; set; }

        public MstInsuranceProvider? InsuranceProvider { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }

        public MstPatientCompanyGuarantor? PatientCompanyGuarantor { get; set; }

        public MstPatientMembership? PatientMembership { get; set; }

        public TrxKioskScanSession? KioskScanSession { get; set; }

        public ApplicationUser? RegisteredByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }

        public ApplicationUser? NoShowByUser { get; set; }
    }
}
