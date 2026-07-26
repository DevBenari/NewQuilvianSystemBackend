using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxCertificationRenewalRequest", Schema = "public")]
    public class TrxCertificationRenewalRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid CertificationId { get; set; }

        public Guid? CredentialingApplicationId { get; set; }

        public Guid? CertificationTypeId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RenewalRequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string RenewalStatus { get; set; } = "Draft";

        [MaxLength(100)]
        public string? OldCertificateNumber { get; set; }

        public DateTime? OldExpiryDate { get; set; }

        [MaxLength(100)]
        public string? NewCertificateNumber { get; set; }

        public DateTime? NewIssueDate { get; set; }

        public DateTime? NewExpiryDate { get; set; }

        public bool IsLifetimeRenewal { get; set; } = false;

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
        public WfpCertification? Certification { get; set; }
        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public MstCertificationType? CertificationType { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

    }
}
