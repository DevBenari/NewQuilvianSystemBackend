using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgObservationDetail", Schema = "public")]
    public class EmgObservationDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

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

        public EmgObservation? EmergencyObservation { get; set; }

        public TrxPatientVitalSign? PatientVitalSign { get; set; }

        public TrxPatientIntegratedProgressNote? ProgressNote { get; set; }

        public ApplicationUser? RecordedByUser { get; set; }
    }
}
