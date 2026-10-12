using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Rincian rujukan satu kunjungan (<c>RJ-DOC-REFERRAL-001</c>, Amendment PM-B).
    /// </summary>
    /// <remarks>
    /// <b>Satu kunjungan, paling banyak satu rincian.</b> Nomor rujukan, instansi perujuk, dan
    /// dokter perujuk <b>tidak</b> disalin ke sini: sumber kebenarannya tetap
    /// <see cref="RegPatientEncounter"/>. Tabel ini memuat yang belum punya tempat — tanggal
    /// rujukan, unit tujuan, diagnosa, alasan — beserta status kelengkapannya.
    ///
    /// <b>Contoh.</b> Kunjungan dari Kiosk tanpa diagnosa dan alasan tersimpan dengan
    /// <see cref="IsComplete"/> <c>false</c>, lalu Daftar Kunjungan RJ menandainya
    /// "Rujukan belum lengkap" sampai petugas melengkapinya (<c>RJ-DOC-DEC-077</c>).
    /// </remarks>
    [Table("RegEncounterReferral", Schema = "public")]
    public class RegEncounterReferral : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientEncounterId { get; set; }

        /// <summary>Tanggal dan jam rujukan; tidak boleh melewati waktu sekarang.</summary>
        public DateTime ReferralDateTime { get; set; }

        public ReferralTargetUnitType TargetUnitType { get; set; }

        [Required]
        public Guid TargetServiceUnitId { get; set; }

        /// <summary>Wajib bila <see cref="TargetUnitType"/> bernilai <c>Clinic</c>.</summary>
        public Guid? TargetClinicId { get; set; }

        /// <summary>Diagnosa rujukan (ICD-10). Data kesehatan — tidak boleh masuk log.</summary>
        public Guid? DiagnosisId { get; set; }

        /// <summary>Catatan diagnosa dari surat. Data kesehatan — tidak boleh masuk log.</summary>
        [MaxLength(500)]
        public string? DiagnosisNote { get; set; }

        /// <summary>Alasan rujukan. Data kesehatan — tidak boleh masuk log.</summary>
        [MaxLength(1000)]
        public string? ReferralReason { get; set; }

        /// <summary>Tanda mitra instansi perujuk saat rincian disimpan.</summary>
        public bool InstitutionIsPartnerSnapshot { get; set; }

        /// <summary>
        /// Kode dan nama fasilitas perujuk saat rincian disimpan (<c>DEC-FRJ-001</c>). Master dapat
        /// berganti nama atau dinonaktifkan kemudian; histori rujukan tetap membaca nilai ini.
        /// Hanya diambil ulang bila fasilitasnya diganti.
        /// </summary>
        [MaxLength(50)]
        public string? InstitutionCodeSnapshot { get; set; }

        [MaxLength(200)]
        public string? InstitutionNameSnapshot { get; set; }

        /// <summary>Nomor perjanjian kerja sama yang membuat fasilitas layak saat dipilih.</summary>
        [MaxLength(100)]
        public string? AgreementNumberSnapshot { get; set; }

        public ReferralCaptureSource CaptureSource { get; set; }

        /// <summary>
        /// Hasil hitung kelengkapan. Disimpan supaya daftar kunjungan dapat menyaring tanpa join
        /// berat, dan selalu dihitung ulang service setiap kali rincian atau suratnya berubah.
        /// </summary>
        public bool IsComplete { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>Token konkurensi; diganti setiap kali rincian berubah.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public RegPatientEncounter? PatientEncounter { get; set; }

        public MstServiceUnit? TargetServiceUnit { get; set; }

        public MstClinic? TargetClinic { get; set; }

        public MstDiagnosis? Diagnosis { get; set; }

        public ICollection<RegEncounterReferralDocument> Documents { get; set; } =
            new List<RegEncounterReferralDocument>();

        public ICollection<RegEncounterReferralRevision> Revisions { get; set; } =
            new List<RegEncounterReferralRevision>();
    }
}
