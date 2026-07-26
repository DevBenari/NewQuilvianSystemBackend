using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("WfpDisciplinaryAction", Schema = "public")]
    public class WfpDisciplinaryAction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? DisciplinaryCaseId { get; set; }
        public Guid? DisciplinaryDecisionId { get; set; }
        public Guid? IncidentReportId { get; set; }

        [Required]
        [MaxLength(60)]
        public string ActionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string ActionType { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? ActionLevel { get; set; }

        public DateTime ActionDate { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(250)]
        public string Subject { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Reason { get; set; }
        [MaxLength(2000)]
        public string? DecisionSummary { get; set; }
        [MaxLength(4000)]
        public string? ConfidentialNotes { get; set; }

        [Required]
        [MaxLength(40)]
        public string ActionStatus { get; set; } = "Draft";

        public bool IsAcknowledged { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
        public bool IsAppealed { get; set; } = false;
        [MaxLength(40)]
        public string? AppealStatus { get; set; }

        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;

        public Guid? IssuedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public TrxDisciplinaryCase? DisciplinaryCase { get; set; }
        public TrxDisciplinaryDecision? DisciplinaryDecision { get; set; }
        public TrxEmployeeIncidentReport? IncidentReport { get; set; }
        public ApplicationUser? IssuedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
