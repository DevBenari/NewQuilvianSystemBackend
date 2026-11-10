using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdTalentPool", Schema = "public")]
    public class HrdTalentPool : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid OriginalVacancyId { get; set; }

        [MaxLength(500)]
        public string QualificationSummary { get; set; } = string.Empty;

        public decimal RankingScore { get; set; }

        public int RankingPosition { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;

        public DateTime RetentionExpiryDate { get; set; }

        public TalentPoolStatus Status { get; set; } = TalentPoolStatus.Active;

        public Guid? PromotedToOfferId { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdCandidate? Candidate { get; set; }
        public HrdJobVacancy? OriginalVacancy { get; set; }
    }
}
