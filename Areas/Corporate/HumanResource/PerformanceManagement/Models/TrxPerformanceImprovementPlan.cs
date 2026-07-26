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
    [Table("TrxPerformanceImprovementPlan", Schema = "public")]
    public class TrxPerformanceImprovementPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid ManagerUserId { get; set; }

        public Guid? PerformanceReviewId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(60)]
        public string PipNumber { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PipStatus { get; set; } = "Draft";

        [Required]
        [MaxLength(3000)]
        public string Reason { get; set; } = string.Empty;

        public string? ObjectivesJson { get; set; }

        public string? ActionPlanJson { get; set; }

        public string? SuccessMetricsJson { get; set; }

        public int CheckInFrequencyDays { get; set; } = 30;

        public decimal ProgressPercentage { get; set; } = 0m;

        [MaxLength(100)]
        public string? Outcome { get; set; }

        [MaxLength(3000)]
        public string? OutcomeNote { get; set; }

        public bool IsEmployeeAcknowledged { get; set; } = false;

        public DateTime? EmployeeAcknowledgedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public WfpPerformanceReview? PerformanceReview { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
