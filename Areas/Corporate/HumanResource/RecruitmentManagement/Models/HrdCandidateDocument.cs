using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdCandidateDocument", Schema = "public")]
    public class HrdCandidateDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateId { get; set; }

        public Guid? ApplicationId { get; set; }

        public DocumentType DocumentType { get; set; }

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string DocumentTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string MimeType { get; set; } = string.Empty;

        public DateOnly? ValidFrom { get; set; }

        public DateOnly? ValidUntil { get; set; }

        public bool IsVerified { get; set; }

        [MaxLength(100)]
        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public HrdCandidate? Candidate { get; set; }
        public HrdCandidateApplication? Application { get; set; }
    }
}
