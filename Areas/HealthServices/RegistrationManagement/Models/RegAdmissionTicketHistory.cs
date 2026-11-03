using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Satu kejadian pada tiket antrean admisi: terbit, cetak, dipanggil, panggil ulang, mulai
    /// dilayani, dilewati, ditautkan ke kunjungan, selesai, atau dibatalkan. Baris tidak pernah
    /// diubah maupun dihapus.
    /// </summary>
    [Table("RegAdmissionTicketHistory", Schema = "public")]
    public class RegAdmissionTicketHistory : IdentityModel
    {
        public const string EventIssued = "ISSUED";
        public const string EventPrinted = "PRINTED";
        public const string EventPrintFailed = "PRINT_FAILED";
        public const string EventReprinted = "REPRINTED";
        public const string EventCalled = "CALLED";
        public const string EventRecalled = "RECALLED";
        public const string EventServiceStarted = "SERVICE_STARTED";
        public const string EventSkipped = "SKIPPED";
        public const string EventRegistrationLinked = "REGISTRATION_LINKED";
        public const string EventCompleted = "COMPLETED";
        public const string EventCancelled = "CANCELLED";

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AdmissionTicketId { get; set; }

        [Required]
        [MaxLength(40)]
        public string EventCode { get; set; } = string.Empty;

        public AdmissionTicketStatus? FromStatus { get; set; }

        [Required]
        public AdmissionTicketStatus ToStatus { get; set; }

        public Guid? ActorUserId { get; set; }

        [MaxLength(30)]
        public string? CounterCode { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public Guid? EncounterId { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public RegAdmissionTicket? AdmissionTicket { get; set; }
    }
}
