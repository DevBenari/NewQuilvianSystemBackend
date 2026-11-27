using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Satu barang katalog di paket (BE-PBJ-PS-004; PBJ-DEC-022): jumlah gabungan seluruh sumber baris PR, spesifikasi,
    /// dan HPS satuan termasuk PPN.
    /// </summary>
    /// <remarks>
    /// Contoh: Kursi Roda Standar 15 unit (IGD 10 + Rawat Inap 5), HPS Rp 2.850.000/unit → Rp 42.750.000.
    /// </remarks>
    [Table("PrcPackageItem", Schema = "public")]
    public class PrcPackageItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageId { get; set; }

        public PrcPackage? Package { get; set; }

        public int LineNo { get; set; }

        public Guid ItemId { get; set; }

        public MstProcurementItem? Item { get; set; }

        /// <summary>Salinan kategori barang saat dibuat; dasar kelayakan vendor dan garis pagu.</summary>
        public Guid ItemCategoryId { get; set; }

        [MaxLength(2000)]
        public string? Specification { get; set; }

        /// <summary>Σ jumlah sumber yang belum dilepas.</summary>
        public decimal Quantity { get; set; }

        /// <summary>SENSITIF (harga). Bawaan = harga perkiraan PR; &gt; 0 saat diajukan.</summary>
        public decimal HpsUnitPrice { get; set; }

        /// <summary>SENSITIF. round(Quantity × HpsUnitPrice, 2).</summary>
        public decimal HpsAmount { get; set; }

        public List<PrcPackageItemSource> Sources { get; set; } = new();

        public List<PrcTechnicalCriterion> Criteria { get; set; } = new();
    }
}
