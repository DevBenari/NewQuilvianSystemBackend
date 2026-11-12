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

    public class EmergencyObservationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string? ObservationLocation { get; set; }
        public EmergencyObservationStatus ObservationStatus { get; set; }
        public DateTime StartDateTime { get; set; }
    }

    public class EmergencyObservationSummaryResponse
    {
        public int TotalObservation { get; set; }
        public int ActiveObservation { get; set; }
        public int PlannedObservation { get; set; }
        public int InProgressObservation { get; set; }
        public int CompletedObservation { get; set; }
        public int DiscontinuedObservation { get; set; }
    }

    public class EmergencyObservationDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EmergencyVisitId { get; set; }
        public EmergencyObservationStatus? ObservationStatus { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "startdatetime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmergencyObservationFilterMetadataResponse
    {
        public EmergencyObservationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> SortableFields { get; set; } = new() { "startdatetime", "createdatetime" };
    }
}
