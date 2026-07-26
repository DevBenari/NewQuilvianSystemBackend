using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("TrxEmployeeGrievance", Schema = "public")]
    public class TrxEmployeeGrievance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ComplainantWorkforceProfileId { get; set; }
        public Guid? ComplainantEmployeeId { get; set; }
        public Guid? ComplainantUserId { get; set; }
        public Guid? AgainstWorkforceProfileId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? AssignedHrUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string GrievanceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GrievanceType { get; set; } = string.Empty;

        public DateTime GrievanceDate { get; set; }

        [Required]
        [MaxLength(250)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string GrievanceSummary { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? GrievanceDetailsRestricted { get; set; }
        [MaxLength(2000)]
        public string? RequestedResolution { get; set; }

        [Required]
        [MaxLength(40)]
        public string GrievanceStatus { get; set; } = "Draft";

        public bool IsIdentityProtected { get; set; } = true;
        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;
        public bool CanComplainantViewStatus { get; set; } = true;

        public string? AttachmentMetadataJson { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? ComplainantWorkforceProfile { get; set; }
        public MstEmployee? ComplainantEmployee { get; set; }
        public ApplicationUser? ComplainantUser { get; set; }
        public MstWorkforceProfile? AgainstWorkforceProfile { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? AssignedHrUser { get; set; }

        public ICollection<TrxWorkplaceInvestigation> Investigations { get; set; } = new List<TrxWorkplaceInvestigation>();
        public ICollection<TrxDisciplinaryCase> DisciplinaryCases { get; set; } = new List<TrxDisciplinaryCase>();
    }
}
