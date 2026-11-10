using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Asesmen medis awal (Sp.KFR) atau asesmen awal profesi — dokumen klinis
    /// <c>ClinicalDocumentKind.RehabAssessment</c> (<c>data-dictionary.md</c> bagian 2.5).
    /// </summary>
    /// <remarks>
    /// Penguncian dan tanda tangan dipegang mesin keutuhan Rekam Medis; <see cref="RecordStatus"/>
    /// hanya status alur kerja. Satu asesmen medis awal final per episode dijaga unique index
    /// <c>UX_RhbAssessment_MedicalFinal</c>.
    /// </remarks>
    [Table("RhbAssessment", Schema = "public")]
    public class RhbAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        public RehabAssessmentType AssessmentType { get; set; }

        /// <summary>Wajib untuk asesmen profesi; kosong untuk asesmen medis.</summary>
        public Guid? DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public Guid? InpEpisodeId { get; set; }

        /// <summary>Penulis. Tidak pernah berubah dan tidak masuk DTO ubah.</summary>
        [Required]
        public Guid AuthorUserId { get; set; }

        public DateTime AssessedAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(4000)]
        public string? HistoryText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(4000)]
        public string? PhysicalFunctionalFindingsText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(2000)]
        public string? RiskAndContraindicationText { get; set; }

        /// <summary>Diagnosis medis atau diagnosis profesi. SENSITIF.</summary>
        [MaxLength(2000)]
        public string? ProblemOrDiagnosisText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? PrognosisText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(2000)]
        public string? ConclusionText { get; set; }

        /// <summary>Harapan pasien. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? PatientGoalText { get; set; }

        public RehabRecordStatus RecordStatus { get; set; } = RehabRecordStatus.Draft;

        public DateTime? SignedAt { get; set; }

        public int Version { get; set; }

        public ICollection<RhbMeasurement> Measurements { get; set; } = new List<RhbMeasurement>();
    }
}
