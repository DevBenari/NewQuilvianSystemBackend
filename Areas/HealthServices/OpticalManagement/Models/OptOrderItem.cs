using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Satu baris barang pesanan; satu fakta tagihan Billing per baris (<c>OPT-DEC-029</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptOrderItem", Schema = "public")]
    public class OptOrderItem : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Induk pesanan</summary>
        [Required]
        public Guid OrderId { get; set; }

        public OptOrder? Order { get; set; }

        /// <summary>Nomor baris 1, 2, 3 …</summary>
        public int LineNo { get; set; }

        /// <summary>Barang; wajib punya OptItemProfile aktif</summary>
        [Required]
        public Guid DrugId { get; set; }

        /// <summary>Disalin saat baris dibuat</summary>
        public OptItemCategory ItemCategory { get; set; }

        /// <summary>Wajib untuk lensa kacamata: Stock/Custom</summary>
        public OptLensSupply? LensSupply { get; set; }

        /// <summary>&gt; 0</summary>
        public decimal Quantity { get; set; }

        /// <summary>Active/Cancelled</summary>
        public OptOrderItemStatus Status { get; set; } = OptOrderItemStatus.Active;

        /// <summary>Diisi saat pengajuan batal sesudah pengerjaan (OPT-DEC-038)</summary>
        public bool? IsLensCut { get; set; }

        /// <summary>Saat stok dipotong (OPT-DEC-019)</summary>
        public DateTime? StockIssuedAt { get; set; }

        /// <summary>Saat stok dikembalikan</summary>
        public DateTime? StockReturnedAt { get; set; }

        /// <summary>Versi fakta tagihan terakhir yang dikirim; 0 = belum (remake selalu 0)</summary>
        public int BillingFactVersion { get; set; } = 0;

        /// <summary>Saat baris dibatalkan</summary>
        public DateTime? CancelledAt { get; set; }
    }
}
