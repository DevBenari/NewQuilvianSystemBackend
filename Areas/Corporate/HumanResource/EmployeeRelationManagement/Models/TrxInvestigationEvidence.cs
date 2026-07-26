using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("TrxInvestigationEvidence", Schema = "public")]
    public class TrxInvestigationEvidence : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkplaceInvestigationId { get; set; }

        [Required]
        [MaxLength(60)]
        public string EvidenceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string EvidenceType { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? EvidenceSource { get; set; }
        [MaxLength(3000)]
        public string? EvidenceDescriptionRestricted { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }
        [MaxLength(255)]
        public string? FileName { get; set; }
        [MaxLength(100)]
        public string? FileContentType { get; set; }
        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        public Guid? CollectedByUserId { get; set; }
        public DateTime? CollectedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public string? ChainOfCustodyJson { get; set; }

        public bool IsConfidential { get; set; } = true;
        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";
        public bool RequiresEnhancedAudit { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public TrxWorkplaceInvestigation? WorkplaceInvestigation { get; set; }
        public ApplicationUser? CollectedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
