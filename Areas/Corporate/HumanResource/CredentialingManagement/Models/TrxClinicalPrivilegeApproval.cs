using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxClinicalPrivilegeApproval", Schema = "public")]
    public class TrxClinicalPrivilegeApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClinicalPrivilegeRequestId { get; set; }

        public Guid? WorkflowStepId { get; set; }

        public Guid? RejectionReasonId { get; set; }

        public int StepOrder { get; set; } = 0;

        [Required]
        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";

        public Guid? ApproverUserId { get; set; }

        public DateTime? ActionAt { get; set; }

        [MaxLength(2000)]
        public string? ApprovedScope { get; set; }

        public DateTime? ApprovedEffectiveStartDate { get; set; }

        public DateTime? ApprovedEffectiveEndDate { get; set; }

        public Guid? CreatedClinicalPrivilegeId { get; set; }

        [MaxLength(2000)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxClinicalPrivilegeRequest? ClinicalPrivilegeRequest { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? ApproverUser { get; set; }
        public WfpClinicalPrivilege? CreatedClinicalPrivilege { get; set; }

    }
}
