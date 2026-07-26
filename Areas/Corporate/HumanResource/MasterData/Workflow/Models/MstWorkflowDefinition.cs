using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models
{
    [Table("MstWorkflowDefinition", Schema = "public")]
    public class MstWorkflowDefinition : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkflowCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string WorkflowName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;
        // LeaveRequest, OvertimeRequest, TravelRequest, ExpenseReimbursement,
        // ScheduleChange, ShiftSwap, PayrollAdjustment, PerformanceReview,
        // Credentialing, TrainingRequest, Other.

        [Required]
        [MaxLength(100)]
        public string WorkflowCategory { get; set; } = "HumanResource";

        public int Version { get; set; } = 1;

        [Required]
        [MaxLength(50)]
        public string WorkflowStatus { get; set; } = "Draft";
        // Draft, Active, Inactive, Retired.

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool AllowRequesterCancel { get; set; } = true;

        public bool AllowRequesterWithdraw { get; set; } = true;

        public bool AllowParallelApproval { get; set; } = false;

        public bool AllowStepSkip { get; set; } = false;

        public bool StopOnRejection { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public ICollection<MstWorkflowStep> WorkflowSteps { get; set; }
            = new List<MstWorkflowStep>();

        public ICollection<MstApprovalMatrix> ApprovalMatrices { get; set; }
            = new List<MstApprovalMatrix>();

        public ICollection<MstApprovalDelegationPolicy> DelegationPolicies { get; set; }
            = new List<MstApprovalDelegationPolicy>();

        public ICollection<MstRequestReason> RequestReasons { get; set; }
            = new List<MstRequestReason>();

        public ICollection<MstRejectionReason> RejectionReasons { get; set; }
            = new List<MstRejectionReason>();
    }
}
