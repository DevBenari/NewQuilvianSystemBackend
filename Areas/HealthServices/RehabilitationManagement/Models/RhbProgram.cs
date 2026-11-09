using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Program intervensi satu disiplin, berversi — dokumen klinis
    /// <c>ClinicalDocumentKind.RehabProgram</c> (<c>data-dictionary.md</c> bagian 2.8, FEAT-013).
    /// </summary>
    /// <remarks>
    /// Satu versi <c>Active</c> per episode per disiplin, dijaga unique index
    /// <c>UX_RhbProgram_ActivePerDiscipline</c>. <see cref="ProgramVersion"/> adalah nomor urut
    /// versi di dalam satu disiplin episode, dihitung di bawah advisory lock — bukan nomor bisnis.
    /// Frekuensi dan jumlah sesi adalah isian terapis, bukan batas sistem (RHB-INV-10).
    /// </remarks>
    [Table("RhbProgram", Schema = "public")]
    public class RhbProgram : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        [Required]
        public Guid DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        public int ProgramVersion { get; set; } = 1;

        public Guid? SupersedesProgramId { get; set; }

        public RhbProgram? SupersedesProgram { get; set; }

        /// <summary>Kunjungan tempat program disusun; dipakai pendaftaran keutuhan RM.</summary>
        [Required]
        public Guid AuthoredEncounterId { get; set; }

        /// <summary>Contoh: "2× seminggu, 4 minggu". SENSITIF.</summary>
        [MaxLength(200)]
        public string? FrequencyText { get; set; }

        /// <summary>Informatif; bukan batas sistem.</summary>
        public int? PlannedSessionCount { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? PrecautionText { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EvaluationDueDate { get; set; }

        public RehabProgramStatus ProgramStatus { get; set; } = RehabProgramStatus.Draft;

        /// <summary>Ditandai deviasi sesi atau keputusan reasesmen <c>Modify</c>.</summary>
        public bool NeedsReview { get; set; }

        /// <summary>Wajib bila versi lebih dari 1. SENSITIF.</summary>
        [MaxLength(500)]
        public string? ModificationReason { get; set; }

        [Required]
        public Guid AuthorUserId { get; set; }

        public DateTime? ActivatedAt { get; set; }

        public DateTime? StoppedAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? StopReason { get; set; }

        public int Version { get; set; }

        public ICollection<RhbProgramIntervention> Interventions { get; set; } = new List<RhbProgramIntervention>();

        public ICollection<RhbProgramGoal> Goals { get; set; } = new List<RhbProgramGoal>();
    }
}
