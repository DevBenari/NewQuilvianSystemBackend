using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Hasil evaluasi teknis per baris penawaran × kriteria × evaluator (BE-PBJ-PS-004; PBJ-DEC-074): Lulus/Tidak lulus.
    /// </summary>
    /// <remarks>
    /// Tidak lulus wajib alasan (422 PBJ-VAL-136). Contoh: kursi roda PT Alkes Maju gagal "beban 120 kg" karena brosur
    /// menyebut 100 kg.
    /// </remarks>
    [Table("PrcTechnicalEvaluationResult", Schema = "public")]
    public class PrcTechnicalEvaluationResult : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid QuotationLineId { get; set; }

        public PrcQuotationLine? QuotationLine { get; set; }

        public Guid CriterionId { get; set; }

        public PrcTechnicalCriterion? Criterion { get; set; }

        public Guid EvaluatorUserId { get; set; }

        /// <summary><c>PASS</c> atau <c>FAIL</c>.</summary>
        [Required]
        [MaxLength(10)]
        public string Result { get; set; } = ProcurementStatuses.TechnicalEvaluationResult.Pass;

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTimeOffset EvaluatedAt { get; set; }

        /// <summary>Hasil asal pada paket hasil pecah.</summary>
        public Guid? SourceResultId { get; set; }

        public PrcTechnicalEvaluationResult? SourceResult { get; set; }
    }
}
