using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxBackgroundCheck", Schema = "public")]
    public class TrxBackgroundCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CheckType { get; set; } = "Identity";
        // Identity, Education, Employment, Criminal, Financial, Credential, SocialMedia, Other.

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(200)]
        public string? ExternalReferenceNumber { get; set; }

        public bool HasCandidateConsent { get; set; } = false;
        public DateTime? ConsentAt { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(30)]
        public string CheckStatus { get; set; } = "Pending";

        [MaxLength(30)]
        public string? CheckResult { get; set; }
        // Clear, ReviewRequired, Failed, UnableToVerify.

        [MaxLength(20)]
        public string RiskLevel { get; set; } = "Low";

        [MaxLength(500)]
        public string? ReportFilePath { get; set; }

        [MaxLength(2000)]
        public string? Findings { get; set; }

        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
    }
}
