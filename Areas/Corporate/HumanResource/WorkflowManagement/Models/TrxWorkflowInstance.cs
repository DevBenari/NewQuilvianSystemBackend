using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    [Table("TrxWorkflowInstance", Schema = "public")]
    public class TrxWorkflowInstance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowDefinitionId { get; set; }
        public Guid? RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedByEmployeeId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ReferenceType { get; set; } = string.Empty;

        public Guid ReferenceId { get; set; }

        [Required]
        [MaxLength(60)]
        public string RequestNumber { get; set; } = string.Empty;

        public int CurrentStepOrder { get; set; } = 0;

        [MaxLength(50)]
        public string? CurrentStepCode { get; set; }

        [Required]
        [MaxLength(40)]
        public string WorkflowStatus { get; set; } = "Draft";

        [Required]
        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? LastActionAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }

        [MaxLength(100)]
        public string? RequestCorrelationId { get; set; }

        [MaxLength(100)]
        public string? ExternalReferenceNumber { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(1000)]
        public string? CompletionNote { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        public string? WorkflowDefinitionSnapshotJson { get; set; }
        public string? RequestContextJson { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public MstEmployee? RequestedByEmployee { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public ICollection<TrxWorkflowStepInstance> StepInstances { get; set; } = new List<TrxWorkflowStepInstance>();
        public ICollection<TrxApprovalAction> ApprovalActions { get; set; } = new List<TrxApprovalAction>();
        public ICollection<TrxWorkflowComment> Comments { get; set; } = new List<TrxWorkflowComment>();
        public ICollection<TrxWorkflowAttachment> Attachments { get; set; } = new List<TrxWorkflowAttachment>();
        public ICollection<TrxWorkflowStatusHistory> StatusHistories { get; set; } = new List<TrxWorkflowStatusHistory>();
    }
}
