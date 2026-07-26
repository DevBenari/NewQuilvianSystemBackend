using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("TrxEmployeeDocumentIssuance", Schema = "public")]
    public class TrxEmployeeDocumentIssuance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EmployeeDocumentRequestId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? WorkforceDocumentId { get; set; }
        public Guid IssuedByUserId { get; set; }
        public Guid? DigitallySignedByUserId { get; set; }
        public Guid? RevokedByUserId { get; set; }

        [Required]
        [MaxLength(80)]
        public string IssuanceNumber { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        [Required]
        [MaxLength(255)]
        public string DocumentFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string DocumentFilePath { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        [MaxLength(100)]
        public string? TemplateVersion { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsDigitallySigned { get; set; } = false;
        public DateTime? DigitallySignedAt { get; set; }
        public bool IsEmployeeDownloadAllowed { get; set; } = true;
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        [MaxLength(1000)]
        public string? RevocationReason { get; set; }

        public string? DocumentSnapshotJson { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxEmployeeDocumentRequest? EmployeeDocumentRequest { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpDocument? WorkforceDocument { get; set; }
        public ApplicationUser? IssuedByUser { get; set; }
        public ApplicationUser? DigitallySignedByUser { get; set; }
        public ApplicationUser? RevokedByUser { get; set; }
    }
}
