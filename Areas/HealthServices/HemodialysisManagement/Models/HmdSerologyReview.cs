using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Rujukan hasil serologi dari Laboratorium beserta status tinjauannya — bagian paling
    /// sensitif modul ini.
    /// </summary>
    /// <remarks>
    /// <see cref="LabExaminationId"/> sengaja tanpa foreign key: hasil laboratorium milik modul
    /// lain, dan FK lintas modul mengunci Hemodialisa pada perubahan skema Laboratorium.
    /// </remarks>
    [Table("HmdSerologyReview", Schema = "public")]
    public class HmdSerologyReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        public HmdSerologyTestType TestType { get; set; }

        public Guid? LabExaminationId { get; set; }

        public DateOnly ResultDate { get; set; }

        /// <summary>SENSITIF.</summary>
        public HmdSerologyResultFlag ResultFlag { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? ResultSummary { get; set; }

        public HmdSerologyReviewStatus ReviewStatus { get; set; } = HmdSerologyReviewStatus.PendingReview;

        public Guid? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ReviewNote { get; set; }
    }
}
