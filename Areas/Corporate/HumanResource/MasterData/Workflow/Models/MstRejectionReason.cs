using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models
{
    [Table("MstRejectionReason", Schema = "public")]
    public class MstRejectionReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowStepId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReasonCategory { get; set; }

        [Required]
        [MaxLength(50)]
        public string RejectAction { get; set; } = "ReturnToRequester";
        // ReturnToRequester, ReturnToPreviousStep, CancelRequest, CloseRequest.

        [MaxLength(50)]
        public string? ReturnToStepCode { get; set; }

        public bool IsCommentRequired { get; set; } = true;

        public bool IsAttachmentRequired { get; set; } = false;

        public bool AllowResubmit { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public MstWorkflowStep? WorkflowStep { get; set; }
    }
}
