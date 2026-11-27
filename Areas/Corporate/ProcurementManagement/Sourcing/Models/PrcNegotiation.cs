using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Satu putaran negosiasi atas satu penawaran (BE-PBJ-PS-004; PBJ-ASM-005): hanya penawaran sah dan Lulus.
    /// </summary>
    /// <remarks>
    /// Contoh: putaran 1, 12-04-2027, peserta Tim Pengadaan dan PT Sehat Medika, harga turun Rp 25.000/unit.
    /// </remarks>
    [Table("PrcNegotiation", Schema = "public")]
    public class PrcNegotiation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid QuotationId { get; set; }

        public PrcQuotation? Quotation { get; set; }

        public int RoundNo { get; set; }

        public DateTimeOffset NegotiatedAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Participants { get; set; } = string.Empty;

        /// <summary>SENSITIF (memuat angka harga).</summary>
        [Required]
        [MaxLength(2000)]
        public string Summary { get; set; } = string.Empty;

        public List<PrcNegotiationLine> Lines { get; set; } = new();
    }
}
