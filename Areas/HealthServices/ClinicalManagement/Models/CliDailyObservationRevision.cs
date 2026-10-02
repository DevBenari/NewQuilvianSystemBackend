using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Nilai observasi harian sebelum dikoreksi — <c>BE-RWI-119</c>. Hanya ditambah.
    /// </summary>
    [Table("CliDailyObservationRevision", Schema = "public")]
    public class CliDailyObservationRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ObservationId { get; set; }

        public int RevisionNumber { get; set; }

        /// <summary>Seluruh isian sebelum koreksi. SENSITIF.</summary>
        [Required]
        public string PreviousValuesJson { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(500)]
        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public DateTime CorrectedAt { get; set; }
    }
}
