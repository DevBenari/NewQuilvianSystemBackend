using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Logbook Mingguan Monitoring Pembinaan PIP 60 Hari (HRD-DEC-167, PRF-B5, VR-PRF-09)
    /// </summary>
    [Table("HrdPerformancePipActionLog", Schema = "public")]
    public class HrdPerformancePipActionLog : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PipId { get; set; }

        public int WeekNumber { get; set; }

        public DateTime LogDate { get; set; }

        [Required]
        public string ProgressSummary { get; set; } = string.Empty;

        public string? DeficienciesObserved { get; set; }

        [Required]
        public string MentorFeedback { get; set; } = string.Empty;

        public bool IsTargetOnTrack { get; set; } = true;

        [Required]
        public Guid LoggedBy { get; set; }

        public virtual HrdPerformanceImprovementPlan? PerformanceImprovementPlan { get; set; }
        public virtual MstWorkforceProfile? LoggedByProfile { get; set; }

        [NotMapped] public Guid PerformanceImprovementPlanId { get => PipId; set => PipId = value; }
        [NotMapped] public string ActionItemCompleted { get => ProgressSummary; set => ProgressSummary = value; }
        [NotMapped] public string MentorObservationNotes { get => MentorFeedback; set => MentorFeedback = value; }
        [NotMapped] public string ProgressStatus { get => IsTargetOnTrack ? "ON_TRACK" : "NEEDS_ATTENTION"; set => IsTargetOnTrack = (value == "ON_TRACK"); }
        [NotMapped] public Guid RecordedByMentorId { get => LoggedBy; set => LoggedBy = value; }

    }
}
