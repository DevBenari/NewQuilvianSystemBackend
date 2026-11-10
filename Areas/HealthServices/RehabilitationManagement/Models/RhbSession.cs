using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Satu sesi terapi — dokumen klinis <c>ClinicalDocumentKind.RehabSession</c>
    /// (<c>data-dictionary.md</c> bagian 2.12, FEAT-016..018 dan FEAT-032).
    /// </summary>
    /// <remarks>
    /// Sesi hanya merujuk program aktif dan kunjungan hari itu yang masih terbuka (RHB-INV-04,
    /// RHB-DEC-009). Sesi <c>NotDone</c> tidak pernah tertagih (RHB-INV-06). Kontraindikasi
    /// dicatat, tidak ditimpa.
    /// </remarks>
    [Table("RhbSession", Schema = "public")]
    public class RhbSession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor dari <c>NumberSeriesAllocator</c>, deret <c>RHB_SESSION</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string SessionNumber { get; set; } = string.Empty;

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        [Required]
        public Guid ProgramId { get; set; }

        public RhbProgram? Program { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public Guid? InpEpisodeId { get; set; }

        [Required]
        public Guid TherapistUserId { get; set; }

        public RehabSessionStatus SessionStatus { get; set; }

        public RehabNotDoneReason? NotDoneReason { get; set; }

        public bool IdentityVerified { get; set; }

        public bool ProgramVerified { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? CurrentConditionText { get; set; }

        public bool ContraindicationFound { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ContraindicationText { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        /// <summary>Toleransi/respons pasien. SENSITIF.</summary>
        [MaxLength(2000)]
        public string? ResponseText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ImmediateOutcomeText { get; set; }

        public RehabDeviationType DeviationType { get; set; } = RehabDeviationType.None;

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? DeviationReasonText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ImmediateActionText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(200)]
        public string? ContactedPartyText { get; set; }

        public RehabRecordStatus RecordStatus { get; set; } = RehabRecordStatus.Draft;

        public DateTime? SignedAt { get; set; }

        public RehabBillingHandoffStatus BillingHandoffStatus { get; set; } = RehabBillingHandoffStatus.NotRequired;

        public DateTime? BillingHandoffAt { get; set; }

        /// <summary>Galat handoff tanpa data klinis.</summary>
        [MaxLength(500)]
        public string? BillingHandoffError { get; set; }

        public int Version { get; set; }

        public ICollection<RhbSessionIntervention> Interventions { get; set; } = new List<RhbSessionIntervention>();
    }
}
