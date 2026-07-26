using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxCredentialingVerification", Schema = "public")]
    public class TrxCredentialingVerification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CredentialingApplicationId { get; set; }

        public Guid? CredentialingDocumentId { get; set; }

        public Guid? CredentialingRequirementId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerificationType { get; set; } = "Document";

        [Required]
        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public Guid? VerifierUserId { get; set; }

        public DateTime? VerificationStartedAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(100)]
        public string? ExternalReferenceNumber { get; set; }

        [MaxLength(2000)]
        public string? Findings { get; set; }

        public string? VerificationResultJson { get; set; }

        public bool IsCompliant { get; set; } = false;

        public bool RequiresFollowUp { get; set; } = false;

        public DateTime? FollowUpDueDate { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public TrxCredentialingDocument? CredentialingDocument { get; set; }
        public MstCredentialingRequirement? CredentialingRequirement { get; set; }
        public ApplicationUser? VerifierUser { get; set; }

    }
}
