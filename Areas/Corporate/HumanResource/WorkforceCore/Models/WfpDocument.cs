using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpDocument", Schema = "public")]
    public class WfpDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(200)]
        public string? IssuingAuthority { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        [MaxLength(255)]
        public string? StoredFileName { get; set; }

        public long? FileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        public bool IsConfidential { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
