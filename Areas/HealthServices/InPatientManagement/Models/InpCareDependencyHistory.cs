using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Riwayat append-only pencatatan High/Low Dependency satu episode (RWI-DEC-302, INV-DNH-06).
    /// Satu baris per pencatatan; baris lama tidak pernah diubah maupun dihapus.
    /// </summary>
    [Table("InpCareDependencyHistory", Schema = "public")]
    public class InpCareDependencyHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        /// <summary>Nilai sebelum pencatatan; <c>null</c> berarti sebelumnya belum dinilai.</summary>
        public InpCareDependencyLevel? PreviousLevel { get; set; }

        public InpCareDependencyLevel NewLevel { get; set; }

        public InpCareDependencySource Source { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime RecordedAtUtc { get; set; }

        /// <summary>Catatan pencatat. Kolom sensitif; tidak boleh masuk payload logger.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        public InpEpisode? Episode { get; set; }

        public ApplicationUser? ActorUser { get; set; }
    }
}
