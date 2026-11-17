using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Berita Acara Kesimpulan Akhir Sidang FPPE
    /// </summary>
    [Table("HrdFppeEvaluationConclusion", Schema = "public")]
    public class HrdFppeEvaluationConclusion : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FppeEngagementId { get; set; }

        [ForeignKey("FppeEngagementId")]
        public virtual HrdFppeEngagement? FppeEngagement { get; set; }

        /// <summary>
        /// 4 Rekomendasi Vonis: GRANT_INDEPENDENT, EXTEND_SUPERVISION, RESTRICT_PRIVILEGE, REVOKE_PRIVILEGE
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RecommendedAction { get; set; } = "GRANT_INDEPENDENT";

        [Required]
        public string PlenaryReviewSummary { get; set; } = string.Empty;

        public string? RemediationPlan { get; set; }

        [Required]
        public Guid ApprovedByCommitteeChairmanId { get; set; }

        public DateTime FinalizedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(256)]
        public string SignatureHash { get; set; } = string.Empty;
    }
}
