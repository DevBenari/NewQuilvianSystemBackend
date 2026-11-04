using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Satu perpindahan status pesanan, termasuk status awalnya. Tidak pernah diubah.</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptOrderStatusHistory", Schema = "public")]
    public class OptOrderStatusHistory : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pesanan</summary>
        [Required]
        public Guid OrderId { get; set; }

        public OptOrder? Order { get; set; }

        /// <summary>Kosong untuk baris pertama</summary>
        public OptOrderStatus? FromStatus { get; set; }

        /// <summary>Status tujuan</summary>
        public OptOrderStatus ToStatus { get; set; }

        /// <summary>Pelaku</summary>
        [Required]
        public Guid ActorUserId { get; set; }

        /// <summary>Waktu kejadian</summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Wajib untuk batal dan tidak diambil</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
