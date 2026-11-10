using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Lembar Rapor Evaluasi OPPE Tahunan/Semesteran Staf Medis
    /// </summary>
    [Table("trx_oppe_staff_evaluations", Schema = "public")]
    public class TrxOppeStaffEvaluation : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PeriodId { get; set; }

        [ForeignKey("PeriodId")]
        public virtual TrxOppeEvaluationPeriod? Period { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? CommitteeId { get; set; }

        /// <summary>
        /// Nilai tertimbang data kuantitatif SIMRS/KMKP (Maksimal 60.00)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal QuantitativeScore { get; set; } = 0.00m;

        /// <summary>
        /// Nilai tertimbang evaluasi kualitatif telaah sejawat / Peer Review SMF (Maksimal 40.00)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal QualitativeScore { get; set; } = 0.00m;

        /// <summary>
        /// Skor komposit akhir 0.00 - 100.00 ((0.60 * Kuantitatif) + (0.40 * Kualitatif))
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal FinalCompositeScore { get; set; } = 0.00m;

        /// <summary>
        /// Nilai mutu: 'A' (>=85), 'B' (75-84), 'C' (65-74), 'D' (<65)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PerformanceGrade { get; set; } = "C";

        public Guid? EvaluatorKsmHeadId { get; set; }

        public string? EvaluatorRecommendationNotes { get; set; }

        public bool IsAcknowledgedByStaff { get; set; } = false;

        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// DRAFT, EVALUATED_KSM, FINALIZED, ACKNOWLEDGED, DISPUTED
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "DRAFT";

        public virtual ICollection<TrxOppeStaffEvaluationDetail> EvaluationDetails { get; set; } = new List<TrxOppeStaffEvaluationDetail>();
    }
}
