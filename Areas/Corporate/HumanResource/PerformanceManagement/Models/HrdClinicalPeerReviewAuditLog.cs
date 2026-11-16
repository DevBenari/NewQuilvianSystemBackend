using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Log Audit Keamanan Akses Rahasia Telaah Sejawat (Peer Review Read-Audit Log)
    /// </summary>
    [Table("HrdClinicalPeerReviewAuditLog", Schema = "public")]
    public class HrdClinicalPeerReviewAuditLog : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AccessedWorkforceProfileId { get; set; }

        /// <summary>
        /// OPPE_REPORT, FPPE_CASE_LOG, COMPLICATION_INCIDENT
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RecordType { get; set; } = "OPPE_REPORT";

        [Required]
        public Guid RecordId { get; set; }

        [Required]
        public Guid AccessedByUserId { get; set; }

        [Required]
        [MaxLength(250)]
        public string AccessReason { get; set; } = string.Empty;

        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string IpAddress { get; set; } = "127.0.0.1";
    }
}
