using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models
{
    [Table("MstApprovalDelegationPolicy", Schema = "public")]
    public class MstApprovalDelegationPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowStepId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DelegationPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DelegationPolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DelegationType { get; set; } = "Temporary";
        // Temporary, Permanent, AutomaticOutOfOffice.

        public int MaximumDelegationDays { get; set; } = 30;

        public int MinimumNoticeHours { get; set; } = 0;

        public bool RequireManagerApproval { get; set; } = false;

        public bool RequireHrVerification { get; set; } = false;

        public bool AllowCrossOrganizationUnit { get; set; } = false;

        public bool AllowCrossHospitalSite { get; set; } = false;

        public bool AllowCrossLegalEntity { get; set; } = false;

        public bool AllowSubDelegation { get; set; } = false;

        public bool AllowSelfDelegation { get; set; } = false;

        public bool PreserveDelegatorAccountability { get; set; } = true;

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public MstWorkflowStep? WorkflowStep { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }
    }
}
