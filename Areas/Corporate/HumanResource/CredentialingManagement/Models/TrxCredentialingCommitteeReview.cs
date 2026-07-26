using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxCredentialingCommitteeReview", Schema = "public")]
    public class TrxCredentialingCommitteeReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CredentialingApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReviewNumber { get; set; } = string.Empty;

        public DateTime MeetingDate { get; set; }

        public Guid? ReviewerUserId { get; set; }

        [MaxLength(100)]
        public string? CommitteeMemberRole { get; set; }

        [Required]
        [MaxLength(50)]
        public string Recommendation { get; set; } = "Pending";

        public decimal? AssessmentScore { get; set; }

        public bool IsQuorumMet { get; set; } = false;

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public string? ReviewEvidenceJson { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public ApplicationUser? ReviewerUser { get; set; }

    }
}
