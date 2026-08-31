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
        /// Snapshot tipe pembayaran: Cash, Insurance, atau CompanyGuarantor.
        /// </summary>
        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        public bool IsActive { get; set; } = true;

        // =========================
        // PAYMENT REFERENCES
        // =========================

        /// <summary>
        /// Diisi hanya untuk Cash. Harus null untuk Insurance dan CompanyGuarantor.
        /// </summary>
        public Guid? PaymentMethodId { get; set; }

        /// <summary>
        /// Wajib untuk Insurance. Harus null untuk Cash dan CompanyGuarantor.
        /// </summary>
        public Guid? PatientInsuranceId { get; set; }

        /// <summary>
        /// Wajib untuk Insurance. Harus null untuk Cash dan CompanyGuarantor.
        /// </summary>
        public Guid? InsuranceProviderId { get; set; }

        /// <summary>
        /// Kartu hubungan pasien-perusahaan yang dipilih saat registrasi.
        /// Wajib untuk CompanyGuarantor dan harus null untuk Cash dan Insurance.
        /// </summary>
        public Guid? PatientCompanyGuarantorId { get; set; }

        /// <summary>
        /// Master perusahaan penjamin yang dirujuk kartu di atas.
        /// Wajib untuk CompanyGuarantor dan harus null untuk Cash dan Insurance.
        /// </summary>
        public Guid? CompanyGuarantorId { get; set; }

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

        /// <summary>
        /// Kode perusahaan penjamin saat registrasi. Diisi hanya untuk CompanyGuarantor.
        /// </summary>
        [MaxLength(50)]
        public string? CompanyGuarantorCodeSnapshot { get; set; }

        /// <summary>
        /// Nomor karyawan pada kartu penjamin perusahaan saat registrasi.
        /// Diisi hanya untuk CompanyGuarantor.
        /// </summary>
        [MaxLength(100)]
        public string? EmployeeNumberSnapshot { get; set; }

        /// <summary>
        /// Nama karyawan pada kartu penjamin perusahaan saat registrasi.
        /// Boleh kosong sesuai master.
        /// </summary>
        [MaxLength(200)]
        public string? EmployeeNameSnapshot { get; set; }

        public DateTime? EffectiveStartDateSnapshot { get; set; }

        public DateTime? EffectiveEndDateSnapshot { get; set; }

        /// <summary>
        /// Snapshot IsEligible kartu penjamin pada waktu registrasi, dibaca dari
        /// MstPatientInsurance untuk Insurance dan MstPatientCompanyGuarantor untuk
        /// CompanyGuarantor. Cash selalu true.
        /// </summary>
        public bool IsEligible { get; set; } = true;

        /// <summary>
        /// True bila tanggal encounter berada dalam masa berlaku kartu penjamin,
        /// baik polis asuransi maupun kartu perusahaan.
        /// Cash selalu false karena bukan kartu penjamin.
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

        public MstPatientCompanyGuarantor? PatientCompanyGuarantor { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }
    }
}
