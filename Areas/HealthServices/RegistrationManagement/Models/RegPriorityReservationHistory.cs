using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Satu kejadian pada Reservasi Prioritas: dibuat, tiba, dikembalikan, diskrining, kode diganti,
    /// dilengkapi, dibatalkan, tidak datang, atau didaftarkan lewat jalur lain (INV-RP-06).
    /// Baris tidak pernah diubah maupun dihapus.
    /// </summary>
    [Table("RegPriorityReservationHistory", Schema = "public")]
    public class RegPriorityReservationHistory : IdentityModel
    {
        public const string EventCreated = "CREATED";
        public const string EventArrived = "ARRIVED";
        public const string EventArrivalReverted = "ARRIVAL_REVERTED";
        public const string EventScreened = "SCREENED";
        public const string EventCodeReissued = "CODE_REISSUED";
        public const string EventConverted = "CONVERTED";
        public const string EventCancelled = "CANCELLED";
        public const string EventNoShow = "NO_SHOW";
        public const string EventRegisteredElsewhere = "REGISTERED_ELSEWHERE";

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PriorityReservationId { get; set; }

        [Required]
        [MaxLength(40)]
        public string EventCode { get; set; } = string.Empty;

        public PriorityReservationStatus? FromStatus { get; set; }

        [Required]
        public PriorityReservationStatus ToStatus { get; set; }

        [Required]
        public PriorityReservationEventSource Source { get; set; }

        /// <summary>Kosong untuk kejadian lewat link pasien dan oleh sistem.</summary>
        public Guid? ActorUserId { get; set; }

        /// <summary>Alasan batal atau mengembalikan tiba. Sensitif.</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        public Guid? EncounterId { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public RegPriorityReservation? PriorityReservation { get; set; }
    }
}
