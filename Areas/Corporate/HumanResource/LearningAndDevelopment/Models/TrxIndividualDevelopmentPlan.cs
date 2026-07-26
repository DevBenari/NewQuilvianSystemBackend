using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("TrxIndividualDevelopmentPlan", Schema = "public")]
    public class TrxIndividualDevelopmentPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? ManagerUserId { get; set; }

        public Guid? PerformanceReviewId { get; set; }

        public Guid? PerformanceCycleId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(60)]
        public string PlanNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string PlanTitle { get; set; } = string.Empty;

        public DateTime PlanStartDate { get; set; }

        public DateTime PlanEndDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string PlanStatus { get; set; } = "Draft";

        [MaxLength(3000)]
        public string? CareerGoal { get; set; }

        [MaxLength(3000)]
        public string? Strengths { get; set; }

        [MaxLength(3000)]
        public string? DevelopmentGaps { get; set; }

        public string? DevelopmentActionsJson { get; set; }

        public decimal ProgressPercentage { get; set; } = 0m;

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNote { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public WfpPerformanceReview? PerformanceReview { get; set; }
        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
