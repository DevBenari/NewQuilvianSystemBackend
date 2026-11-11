using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Rapor Evaluasi Kinerja Komposit 60% KPI + 40% Perilaku Core Values BerAKHLAK (HRD-DEC-164..166, PRF-B1..B4)
    /// </summary>
    [Table("trx_performance_appraisals", Schema = "public")]
    public class TrxPerformanceAppraisal : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PerformanceCycleId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid FirstLineManagerId { get; set; }

        public Guid? SecondLineManagerId { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? KpiScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? BehaviorScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PreCalibrationScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalCompositeScore { get; set; }

        /// <summary>
        /// SANGAT_BAIK_A, BAIK_B, CUKUP_C, KURANG_D, BURUK_E
        /// </summary>
        [MaxLength(20)]
        public string? PerformanceBand { get; set; }

        /// <summary>
        /// Draft, SelfSubmitted, ManagerAssessed, SecondLineReviewed, InCalibration, FinalApproved, Disputed
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft";

        public string? CalibrationNotes { get; set; }

        public string? ManagerFeedbackNotes { get; set; }

        public DateTime? EmployeeAcknowledgedAt { get; set; }

        public virtual TrxPerformanceCycle? PerformanceCycle { get; set; }
        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
        public virtual MstWorkforceProfile? FirstLineManager { get; set; }
        public virtual MstWorkforceProfile? SecondLineManager { get; set; }
        public virtual ICollection<TrxPerformanceAppraisalDetail> Details { get; set; } = new List<TrxPerformanceAppraisalDetail>();

        [NotMapped] public decimal SelfScoreQuantitative { get; set; }
        [NotMapped] public decimal SelfScoreBehavioral { get; set; }
        [NotMapped] public decimal SelfOverallScore { get; set; }
        [NotMapped] public decimal ManagerScoreQuantitative { get => KpiScore ?? 0m; set => KpiScore = value; }
        [NotMapped] public decimal ManagerScoreBehavioral { get => BehaviorScore ?? 0m; set => BehaviorScore = value; }
        [NotMapped] public decimal ManagerOverallScore { get => PreCalibrationScore ?? 0m; set => PreCalibrationScore = value; }
        [NotMapped] public decimal CalibratedScore { get => FinalCompositeScore ?? 0m; set => FinalCompositeScore = value; }
        [NotMapped] public decimal FinalScore { get => FinalCompositeScore ?? 0m; set => FinalCompositeScore = value; }
        [NotMapped] public string FinalGrade { get => PerformanceBand ?? string.Empty; set => PerformanceBand = value; }
        [NotMapped] public bool HasDispute { get; set; } = false;
        [NotMapped] public string? SelfNotes { get; set; }
        [NotMapped] public DateTime? SelfAssessmentDate { get; set; }
        [NotMapped] public Guid? ManagerId { get => FirstLineManagerId; set => FirstLineManagerId = value ?? Guid.Empty; }
        [NotMapped] public DateTime? ManagerAssessmentDate { get; set; }
        [NotMapped] public Guid? SecondLineReviewerId { get => SecondLineManagerId; set => SecondLineManagerId = value; }
        [NotMapped] public DateTime? SecondLineReviewDate { get; set; }
        [NotMapped] public string? SecondLineReviewNotes { get; set; }
        [NotMapped] public bool IsAcknowledgedByEmployee { get => EmployeeAcknowledgedAt.HasValue; set { if (value) EmployeeAcknowledgedAt = DateTime.UtcNow; } }
        [NotMapped] public DateTime? EmployeeAcknowledgmentDate { get => EmployeeAcknowledgedAt; set => EmployeeAcknowledgedAt = value; }
        [NotMapped] public string? EmployeeAcknowledgmentNotes { get; set; }
        [NotMapped] public DateTime? PublishedDate { get; set; }

    }
}
