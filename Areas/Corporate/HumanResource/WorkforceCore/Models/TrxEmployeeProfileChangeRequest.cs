using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("TrxEmployeeProfileChangeRequest", Schema = "public")]
    public class TrxEmployeeProfileChangeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? RequestReasonId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RequestCategory { get; set; } = "Profile";

        [Required]
        [MaxLength(50)]
        public string RequestStatus { get; set; } = "Draft";
        // Draft, Submitted, UnderVerification, NeedRevision, Approved, Rejected, Cancelled, Applied.

        [MaxLength(500)]
        public string? RequestReasonText { get; set; }

        public Guid RequestedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public DateTime? AppliedAt { get; set; }
        public Guid? AppliedByUserId { get; set; }
        public int CurrentStepOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public ICollection<TrxEmployeeProfileChangeDetail> Details { get; set; } = new List<TrxEmployeeProfileChangeDetail>();
        public ICollection<TrxEmployeeProfileChangeVerification> Verifications { get; set; } = new List<TrxEmployeeProfileChangeVerification>();
    }
}
