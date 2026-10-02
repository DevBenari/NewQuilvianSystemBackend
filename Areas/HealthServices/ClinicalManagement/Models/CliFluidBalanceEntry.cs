using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Satu entri cairan masuk atau keluar — <c>BE-RWI-119</c>, <c>BE-RWI-122</c>, kamus data 0.4 bagian 11.8.
    /// </summary>
    [Table("CliFluidBalanceEntry", Schema = "public")]
    public class CliFluidBalanceEntry : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public FluidDirection Direction { get; set; }

        public FluidSourceCategory SourceCategory { get; set; }

        [MaxLength(200)]
        public string? SourceDetail { get; set; }

        public decimal VolumeMl { get; set; }

        public DateTime EntryDateTime { get; set; }

        public Guid RecordedByEmployeeId { get; set; }

        public Guid RecordedByUserId { get; set; }

        /// <summary>Wajib bila sumber Obat; dilarang untuk sumber lain.</summary>
        public Guid? MedicationAdministrationId { get; set; }

        public ClinicalMeasurementStatus EntryStatus { get; set; } = ClinicalMeasurementStatus.Active;

        public int RevisionNumber { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>Diisi saat dosis tertaut dikoreksi menjadi selain Administered — usulan <c>G-26</c>.</summary>
        public DateTime? DoseCorrectionFlaggedAt { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
