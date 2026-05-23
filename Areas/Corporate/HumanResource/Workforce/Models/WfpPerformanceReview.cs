using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpPerformanceReview", Schema = "public")]
    public class WfpPerformanceReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReviewPeriod { get; set; } = string.Empty;

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public Guid ReviewerUserId { get; set; }

        public PerformanceReviewType ReviewType { get; set; } = PerformanceReviewType.Unknown;

        [Column(TypeName = "numeric(18,2)")]
        public decimal TotalScore { get; set; } = 0;

        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        public PerformanceReviewStatus ReviewStatus { get; set; } = PerformanceReviewStatus.Draft;

        [MaxLength(1000)]
        public string? StrengthNotes { get; set; }

        [MaxLength(1000)]
        public string? ImprovementNotes { get; set; }

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }

        public bool IsFinalized { get; set; } = false;

        public DateTime? FinalizedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ApplicationUser? ReviewerUser { get; set; }

        public ICollection<WfpPerformanceReviewDetail> Details { get; set; } = new List<WfpPerformanceReviewDetail>();
    }

    [Table("WfpPerformanceReviewDetail", Schema = "public")]
    public class WfpPerformanceReviewDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PerformanceReviewId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CriteriaCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = string.Empty;

        [Column(TypeName = "numeric(18,2)")]
        public decimal Score { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal Weight { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal WeightedScore { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public WfpPerformanceReview? PerformanceReview { get; set; }
    }
}
