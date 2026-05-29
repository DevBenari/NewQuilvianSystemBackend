using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforcePerformanceReviewResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string ReviewPeriod { get; set; } = string.Empty;

        public DateTime ReviewDate { get; set; }

        public Guid ReviewerUserId { get; set; }

        public string? ReviewerUserName { get; set; }

        public PerformanceReviewType ReviewType { get; set; }

        public decimal TotalScore { get; set; }

        public PerformanceFinalRating FinalRating { get; set; }

        public PerformanceReviewStatus ReviewStatus { get; set; }

        public string? StrengthNotes { get; set; }

        public string? ImprovementNotes { get; set; }

        public string? RecommendationNotes { get; set; }

        public bool IsFinalized { get; set; }

        public DateTime? FinalizedAt { get; set; }

        public bool IsActive { get; set; }

        public int DetailCount { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforcePerformanceReviewDetailResponse> Details { get; set; } = new();
    }

    public class WorkforcePerformanceReviewListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int InProgressData { get; set; }

        public int CompletedData { get; set; }

        public int FinalizedData { get; set; }

        public decimal AverageScore { get; set; }

        public List<WorkforcePerformanceReviewResponse> Items { get; set; } = new();
    }

    public class WorkforcePerformanceReviewDetailResponse
    {
        public Guid Id { get; set; }

        public Guid PerformanceReviewId { get; set; }

        public string CriteriaCode { get; set; } = string.Empty;

        public string CriteriaName { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal Weight { get; set; }

        public decimal WeightedScore { get; set; }

        public string? Notes { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateWorkforcePerformanceReviewRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReviewPeriod { get; set; } = string.Empty;

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public Guid ReviewerUserId { get; set; }

        public PerformanceReviewType ReviewType { get; set; } = PerformanceReviewType.Unknown;

        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        public PerformanceReviewStatus ReviewStatus { get; set; } = PerformanceReviewStatus.Draft;

        [MaxLength(1000)]
        public string? StrengthNotes { get; set; }

        [MaxLength(1000)]
        public string? ImprovementNotes { get; set; }

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateWorkforcePerformanceReviewDetailRequest> Details { get; set; } = new();
    }

    public class UpdateWorkforcePerformanceReviewRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReviewPeriod { get; set; } = string.Empty;

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public Guid ReviewerUserId { get; set; }

        public PerformanceReviewType ReviewType { get; set; } = PerformanceReviewType.Unknown;

        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        public PerformanceReviewStatus ReviewStatus { get; set; } = PerformanceReviewStatus.Draft;

        [MaxLength(1000)]
        public string? StrengthNotes { get; set; }

        [MaxLength(1000)]
        public string? ImprovementNotes { get; set; }

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforcePerformanceReviewStatusRequest
    {
        public PerformanceReviewStatus ReviewStatus { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }
    }

    public class FinalizeWorkforcePerformanceReviewRequest
    {
        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }
    }

    public class CreateWorkforcePerformanceReviewDetailRequest
    {
        [Required]
        [MaxLength(100)]
        public string CriteriaCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = string.Empty;

        public decimal Score { get; set; } = 0;

        public decimal Weight { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateWorkforcePerformanceReviewDetailRequest
    {
        [Required]
        [MaxLength(100)]
        public string CriteriaCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = string.Empty;

        public decimal Score { get; set; } = 0;

        public decimal Weight { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
