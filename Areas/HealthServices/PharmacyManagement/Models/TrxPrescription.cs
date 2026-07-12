using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescription", Schema = "public")]
    public class TrxPrescription : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PrescriptionNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ConsultationId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? PaymentSourceId { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public EncounterPaymentType PaymentTypeSnapshot { get; set; } = EncounterPaymentType.Cash;

        [MaxLength(100)]
        public string? PatientClassNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? PaymentSourceNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? InsuranceProviderNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? PolicyNumberSnapshot { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCodeSnapshot { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanNameSnapshot { get; set; }

        public PrescriptionStatus PrescriptionStatus { get; set; } = PrescriptionStatus.Draft;

        public PrescriptionPaymentStatus PaymentStatus { get; set; } = PrescriptionPaymentStatus.NotBilled;

        public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; } = PrescriptionFulfillmentStatus.WaitingForClinicalFinalization;

        public DateTime PrescriptionDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public Guid? BillingId { get; set; }

        public DateTime? BillingGeneratedAt { get; set; }

        public DateTime? PaymentCompletedAt { get; set; }

        public Guid? PaymentCompletedByUserId { get; set; }

        public DateTime? ReadyForPharmacyAt { get; set; }

        public Guid? PharmacyQueueId { get; set; }

        public DateTime? PharmacyQueuedAt { get; set; }

        public DateTime? PharmacyVerifiedAt { get; set; }

        public Guid? PharmacyVerifiedByUserId { get; set; }

        public DateTime? PreparationStartedAt { get; set; }

        public DateTime? ReadyToDispenseAt { get; set; }

        public DateTime? DispensedAt { get; set; }

        public Guid? DispensedByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? DoctorInstruction { get; set; }

        [MaxLength(1000)]
        public string? PharmacyNote { get; set; }

        public int RegularItemCount { get; set; } = 0;

        public int CompoundCount { get; set; } = 0;

        public int CompoundIngredientCount { get; set; } = 0;

        public int TotalItemCount { get; set; } = 0;

        public decimal TotalPrice { get; set; } = 0;

        public decimal CoveredAmount { get; set; } = 0;

        public decimal PatientPayAmount { get; set; } = 0;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public MstPatient? Patient { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public TrxPatientEncounterGuarantor? PaymentSource { get; set; }

        public MstPatientInsurance? PatientInsurance { get; set; }

        public MstInsuranceProvider? InsuranceProvider { get; set; }

        public ApplicationUser? SubmittedByUser { get; set; }

        public ApplicationUser? PaymentCompletedByUser { get; set; }

        public ApplicationUser? PharmacyVerifiedByUser { get; set; }

        public ApplicationUser? DispensedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxPrescriptionItem> Items { get; set; } = new List<TrxPrescriptionItem>();

        public ICollection<TrxPrescriptionCompound> Compounds { get; set; } = new List<TrxPrescriptionCompound>();
    }
}
