using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxBenefitClaimApproval", Schema = "public")]
    public class TrxBenefitClaimApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid BenefitClaimId { get; set; }

        public Guid? WorkflowStepId { get; set; }

        public Guid? RejectionReasonId { get; set; }

        public int StepOrder { get; set; } = 0;

        [MaxLength(100)]
        public string? ApprovalLevel { get; set; }

        [Required]
        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";

        public Guid? ApproverUserId { get; set; }

        public DateTime? ActionAt { get; set; }

        public decimal? ApprovedAmount { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBenefitClaim? BenefitClaim { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? ApproverUser { get; set; }

    }
}
