using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{

    public class WfpPerformanceReviewSummaryResponse
    {
        public int TotalReview { get; set; }
        public int ActiveReview { get; set; }
        public int DraftReview { get; set; }
        public int FinalizedReview { get; set; }
        public int AcknowledgedReview { get; set; }
        public decimal AverageFinalScore { get; set; }
    }

    public class WfpPerformanceReviewResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? PerformanceCycleId { get; set; }
        public Guid? MasterPerformanceCycleId { get; set; }
        public string? MasterPerformanceCycleName { get; set; }
        public Guid? PerformanceTemplateId { get; set; }
        public string? PerformanceTemplateName { get; set; }
        public Guid? RatingScaleId { get; set; }
        public string? RatingScaleName { get; set; }
        public Guid? ReviewerUserId { get; set; }
        public string? ReviewerUserName { get; set; }
        public Guid? ManagerUserId { get; set; }
        public string? ManagerUserName { get; set; }
        public string ReviewNumber { get; set; } = string.Empty;
        public string ReviewType { get; set; } = string.Empty;
        public string ReviewPeriod { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public decimal OverallScore { get; set; }
        public decimal FinalScore { get; set; }
        public string? FinalRating { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public bool IsFinalized { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public bool IsActive { get; set; }
        public int DetailCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpPerformanceReviewDetailResponse : WfpPerformanceReviewResponse
    {
        public string? Strengths { get; set; }
        public string? ImprovementAreas { get; set; }
        public string? EmployeeComments { get; set; }
        public string? ReviewerComments { get; set; }
        public string? FinalComments { get; set; }
        public Guid? FinalizedByUserId { get; set; }
        public string? FinalizedByUserName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpPerformanceReviewFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public WfpPerformanceReviewDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpPerformanceStringOptionResponse> ReviewTypeOptions { get; set; } = new();
        public List<WfpPerformanceStringOptionResponse> ReviewStatusOptions { get; set; } = new();
        public List<WfpPerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpPerformanceReviewDefaultFilterResponse
    {
        public string? ReviewType { get; set; }
        public string? ReviewStatus { get; set; }
        public bool? IsAcknowledged { get; set; }
        public bool? IsFinalized { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "periodStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateWfpPerformanceReviewRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? PerformanceCycleId { get; set; }
        public Guid? MasterPerformanceCycleId { get; set; }
        public Guid? PerformanceTemplateId { get; set; }
        public Guid? RatingScaleId { get; set; }
        public Guid? ReviewerUserId { get; set; }
        public Guid? ManagerUserId { get; set; }
        [Required, MaxLength(40)]
        public string ReviewType { get; set; } = "Annual";
        [Required, MaxLength(100)]
        public string ReviewPeriod { get; set; } = string.Empty;
        [Required]
        public DateTime PeriodStartDate { get; set; }
        [Required]
        public DateTime PeriodEndDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        [Required, MaxLength(50)]
        public string ReviewStatus { get; set; } = "Draft";
        public decimal OverallScore { get; set; }
        public decimal FinalScore { get; set; }
        [MaxLength(100)]
        public string? FinalRating { get; set; }
        [MaxLength(3000)]
        public string? Strengths { get; set; }
        [MaxLength(3000)]
        public string? ImprovementAreas { get; set; }
        [MaxLength(3000)]
        public string? EmployeeComments { get; set; }
        [MaxLength(3000)]
        public string? ReviewerComments { get; set; }
        [MaxLength(3000)]
        public string? FinalComments { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpPerformanceReviewRequest : CreateWfpPerformanceReviewRequest
    {
    }

    public class UpdateWfpPerformanceReviewStatusRequest
    {
        [Required, MaxLength(50)]
        public string ReviewStatus { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }

    public class FinalizeWfpPerformanceReviewRequest
    {
        public decimal FinalScore { get; set; }
        [MaxLength(100)]
        public string? FinalRating { get; set; }
        [MaxLength(3000)]
        public string? FinalComments { get; set; }
    }

    public class AcknowledgeWfpPerformanceReviewRequest
    {
        [MaxLength(3000)]
        public string? EmployeeComments { get; set; }
    }

    public class WfpPerformanceStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpPerformanceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
