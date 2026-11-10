using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>Status capaian satu tujuan pada satu reasesmen — <c>data-dictionary.md</c> bagian 2.15.</summary>
    [Table("RhbGoalReview", Schema = "public")]
    public class RhbGoalReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProgressReviewId { get; set; }

        public RhbProgressReview? ProgressReview { get; set; }

        [Required]
        public Guid GoalId { get; set; }

        public RhbGoal? Goal { get; set; }

        public RehabGoalStatus AchievementStatus { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? NoteText { get; set; }
    }
}
