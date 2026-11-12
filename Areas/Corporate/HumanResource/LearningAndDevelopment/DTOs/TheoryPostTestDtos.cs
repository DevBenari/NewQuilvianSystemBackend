using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class SubmitTheoryPostTestRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public Guid? CompetencyId { get; set; }

        [Range(0, 100, ErrorMessage = "Nilai harus berada di rentang 0 sampai 100.")]
        public decimal Score { get; set; }

        public decimal PassingScore { get; set; } = 75.00m;

        public DateTime? StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string? AnswerSnapshotJson { get; set; }

        public string? AssessmentResultJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class TheoryPostTestResultDto
    {
        public Guid Id { get; set; }
        public Guid TrainingSessionId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public Guid? CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public string AssessmentType { get; set; } = "PostTest";
        public int AttemptNumber { get; set; }
        public decimal Score { get; set; }
        public decimal MaximumScore { get; set; } = 100.00m;
        public decimal PassingScore { get; set; } = 75.00m;
        public bool IsPassed { get; set; }
        public int RemainingRemedialAttempts { get; set; }
        public string ResultStatus { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ParticipantAssessmentSummaryDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public Guid TrainingSessionId { get; set; }
        public int TotalAttempts { get; set; }
        public decimal HighestScore { get; set; }
        public decimal LatestScore { get; set; }
        public bool IsPassed { get; set; }
        public bool CanRemedial { get; set; }
        public int RemainingRemedials { get; set; }
        public string FinalVerdict { get; set; } = string.Empty;
        public List<TheoryPostTestResultDto> History { get; set; } = new List<TheoryPostTestResultDto>();
    }
}
