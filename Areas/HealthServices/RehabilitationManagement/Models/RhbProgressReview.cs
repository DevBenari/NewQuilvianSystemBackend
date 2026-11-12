using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Reasesmen dan keputusan lanjut/modifikasi/henti/rujuk/selesai — dokumen klinis
    /// <c>ClinicalDocumentKind.RehabProgressReview</c> (<c>data-dictionary.md</c> bagian 2.14,
    /// FEAT-024/025).
    /// </summary>
    /// <remarks>
    /// <see cref="DisciplineId"/> kosong berarti reasesmen medis Sp.KFR. Efek keputusan
    /// diterapkan saat tanda tangan (<c>state-transition-matrix.md</c> bagian 7).
    /// </remarks>
    [Table("RhbProgressReview", Schema = "public")]
    public class RhbProgressReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        public Guid? DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        /// <summary>Wajib bila reasesmen disiplin.</summary>
        public Guid? ProgramId { get; set; }

        public RhbProgram? Program { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid AuthorUserId { get; set; }

        public DateTime ReviewedAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(4000)]
        public string? ProgressSummaryText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? BarrierText { get; set; }

        /// <summary>Wajib bila instrumen berbeda dari baseline (FEAT-024).</summary>
        [MaxLength(500)]
        public string? InstrumentChangeNote { get; set; }

        public RehabProgressDecision? Decision { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? DecisionReasonText { get; set; }

        [MaxLength(200)]
        public string? ReferralTargetText { get; set; }

        public RehabRecordStatus RecordStatus { get; set; } = RehabRecordStatus.Draft;

        public DateTime? SignedAt { get; set; }

        public int Version { get; set; }

        public ICollection<RhbGoalReview> GoalReviews { get; set; } = new List<RhbGoalReview>();

        public ICollection<RhbMeasurement> Measurements { get; set; } = new List<RhbMeasurement>();
    }
}
