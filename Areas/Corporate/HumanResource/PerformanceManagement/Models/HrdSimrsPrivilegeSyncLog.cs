using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Log pengiriman wewenang klinis ke modul SIMRS EMR sesudah putusan rekredensialing (INT-HR-76, BE-HRD-235).
    /// </summary>
    /// <remarks>
    /// Durasi (<see cref="LatencyMs"/>) adalah waktu nyata yang terukur saat pengiriman diproses, bukan angka
    /// acak seperti sebelum 10 November 2026. Status dan kode respons mencerminkan hasil sebenarnya.
    /// </remarks>
    [Table("HrdSimrsPrivilegeSyncLog", Schema = "public")]
    public class HrdSimrsPrivilegeSyncLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid VerdictId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DoctorCode { get; set; } = string.Empty;

        /// <summary>Level wewenang klinis tujuan 1 s.d. 4.</summary>
        public int TargetLevel { get; set; }

        public bool IsHardStopBlocked { get; set; }

        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        public int ResponseStatusCode { get; set; }

        public long LatencyMs { get; set; }

        public DateTime DispatchedAt { get; set; } = DateTime.UtcNow;

        /// <summary>SUCCESS, PENDING_INTEGRATION, atau FAILED.</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "SUCCESS";

        [MaxLength(1000)]
        public string? Message { get; set; }

        public HrdRecredentialingVerdict? Verdict { get; set; }
    }
}
