using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Penetapan penyedia paket (BE-PBJ-PS-004; PBJ-DEC-070, 073, 076, 078, 085, 092..094): vendor terpilih, nilai akhir,
    /// peringkat tersimpan, dan alasan.
    /// </summary>
    /// <remarks>
    /// Satu penetapan hidup (<c>PENDING_APPROVAL</c>/<c>RETURNED</c>/<c>APPROVED</c>) per paket. Contoh: PT Sehat Medika
    /// Rp 41.250.000 untuk HPS Rp 42.750.000, terendah, disetujui → komitmen pagu IGD Rp 27.500.000 dan Rawat Inap
    /// Rp 13.750.000.
    /// </remarks>
    [Table("PrcPackageAward", Schema = "public")]
    public class PrcPackageAward : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageId { get; set; }

        public PrcPackage? Package { get; set; }

        public Guid QuotationId { get; set; }

        public PrcQuotation? Quotation { get; set; }

        public Guid SupplierId { get; set; }

        public MstSupplier? Supplier { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = ProcurementStatuses.Award.PendingApproval;

        /// <summary>SENSITIF. = total penawaran saat diajukan, termasuk PPN.</summary>
        public decimal AwardTotal { get; set; }

        /// <summary>SENSITIF. = HPS yang disetujui.</summary>
        public decimal HpsTotalSnapshot { get; set; }

        public bool IsOverHps { get; set; }

        [MaxLength(500)]
        public string? OverHpsReason { get; set; }

        public bool IsLowest { get; set; }

        [MaxLength(500)]
        public string? NonLowestReason { get; set; }

        public bool IsRestrictedVendor { get; set; }

        [MaxLength(500)]
        public string? RestrictedReason { get; set; }

        /// <summary>SENSITIF. Peringkat seluruh penawaran saat diajukan (jsonb).</summary>
        [Required]
        public string RankingSnapshotJson { get; set; } = "[]";

        public Guid? WorkflowInstanceId { get; set; }

        public int SubmissionCount { get; set; } = 1;

        public DateTimeOffset? SubmittedAt { get; set; }

        public DateTimeOffset? ApprovedAt { get; set; }

        public DateTimeOffset? RevokedAt { get; set; }

        [MaxLength(500)]
        public string? RevokeReason { get; set; }

        /// <summary>Disetujui tetapi vendor sudah tidak layak saat diterapkan (PBJ-DEC-092).</summary>
        [MaxLength(500)]
        public string? NotAppliedReason { get; set; }

        /// <summary>Token konkurensi; nilai lama ditolak 409 <c>PBJ-VAL-090</c>.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();
    }
}
