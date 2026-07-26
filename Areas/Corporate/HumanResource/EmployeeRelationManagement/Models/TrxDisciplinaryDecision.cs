using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("TrxDisciplinaryDecision", Schema = "public")]
    public class TrxDisciplinaryDecision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DisciplinaryCaseId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [Required]
        [MaxLength(60)]
        public string DecisionNumber { get; set; } = string.Empty;

        public DateTime DecisionDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string DecisionType { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? DecisionLevel { get; set; }

        [Required]
        [MaxLength(2500)]
        public string DecisionSummary { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? DecisionRationaleRestricted { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string DecisionStatus { get; set; } = "Draft";

        public bool IsAppealAllowed { get; set; } = true;
        public DateTime? AppealDeadline { get; set; }
        public bool IsFinalDecision { get; set; } = false;

        public Guid? IssuedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public TrxDisciplinaryCase? DisciplinaryCase { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public ApplicationUser? IssuedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<WfpDisciplinaryAction> DisciplinaryActions { get; set; } = new List<WfpDisciplinaryAction>();
    }
}
