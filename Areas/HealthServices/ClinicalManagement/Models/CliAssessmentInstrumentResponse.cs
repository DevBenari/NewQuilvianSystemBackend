using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Jawaban satu dokumen terhadap satu versi instrumen beserta hasil hitung server — <c>BE-RWI-107</c>, kamus data 0.4 bagian 11.6.
    /// </summary>
    [Table("CliAssessmentInstrumentResponse", Schema = "public")]
    public class CliAssessmentInstrumentResponse : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? AssessmentId { get; set; }

        public Guid? CaseManagementEvaluationId { get; set; }

        public Guid InstrumentVersionId { get; set; }

        [Required]
        [MaxLength(64)]
        public string DefinitionHashSnapshot { get; set; } = string.Empty;

        /// <summary>Jawaban isian tanpa pengikatan kolom. SENSITIF.</summary>
        [Required]
        public string ResponsesJson { get; set; } = "{}";

        public decimal? TotalScore { get; set; }

        [MaxLength(50)]
        public string? BandCode { get; set; }

        [MaxLength(100)]
        public string? BandLabelSnapshot { get; set; }

        public bool IsAlertBand { get; set; } = false;

        public DateTime? ComputedAt { get; set; }
    }
}
