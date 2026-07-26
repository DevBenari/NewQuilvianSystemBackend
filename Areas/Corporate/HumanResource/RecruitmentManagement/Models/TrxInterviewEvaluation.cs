using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxInterviewEvaluation", Schema = "public")]
    public class TrxInterviewEvaluation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateInterviewId { get; set; }

        public Guid? EvaluatorUserId { get; set; }
        public Guid? EvaluatorWorkforceProfileId { get; set; }
        public Guid? RatingScaleId { get; set; }
        public decimal? TechnicalScore { get; set; }
        public decimal? BehavioralScore { get; set; }
        public decimal? CultureFitScore { get; set; }
        public decimal? OverallScore { get; set; }

        [MaxLength(30)]
        public string Recommendation { get; set; } = "Hold";

        [MaxLength(2000)]
        public string? Strengths { get; set; }

        [MaxLength(2000)]
        public string? Concerns { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public string? CriteriaResultJson { get; set; }
        public bool IsSubmitted { get; set; } = false;
        public DateTime? SubmittedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxCandidateInterview? CandidateInterview { get; set; }
        public ApplicationUser? EvaluatorUser { get; set; }
        public MstWorkforceProfile? EvaluatorWorkforceProfile { get; set; }
        public MstPerformanceRatingScale? RatingScale { get; set; }
    }
}
