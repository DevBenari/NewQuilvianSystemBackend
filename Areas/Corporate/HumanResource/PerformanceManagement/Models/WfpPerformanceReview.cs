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
    [Table("WfpPerformanceReview", Schema = "public")]
    public class WfpPerformanceReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? PerformanceCycleId { get; set; }

        public Guid? MasterPerformanceCycleId { get; set; }

        public Guid? PerformanceTemplateId { get; set; }

        public Guid? RatingScaleId { get; set; }

        public Guid? ReviewerUserId { get; set; }

        public Guid? ManagerUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string ReviewNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string ReviewType { get; set; } = "Annual";

        [Required]
        [MaxLength(100)]
        public string ReviewPeriod { get; set; } = string.Empty;

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        public DateTime? ReviewDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReviewStatus { get; set; } = "Draft";

        public decimal OverallScore { get; set; } = 0m;

        public decimal FinalScore { get; set; } = 0m;

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

        public bool IsAcknowledged { get; set; } = false;

        public DateTime? AcknowledgedAt { get; set; }

        public bool IsFinalized { get; set; } = false;

        public DateTime? FinalizedAt { get; set; }

        public Guid? FinalizedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstPerformanceCycle? MasterPerformanceCycle { get; set; }
        public MstPerformanceTemplate? PerformanceTemplate { get; set; }
        public MstPerformanceRatingScale? RatingScale { get; set; }
        public ApplicationUser? ReviewerUser { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public ApplicationUser? FinalizedByUser { get; set; }

        public ICollection<WfpPerformanceReviewDetail> Details { get; set; } = new List<WfpPerformanceReviewDetail>();
    }
}
