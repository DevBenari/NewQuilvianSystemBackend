using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxEmployeeOnboarding", Schema = "public")]
    public class TrxEmployeeOnboarding : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string OnboardingNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? CandidateHiringId { get; set; }
        public Guid? OnboardingTemplateId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        public Guid? CoordinatorUserId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        [MaxLength(30)] public string OnboardingStatus { get; set; } = "Draft";
        public decimal ProgressPercentage { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }
        [MaxLength(1500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxCandidateHiring? CandidateHiring { get; set; }
        public MstOnboardingTemplate? OnboardingTemplate { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstWorkforceProfile? ManagerWorkforceProfile { get; set; }
        public ApplicationUser? CoordinatorUser { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
        public ICollection<TrxEmployeeOnboardingTask> Tasks { get; set; } = new List<TrxEmployeeOnboardingTask>();
    }
}
