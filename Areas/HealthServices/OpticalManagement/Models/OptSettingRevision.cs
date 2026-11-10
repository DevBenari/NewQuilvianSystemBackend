using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Nilai lama dan baru setiap perubahan pengaturan Optik — audit database, terpisah dari logger (<c>QBE-AUD-001</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptSettingRevision", Schema = "public")]
    public class OptSettingRevision : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pengaturan yang berubah</summary>
        [Required]
        public Guid SettingId { get; set; }

        public OptSetting? Setting { get; set; }

        /// <summary>Nilai lama → baru per kolom, contoh PickupReminderDays: 30 → 21</summary>
        [Required]
        [MaxLength(2000)]
        public string ChangedFieldsSummary { get; set; } = string.Empty;

        /// <summary>Admin Optik</summary>
        [Required]
        public Guid ChangedBy { get; set; }

        /// <summary>Waktu perubahan</summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
