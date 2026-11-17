using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models
{
    /// <summary>
    /// Rencana paket pengadaan hasil konsolidasi rencana kebutuhan — kamus data bagian 3
    /// (BE-PBJ-011; PBJ-DEC-028).
    /// </summary>
    /// <remarks>
    /// Bukan paket eksekusi; paket eksekusi milik sub-modul <c>pemilihan-penyedia</c>. Semua baris paket
    /// berjenis pengadaan yang sama dengan kepalanya (PBJ-VAL-051). Contoh: RPK-000002 "Alat Bantu Mobilitas
    /// 2027" berjenis Aset memuat kursi roda dari IGD 3, Rawat Inap 5, dan Poli 22.
    /// </remarks>
    [Table("PrcPackagePlan", Schema = "public")]
    public class PrcPackagePlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Diisi saat pertama diajukan (<c>RPK-000002</c>).</summary>
        [MaxLength(30)]
        public string? PackagePlanNumber { get; set; }

        public int FiscalYear { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public Guid ProcurementTypeId { get; set; }
        public MstProcurementType? ProcurementType { get; set; }

        /// <summary>Teks bebas sampai metode pengadaan diputuskan (<c>PBJ-OQ-005</c>).</summary>
        [MaxLength(100)]
        public string? IndicativeMethod { get; set; }

        /// <summary>Bulan target 1–12.</summary>
        public int? TargetMonth { get; set; }

        /// <summary>Σ alokasi × harga estimasi baris rencana, dihitung server.</summary>
        [Column(TypeName = "numeric(18,2)")]
        public decimal EstimatedValue { get; set; }

        /// <summary>Penanggung jawab paket.</summary>
        public Guid? PicUserId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = ProcurementStatuses.Plan.Draft;

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? CancelReason { get; set; }

        /// <summary>Naik setiap kali diajukan; bagian kunci idempoten instance approval.</summary>
        public int SubmissionCount { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public DateTimeOffset? SubmittedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }

        /// <summary>Penjaga data basi (409 PBJ-VAL-090).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public ICollection<PrcPackagePlanLine> Lines { get; set; } = new List<PrcPackagePlanLine>();
    }
}
