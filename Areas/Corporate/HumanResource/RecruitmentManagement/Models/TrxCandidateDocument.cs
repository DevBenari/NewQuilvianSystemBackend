using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidateDocument", Schema = "public")]
    public class TrxCandidateDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateId { get; set; }
        public Guid? CandidateApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        [MaxLength(150)]
        public string? MimeType { get; set; }

        public long? FileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsConfidential { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public TrxCandidate? Candidate { get; set; }
        public TrxCandidateApplication? CandidateApplication { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
