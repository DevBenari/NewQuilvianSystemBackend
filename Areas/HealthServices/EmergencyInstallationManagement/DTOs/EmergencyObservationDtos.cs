using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyObservationResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string ObservationNumber { get; set; } = string.Empty;
        public EmergencyObservationStatus ObservationStatus { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? ObservationLocation { get; set; }
        public string? Indication { get; set; }
        public string? ObservationPlan { get; set; }
        public Guid? ResponsibleDoctorId { get; set; }
        public Guid? ResponsibleNurseUserId { get; set; }
        public string? CompletionSummary { get; set; }
        public string? EscalationReason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyObservationRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [MaxLength(50)]
        public string? ObservationNumber { get; set; }

        public EmergencyObservationStatus ObservationStatus { get; set; } = EmergencyObservationStatus.Active;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        [MaxLength(250)]
        public string? ObservationLocation { get; set; }

        [MaxLength(1000)]
        public string? Indication { get; set; }

        [MaxLength(2000)]
        public string? ObservationPlan { get; set; }

        public Guid? ResponsibleDoctorId { get; set; }

        public Guid? ResponsibleNurseUserId { get; set; }

        [MaxLength(1000)]
        public string? CompletionSummary { get; set; }

        [MaxLength(1000)]
        public string? EscalationReason { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyObservationRequest : CreateEmergencyObservationRequest
    {
    }

    public class UpdateEmergencyObservationObservationStatusRequest
    {
        [Required]
        public EmergencyObservationStatus ObservationStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
