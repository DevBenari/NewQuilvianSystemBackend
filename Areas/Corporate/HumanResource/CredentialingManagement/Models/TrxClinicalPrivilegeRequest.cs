using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxClinicalPrivilegeRequest", Schema = "public")]
    public class TrxClinicalPrivilegeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? ExistingClinicalPrivilegeId { get; set; }

        public Guid ClinicalPrivilegeCatalogId { get; set; }

        public Guid? CredentialingApplicationId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PrivilegeRequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = "New";

        [Required]
        [MaxLength(30)]
        public string RequestStatus { get; set; } = "Draft";

        [MaxLength(2000)]
        public string? RequestedScope { get; set; }

        public DateTime? RequestedEffectiveStartDate { get; set; }

        public DateTime? RequestedEffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? RequestReason { get; set; }

        public bool RequiresSupervision { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;

        public bool BlocksSchedulingUntilApproved { get; set; } = true;

        public bool BlocksClinicalServiceUntilApproved { get; set; } = true;

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public string? SupportingEvidenceJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpClinicalPrivilege? ExistingClinicalPrivilege { get; set; }
        public MstClinicalPrivilegeCatalog? ClinicalPrivilegeCatalog { get; set; }
        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }

        public ICollection<TrxClinicalPrivilegeAssessment> Assessments { get; set; } = new List<TrxClinicalPrivilegeAssessment>();
        public ICollection<TrxClinicalPrivilegeApproval> Approvals { get; set; } = new List<TrxClinicalPrivilegeApproval>();
    }
}
