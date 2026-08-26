using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Catatan kejadian infeksi terkait pelayanan kesehatan (nosokomial) pada satu pasien.
    /// </summary>
    /// <remarks>
    /// Rumahnya di Clinical Management, bukan di modul IGD, karena surveilans infeksi berlaku
    /// untuk seluruh unit pelayanan — IGD, rawat inap, kamar operasi, dan ICU. Menaruhnya di
    /// IGD berarti unit lain kelak membuat tabel keduanya untuk fakta yang sama.
    ///
    /// Entitas ini mencatat, bukan menyimpulkan. Angka mutu seperti "insiden per 1000 hari
    /// pemakaian alat" dihitung dari baris-baris ini, sehingga yang disimpan di sini adalah
    /// fakta mentah beserta siapa yang menyatakannya dan kapan.
    /// </remarks>
    [Table("TrxNosocomialInfection", Schema = "public")]
    public class TrxNosocomialInfection : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor catatan yang dapat dibaca petugas, misalnya NOS-2026-000123.</summary>
        [Required]
        [MaxLength(50)]
        public string NosocomialRecordNumber { get; set; } = string.Empty;

        // =========================
        // KONTEKS KLINIS
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        /// <summary>Kunjungan IGD, bila kejadian ditemukan saat pasien berada di IGD.</summary>
        public Guid? EmergencyVisitId { get; set; }

        public Guid? AssessmentId { get; set; }

        /// <summary>Unit tempat infeksi ditemukan, belum tentu unit tempat infeksi didapat.</summary>
        public Guid? ServiceUnitId { get; set; }

        public Guid? DoctorId { get; set; }

        // =========================
        // KEJADIAN
        // =========================

        public NosocomialInfectionType InfectionType { get; set; }
            = NosocomialInfectionType.Unknown;

        /// <summary>Diisi hanya bila <see cref="InfectionType"/> bernilai Other.</summary>
        [MaxLength(250)]
        public string? InfectionTypeOther { get; set; }

        public NosocomialInfectionStatus Status { get; set; }
            = NosocomialInfectionStatus.Suspected;

        public NosocomialInfectionOnsetCategory OnsetCategory { get; set; }
            = NosocomialInfectionOnsetCategory.Unknown;

        /// <summary>Waktu tanda atau gejala pertama kali ditemukan.</summary>
        [Required]
        public DateTime OnsetDateTime { get; set; }

        /// <summary>
        /// Waktu pasien mulai dirawat, disalin saat catatan dibuat.
        /// </summary>
        /// <remarks>
        /// Sengaja berupa salinan, bukan dibaca ulang dari kunjungan. Selisihnya terhadap
        /// <see cref="OnsetDateTime"/> adalah dasar penetapan 48 jam, dan dasar itu harus
        /// tetap sama isinya ketika kelak ditinjau — termasuk bila tanggal masuk dikoreksi.
        /// </remarks>
        public DateTime? AdmissionDateTimeSnapshot { get; set; }

        /// <summary>Jam sejak pasien dirawat sampai gejala muncul, dihitung saat pencatatan.</summary>
        public int? HoursSinceAdmission { get; set; }

        // =========================
        // KAITAN DENGAN ALAT
        // =========================

        public bool IsDeviceAssociated { get; set; } = false;

        /// <summary>Alat yang dikaitkan, misalnya kateter urin, CVC, atau ventilator.</summary>
        [MaxLength(150)]
        public string? DeviceName { get; set; }

        public DateTime? DeviceInsertedAt { get; set; }

        /// <summary>Lama pemakaian alat dalam hari, dipakai sebagai penyebut indikator mutu.</summary>
        public int? DeviceUsageDays { get; set; }

        // =========================
        // BUKTI
        // =========================

        /// <summary>Kriteria surveilans yang terpenuhi, ditulis apa adanya oleh petugas.</summary>
        [MaxLength(2000)]
        public string? CriteriaMet { get; set; }

        [MaxLength(250)]
        public string? CultureSpecimenType { get; set; }

        public DateTime? CultureTakenAt { get; set; }

        [MaxLength(500)]
        public string? CultureResult { get; set; }

        [MaxLength(250)]
        public string? CausativeOrganism { get; set; }

        [MaxLength(1000)]
        public string? AntibioticTherapy { get; set; }

        // =========================
        // PELAPORAN DAN VERIFIKASI
        // =========================

        [Required]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        public Guid? ReportedByUserId { get; set; }

        [MaxLength(150)]
        public string? ReportedByNameSnapshot { get; set; }

        /// <summary>Perawat pengendali infeksi (IPCN) yang memverifikasi.</summary>
        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(150)]
        public string? VerifiedByNameSnapshot { get; set; }

        public DateTime? VerifiedAt { get; set; }

        /// <summary>Alasan bila kejadian dinyatakan bukan infeksi terkait pelayanan.</summary>
        [MaxLength(1000)]
        public string? RuledOutReason { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
