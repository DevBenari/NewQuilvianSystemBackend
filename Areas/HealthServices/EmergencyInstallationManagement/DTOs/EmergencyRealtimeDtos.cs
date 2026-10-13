using System;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyRealtimeEventResponse
    {
        public string EventType { get; set; } = string.Empty;
        public Guid? EncounterId { get; set; }
        public Guid? VisitId { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? VisitStatus { get; set; }
        public string? RegistrationStatus { get; set; }
        public Guid? ActorUserId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string? Message { get; set; }
    }
}
