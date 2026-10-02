using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Nilai entri cairan sebelum dikoreksi — <c>BE-RWI-119</c>. Hanya ditambah.
    /// </summary>
    [Table("CliFluidBalanceEntryRevision", Schema = "public")]
    public class CliFluidBalanceEntryRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EntryId { get; set; }

        public int RevisionNumber { get; set; }

        public decimal PreviousVolumeMl { get; set; }

        public DateTime PreviousEntryDateTime { get; set; }

        public FluidSourceCategory PreviousSourceCategory { get; set; }

        [MaxLength(200)]
        public string? PreviousSourceDetail { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(500)]
        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public DateTime CorrectedAt { get; set; }
    }
}
