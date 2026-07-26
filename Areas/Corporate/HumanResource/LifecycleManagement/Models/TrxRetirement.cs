using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxRetirement", Schema = "public")]
    public class TrxRetirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string RetirementNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        [MaxLength(50)] public string RetirementType { get; set; } = "Normal";
        public DateTime NormalRetirementDate { get; set; }
        public DateTime? ActualRetirementDate { get; set; }
        public int? RetirementAge { get; set; }
        [MaxLength(30)] public string RetirementStatus { get; set; } = "Planned";
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [MaxLength(1500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
