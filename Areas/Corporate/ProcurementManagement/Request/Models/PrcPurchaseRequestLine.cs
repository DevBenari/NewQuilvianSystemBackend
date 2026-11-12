using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models
{
    /// <summary>
    /// Baris permintaan pengadaan — kamus data bagian 4 (BE-PBJ-006).
    /// </summary>
    /// <remarks>
    /// Barang wajib dari katalog (PBJ-DEC-022). Pembagian dalam/luar rencana diisi saat diajukan
    /// (BE-PBJ-007), dan kolom cek stok serta kebutuhan beli diisi saat verifikasi (BE-PBJ-008).
    /// Contoh: 20 kursi roda diminta, 5 dipenuhi stok → <see cref="PurchaseQuantity"/> = 15.
    /// </remarks>
    [Table("PrcPurchaseRequestLine", Schema = "public")]
    public class PrcPurchaseRequestLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PurchaseRequestId { get; set; }
        public PrcPurchaseRequest? PurchaseRequest { get; set; }

        public int LineNo { get; set; }

        public Guid ItemId { get; set; }
        public MstProcurementItem? Item { get; set; }

        [MaxLength(1000)]
        public string? Specification { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal RequestedQuantity { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal EstimatedUnitPrice { get; set; }

        /// <summary>Kunci baris rencana (stabil lintas versi), tanpa FK fisik.</summary>
        public Guid? NeedPlanLineKey { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal InPlanQuantity { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal OutOfPlanQuantity { get; set; }

        /// <summary>SENSITIF. Wajib bila ada kuantitas di luar rencana (PBJ-VAL-009).</summary>
        [MaxLength(1000)]
        public string? OutOfPlanReason { get; set; }

        /// <summary><c>null</c> = belum dicek stoknya.</summary>
        [Column(TypeName = "numeric(18,4)")]
        public decimal? StockAvailableQuantity { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal? StockFulfilledQuantity { get; set; }

        [MaxLength(300)]
        public string? StockSourceNote { get; set; }

        public DateTimeOffset? StockCheckedAt { get; set; }

        public Guid? StockCheckedByUserId { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal PurchaseQuantity { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal PurchaseAmount { get; set; }
    }
}
