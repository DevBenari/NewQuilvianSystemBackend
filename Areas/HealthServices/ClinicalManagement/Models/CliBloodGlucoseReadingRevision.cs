using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Nilai GDS bangsal sebelum dikoreksi — <c>BE-RWI-119</c>. Hanya ditambah.
    /// </summary>
    [Table("CliBloodGlucoseReadingRevision", Schema = "public")]
    public class CliBloodGlucoseReadingRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ReadingId { get; set; }

        public int RevisionNumber { get; set; }

        public decimal PreviousValue { get; set; }

        public BloodGlucoseUnit PreviousUnit { get; set; }

        public DateTime PreviousMeasuredAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(500)]
        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public DateTime CorrectedAt { get; set; }
    }
}
