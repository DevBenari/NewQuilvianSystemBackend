using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Keputusan dokter apakah pasien layak menjalani HD. Sistem tidak pernah menentukan
    /// kelayakan sendiri; baris ini selalu lahir dari keputusan manusia.
    /// </summary>
    [Table("HmdEligibilityAssessment", Schema = "public")]
    public class HmdEligibilityAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        [Required]
        public Guid AssessedByDoctorId { get; set; }

        public MstDoctor? AssessedByDoctor { get; set; }

        public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

        public HmdEligibilityOutcome Outcome { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string IndicationSummary { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string DecisionReason { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? FollowUpInstruction { get; set; }
    }
}
