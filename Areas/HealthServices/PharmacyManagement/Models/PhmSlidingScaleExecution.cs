using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Satu pelaksanaan protokol sliding scale — <c>BE-RWI-123</c>, kamus data 0.4 bagian 11.15.
    /// </summary>
    [Table("PhmSlidingScaleExecution", Schema = "public")]
    public class PhmSlidingScaleExecution : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string ExecutionNumber { get; set; } = string.Empty;

        public Guid OrderId { get; set; }

        /// <summary>Versi order berlaku saat hitung.</summary>
        public Guid OrderVersionId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public Guid BloodGlucoseReadingId { get; set; }

        /// <summary>Salinan saat hitung, bukan sumber.</summary>
        public decimal GlucoseValueSnapshot { get; set; }

        public BloodGlucoseUnit GlucoseUnitSnapshot { get; set; }

        public Guid MatchedRangeId { get; set; }

        public decimal ComputedDoseUnits { get; set; }

        public Guid MedicationAdministrationId { get; set; }

        public bool IsException { get; set; } = false;

        /// <summary>Wajib bila dosis aktual berbeda dari dosis hitung. SENSITIF.</summary>
        [MaxLength(500)]
        public string? ExceptionReason { get; set; }

        public bool ReadingCorrectedAfterExecution { get; set; } = false;

        public Guid ExecutedByEmployeeId { get; set; }

        public Guid ExecutedByUserId { get; set; }

        public DateTime ExecutedAt { get; set; }

        public SlidingScaleExecutionStatus ExecutionStatus { get; set; } = SlidingScaleExecutionStatus.Recorded;

        /// <summary>Wajib — tombol ganda tidak boleh memberi insulin dua kali.</summary>
        [Required]
        [MaxLength(100)]
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
