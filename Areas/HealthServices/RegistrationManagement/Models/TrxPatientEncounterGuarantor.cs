using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Sumber pembayaran satu-ke-satu milik encounter. Nama tabel lama dipertahankan
    /// agar perubahan tidak memerlukan rename table yang tidak perlu.
    /// </summary>
    [Table("TrxPatientEncounterGuarantor", Schema = "public")]
    public class TrxPatientEncounterGuarantor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PaymentSourceNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Snapshot tipe pembayaran. Hanya Cash atau Insurance.
        /// </summary>
        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        public bool IsActive { get; set; } = true;

        // =========================
        // PAYMENT REFERENCES
        // =========================

        /// <summary>
        /// Diisi untuk Cash dan harus null untuk Insurance.
        /// </summary>
        public Guid? PaymentMethodId { get; set; }

        /// <summary>
        /// Wajib untuk Insurance dan harus null untuk Cash.
        /// </summary>
        public Guid? PatientInsuranceId { get; set; }

        /// <summary>
        /// Wajib untuk Insurance dan harus null untuk Cash.
        /// </summary>
        public Guid? InsuranceProviderId { get; set; }

        // =========================
        // REGISTRATION SNAPSHOT
        // =========================

        [MaxLength(250)]
        public string? PaymentSourceNameSnapshot { get; set; }

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

        /// <summary>
        /// Snapshot MstPatientInsurance.IsEligible pada waktu registrasi.
        /// Cash selalu true.
        /// </summary>
        public bool IsEligible { get; set; } = true;

        /// <summary>
        /// True bila tanggal encounter berada dalam periode polis.
        /// Cash selalu false karena bukan polis.
        /// </summary>
        public bool IsPolicyActive { get; set; } = false;

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
    }
}
