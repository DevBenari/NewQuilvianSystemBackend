using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Satu-satunya tempat GDS bangsal — <c>BE-RWI-119</c>, <c>BE-RWI-123</c>, kamus data 0.4 bagian 11.9.
    /// </summary>
    [Table("CliBloodGlucoseReading", Schema = "public")]
    public class CliBloodGlucoseReading : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public DateTime MeasuredAt { get; set; }

        public decimal GlucoseValue { get; set; }

        /// <summary>Wajib dipilih, tanpa bawaan — gate <c>G-25</c>.</summary>
        public BloodGlucoseUnit GlucoseUnit { get; set; }

        public BloodGlucoseMethod Method { get; set; } = BloodGlucoseMethod.WardGlucometer;

        public Guid RecordedByEmployeeId { get; set; }

        public Guid RecordedByUserId { get; set; }

        public ClinicalMeasurementStatus ReadingStatus { get; set; } = ClinicalMeasurementStatus.Active;

        public int RevisionNumber { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
