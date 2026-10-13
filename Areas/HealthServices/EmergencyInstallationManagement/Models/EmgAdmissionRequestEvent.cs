using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgAdmissionRequestEvent", Schema = "public")]
    public class EmgAdmissionRequestEvent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AdmissionRequestId { get; set; }

        public EmergencyAdmissionRequestEventType EventType { get; set; }

        public EmergencyAdmissionRequestStatus? FromStatus { get; set; }

        public EmergencyAdmissionRequestStatus ToStatus { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public EmgAdmissionRequest? AdmissionRequest { get; set; }
    }
}
