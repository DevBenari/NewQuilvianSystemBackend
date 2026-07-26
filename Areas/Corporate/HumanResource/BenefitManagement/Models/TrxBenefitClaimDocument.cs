using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxBenefitClaimDocument", Schema = "public")]
    public class TrxBenefitClaimDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid BenefitClaimId { get; set; }

        public Guid? BenefitClaimItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

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

        public bool IsActive { get; set; } = true;

        public TrxBenefitClaim? BenefitClaim { get; set; }
        public TrxBenefitClaimItem? BenefitClaimItem { get; set; }
        public ApplicationUser? UploadedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

    }
}
