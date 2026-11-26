using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models
{
    /// <summary>
    /// Nilai satu kriteria pada penilaian vendor (BE-PBJ-VD-001; PBJ-DEC-050). Nama dan bobot kriteria disalin saat
    /// penilaian dibuat, sehingga perubahan master kriteria tidak mengubah nilai lama.
    /// </summary>
    [Table("PrcVendorEvaluationScore", Schema = "public")]
    public class PrcVendorEvaluationScore : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EvaluationId { get; set; }

        public Guid CriterionId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CriterionName { get; set; } = string.Empty;

        [Column(TypeName = "numeric(5,2)")]
        public decimal WeightPercent { get; set; }

        /// <summary>1–5; wajib terisi semua sebelum final (PBJ-VAL-110).</summary>
        public int? Score { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public PrcVendorEvaluation? Evaluation { get; set; }

        public MstProcurementEvaluationCriterion? Criterion { get; set; }
    }
}
