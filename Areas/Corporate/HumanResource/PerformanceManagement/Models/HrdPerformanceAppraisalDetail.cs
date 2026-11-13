using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Detail Butir Penilaian Metrik KPI & Rubrik Perilaku Core Values BerAKHLAK (HRD-DEC-164, VR-PRF-04)
    /// </summary>
    [Table("HrdPerformanceAppraisalDetail", Schema = "public")]
    public class HrdPerformanceAppraisalDetail : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PerformanceAppraisalId { get; set; }

        /// <summary>
        /// QUANTITATIVE_KPI, CORE_VALUES_BEHAVIOR
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ComponentType { get; set; } = "QUANTITATIVE_KPI";

        [Required]
        [MaxLength(50)]
        public string MetricOrCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemTitle { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal WeightPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SelfScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ManagerScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalScore { get; set; }

        [MaxLength(250)]
        public string? EvidenceDocumentUrl { get; set; }

        public string? EvaluatorNotes { get; set; }

        public virtual HrdPerformanceAppraisal? PerformanceAppraisal { get; set; }

        [NotMapped] public Guid MetricCatalogId { get; set; }
        [NotMapped] public decimal TargetValue { get; set; }
        [NotMapped] public decimal ActualValue { get; set; }
        [NotMapped] public decimal SelfRatingScore { get => SelfScore ?? 0m; set => SelfScore = value; }
        [NotMapped] public decimal ManagerRatingScore { get => ManagerScore ?? 0m; set => ManagerScore = value; }
        [NotMapped] public decimal WeightedScore { get => FinalScore ?? 0m; set => FinalScore = value; }
        [NotMapped] public string? EvidenceUrl { get => EvidenceDocumentUrl; set => EvidenceDocumentUrl = value; }
        [NotMapped] public string? Comments { get => EvaluatorNotes; set => EvaluatorNotes = value; }

    }
}
