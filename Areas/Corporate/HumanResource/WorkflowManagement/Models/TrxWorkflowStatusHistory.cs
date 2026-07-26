using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxWorkflowStatusHistory", Schema = "public")]
    public class TrxWorkflowStatusHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public Guid? WorkflowStepInstanceId { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public Guid? ChangedByWorkforceProfileId { get; set; }
        public int SequenceNumber { get; set; }

        [MaxLength(40)]
        public string? FromWorkflowStatus { get; set; }

        [Required]
        [MaxLength(40)]
        public string ToWorkflowStatus { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? FromStepStatus { get; set; }

        [MaxLength(40)]
        public string? ToStepStatus { get; set; }

        [Required]
        [MaxLength(40)]
        public string ActionType { get; set; } = "Submit";

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public bool IsSystemGenerated { get; set; } = false;
        public string? StatusSnapshotJson { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxWorkflowInstance? WorkflowInstance { get; set; }
        public TrxWorkflowStepInstance? WorkflowStepInstance { get; set; }
        public ApplicationUser? ChangedByUser { get; set; }
        public MstWorkforceProfile? ChangedByWorkforceProfile { get; set; }
    }
}
