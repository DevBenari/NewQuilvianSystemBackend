using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxTermination", Schema = "public")]
    public class TrxTermination : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string TerminationNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public Guid? TerminationReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? IncidentDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public int? NoticePeriodDays { get; set; }
        public decimal? SeveranceAmount { get; set; }
        public decimal? FinalPayAmount { get; set; }
        public bool RequiresLegalReview { get; set; }
        public bool IsLegalReviewCompleted { get; set; }
        [Required, MaxLength(2500)] public string TerminationReasonText { get; set; } = string.Empty;
        [MaxLength(2500)] public string? InvestigationSummary { get; set; }
        [MaxLength(30)] public string TerminationStatus { get; set; } = "Draft";
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [MaxLength(1500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstTerminationReason? TerminationReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
