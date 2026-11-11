using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Penugasan FPPE (Evaluasi Terfokus Praktik Profesional)
    /// </summary>
    [Table("trx_fppe_engagements", Schema = "public")]
    public class TrxFppeEngagement : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string EngagementNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PrivilegeCatalogId { get; set; }

        /// <summary>
        /// NEW_STAFF, NEW_PRIVILEGE_PROCTORING, SENTINEL_EVENT, ABNORMAL_COMPLICATION, EMR_DELINQUENCY, ETHICAL_DISCIPLINARY
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TriggerSource { get; set; } = "NEW_STAFF";

        [Required]
        public string InitialReasonJustification { get; set; } = string.Empty;

        [Required]
        public Guid AssignedProctorWorkforceId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime TargetEndDate { get; set; }

        public int TargetCaseCount { get; set; } = 10;

        public int CompletedCaseCount { get; set; } = 0;

        /// <summary>
        /// ACTIVE, IN_REVIEW, COMPLETED_SUCCESS, FAILED_RESTRICTED, CANCELLED
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "ACTIVE";

        public virtual ICollection<TrxFppeCaseLog> CaseLogs { get; set; } = new List<TrxFppeCaseLog>();
    }
}
