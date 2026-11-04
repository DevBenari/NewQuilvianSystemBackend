using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Satu pemeriksaan refraksi pada satu kunjungan Optik (<c>AGG-OPT-01</c>). Nilai ukur tidak disimpan di sini, melainkan di <see cref="OptRefractionExamRevision"/>; <see cref="BillingFactEmittedAt"/> mencegah tarif pemeriksaan tertagih ulang saat revisi (<c>OPT-DEC-042</c>, <c>OPT-DEC-047</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptRefractionExam", Schema = "public")]
    public class OptRefractionExam : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor pemeriksaan dari OptNumberService, contoh OPT-EXM-RSMMC-000001</summary>
        [Required]
        [MaxLength(30)]
        public string ExamNumber { get; set; } = string.Empty;

        /// <summary>Kunjungan Optik; wajib belum ditutup saat mulai</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Disalin dari kunjungan; dipakai pencarian riwayat</summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Pemeriksa RO/Sp.M (OPT-DEC-005)</summary>
        [Required]
        public Guid ExaminerWorkforceProfileId { get; set; }

        /// <summary>Nomor SIP yang berlaku saat pemeriksaan (OPT-DEC-012)</summary>
        [Required]
        [MaxLength(100)]
        public string ExaminerLicenseNumberSnapshot { get; set; } = string.Empty;

        /// <summary>Draft/Final/Cancelled</summary>
        public OptRefractionExamStatus Status { get; set; } = OptRefractionExamStatus.Draft;

        /// <summary>Nomor revisi hasil ukur yang berlaku</summary>
        public int CurrentRevisionNo { get; set; } = 1;

        /// <summary>Waktu mulai</summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Waktu disimpan final</summary>
        public DateTime? FinalizedAt { get; set; }

        /// <summary>Akun yang menyimpan final</summary>
        public Guid? FinalizedBy { get; set; }

        /// <summary>Terisi sekali saat fakta tarif pemeriksaan diterima Billing; mencegah tagih ulang (OPT-DEC-042, 047)</summary>
        public DateTime? BillingFactEmittedAt { get; set; }

        /// <summary>Wajib bila Cancelled</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>Penanda concurrency (IsConcurrencyToken); naik setiap perubahan</summary>
        public int Version { get; set; } = 0;

        public ICollection<OptRefractionExamRevision> Revisions { get; set; } = new List<OptRefractionExamRevision>();
    }
}
