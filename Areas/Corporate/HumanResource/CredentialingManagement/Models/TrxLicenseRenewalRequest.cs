using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxLicenseRenewalRequest", Schema = "public")]
    public class TrxLicenseRenewalRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid CredentialLicenseId { get; set; }

        public Guid? CredentialingApplicationId { get; set; }

        public Guid? LicenseTypeId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RenewalRequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string RenewalStatus { get; set; } = "Draft";

        [MaxLength(100)]
        public string? OldLicenseNumber { get; set; }

        public DateTime? OldExpiryDate { get; set; }

        [MaxLength(100)]
        public string? NewLicenseNumber { get; set; }

        public DateTime? NewIssueDate { get; set; }

        public DateTime? NewExpiryDate { get; set; }

        [MaxLength(1000)]
        public string? RenewalReason { get; set; }

        public string? SupportingDocumentJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpCredentialLicense? CredentialLicense { get; set; }
        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public MstLicenseType? LicenseType { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

    }
}
