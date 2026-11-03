using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Keluhan garansi atas pesanan yang sudah diserahkan, dicatat pada kunjungan baru (<c>AGG-OPT-04</c>, <c>OPT-DEC-015</c>, <c>OPT-DEC-043</c>). Tautan ke pesanan remake disimpan di sini saja agar relasi tidak berputar.</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptWarrantyClaim", Schema = "public")]
    public class OptWarrantyClaim : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Contoh OPT-CLM-RSMMC-000001</summary>
        [Required]
        [MaxLength(30)]
        public string ClaimNumber { get; set; } = string.Empty;

        /// <summary>Pesanan asal; wajib HandedOver</summary>
        [Required]
        public Guid OrderId { get; set; }

        /// <summary>Kunjungan keluhan baru (OPT-DEC-043)</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>FitDiscomfort/LensDefect/FrameDefect/Damage</summary>
        public OptComplaintType ComplaintType { get; set; }

        /// <summary>Keluhan pasien SENSITIF: jangan masuk logger.</summary>
        [Required]
        [MaxLength(1000)]
        public string ComplaintNote { get; set; } = string.Empty;

        /// <summary>Dihitung saat dicatat dari snapshot garansi pesanan; Damage selalu false</summary>
        public bool IsWithinWarranty { get; set; }

        /// <summary>Batas garansi untuk jenis keluhan ini</summary>
        public DateOnly? WarrantyDeadlineSnapshot { get; set; }

        /// <summary>Submitted/Examined/Approved/Rejected</summary>
        public OptWarrantyClaimStatus Status { get; set; } = OptWarrantyClaimStatus.Submitted;

        /// <summary>RO/Sp.M pemeriksa ulang</summary>
        public Guid? ExaminedByWorkforceProfileId { get; set; }

        /// <summary>Waktu pemeriksaan ulang</summary>
        public DateTime? ExaminedAt { get; set; }

        /// <summary>Temuan pemeriksaan ulang SENSITIF: jangan masuk logger.</summary>
        [MaxLength(1000)]
        public string? ExaminationFinding { get; set; }

        /// <summary>Pemeriksaan refraksi baru, bila dilakukan</summary>
        public Guid? ReexamRefractionExamId { get; set; }

        /// <summary>Kepala Optik</summary>
        public Guid? DecidedBy { get; set; }

        /// <summary>Waktu keputusan</summary>
        public DateTime? DecidedAt { get; set; }

        /// <summary>Wajib bila ditolak</summary>
        [MaxLength(500)]
        public string? DecisionReason { get; set; }

        /// <summary>Pesanan remake yang lahir; paling banyak satu</summary>
        public Guid? RemakeOrderId { get; set; }

        /// <summary>Penanda concurrency (IsConcurrencyToken); naik setiap perubahan</summary>
        public int Version { get; set; } = 0;
    }
}
