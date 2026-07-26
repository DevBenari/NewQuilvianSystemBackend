using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidateScreening", Schema = "public")]
    public class TrxCandidateScreening : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateApplicationId { get; set; }
        public Guid? RecruitmentStageId { get; set; }

        [Required]
        [MaxLength(30)]
        public string ScreeningType { get; set; } = "CvScreening";
        // CvScreening, Eligibility, Document, PhoneScreening, CredentialPrecheck, Other.

        [MaxLength(30)]
        public string ScreeningStatus { get; set; } = "Pending";

        public decimal? Score { get; set; }

        [MaxLength(30)]
        public string? ScreeningResult { get; set; }
        // Pass, Fail, Hold, NeedReview.

        public bool IsShortlisted { get; set; } = false;
        public Guid? ScreenedByUserId { get; set; }
        public DateTime? ScreenedAt { get; set; }

        [MaxLength(1500)]
        public string? Notes { get; set; }

        public string? ScreeningDataJson { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public MstRecruitmentStage? RecruitmentStage { get; set; }
        public ApplicationUser? ScreenedByUser { get; set; }
    }
}
