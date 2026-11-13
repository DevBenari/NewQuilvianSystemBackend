using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Evaluasi Kenaikan Jenjang Karier Klinis Nakes PK I-V Berdasarkan Kinerja & Logbook (HRD-DEC-168, PRF-B7, VR-PRF-14)
    /// </summary>
    [Table("HrdClinicalCareerLadderReview", Schema = "public")]
    public class HrdClinicalCareerLadderReview : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ReviewNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid PerformanceAppraisalId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CurrentClinicalLevel { get; set; } = "PK_I";

        [Required]
        [MaxLength(20)]
        public string TargetClinicalLevel { get; set; } = "PK_II";

        [Column(TypeName = "decimal(4,1)")]
        public decimal ClinicalExperienceYears { get; set; } = 0;

        public bool MeetsPerformanceThreshold { get; set; } = false;

        public bool MeetsClinicalCompetency { get; set; } = false;

        public bool FreeFromSentinelIncident { get; set; } = true;

        /// <summary>
        /// RECOMMENDED_PROMOTION, RECOMMENDED_RETENTION, CONDITIONAL_REASSESSMENT
        /// </summary>
        [MaxLength(30)]
        public string? CommitteeRecommendation { get; set; }

        public string? CommitteeNotes { get; set; }

        /// <summary>
        /// Submitted, AdministrativeVerified, CommitteeReviewed, ApprovedByDirector, Rejected
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string FinalStatus { get; set; } = "Submitted";

        public Guid? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(60)]
        public string? NewSkNumber { get; set; }

        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
        public virtual HrdPerformanceAppraisal? PerformanceAppraisal { get; set; }
        public virtual MstWorkforceProfile? ApprovedByProfile { get; set; }

        [NotMapped] public string CurrentLevel { get => CurrentClinicalLevel; set => CurrentClinicalLevel = value; }
        [NotMapped] public string RequestedLevel { get => TargetClinicalLevel; set => TargetClinicalLevel = value; }
        [NotMapped] public string Status { get => FinalStatus; set => FinalStatus = value; }
        [NotMapped] public DateTime SubmissionDate { get => CreateDateTime; set => CreateDateTime = value; }
        [NotMapped] public string PortfolioDocUrl { get; set; } = string.Empty;
        [NotMapped] public string? LogbookProceduresSummary { get; set; }
        [NotMapped] public Guid? CommitteeAssessorId { get => ApprovedBy; set => ApprovedBy = value; }
        [NotMapped] public string? KomiteRecommendation { get => CommitteeRecommendation; set => CommitteeRecommendation = value; }
        [NotMapped] public DateTime? ReviewDate { get => ApprovedAt; set => ApprovedAt = value; }
        [NotMapped] public string? DecreeNumber { get => NewSkNumber; set => NewSkNumber = value; }
        [NotMapped] public DateTime? DecreeDate { get; set; }

    }
}
