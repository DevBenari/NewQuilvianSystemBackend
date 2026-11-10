using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Tujuan rehabilitasi terukur — <c>data-dictionary.md</c> bagian 2.7, FEAT-012.
    /// </summary>
    /// <remarks>
    /// Tujuan yang dipakai program aktif tidak disunting; perubahan dibuat lewat revisi yang
    /// membentuk baris baru dengan <see cref="SupersedesGoalId"/>.
    /// </remarks>
    [Table("RhbGoal", Schema = "public")]
    public class RhbGoal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        /// <summary>Kosong berarti tujuan medis/bersama.</summary>
        public Guid? DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        public RehabGoalTerm Term { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string DescriptionText { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(500)]
        public string BaselineText { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(500)]
        public string TargetText { get; set; } = string.Empty;

        public DateOnly TargetDate { get; set; }

        public RehabGoalStatus GoalStatus { get; set; } = RehabGoalStatus.Active;

        public Guid? SupersedesGoalId { get; set; }

        public RhbGoal? SupersedesGoal { get; set; }

        [Required]
        public Guid AuthorUserId { get; set; }

        /// <summary>Tujuan sudah ditinjau bersama pasien/keluarga (FEAT-012).</summary>
        public bool AgreedWithPatient { get; set; }

        public int Version { get; set; }
    }
}
