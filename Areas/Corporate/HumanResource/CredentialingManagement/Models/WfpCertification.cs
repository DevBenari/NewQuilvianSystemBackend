using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("WfpCertification", Schema = "public")]
    public class WfpCertification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? CertificationTypeId { get; set; }

        public Guid? CredentialingRequirementId { get; set; }

        [MaxLength(50)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string CertificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CertificationName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; } = false;

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; } = false;

        [Required]
        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public bool BlocksSchedulingWhenInvalid { get; set; } = true;

        public bool BlocksClinicalServiceWhenInvalid { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCertificationType? CertificationTypeMaster { get; set; }
        public MstCredentialingRequirement? CredentialingRequirement { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

        public ICollection<TrxCertificationRenewalRequest> RenewalRequests { get; set; } = new List<TrxCertificationRenewalRequest>();
    }
}
