using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxWorkflowComment", Schema = "public")]
    public class TrxWorkflowComment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkflowInstanceId { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public Guid? CommentByUserId { get; set; }

        public Guid? CommentByWorkforceProfileId { get; set; }

        public Guid? ParentCommentId { get; set; }

        [Required]
        [MaxLength(40)]
        public string CommentType { get; set; }
            = WorkflowValueConstants.CommentType.General;

        [Required]
        [MaxLength(5000)]
        public string CommentText { get; set; } = string.Empty;

        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;

        public bool IsRequesterVisible { get; set; } = true;

        public bool IsInternalComment { get; set; } = false;

        public bool IsSystemGenerated { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public TrxWorkflowInstance? WorkflowInstance { get; set; }

        public TrxWorkflowStepInstance? WorkflowStepInstance { get; set; }

        public ApplicationUser? CommentByUser { get; set; }

        public MstWorkforceProfile? CommentByWorkforceProfile { get; set; }

        public TrxWorkflowComment? ParentComment { get; set; }

        public ICollection<TrxWorkflowComment> Replies { get; set; }
            = new List<TrxWorkflowComment>();

        public ICollection<TrxWorkflowAttachment> Attachments { get; set; }
            = new List<TrxWorkflowAttachment>();
    }
}
