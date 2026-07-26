using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxReferenceCheck", Schema = "public")]
    public class TrxReferenceCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateApplicationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ReferenceName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ReferenceCompany { get; set; }

        [MaxLength(150)]
        public string? ReferencePosition { get; set; }

        [MaxLength(100)]
        public string? RelationshipToCandidate { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string CheckStatus { get; set; } = "Pending";

        public DateTime? ContactedAt { get; set; }
        public Guid? CheckedByUserId { get; set; }

        [MaxLength(30)]
        public string? CheckResult { get; set; }
        // Positive, Neutral, Negative, UnableToVerify.

        [MaxLength(2000)]
        public string? Feedback { get; set; }

        public bool IsEmploymentVerified { get; set; } = false;
        public bool IsPerformanceVerified { get; set; } = false;
        public bool IsRehireEligible { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public ApplicationUser? CheckedByUser { get; set; }
    }
}
