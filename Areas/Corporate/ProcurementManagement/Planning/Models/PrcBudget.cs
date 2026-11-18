using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models
{
    /// <summary>
    /// Pagu pengadaan per tahun × unit × kategori × sumber dana, berversi — kamus data bagian 3
    /// (BE-PBJ-010; PBJ-DEC-009, 030).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu kunci hanya punya satu versi berlaku (<see cref="IsCurrentVersion"/>). Revisi membuat versi
    /// baru; versi lama menjadi <c>SUPERSEDED</c> saat revisi disetujui.
    /// </para>
    /// <para>
    /// <see cref="LineageKey"/> sama untuk seluruh versi satu pagu (nilai <c>Id</c> versi 1). Komitmen
    /// menunjuk garis keturunan ini, bukan <c>Id</c> versi, agar tidak hilang saat pagu direvisi.
    /// Contoh: pagu Radiologi v1 Rp 500 juta dengan komitmen Rp 120 juta; revisi v2 Rp 600 juta → sisa
    /// Rp 480 juta.
    /// </para>
    /// </remarks>
    [Table("PrcBudget", Schema = "public")]
    public class PrcBudget : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int FiscalYear { get; set; }

        public Guid DepartmentId { get; set; }
        public MstDepartment? Department { get; set; }

        public Guid ItemCategoryId { get; set; }
        public MstProcurementItemCategory? ItemCategory { get; set; }

        public Guid FundSourceId { get; set; }
        public MstProcurementFundSource? FundSource { get; set; }

        /// <summary>SENSITIF (rahasia bisnis). &gt; 0.</summary>
        [Column(TypeName = "numeric(18,2)")]
        public decimal Amount { get; set; }

        public int VersionNo { get; set; } = 1;

        public Guid? PreviousVersionId { get; set; }
        public PrcBudget? PreviousVersion { get; set; }

        /// <summary>Garis keturunan pagu: <c>Id</c> versi 1, disalin ke setiap revisi.</summary>
        public Guid LineageKey { get; set; }

        public bool IsCurrentVersion { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = ProcurementStatuses.Plan.Draft;

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? RevisionReason { get; set; }

        /// <summary>Naik setiap kali diajukan; bagian kunci idempoten instance approval.</summary>
        public int SubmissionCount { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public DateTimeOffset? SubmittedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }

        /// <summary>Penjaga data basi (409 PBJ-VAL-090).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();
    }
}
