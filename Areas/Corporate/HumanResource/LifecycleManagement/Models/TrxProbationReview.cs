using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxProbationReview", Schema = "public")]
    public class TrxProbationReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string ReviewNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? EmployeeOnboardingId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ReviewerWorkforceProfileId { get; set; }
        public Guid? ReviewerUserId { get; set; }
        public Guid? PerformanceRatingScaleId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime ProbationStartDate { get; set; }
        public DateTime ProbationEndDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public decimal? PerformanceScore { get; set; }
        public decimal? CompetencyScore { get; set; }
        public decimal? AttendanceScore { get; set; }
        public decimal? OverallScore { get; set; }
        [MaxLength(30)] public string ReviewResult { get; set; } = "Pending";
        public DateTime? ExtendedProbationEndDate { get; set; }
        [MaxLength(2000)] public string? Strengths { get; set; }
        [MaxLength(2000)] public string? ImprovementAreas { get; set; }
        [MaxLength(2000)] public string? Recommendation { get; set; }
        [MaxLength(30)] public string ReviewStatus { get; set; } = "Draft";
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeOnboarding? EmployeeOnboarding { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkforceProfile? ReviewerWorkforceProfile { get; set; }
        public ApplicationUser? ReviewerUser { get; set; }
        public MstPerformanceRatingScale? PerformanceRatingScale { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
