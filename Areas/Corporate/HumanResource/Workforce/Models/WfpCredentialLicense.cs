using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpCredentialLicense", Schema = "public")]
    public class WfpCredentialLicense : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;
        // STR, SIP, SIK, SIPP, SIPA, SIPB

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime ExpiredDate { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public CredentialVerificationStatus VerificationStatus { get; set; }
            = CredentialVerificationStatus.Unverified;

        public bool IsVerified { get; set; } = false;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(250)]
        public string? VerificationNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public Guid? RevokedByUserId { get; set; }

        public DateTime? RevokedAt { get; set; }

        [MaxLength(250)]
        public string? RevokedReason { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow.Date > ExpiredDate.Date;

        [NotMapped]
        public bool IsCurrentlyValid =>
            IsActive &&
            !IsDelete &&
            IsVerified &&
            VerificationStatus == CredentialVerificationStatus.Verified &&
            DateTime.UtcNow.Date <= ExpiredDate.Date;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }

        public ApplicationUser? RevokedByUser { get; set; }
    }
}