using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class QueueRealtimeEventResponse
    {
        public string EventType { get; set; } = string.Empty;

        public Guid QueueId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public List<Guid> NurseStationClusterIds { get; set; } = new();

        public DateTime QueueDate { get; set; }

        public int QueueNumber { get; set; }

        public string QueueCode { get; set; } = string.Empty;

        public QueueStatus QueueStatus { get; set; }

        public string QueueStatusName { get; set; } = string.Empty;

        public bool IsScreeningRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public bool IsPriorityQueue { get; set; }

        public DateTime? NurseCallExpiresAt { get; set; }

        public DateTime? DoctorCallExpiresAt { get; set; }

        public Guid? ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }

        public string? Message { get; set; }
    }
}
