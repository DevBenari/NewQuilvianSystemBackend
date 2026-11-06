using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Pesanan kacamata, lensa kontak, atau barang Optik (<c>AGG-OPT-03</c>). Harga dan diskon sengaja tidak disimpan — keduanya milik tarif dan Billing (<c>OPT-DEC-021</c>, <c>OPT-DEC-027</c>, <c>QBE-ENT-003</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptOrder", Schema = "public")]
    public class OptOrder : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Contoh OPT-ORD-RSMMC-000001</summary>
        [Required]
        [MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>Kunjungan pemesanan; tagihan menempel di sini (OPT-DEC-043)</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Pemilik pesanan</summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Fabrication/DirectSale/Remake</summary>
        public OptOrderKind OrderKind { get; set; }

        /// <summary>Wajib untuk pesanan berukuran dan lensa kontak</summary>
        public Guid? PrescriptionId { get; set; }

        /// <summary>Versi yang dikerjakan; tidak berubah setelah InFabrication</summary>
        public Guid? PrescriptionVersionId { get; set; }

        /// <summary>Bagian 3 contracts/state-transition-matrix.md</summary>
        public OptOrderStatus Status { get; set; } = OptOrderStatus.Draft;

        /// <summary>Dibekukan saat dibuat (OPT-DEC-009)</summary>
        public decimal DownPaymentMinPercentSnapshot { get; set; }

        /// <summary>Wajib saat kirim ke pengerjaan</summary>
        public OptWorkshopType? WorkshopType { get; set; }

        /// <summary>Wajib bila ExternalLab</summary>
        public Guid? ExternalLabSupplierId { get; set; }

        /// <summary>Perkiraan jadi (OPT-DEC-013)</summary>
        public DateOnly? EstimatedReadyDate { get; set; }

        /// <summary>Saat fakta tagihan item diterbitkan</summary>
        public DateTime? BilledAt { get; set; }

        /// <summary>Saat InFabrication</summary>
        public DateTime? SentToWorkAt { get; set; }

        /// <summary>Dasar daftar "perlu dihubungi" (OPT-DEC-018)</summary>
        public DateTime? ReadyAt { get; set; }

        /// <summary>Awal masa garansi</summary>
        public DateTime? HandedOverAt { get; set; }

        /// <summary>Petugas yang menyerahkan</summary>
        public Guid? HandedOverBy { get; set; }

        /// <summary>Wajib true untuk menyerahkan (OPT-DEC-048)</summary>
        public bool HandoverIdentityVerified { get; set; } = false;

        /// <summary>Diisi bila diambil wakil SENSITIF: jangan masuk logger.</summary>
        [MaxLength(150)]
        public string? HandoverReceiverName { get; set; }

        /// <summary>Hubungan wakil dengan pasien</summary>
        [MaxLength(50)]
        public string? HandoverReceiverRelationship { get; set; }

        /// <summary>Catatan penyetelan</summary>
        [MaxLength(500)]
        public string? FittingNote { get; set; }

        /// <summary>Dibekukan saat HandedOver (OPT-DEC-041)</summary>
        public int? WarrantyFitDaysSnapshot { get; set; }

        /// <summary>Dibekukan saat HandedOver</summary>
        public int? WarrantyLensDefectMonthsSnapshot { get; set; }

        /// <summary>Dibekukan saat HandedOver</summary>
        public int? WarrantyFrameDefectMonthsSnapshot { get; set; }

        /// <summary>Saat ditutup NotCollected</summary>
        public DateTime? NotCollectedAt { get; set; }

        /// <summary>Saat Cancelled</summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>Pesanan remake Rp 0 (OPT-DEC-015)</summary>
        public bool IsRemake { get; set; } = false;

        /// <summary>Pesanan asal, wajib untuk remake</summary>
        public Guid? ParentOrderId { get; set; }

        /// <summary>Penanda concurrency (IsConcurrencyToken); naik setiap perubahan</summary>
        public int Version { get; set; } = 0;

        /// <summary>Kunci dari header Idempotency-Key agar Simpan dua kali tidak membuat data ganda (OPT-DEC-077). Kosong pada baris lama dan pesanan remake, yang dibuat sistem.</summary>
        public Guid? IdempotencyKey { get; set; }

        public ICollection<OptOrderItem> Items { get; set; } = new List<OptOrderItem>();

        public ICollection<OptOrderCancellation> Cancellations { get; set; } = new List<OptOrderCancellation>();

        public ICollection<OptOrderStatusHistory> StatusHistories { get; set; } = new List<OptOrderStatusHistory>();
    }
}
