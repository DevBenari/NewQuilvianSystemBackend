using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models
{
    [Table("MstRequestReason", Schema = "public")]
    public class MstRequestReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? WorkflowDefinitionId { get; set; }

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

        public bool IsCommentRequired { get; set; } = false;

        public bool IsAttachmentRequired { get; set; } = false;

        public bool IsEmployeeSelectable { get; set; } = true;

        public bool IsManagerSelectable { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
    }
}
