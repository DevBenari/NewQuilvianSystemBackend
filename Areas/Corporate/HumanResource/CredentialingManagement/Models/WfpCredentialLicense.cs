using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Enums.HumanResource;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("WfpCredentialLicense", Schema = "public")]
    public class WfpCredentialLicense : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? LicenseTypeId { get; set; }

        public Guid? CredentialingRequirementId { get; set; }

        [MaxLength(50)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(250)]
        public string? PracticeLocation { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpiredDate { get; set; }

        public CredentialVerificationStatus VerificationStatus { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        public Guid? RevokedByUserId { get; set; }

        [MaxLength(1000)]
        public string? RevocationReason { get; set; }

        public bool BlocksSchedulingWhenInvalid { get; set; } = true;

        public bool BlocksClinicalServiceWhenInvalid { get; set; } = true;

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLicenseType? LicenseTypeMaster { get; set; }
        public MstCredentialingRequirement? CredentialingRequirement { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? RevokedByUser { get; set; }

        public ICollection<WfpClinicalPrivilege> ClinicalPrivileges { get; set; } = new List<WfpClinicalPrivilege>();
        public ICollection<TrxLicenseRenewalRequest> RenewalRequests { get; set; } = new List<TrxLicenseRenewalRequest>();
    }
}
