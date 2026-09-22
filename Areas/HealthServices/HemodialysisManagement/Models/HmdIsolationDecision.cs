using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Keputusan operasional apakah pasien perlu mesin atau station khusus. Sistem tidak pernah
    /// menyimpulkannya sendiri dari hasil laboratorium.
    /// </summary>
    [Table("HmdIsolationDecision", Schema = "public")]
    public class HmdIsolationDecision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        /// <summary>SENSITIF — menyiratkan status infeksi pasien.</summary>
        public HmdIsolationRequirement Requirement { get; set; } = HmdIsolationRequirement.None;

        public DateOnly EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; }

        /// <summary>Keputusan yang sedang berlaku. Hanya satu per episode.</summary>
        public bool IsActive { get; set; } = true;

        [Required]
        public Guid DecidedByUserId { get; set; }

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Dasar keputusan. SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public Guid? SerologyReviewId { get; set; }

        public HmdSerologyReview? SerologyReview { get; set; }
    }
}
