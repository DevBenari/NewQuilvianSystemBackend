using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Log Kasus Klinis / Bedah Pendampingan Proctor FPPE
    /// </summary>
    [Table("trx_fppe_case_logs", Schema = "public")]
    public class TrxFppeCaseLog : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FppeEngagementId { get; set; }

        [ForeignKey("FppeEngagementId")]
        public virtual TrxFppeEngagement? FppeEngagement { get; set; }

        /// <summary>
        /// ID Episode SIMRS atau Pseudonim Pasien
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SimrsPatientEpisodeId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ProcedurePerformed { get; set; } = string.Empty;

        public DateTime ProcedureDate { get; set; }

        /// <summary>
        /// Skor keterampilan teknik klinis (Skala Likert 1 s.d. 5)
        /// </summary>
        public int TechnicalSkillScore { get; set; } = 3;

        /// <summary>
        /// Skor pengambilan keputusan kritis (Skala Likert 1 s.d. 5)
        /// </summary>
        public int CriticalDecisionScore { get; set; } = 3;

        public bool HasComplication { get; set; } = false;

        public string? ComplicationDescription { get; set; }

        [Required]
        public string ProctorFeedbackNotes { get; set; } = string.Empty;

        /// <summary>
        /// Kemandirian: Level 1 (Mandiri Penuh) s.d. Level 4 (Perlu Bantuan Total)
        /// </summary>
        public int ObservedIndependenceLevel { get; set; } = 2;

        public bool IsVerifiedByProctor { get; set; } = true;

        public DateTime? ProctorVerifiedAt { get; set; }
    }
}
