using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Penawaran satu vendor yang dicatat Tim Pengadaan (BE-PBJ-PS-004; PBJ-DEC-067, 081, 084).
    /// </summary>
    /// <remarks>
    /// Contoh: PT Sehat Medika, diterima 10-04-2027 15.20 (tepat waktu), PPN 11%, total Rp 41.625.000. Satu penawaran
    /// <c>RECORDED</c> per vendor per RFQ; catatan salah dibatalkan menjadi <c>VOID</c>.
    /// </remarks>
    [Table("PrcQuotation", Schema = "public")]
    public class PrcQuotation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RfqId { get; set; }

        public PrcRfq? Rfq { get; set; }

        public Guid PackageId { get; set; }

        public PrcPackage? Package { get; set; }

        public Guid SupplierId { get; set; }

        public MstSupplier? Supplier { get; set; }

        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>Pencatat; tidak boleh mengevaluasi penawaran ini (PBJ-DEC-074).</summary>
        public Guid RecordedByUserId { get; set; }

        /// <summary>ReceivedAt &gt; batas waktu RFQ saat dicatat; tidak sah (PBJ-DEC-081).</summary>
        public bool IsLate { get; set; }

        /// <summary>Menawar semua item paket (G-P02, PBJ-DEC-095).</summary>
        public bool IsComplete { get; set; }

        /// <summary>0–100; bawaan dari MstSupplier bila kena pajak.</summary>
        public decimal TaxPercent { get; set; }

        /// <summary>SENSITIF (harga).</summary>
        public decimal TotalBeforeTax { get; set; }

        /// <summary>SENSITIF.</summary>
        public decimal TaxAmount { get; set; }

        /// <summary>SENSITIF. Termasuk PPN.</summary>
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = ProcurementStatuses.Quotation.Recorded;

        [MaxLength(500)]
        public string? VoidReason { get; set; }

        /// <summary>Penawaran asal pada paket hasil pecah.</summary>
        public Guid? SourceQuotationId { get; set; }

        public PrcQuotation? SourceQuotation { get; set; }

        /// <summary>Token konkurensi; nilai lama ditolak 409 <c>PBJ-VAL-090</c>.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public List<PrcQuotationLine> Lines { get; set; } = new();
    }
}
