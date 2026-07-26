using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxCredentialingDocument", Schema = "public")]
    public class TrxCredentialingDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CredentialingApplicationId { get; set; }

        public Guid? CredentialingRequirementId { get; set; }

        public Guid? CertificationId { get; set; }

        public Guid? CredentialLicenseId { get; set; }

        public Guid? ClinicalPrivilegeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        public long FileSizeBytes { get; set; } = 0;

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        [Required]
        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public DateTime UploadedAt { get; set; }

        public Guid? UploadedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsMandatory { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public MstCredentialingRequirement? CredentialingRequirement { get; set; }
        public WfpCertification? Certification { get; set; }
        public WfpCredentialLicense? CredentialLicense { get; set; }
        public WfpClinicalPrivilege? ClinicalPrivilege { get; set; }
        public ApplicationUser? UploadedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

    }
}
