using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Ringkasan akhir dan tindak lanjut — dokumen klinis
    /// <c>ClinicalDocumentKind.RehabDischargeSummary</c> (<c>data-dictionary.md</c> bagian 2.16,
    /// FEAT-026/027). Satu per episode; tanda tangannya menutup episode.
    /// </summary>
    [Table("RhbDischargeSummary", Schema = "public")]
    public class RhbDischargeSummary : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public RehabEpisodeClosureReason? ClosureReason { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(2000)]
        public string? MainServicesText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(2000)]
        public string? GoalAchievementText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(2000)]
        public string? FinalConditionText { get; set; }

        public RehabFollowUpType FollowUpType { get; set; } = RehabFollowUpType.None;

        /// <summary>SENSITIF.</summary>
        [MaxLength(2000)]
        public string? FollowUpInstructionText { get; set; }

        [MaxLength(200)]
        public string? FollowUpTargetText { get; set; }

        public DateOnly? FollowUpDueDate { get; set; }

        [Required]
        public Guid AuthorUserId { get; set; }

        public RehabRecordStatus RecordStatus { get; set; } = RehabRecordStatus.Draft;

        public DateTime? SignedAt { get; set; }

        public int Version { get; set; }
    }
}
