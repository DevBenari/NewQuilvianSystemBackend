using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models
{
    /// <summary>
    /// Penilaian vendor berkala per vendor × kategori × periode (BE-PBJ-VD-001; PBJ-DEC-044, PBJ-DEC-049, PBJ-DEC-059).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Penilai (<c>CreateBy</c>) tidak boleh memfinalkan penilaiannya sendiri. Penilaian <c>FINAL</c> terkunci; koreksi
    /// dibuat sebagai revisi (<see cref="PreviousEvaluationId"/>) dan versi lama menjadi <c>SUPERSEDED</c> saat revisi
    /// difinalkan. Rekomendasi tidak mengubah status vendor (PBJ-DEC-048).
    /// </para>
    /// <para>
    /// <see cref="SourceType"/> <c>MANUAL</c> untuk penilaian berkala; <c>CONTRACT</c> dicadangkan bagi evaluasi per
    /// kontrak (S-V7) supaya laporan dapat membedakan sumber nilai. Contoh: PT Sehat Medika, Alat Kesehatan,
    /// Semester I 2027, nilai akhir 4,20, rekomendasi Lanjutkan.
    /// </para>
    /// </remarks>
    [Table("PrcVendorEvaluation", Schema = "public")]
    public class PrcVendorEvaluation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VendorProfileId { get; set; }

        public Guid ItemCategoryId { get; set; }

        public int PeriodYear { get; set; }

        /// <summary><c>SEMESTER</c> (bawaan), <c>QUARTER</c>, <c>YEAR</c>.</summary>
        [Required]
        [MaxLength(10)]
        public string PeriodType { get; set; } = "SEMESTER";

        public int PeriodNo { get; set; }

        [Required]
        [MaxLength(10)]
        public string SourceType { get; set; } = "MANUAL";

        /// <summary><c>DRAFT</c>, <c>FINAL</c>, <c>SUPERSEDED</c>, <c>CANCELLED</c>.</summary>
        [Required]
        [MaxLength(12)]
        public string Status { get; set; } = "DRAFT";

        /// <summary>Rata-rata berbobot, dihitung server.</summary>
        [Column(TypeName = "numeric(4,2)")]
        public decimal? FinalScore { get; set; }

        /// <summary><c>CONTINUE</c>, <c>RESTRICT</c>, <c>BLOCK</c>; wajib saat final.</summary>
        [MaxLength(10)]
        public string? Recommendation { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Guid? PreviousEvaluationId { get; set; }

        [MaxLength(1000)]
        public string? RevisionReason { get; set; }

        public Guid? FinalizedByUserId { get; set; }

        public DateTimeOffset? FinalizedAt { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public PrcVendorProfile? VendorProfile { get; set; }

        public MstProcurementItemCategory? ItemCategory { get; set; }

        public PrcVendorEvaluation? PreviousEvaluation { get; set; }

        public ICollection<PrcVendorEvaluationScore> Scores { get; set; } = new List<PrcVendorEvaluationScore>();
    }
}
