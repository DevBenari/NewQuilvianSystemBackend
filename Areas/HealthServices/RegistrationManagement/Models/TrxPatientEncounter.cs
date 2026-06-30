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

        // =========================
        // PATIENT / SERVICE CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        public Guid? DoctorServiceRuleId { get; set; }

        public Guid? PatientClassId { get; set; }

        public Guid? KioskScanSessionId { get; set; }

        // =========================
        // AGE CATEGORY SNAPSHOT
        // =========================
        // Nilai ini dihitung saat kunjungan dibuat berdasarkan BirthDate pasien
        // dan EncounterDate/OperationalDate. Disimpan sebagai snapshot agar histori
        // kunjungan tidak berubah walaupun aturan MstAgeCategory berubah di kemudian hari.

        public Guid? AgeCategoryId { get; set; }

        public int? AgeYearAtEncounter { get; set; }

        public int? AgeMonthAtEncounter { get; set; }

        public int? AgeDayAtEncounter { get; set; }

        public int? TotalAgeDaysAtEncounter { get; set; }

        [MaxLength(100)]
        public string? AgeTextAtEncounter { get; set; }

        [MaxLength(50)]
        public string? AgeCategoryCodeSnapshot { get; set; }

        [MaxLength(150)]
        public string? AgeCategoryNameSnapshot { get; set; }

        public DateTime? AgeReferenceDate { get; set; }

        public DateTime? AgeCalculatedAt { get; set; }

        // =========================
        // ENCOUNTER INFORMATION
        // =========================

        public DateTime EncounterDate { get; set; } = DateTime.UtcNow;

        public EncounterType EncounterType { get; set; } = EncounterType.Outpatient;

        public VisitType VisitType { get; set; } = VisitType.NewVisit;

        public EncounterRegistrationSource RegistrationSource { get; set; } =
            EncounterRegistrationSource.FrontDesk;

        public EncounterStatus EncounterStatus { get; set; } = EncounterStatus.Registered;

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        // =========================
        // PAYMENT SUMMARY
        // =========================
        // Detail penjamin tidak lagi disimpan di encounter.
        // Detailnya masuk ke TrxPatientEncounterGuarantor.

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        public Guid? PaymentMethodId { get; set; }

        public bool IsInsurancePatient { get; set; } = false;

        public bool IsCompanyPatient { get; set; } = false;

        public bool IsMembershipPatient { get; set; } = false;

        public bool IsMixedPayment { get; set; } = false;

        [MaxLength(250)]
        public string? PrimaryGuarantorNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? PrimaryGuarantorTypeSnapshot { get; set; }

        [MaxLength(250)]
        public string? EligibilityReferenceNumber { get; set; }

        public DateTime? EligibilityCheckedAt { get; set; }

        public bool IsEligibilityRequired { get; set; } = false;

        public bool IsEligibilityCompleted { get; set; } = false;

        // =========================
        // REFERRAL SUMMARY
        // =========================
        // Detail rujukan nanti masuk ke TrxPatientReferral.
        // Encounter cukup menyimpan snapshot cepat.

        public bool IsReferral { get; set; } = false;

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        public bool IsReferralRequired { get; set; } = false;

        public bool IsReferralVerified { get; set; } = false;

        // =========================
        // REGISTRATION FLAGS
        // =========================

        public bool IsNewPatient { get; set; } = false;

        public bool IsFromKiosk { get; set; } = false;

        public bool IsWalkIn { get; set; } = true;

        public bool IsAppointment { get; set; } = false;

        public bool IsScreeningRequired { get; set; } = false;

        public bool IsQueueRequired { get; set; } = true;

        public bool IsDoctorRequired { get; set; } = true;

        // =========================
        // REGISTRATION TIMELINE
        // =========================

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

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstDoctorSchedule? DoctorSchedule { get; set; }

        public MstDoctorServiceRule? DoctorServiceRule { get; set; }

        public MstPatientClass? PatientClass { get; set; }

        public MstAgeCategory? AgeCategory { get; set; }

        public MstPaymentMethod? PaymentMethod { get; set; }

        public TrxKioskScanSession? KioskScanSession { get; set; }

        public ApplicationUser? RegisteredByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }

        public ApplicationUser? NoShowByUser { get; set; }

        public ICollection<TrxPatientEncounterGuarantor> EncounterGuarantors { get; set; } =
            new List<TrxPatientEncounterGuarantor>();
    }
}
