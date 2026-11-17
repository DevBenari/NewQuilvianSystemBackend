using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models
{
    /// <summary>
    /// Rencana kebutuhan tahunan satu unit, berversi — kamus data bagian 3 (BE-PBJ-009; G-05).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu unit × tahun hanya punya satu versi berlaku (<see cref="IsCurrentVersion"/>). Versi yang
    /// disetujui tidak diubah; perubahan dibuat sebagai versi baru (<see cref="PreviousVersionId"/>),
    /// dan saat versi baru disetujui versi lama menjadi <c>SUPERSEDED</c>. Contoh: RKB-000004 v1 IGD
    /// 2027 berisi 10 monitor; revisi v2 menaikkannya menjadi 12 dan memakai nomor yang sama.
    /// </para>
    /// <para>
    /// <see cref="PlanNumber"/> dialokasikan saat versi 1 pertama kali diajukan; versi revisi menyalin
    /// nomornya, sehingga keunikan berlaku pada pasangan nomor + versi.
    /// </para>
    /// </remarks>
    [Table("PrcNeedPlan", Schema = "public")]
    public class PrcNeedPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(30)]
        public string? PlanNumber { get; set; }

        public int FiscalYear { get; set; }

        /// <summary>Unit penyusun (<c>MstDepartment</c>, PBJ-DEC-020).</summary>
        public Guid DepartmentId { get; set; }
        public MstDepartment? Department { get; set; }

        public int VersionNo { get; set; } = 1;

        public Guid? PreviousVersionId { get; set; }
        public PrcNeedPlan? PreviousVersion { get; set; }

        /// <summary>Hanya satu versi berlaku per unit-tahun (unik parsial di basis data).</summary>
        public bool IsCurrentVersion { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = ProcurementStatuses.Plan.Draft;

        /// <summary>SENSITIF. Wajib pada versi ≥ 2.</summary>
        [MaxLength(1000)]
        public string? RevisionReason { get; set; }

        /// <summary>Σ total baris, dihitung server; menjadi nominal approval.</summary>
        [Column(TypeName = "numeric(18,2)")]
        public decimal EstimatedTotal { get; set; }

        /// <summary>Naik setiap kali diajukan; bagian kunci idempoten instance approval.</summary>
        public int SubmissionCount { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public DateTimeOffset? SubmittedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }

        /// <summary>Penjaga data basi (409 PBJ-VAL-090).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public ICollection<PrcNeedPlanLine> Lines { get; set; } = new List<PrcNeedPlanLine>();
    }
}
