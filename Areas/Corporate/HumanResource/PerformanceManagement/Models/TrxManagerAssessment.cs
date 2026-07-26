using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    [Table("TrxManagerAssessment", Schema = "public")]
    public class TrxManagerAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid ManagerUserId { get; set; }

        public Guid? PerformanceReviewId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssessmentStatus { get; set; } = "Draft";

        public decimal? OverallScore { get; set; }

        [MaxLength(100)]
        public string? OverallRating { get; set; }

        [MaxLength(3000)]
        public string? Strengths { get; set; }

        [MaxLength(3000)]
        public string? ImprovementAreas { get; set; }

        [MaxLength(3000)]
        public string? ManagerRecommendation { get; set; }

        public string? AssessmentJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public WfpPerformanceReview? PerformanceReview { get; set; }
    }
}
