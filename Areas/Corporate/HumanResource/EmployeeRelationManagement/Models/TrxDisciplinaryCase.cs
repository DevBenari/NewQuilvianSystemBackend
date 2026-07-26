using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("TrxDisciplinaryCase", Schema = "public")]
    public class TrxDisciplinaryCase : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubjectWorkforceProfileId { get; set; }
        public Guid? SubjectEmployeeId { get; set; }
        public Guid? IncidentReportId { get; set; }
        public Guid? EmployeeGrievanceId { get; set; }
        public Guid? WorkplaceInvestigationId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? HrOwnerUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string CaseNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CaseType { get; set; } = string.Empty;

        public DateTime OpenedDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string CaseStatus { get; set; } = "Draft";

        [Required]
        [MaxLength(30)]
        public string SeverityLevel { get; set; } = "Medium";

        [MaxLength(2500)]
        public string? AllegationSummary { get; set; }
        [MaxLength(5000)]
        public string? FindingsRestricted { get; set; }
        [MaxLength(2000)]
        public string? ProposedAction { get; set; }

        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;

        public DateTime? SubmittedAt { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? SubjectWorkforceProfile { get; set; }
        public MstEmployee? SubjectEmployee { get; set; }
        public TrxEmployeeIncidentReport? IncidentReport { get; set; }
        public TrxEmployeeGrievance? EmployeeGrievance { get; set; }
        public TrxWorkplaceInvestigation? WorkplaceInvestigation { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? HrOwnerUser { get; set; }

        public ICollection<TrxDisciplinaryDecision> Decisions { get; set; } = new List<TrxDisciplinaryDecision>();
        public ICollection<WfpDisciplinaryAction> DisciplinaryActions { get; set; } = new List<WfpDisciplinaryAction>();
    }
}
