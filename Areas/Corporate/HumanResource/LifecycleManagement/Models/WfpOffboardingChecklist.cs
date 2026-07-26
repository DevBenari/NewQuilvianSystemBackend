using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("WfpOffboardingChecklist", Schema = "public")]
    public class WfpOffboardingChecklist : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OffboardingTemplateId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        [Required, MaxLength(50)] public string ChecklistNumber { get; set; } = string.Empty;
        [MaxLength(50)] public string OffboardingType { get; set; } = "Resignation";
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
        public bool IsFinalPayrollCleared { get; set; }
        public bool IsAssetCleared { get; set; }
        public bool IsAccessRevoked { get; set; }
        public bool IsExitClearanceCompleted { get; set; }
        public Guid? CoordinatorUserId { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstOffboardingTemplate? OffboardingTemplate { get; set; }
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public ApplicationUser? CoordinatorUser { get; set; }
        public MstWorkforceProfile? ManagerWorkforceProfile { get; set; }
        public ICollection<WfpOffboardingTask> Tasks { get; set; } = new List<WfpOffboardingTask>();
    }
}
