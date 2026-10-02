using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Observasi harian: diet, mobilisasi, lingkar perut, agitasi — <c>BE-RWI-119</c>, kamus data 0.4 bagian 11.10.
    /// </summary>
    [Table("CliDailyObservation", Schema = "public")]
    public class CliDailyObservation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public DateTime ObservedAt { get; set; }

        /// <summary>0–100.</summary>
        public int? DietIntakePercent { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? DietNote { get; set; }

        public MobilizationLevel MobilizationLevel { get; set; }

        /// <summary>20–250 cm.</summary>
        public decimal? AbdominalCircumferenceCm { get; set; }

        /// <summary><c>null</c> = belum dinilai, bukan "tidak agitasi".</summary>
        public bool? IsAgitated { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? Note { get; set; }

        public Guid RecordedByEmployeeId { get; set; }

        public Guid RecordedByUserId { get; set; }

        public ClinicalMeasurementStatus ObservationStatus { get; set; } = ClinicalMeasurementStatus.Active;

        public int RevisionNumber { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
