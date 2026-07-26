using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxEmployeeSeparation", Schema = "public")]
    public class TrxEmployeeSeparation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string SeparationNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        [MaxLength(50)] public string SeparationType { get; set; } = "Resignation";
        public Guid? TerminationReasonId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? PreviousEmploymentStatusId { get; set; }
        public Guid? FinalEmploymentStatusId { get; set; }
        public Guid? FinalPayrollPeriodId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime EffectiveSeparationDate { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public int? NoticePeriodDays { get; set; }
        [MaxLength(30)] public string SeparationStatus { get; set; } = "Draft";
        public bool IsEligibleForRehire { get; set; } = true;
        public bool IsFinalPayrollCompleted { get; set; }
        public bool IsExitClearanceCompleted { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }
        [MaxLength(2000)] public string? ReasonText { get; set; }
        [MaxLength(2000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstTerminationReason? TerminationReason { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstEmploymentStatus? PreviousEmploymentStatus { get; set; }
        public MstEmploymentStatus? FinalEmploymentStatus { get; set; }
        public MstPayrollPeriod? FinalPayrollPeriod { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
