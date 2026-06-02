using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    [Table("TrxPatientEncounterGuarantor", Schema = "public")]
    public class TrxPatientEncounterGuarantor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EncounterGuarantorNumber { get; set; } = string.Empty;

        // =========================
        // PARENT CONTEXT
        // =========================

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        // =========================
        // GUARANTOR CLASSIFICATION
        // =========================

        public PatientEncounterGuarantorType GuarantorType { get; set; } =
            PatientEncounterGuarantorType.PatientCash;

        public PatientEncounterGuarantorRole GuarantorRole { get; set; } =
            PatientEncounterGuarantorRole.Primary;

        public PatientEncounterGuarantorStatus GuarantorStatus { get; set; } =
            PatientEncounterGuarantorStatus.Draft;

        public PatientEncounterGuarantorCheckMethod CheckMethod { get; set; } =
            PatientEncounterGuarantorCheckMethod.None;

        public int CoveragePriority { get; set; } = 1;

        public bool IsPrimary { get; set; } = true;

        public bool IsActive { get; set; } = true;

        // =========================
        // PAYMENT / GUARANTOR REFERENCES
        // =========================

        public Guid? PaymentMethodId { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public Guid? CompanyGuarantorId { get; set; }

        public Guid? PatientCompanyGuarantorId { get; set; }

        public Guid? PatientMembershipId { get; set; }

        // =========================
        // SNAPSHOT INFORMATION
        // =========================
        // Snapshot penting supaya data encounter tidak berubah
        // walaupun master insurance/company/patient insurance berubah.

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

        // =========================
        // ELIGIBILITY / VERIFICATION
        // =========================

        public bool IsEligibilityRequired { get; set; } = false;

        public bool IsEligible { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

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

        // =========================
        // BENEFIT / LIMIT SNAPSHOT
        // =========================

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

        // =========================
        // RISK / SPECIAL CONDITION
        // =========================

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

        // =========================
        // FLEXIBLE SNAPSHOT JSON
        // =========================
        // Untuk data benefit / hasil cek manual yang berbeda antar asuransi.

        [MaxLength(4000)]
        public string? BenefitSnapshotJson { get; set; }

        [MaxLength(4000)]
        public string? ManualCheckResultJson { get; set; }

        // =========================
        // CANCEL
        // =========================

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // =========================
        // NAVIGATION
        // =========================

        public TrxPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MstPaymentMethod? PaymentMethod { get; set; }

        public MstPatientInsurance? PatientInsurance { get; set; }

        public MstInsuranceProvider? InsuranceProvider { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }

        public MstPatientCompanyGuarantor? PatientCompanyGuarantor { get; set; }

        public MstPatientMembership? PatientMembership { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
