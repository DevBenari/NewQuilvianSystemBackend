using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models
{
    /// <summary>
    /// Satu barang dalam rencana kebutuhan — kamus data bagian 3 (BE-PBJ-009).
    /// </summary>
    /// <remarks>
    /// <see cref="LineKey"/> adalah identitas baris lintas versi: dibuat saat baris pertama kali
    /// ditambahkan, lalu disalin ke setiap revisi. PR dan rencana paket menunjuk <see cref="LineKey"/>,
    /// bukan <see cref="Id"/>, sehingga tetap tertaut walau rencana direvisi.
    /// </remarks>
    [Table("PrcNeedPlanLine", Schema = "public")]
    public class PrcNeedPlanLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid NeedPlanId { get; set; }
        public PrcNeedPlan? NeedPlan { get; set; }

        public Guid LineKey { get; set; } = Guid.NewGuid();

        public Guid ItemId { get; set; }
        public MstProcurementItem? Item { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal EstimatedUnitPrice { get; set; }

        /// <summary><c>Quantity × EstimatedUnitPrice</c>, dihitung server.</summary>
        [Column(TypeName = "numeric(18,2)")]
        public decimal EstimatedTotal { get; set; }

        /// <summary>Bulan kebutuhan 1–12.</summary>
        public int? NeededMonth { get; set; }

        public Guid? FundSourceId { get; set; }
        public MstProcurementFundSource? FundSource { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? Justification { get; set; }

        [MaxLength(1000)]
        public string? Specification { get; set; }
    }
}
