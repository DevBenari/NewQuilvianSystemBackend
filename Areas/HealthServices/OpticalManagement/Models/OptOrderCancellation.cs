using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Pengajuan dan keputusan batal sesudah pengerjaan (<c>OPT-DEC-010</c>, <c>OPT-DEC-038</c>). Paling banyak satu berstatus Requested per pesanan.</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptOrderCancellation", Schema = "public")]
    public class OptOrderCancellation : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pesanan yang diajukan batal</summary>
        [Required]
        public Guid OrderId { get; set; }

        public OptOrder? Order { get; set; }

        /// <summary>Requested/Approved/Rejected</summary>
        public OptCancellationStatus Status { get; set; } = OptCancellationStatus.Requested;

        /// <summary>Alasan wajib</summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Pengaju</summary>
        [Required]
        public Guid RequestedBy { get; set; }

        /// <summary>Waktu pengajuan</summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Kepala Optik</summary>
        public Guid? DecidedBy { get; set; }

        /// <summary>Waktu keputusan</summary>
        public DateTime? DecidedAt { get; set; }

        /// <summary>Wajib bila ditolak</summary>
        [MaxLength(500)]
        public string? DecisionNote { get; set; }
    }
}
