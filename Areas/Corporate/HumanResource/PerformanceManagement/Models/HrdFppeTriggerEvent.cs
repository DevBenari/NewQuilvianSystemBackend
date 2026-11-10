using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Event pemicu kritis FPPE yang diterima dari modul lain — insiden sentinel, komplikasi di atas 2 SD,
    /// keterlambatan resume medis, dan sebagainya (INT-HR-74, BE-HRD-229).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 feed yang diterima hanya ditampung di <c>List</c> statis dalam memori proses
    /// dan hilang saat aplikasi di-restart, sehingga pemicu yang sudah dilaporkan modul mutu bisa lenyap
    /// sebelum ditindaklanjuti komite.
    /// </remarks>
    [Table("HrdFppeTriggerEvent", Schema = "public")]
    public class HrdFppeTriggerEvent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(200)]
        public string? DoctorNameSnapshot { get; set; }

        [MaxLength(50)]
        public string? DoctorCodeSnapshot { get; set; }

        [MaxLength(150)]
        public string? SpecializationSnapshot { get; set; }

        /// <summary>SENTINEL_EVENT, ABNORMAL_COMPLICATION, EMR_DELINQUENCY, ETHICAL_DISCIPLINARY, NEW_STAFF_ONBOARDING, NEW_PRIVILEGE_REQUEST.</summary>
        [Required]
        [MaxLength(50)]
        public string TriggerType { get; set; } = string.Empty;

        /// <summary>FATAL, ERROR, WARNING, INFO.</summary>
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "WARNING";

        public DateTime IncidentDate { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? MetricEvidence { get; set; }

        [MaxLength(500)]
        public string? RecommendedAction { get; set; }

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }
}
