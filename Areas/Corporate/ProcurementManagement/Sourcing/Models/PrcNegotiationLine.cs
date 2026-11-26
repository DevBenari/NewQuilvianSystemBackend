using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Harga baru per baris penawaran dalam satu putaran negosiasi (BE-PBJ-PS-004; PBJ-ASM-005): tidak boleh naik.
    /// </summary>
    [Table("PrcNegotiationLine", Schema = "public")]
    public class PrcNegotiationLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid NegotiationId { get; set; }

        public PrcNegotiation? Negotiation { get; set; }

        public Guid QuotationLineId { get; set; }

        public PrcQuotationLine? QuotationLine { get; set; }

        /// <summary>SENSITIF.</summary>
        public decimal PreviousUnitPriceBeforeTax { get; set; }

        /// <summary>SENSITIF. &gt; 0 dan ≤ harga sebelumnya.</summary>
        public decimal NewUnitPriceBeforeTax { get; set; }
    }
}
