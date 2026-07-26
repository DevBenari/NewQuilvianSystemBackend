using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("WfpOnboardingChecklist", Schema = "public")]
    public class WfpOnboardingChecklist : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OnboardingTemplateId { get; set; }
        public Guid? CandidateHiringId { get; set; }
        [Required, MaxLength(50)] public string ChecklistNumber { get; set; } = string.Empty;
        [MaxLength(50)] public string OnboardingType { get; set; } = "NewHire";
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        [MaxLength(30)] public string ChecklistStatus { get; set; } = "Draft";
        public int TotalTask { get; set; }
        public int RequiredTask { get; set; }
        public int CompletedTask { get; set; }
        public int CompletedRequiredTask { get; set; }
        public decimal ProgressPercentage { get; set; }
        public Guid? CoordinatorUserId { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstOnboardingTemplate? OnboardingTemplate { get; set; }
        public TrxCandidateHiring? CandidateHiring { get; set; }
        public ApplicationUser? CoordinatorUser { get; set; }
        public MstWorkforceProfile? ManagerWorkforceProfile { get; set; }
        public ICollection<WfpOnboardingTask> Tasks { get; set; } = new List<WfpOnboardingTask>();
    }
}
