using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Sengketa Sanggah Banding Nilai Kinerja SLA 7/14 Hari (HRD-DEC-169, PRF-B8, VR-PRF-12..13)
    /// </summary>
    [Table("HrdPerformanceAppraisalDispute", Schema = "public")]
    public class HrdPerformanceAppraisalDispute : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string DisputeNumber { get; set; } = string.Empty;

        [Required]
        public Guid PerformanceAppraisalId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public DateTime DisputeDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// QUANTITATIVE_KPI, CORE_VALUES, BOTH
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string DisputedComponent { get; set; } = "QUANTITATIVE_KPI";

        [Required]
        public string GrievanceReason { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string EvidenceDocumentUrl { get; set; } = string.Empty;

        public string? MediationNotes { get; set; }

        /// <summary>
        /// SCORE_UPHELD_ORIGINAL, SCORE_REVISED_ADJUSTED, REASSIGN_EVALUATOR
        /// </summary>
        [MaxLength(30)]
        public string? FinalVerdict { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AdjustedCompositeScore { get; set; }

        public DateTime? VerdictDeliveredAt { get; set; }

        public Guid? DecidedBy { get; set; }

        /// <summary>
        /// Submitted, MediationPhase, PlenaryReview, Resolved, Rejected
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Submitted";

        public virtual HrdPerformanceAppraisal? PerformanceAppraisal { get; set; }
        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
        public virtual MstWorkforceProfile? DecidedByProfile { get; set; }

        [NotMapped] public DateTime SubmissionDate { get => DisputeDate; set => DisputeDate = value; }
        [NotMapped] public string DisputeReason { get => GrievanceReason; set => GrievanceReason = value; }
        [NotMapped] public string? CounterEvidenceUrl { get => EvidenceDocumentUrl; set => EvidenceDocumentUrl = value ?? string.Empty; }
        [NotMapped] public string? InvestigationNotes { get => MediationNotes; set => MediationNotes = value; }
        [NotMapped] public decimal? FinalResolutionScore { get => AdjustedCompositeScore; set => AdjustedCompositeScore = value; }
        [NotMapped] public Guid? AdjudicatedById { get => DecidedBy; set => DecidedBy = value; }
        [NotMapped] public DateTime? AdjudicatedDateTime { get => VerdictDeliveredAt; set => VerdictDeliveredAt = value; }

    }
}
