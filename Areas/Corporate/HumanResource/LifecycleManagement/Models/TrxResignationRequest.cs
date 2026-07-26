using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxResignationRequest", Schema = "public")]
    public class TrxResignationRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string RequestNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime ProposedLastWorkingDate { get; set; }
        public int NoticePeriodDays { get; set; }
        [Required, MaxLength(2000)] public string ResignationReason { get; set; } = string.Empty;
        [MaxLength(2000)] public string? HandoverPlan { get; set; }
        [MaxLength(2000)] public string? ManagerComment { get; set; }
        [MaxLength(30)] public string RequestStatus { get; set; } = "Draft";
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        [MaxLength(500)] public string? WithdrawalReason { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
    }
}
