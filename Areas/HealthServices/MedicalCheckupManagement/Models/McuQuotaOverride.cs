using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Pelampauan kuota yang disetujui kepala unit MCU, contoh +10 untuk
    /// <i>Kontrak tahunan PT Contoh</i>.
    /// </summary>
    [Table("McuQuotaOverride", Schema = "public")]
    public class McuQuotaOverride : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DailyScheduleId { get; set; }

        public McuDailySchedule? DailySchedule { get; set; }

        /// <summary>≥ 1.</summary>
        public int AdditionalQuota { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public Guid ApprovedByUserId { get; set; }

        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
    }
}
