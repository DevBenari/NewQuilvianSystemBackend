using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models
{
    [Table("NvwPatientCallEvent", Schema = "public")]
    public class NvwPatientCallEvent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientCallId { get; set; }

        public NvwPatientCall? PatientCall { get; set; }

        public NvwPatientCallEventType EventType { get; set; }

        public NvwPatientCallActorKind ActorKind { get; set; }

        public Guid? ActorUserId { get; set; }

        public Guid? PreviousClaimerUserId { get; set; }

        public NvwPatientCallCloseReason? CloseReason { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }

        public DateTime OccurredAt { get; set; }

        public Guid? IdempotencyKey { get; set; }
    }
}
