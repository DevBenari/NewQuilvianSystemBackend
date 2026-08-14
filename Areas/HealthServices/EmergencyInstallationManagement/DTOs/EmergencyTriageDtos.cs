using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyTriageResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public Guid TriageLevelId { get; set; }
        public Guid? PatientVitalSignId { get; set; }
        public int Sequence { get; set; }
        public bool IsRetriage { get; set; }
        public Guid? PreviousTriageId { get; set; }
        public EmergencyTriageSystem TriageSystem { get; set; }
        public EmergencyTriageStatus TriageStatus { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? MaxWaitingMinutesSnapshot { get; set; }
        public DateTime? ResponseDueAt { get; set; }
        public bool ImmediateCareAllowed { get; set; }
        public string? TriageReason { get; set; }
        public string? AirwaySummary { get; set; }
        public string? BreathingSummary { get; set; }
        public string? CirculationSummary { get; set; }
        public string? DisabilitySummary { get; set; }
        public string? ExposureSummary { get; set; }
        public string? RedFlagSummary { get; set; }
        public Guid PerformedByUserId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyTriageRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid TriageLevelId { get; set; }

        public Guid? PatientVitalSignId { get; set; }

        public int Sequence { get; set; } = 1;

        public bool IsRetriage { get; set; }

        public Guid? PreviousTriageId { get; set; }

        public EmergencyTriageSystem TriageSystem { get; set; } = EmergencyTriageSystem.ATS;

        public EmergencyTriageStatus TriageStatus { get; set; } = EmergencyTriageStatus.Draft;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public int? MaxWaitingMinutesSnapshot { get; set; }

        public DateTime? ResponseDueAt { get; set; }

        public bool ImmediateCareAllowed { get; set; }

        [MaxLength(1000)]
        public string? TriageReason { get; set; }

        [MaxLength(1000)]
        public string? AirwaySummary { get; set; }

        [MaxLength(1000)]
        public string? BreathingSummary { get; set; }

        [MaxLength(1000)]
        public string? CirculationSummary { get; set; }

        [MaxLength(1000)]
        public string? DisabilitySummary { get; set; }

        [MaxLength(1000)]
        public string? ExposureSummary { get; set; }

        [MaxLength(1000)]
        public string? RedFlagSummary { get; set; }

        public Guid PerformedByUserId { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyTriageRequest : CreateEmergencyTriageRequest
    {
    }

    public class UpdateEmergencyTriageTriageStatusRequest
    {
        [Required]
        public EmergencyTriageStatus TriageStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
