using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Satu kejadian pada Pendaftaran Pasien Online: dikirim, check-in, didaftarkan lewat jalur
    /// lain, tidak datang, dibatalkan, jadwal diganti, atau kontak diganti (<c>INV-PO-08</c>).
    /// Baris tidak pernah diubah maupun dihapus. Percobaan yang gagal tidak dicatat di sini
    /// (<c>GATE-PO-09</c>).
    /// </summary>
    [Table("RegOnlineRegistrationHistory", Schema = "public")]
    public class RegOnlineRegistrationHistory : IdentityModel
    {
        public const string EventSubmitted = "SUBMITTED";
        public const string EventCheckedIn = "CHECKED_IN";
        public const string EventRegisteredElsewhere = "REGISTERED_ELSEWHERE";
        public const string EventNoShow = "NO_SHOW";
        public const string EventCancelled = "CANCELLED";
        public const string EventScheduleChanged = "SCHEDULE_CHANGED";
        public const string EventContactChanged = "CONTACT_CHANGED";

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnlineRegistrationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventCode { get; set; } = string.Empty;

        public OnlineRegistrationStatus? FromStatus { get; set; }

        [Required]
        public OnlineRegistrationStatus ToStatus { get; set; }

        [Required]
        public OnlineRegistrationEventSource Source { get; set; }

        /// <summary>Petugas atau akun kiosk; kosong untuk halaman publik dan sistem.</summary>
        public Guid? ActorUserId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public Guid? EncounterId { get; set; }

        public DateTime? FromVisitDate { get; set; }

        public DateTime? ToVisitDate { get; set; }

        public Guid? FromDoctorScheduleId { get; set; }

        public Guid? ToDoctorScheduleId { get; set; }

        /// <summary>Kontak lama (<c>CONTACT_CHANGED</c>). Sensitif.</summary>
        [MaxLength(150)]
        public string? FromContact { get; set; }

        /// <summary>Kontak baru (<c>CONTACT_CHANGED</c>). Sensitif.</summary>
        [MaxLength(150)]
        public string? ToContact { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public RegOnlineRegistration? OnlineRegistration { get; set; }
    }
}
