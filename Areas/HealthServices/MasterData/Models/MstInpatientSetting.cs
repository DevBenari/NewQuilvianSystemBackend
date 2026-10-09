using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInpatientSetting", Schema = "public")]
    public class MstInpatientSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = "DEFAULT";

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = "Pengaturan Rawat Inap Default";

        public int BedReservationMinutes { get; set; } = 120;

        public int DraftEpisodeExpiryHours { get; set; } = 24;

        public int InitialAssessmentTargetHours { get; set; } = 24;

        public int ProgressNoteVerificationTargetHours { get; set; } = 24;

        public int PendingClosureThresholdHours { get; set; } = 4;

        /// <summary>
        /// Berapa hari sekali kekurangan uang muka ditagih ulang selama episode masih
        /// berjalan. Bawaan 3 hari, sesuai <c>RWI-DEC-096</c>.
        /// </summary>
        /// <remarks>
        /// Angka ini hanya menjadwalkan pengingat kerja. Ia <b>tidak pernah</b> dipakai
        /// menahan perawatan, menutup episode, atau menolak tindakan apa pun.
        /// </remarks>
        public int DepositFollowUpIntervalDays { get; set; } = 3;

        /// <summary>
        /// Ambang menit serah terima pasca operasi yang belum diterima sebelum tampil di Daftar
        /// Pantau. Bawaan 60 menit, rentang 1–1440 (<c>RWI-DEC-220</c> butir 6, gate G-18,
        /// <c>BE-RWI-172</c>).
        /// </summary>
        public int PendingSurgicalHandoverAlertMinutes { get; set; } = 60;

        /// <summary>
        /// Ambang menit permintaan admisi dari kamar pulih yang belum ditindaklanjuti sebelum tampil
        /// di Daftar Pantau. Bawaan 30 menit, rentang 1–1440 (<c>RWI-DEC-220</c> butir 6).
        /// </summary>
        public int PendingAdmissionReferralAlertMinutes { get; set; } = 30;

        [Required]
        [MaxLength(20)]
        public string EpisodeNumberPrefix { get; set; } = "RI";

        public bool IsDefault { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ------------------------------------------------------------------
        // BE-RWI-185 / E9 — isian cetak Workspace PPRI (kamus data 20.16, RWI-DEC-247, 243).
        // Kode formulir yang kosong dicetak tanpa kode; tidak pernah diganti nilai bawaan dari
        // program. Nilai awal V1 hanya diisi seeder non-produksi atau admin produksi.
        // ------------------------------------------------------------------

        /// <summary>Kode formulir General Consent, misalnya <c>GC/ADM/001/Rev01/2024</c>.</summary>
        [MaxLength(50)]
        public string? GeneralConsentFormCode { get; set; }

        /// <summary>Kode formulir Ceklist Serah Terima Pasien Baru.</summary>
        [MaxLength(50)]
        public string? NewPatientHandoverFormCode { get; set; }

        /// <summary>Kode formulir Permintaan Privasi.</summary>
        [MaxLength(50)]
        public string? PrivacyRequestFormCode { get; set; }

        /// <summary>Kode formulir Identifikasi Nilai-Nilai dan Kepercayaan Pasien.</summary>
        [MaxLength(50)]
        public string? BeliefValuesFormCode { get; set; }

        /// <summary>Kode formulir Surat Pernyataan Selisih Biaya.</summary>
        [MaxLength(50)]
        public string? CostDifferenceFormCode { get; set; }

        /// <summary>Kode formulir Pernyataan Kesediaan Melunaskan Deposit.</summary>
        [MaxLength(50)]
        public string? DepositSettlementFormCode { get; set; }

        /// <summary>Kode formulir Estimasi Biaya; V1 tidak punya kode.</summary>
        [MaxLength(50)]
        public string? CostEstimateFormCode { get; set; }

        /// <summary>Kode formulir Data Dasar Rawat Inap (IPD); V1 tidak punya kode.</summary>
        [MaxLength(50)]
        public string? InpatientBaseDataFormCode { get; set; }

        /// <summary>Kota penandatanganan bawaan dokumen admisi.</summary>
        [MaxLength(100)]
        public string? DocumentSigningCity { get; set; }

        /// <summary>
        /// Umur tertinggi (tahun) yang mendapat Gelang Bayi, 0–16. Bawaan 5 seperti V1
        /// (<c>RWI-DEC-243</c>).
        /// </summary>
        public int InfantWristbandMaxAgeYears { get; set; } = 5;

        /// <summary>
        /// Kode singkat rumah sakit pada label pasien. Kosong berarti label memakai
        /// <c>MstHospitalSite.SiteCode</c> (G-36).
        /// </summary>
        [MaxLength(30)]
        public string? PatientLabelHospitalCode { get; set; }
    }
}
