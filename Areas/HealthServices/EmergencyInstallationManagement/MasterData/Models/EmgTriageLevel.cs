using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models
{
    [Table("EmgTriageLevel", Schema = "public")]
    public class EmgTriageLevel : IdentityModel
    {
        /// <summary>
        /// Nilai <see cref="Level"/> yang menandai kategori di luar skala antrean, yaitu
        /// Hitam. Dipakai bersama oleh seeder, service, dan penyaring daftar level supaya
        /// arti angka 0 tidak ditulis ulang di banyak tempat lalu berbeda-beda.
        /// </summary>
        public const int OutOfQueueScaleLevel = 0;

        public Guid Id { get; set; } = Guid.NewGuid();

        public EmergencyTriageSystem TriageSystem { get; set; }
            = EmergencyTriageSystem.ATS;

        /// <summary>
        /// Urutan level pada skala antrean triase. Nilai 1 sampai 5 adalah skala antrean
        /// biasa. Nilai 0 dipakai khusus kategori Hitam, yang menurut arsitektur berada
        /// "di luar skala antrean" sehingga tidak pernah ikut diurutkan sebagai antrean
        /// dan tidak boleh ditetapkan otomatis oleh aplikasi.
        /// </summary>
        [Range(0, 5)]
        public int Level { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ColorName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ColorHex { get; set; }

        /// <summary>
        /// Target waktu respons dalam menit. Kosong berarti SOP belum menetapkan target,
        /// bukan berarti nol menit. Nol menit berarti harus dilayani seketika.
        /// </summary>
        public int? MaxWaitingMinutes { get; set; }

        public bool AllowsTreatmentBeforeRegistration { get; set; }

        public int Sequence { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<EmgTriageIndicator> Indicators { get; set; }
            = new List<EmgTriageIndicator>();

        public ICollection<EmgTriage> Triages { get; set; }
            = new List<EmgTriage>();
    }
}
