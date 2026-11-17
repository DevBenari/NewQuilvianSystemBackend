using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models
{
    /// <summary>
    /// Kepala permintaan pengadaan (PR) — kamus data bagian 4 (BE-PBJ-006).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="RequestNumber"/> kosong selama draf dan diisi saat pertama diajukan
    /// (<c>PR-000031</c>, PBJ-DEC-025/031). Nominal penentu jenjang approval adalah
    /// <see cref="PurchaseTotal"/>, yaitu nilai kebutuhan beli setelah cek stok (PBJ-DEC-027).
    /// </para>
    /// <para>
    /// Status mengikuti <see cref="ProcurementStatuses.PurchaseRequest"/>. Perubahan isi hanya boleh
    /// saat <c>DRAFT</c>/<c>RETURNED</c> oleh pembuatnya, dengan <see cref="RowVersion"/> sebagai
    /// penjaga data basi.
    /// </para>
    /// </remarks>
    [Table("PrcPurchaseRequest", Schema = "public")]
    public class PrcPurchaseRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(30)]
        public string? RequestNumber { get; set; }

        /// <summary>Unit peminta (<c>MstDepartment</c>, PBJ-DEC-020).</summary>
        public Guid DepartmentId { get; set; }
        public MstDepartment? Department { get; set; }

        public Guid RequesterUserId { get; set; }

        public int FiscalYear { get; set; }

        public Guid FundSourceId { get; set; }
        public MstProcurementFundSource? FundSource { get; set; }

        public DateOnly NeededByDate { get; set; }

        /// <summary><c>NORMAL</c> atau <c>URGENT</c>.</summary>
        [Required]
        [MaxLength(20)]
        public string UrgencyLevel { get; set; } = ProcurementUrgencyLevels.Normal;

        /// <summary>SENSITIF. Wajib bila mendesak (PBJ-VAL-008).</summary>
        [MaxLength(1000)]
        public string? UrgencyReason { get; set; }

        public DateOnly? UrgencyDeadline { get; set; }

        /// <summary>SENSITIF. Alasan kebutuhan; wajib saat diajukan (PBJ-VAL-006).</summary>
        [Required]
        [MaxLength(2000)]
        public string Justification { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = ProcurementStatuses.PurchaseRequest.Draft;

        public bool IsOutOfPlan { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal RequestedTotal { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal PurchaseTotal { get; set; }

        public bool IsOverBudget { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal OverBudgetAmount { get; set; }

        /// <summary>Naik setiap kali diajukan; bagian kunci idempoten instance approval.</summary>
        public int SubmissionCount { get; set; }

        public DateTimeOffset? SubmittedAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        /// <summary>Diisi sub-modul Pemilihan Penyedia saat PR masuk paket.</summary>
        public Guid? ConvertedPackageId { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? CancelReason { get; set; }

        /// <summary>Penjaga data basi (409 PBJ-VAL-090).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public ICollection<PrcPurchaseRequestLine> Lines { get; set; } = new List<PrcPurchaseRequestLine>();
    }
}
