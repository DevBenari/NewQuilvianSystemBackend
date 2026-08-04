using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyObservationDetailResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyObservationId { get; set; }
        public Guid? PatientVitalSignId { get; set; }
        public Guid? ProgressNoteId { get; set; }
        public DateTime RecordedAt { get; set; }
        public Guid RecordedByUserId { get; set; }
        public string? ClinicalConditionSummary { get; set; }
        public string? InterventionSummary { get; set; }
        public string? PatientResponseSummary { get; set; }
        public decimal? FluidIntakeMl { get; set; }
        public decimal? UrineOutputMl { get; set; }
        public decimal? OtherOutputMl { get; set; }
        public decimal? BleedingEstimatedMl { get; set; }
        public decimal? VomitEstimatedMl { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyObservationDetailRequest
    {
        [Required]
        public Guid EmergencyObservationId { get; set; }

        public Guid? PatientVitalSignId { get; set; }

        public Guid? ProgressNoteId { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public Guid RecordedByUserId { get; set; }

        [MaxLength(2000)]
        public string? ClinicalConditionSummary { get; set; }

        [MaxLength(2000)]
        public string? InterventionSummary { get; set; }

        [MaxLength(2000)]
        public string? PatientResponseSummary { get; set; }

        public decimal? FluidIntakeMl { get; set; }

        public decimal? UrineOutputMl { get; set; }

        public decimal? OtherOutputMl { get; set; }

        public decimal? BleedingEstimatedMl { get; set; }

        public decimal? VomitEstimatedMl { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyObservationDetailRequest : CreateEmergencyObservationDetailRequest
    {
    }
}
