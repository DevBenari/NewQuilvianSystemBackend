using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Harga penawaran per item paket (BE-PBJ-PS-004; PBJ-DEC-084, PBJ-ASM-005): harga awal, harga terkini sesudah
    /// negosiasi, dan harga akhir termasuk PPN.
    /// </summary>
    /// <remarks>
    /// Contoh: awal Rp 2.500.000, PPN 11% → akhir Rp 2.775.000,00; negosiasi menurunkan terkini menjadi Rp 2.477.000 → akhir Rp 2.749.470,00.
    /// </remarks>
    [Table("PrcQuotationLine", Schema = "public")]
    public class PrcQuotationLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid QuotationId { get; set; }

        public PrcQuotation? Quotation { get; set; }

        public Guid PackageItemId { get; set; }

        public PrcPackageItem? PackageItem { get; set; }

        /// <summary>SENSITIF. &gt; 0.</summary>
        public decimal OfferedUnitPriceBeforeTax { get; set; }

        /// <summary>SENSITIF. ≤ harga awal; tidak pernah naik (PBJ-ASM-005).</summary>
        public decimal CurrentUnitPriceBeforeTax { get; set; }

        /// <summary>SENSITIF. round(terkini × (1 + PPN%), 2).</summary>
        public decimal FinalUnitPrice { get; set; }

        /// <summary>SENSITIF. round(FinalUnitPrice × jumlah item, 2).</summary>
        public decimal LineAmount { get; set; }

        public Guid? SourceQuotationLineId { get; set; }

        public PrcQuotationLine? SourceQuotationLine { get; set; }
    }
}
