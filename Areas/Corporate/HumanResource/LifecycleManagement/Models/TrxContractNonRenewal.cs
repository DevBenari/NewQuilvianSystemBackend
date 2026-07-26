using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxContractNonRenewal", Schema = "public")]
    public class TrxContractNonRenewal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string NonRenewalNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ContractHistoryId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime ContractEndDate { get; set; }
        public DateTime DecisionDate { get; set; }
        public DateTime? NotificationDate { get; set; }
        [MaxLength(30)] public string NonRenewalStatus { get; set; } = "Draft";
        [MaxLength(2000)] public string? ReasonText { get; set; }
        public Guid? InitiatedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? EmployeeAcknowledgedAt { get; set; }
        [MaxLength(1500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpContractHistory? ContractHistory { get; set; }
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstContractType? ContractType { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? InitiatedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
