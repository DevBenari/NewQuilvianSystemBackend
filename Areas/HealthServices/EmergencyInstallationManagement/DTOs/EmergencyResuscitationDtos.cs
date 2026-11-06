using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyResuscitationResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string ResuscitationNumber { get; set; } = string.Empty;
        public EmergencyResuscitationStatus ResuscitationStatus { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Location { get; set; }
        public string? TriggerCondition { get; set; }
        public Guid? TeamLeaderDoctorId { get; set; }
        public Guid? RecordedByUserId { get; set; }
        public bool WasCardiopulmonaryResuscitationPerformed { get; set; }
        public DateTime? CardiopulmonaryResuscitationStartedAt { get; set; }
        public DateTime? ReturnOfSpontaneousCirculationAt { get; set; }
        public int DefibrillationCount { get; set; }
        public string? AirwayManagementSummary { get; set; }
        public string? BreathingManagementSummary { get; set; }
        public string? CirculationManagementSummary { get; set; }
        public string? NeurologicalManagementSummary { get; set; }
        public string? OutcomeSummary { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyResuscitationRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [MaxLength(50)]
        public string? ResuscitationNumber { get; set; }

        public EmergencyResuscitationStatus ResuscitationStatus { get; set; } = EmergencyResuscitationStatus.Planned;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(250)]
        public string? Location { get; set; }

        [MaxLength(1000)]
        public string? TriggerCondition { get; set; }

        public Guid? TeamLeaderDoctorId { get; set; }

        public Guid? RecordedByUserId { get; set; }

        public bool WasCardiopulmonaryResuscitationPerformed { get; set; }

        public DateTime? CardiopulmonaryResuscitationStartedAt { get; set; }

        public DateTime? ReturnOfSpontaneousCirculationAt { get; set; }

        public int DefibrillationCount { get; set; }

        [MaxLength(1000)]
        public string? AirwayManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? BreathingManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? CirculationManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? NeurologicalManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? OutcomeSummary { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyResuscitationRequest : CreateEmergencyResuscitationRequest
    {
    }

    public class UpdateEmergencyResuscitationResuscitationStatusRequest
    {
        [Required]
        public EmergencyResuscitationStatus ResuscitationStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class EmergencyResuscitationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string ResuscitationNumber { get; set; } = string.Empty;
        public EmergencyResuscitationStatus ResuscitationStatus { get; set; }
        public DateTime StartDateTime { get; set; }
    }

    public class EmergencyResuscitationSummaryResponse
    {
        public int TotalResuscitation { get; set; }
        public int ActiveResuscitation { get; set; }
        public int InProgressResuscitation { get; set; }
        public int CompletedResuscitation { get; set; }
        public int TerminatedResuscitation { get; set; }
    }

    public class EmergencyResuscitationDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EmergencyVisitId { get; set; }
        public EmergencyResuscitationStatus? ResuscitationStatus { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "startdatetime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmergencyResuscitationFilterMetadataResponse
    {
        public EmergencyResuscitationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> SortableFields { get; set; } = new() { "startdatetime", "resuscitationnumber", "createdatetime" };
    }
}
