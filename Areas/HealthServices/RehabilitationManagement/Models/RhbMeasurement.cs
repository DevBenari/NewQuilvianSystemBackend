using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Satu hasil ukur — berinstrumen (mesin instrumen klinis) atau terstruktur
    /// (<c>data-dictionary.md</c> bagian 2.6, RHB-DEC-006/017).
    /// </summary>
    /// <remarks>
    /// Tepat satu dari <see cref="AssessmentId"/> atau <see cref="ProgressReviewId"/> terisi,
    /// dijaga check constraint <c>CK_RhbMeasurement_Owner</c>.
    /// </remarks>
    [Table("RhbMeasurement", Schema = "public")]
    public class RhbMeasurement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? AssessmentId { get; set; }

        public RhbAssessment? Assessment { get; set; }

        public Guid? ProgressReviewId { get; set; }

        public RhbProgressReview? ProgressReview { get; set; }

        public RehabMeasurementSource Source { get; set; }

        /// <summary>Versi instrumen (<c>CliClinicalInstrumentVersion</c>) bila berinstrumen.</summary>
        public Guid? InstrumentVersionId { get; set; }

        /// <summary>Jenis instrumen bila berinstrumen; disimpan sebagai integer.</summary>
        public ClinicalInstrumentKind? InstrumentKind { get; set; }

        [Required]
        [MaxLength(150)]
        public string InstrumentName { get; set; } = string.Empty;

        /// <summary>Contoh: "Bahu kanan — fleksi". SENSITIF.</summary>
        [MaxLength(150)]
        public string? BodyPartOrDomain { get; set; }

        /// <summary>Jawaban instrumen (jsonb), divalidasi mesin instrumen. SENSITIF.</summary>
        public string? AnswersJson { get; set; }

        /// <summary>SENSITIF.</summary>
        public decimal? NumericValue { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(200)]
        public string? ValueText { get; set; }

        /// <summary>Contoh: "derajat", "detik", "skor".</summary>
        [MaxLength(30)]
        public string? Unit { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(50)]
        public string? ScoreBandCode { get; set; }

        public DateTime MeasuredAt { get; set; }
    }
}
