using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Satu pendaftaran Rawat Jalan yang diisi pasien sendiri di halaman publik, sebelum menjadi
    /// kunjungan (kiosk Amendment PO, <c>KSK-DEC-030</c>). Pendaftaran bukan kunjungan: tidak
    /// membuat antrean maupun tagihan, tetapi memakai satu kuota appointment selama berstatus
    /// Menunggu Check-in (<c>INV-PO-01</c>, <c>INV-PO-02</c>). Kunjungan baru terbit saat pasien
    /// check-in di kiosk pada tanggal kunjungan.
    /// </summary>
    /// <remarks>
    /// Kolom di sini menampung data yang belum boleh masuk tabel kunjungan atau master: data diri
    /// pasien baru, kontak, pilihan penjamin dan kartu baru, rujukan, dan consent
    /// (<c>data/data-dictionary.md</c> PO-D.2). Kode pendaftaran tidak pernah disimpan dalam bentuk
    /// asli (<see cref="CodeHash"/>, <c>INV-PO-05</c>). Data diri pasien baru tidak pernah disalin ke
    /// master pasien; master dibuat dari hasil pindai KTP/KIA saat check-in (<c>INV-PO-06</c>).
    /// </remarks>
    [Table("RegOnlineRegistration", Schema = "public")]
    public class RegOnlineRegistration : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>PO-yyMMdd-nnnn</c> dari <c>NumberSeriesAllocator</c>. Bukan rahasia; dipakai petugas dan log.</summary>
        [Required]
        [MaxLength(30)]
        public string RegistrationNumber { get; set; } = string.Empty;

        /// <summary>Hex SHA-256 dari kode yang dinormalkan. Sensitif: tidak masuk log maupun respons.</summary>
        [Required]
        [MaxLength(64)]
        public string CodeHash { get; set; } = string.Empty;

        [Required]
        public OnlineRegistrationStatus Status { get; set; } = OnlineRegistrationStatus.AwaitingCheckIn;

        [Required]
        public OnlineRegistrationPatientMode PatientMode { get; set; }

        /// <summary>Pasien lama: diisi saat daftar. Pasien baru: diisi saat check-in.</summary>
        public Guid? PatientId { get; set; }

        /// <summary>NIK 16 digit. Wajib untuk pasien baru; pasien lama disalin dari master bila ada. Sensitif.</summary>
        [MaxLength(16)]
        public string? IdentityNumber { get; set; }

        /// <summary>Nama yang diketik pasien baru. Sensitif.</summary>
        [MaxLength(150)]
        public string? FullName { get; set; }

        /// <summary>Tanggal lahir pasien baru (kalender, tanpa jam). Sensitif.</summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>Jenis kelamin pasien baru. Sensitif.</summary>
        public Gender? Gender { get; set; }

        /// <summary>Tanggal kunjungan, kalender WIB, tanpa jam. H+1 s.d. H+30 (<c>INV-PO-04</c>).</summary>
        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        public Guid ClinicId { get; set; }

        [Required]
        public Guid DoctorScheduleId { get; set; }

        [Required]
        public OnlineRegistrationPatientType PatientType { get; set; }

        [Required]
        public EncounterPaymentType PaymentType { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        public Guid? PatientCompanyGuarantorId { get; set; }

        /// <summary>Kartu asuransi baru: penyedia. Kartunya dibuat saat check-in.</summary>
        public Guid? NewInsuranceProviderId { get; set; }

        /// <summary>Kartu asuransi baru: nomor polis/kartu. Sensitif.</summary>
        [MaxLength(50)]
        public string? NewInsurancePolicyNumber { get; set; }

        /// <summary>Kartu asuransi baru: nama pemegang. Sensitif.</summary>
        [MaxLength(150)]
        public string? NewInsuranceHolderName { get; set; }

        [MaxLength(50)]
        public string? NewInsuranceHolderRelationship { get; set; }

        public bool IsReferral { get; set; }

        public Guid? ReferralInstitutionId { get; set; }

        /// <summary>Sensitif.</summary>
        [MaxLength(50)]
        public string? ReferralNumber { get; set; }

        /// <summary>Tanggal surat rujukan (kalender, tanpa jam).</summary>
        public DateTime? ReferralDate { get; set; }

        [MaxLength(150)]
        public string? ReferralDoctorName { get; set; }

        /// <summary>Sensitif: data kesehatan.</summary>
        [MaxLength(500)]
        public string? ReferralDiagnosisNote { get; set; }

        /// <summary>Sensitif: data kesehatan.</summary>
        [MaxLength(500)]
        public string? ReferralReason { get; set; }

        /// <summary><c>true</c> untuk pasien baru yang menandatangani persetujuan umum (<c>KSK-DEC-034</c>).</summary>
        public bool HasConsent { get; set; }

        public GeneralConsentSignerType? ConsentSignerType { get; set; }

        /// <summary>Sensitif.</summary>
        [MaxLength(150)]
        public string? ConsentSignerName { get; set; }

        [MaxLength(100)]
        public string? ConsentSignerRelationshipText { get; set; }

        /// <summary>Sensitif.</summary>
        [MaxLength(50)]
        public string? ConsentSignerIdentityNumber { get; set; }

        /// <summary>Sensitif.</summary>
        [MaxLength(30)]
        public string? ConsentSignerPhoneNumber { get; set; }

        /// <summary>Sensitif.</summary>
        [MaxLength(500)]
        public string? ConsentSignerAddress { get; set; }

        /// <summary>Klausul yang disetujui, sama dengan <c>RegPatientGeneralConsent.AgreedClauses</c>.</summary>
        public int[] ConsentAgreedClauses { get; set; } = Array.Empty<int>();

        /// <summary>Gambar tanda tangan. Sensitif.</summary>
        public string? ConsentSignatureImageBase64 { get; set; }

        /// <summary>Waktu tanda tangan online; dipertahankan saat ditulis ke consent kunjungan.</summary>
        public DateTime? ConsentSignedAt { get; set; }

        /// <summary>Format <c>08…</c>. Wajib salah satu dengan email (<c>KSK-DEC-043</c>). Sensitif.</summary>
        [MaxLength(20)]
        public string? ContactWhatsAppNumber { get; set; }

        /// <summary>Wajib salah satu dengan No. WhatsApp (<c>KSK-DEC-043</c>). Sensitif.</summary>
        [MaxLength(150)]
        public string? ContactEmail { get; set; }

        public OnlineRegistrationEmailStatus EmailDeliveryStatus { get; set; } = OnlineRegistrationEmailStatus.NotRequested;

        public DateTime? EmailAttemptedAt { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Hash token check-in kiosk; dikosongkan setelah check-in. Sensitif.</summary>
        [MaxLength(64)]
        public string? CheckInTokenHash { get; set; }

        public DateTime? CheckInTokenExpiresAt { get; set; }

        /// <summary>Kunjungan hasil check-in atau kunjungan jalur lain.</summary>
        public Guid? EncounterId { get; set; }

        public DateTime? CheckedInAt { get; set; }

        /// <summary>Waktu menjadi status akhir selain Menjadi Kunjungan.</summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>Alasan batal petugas (5–500 karakter).</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstPatient? Patient { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDoctorSchedule? DoctorSchedule { get; set; }

        public MstPatientInsurance? PatientInsurance { get; set; }

        public MstPatientCompanyGuarantor? PatientCompanyGuarantor { get; set; }

        public MstInsuranceProvider? NewInsuranceProvider { get; set; }

        public MstReferralInstitution? ReferralInstitution { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public ICollection<RegOnlineRegistrationReferralDocument> ReferralDocuments { get; set; } =
            new List<RegOnlineRegistrationReferralDocument>();

        public ICollection<RegOnlineRegistrationHistory> Histories { get; set; } =
            new List<RegOnlineRegistrationHistory>();
    }
}
