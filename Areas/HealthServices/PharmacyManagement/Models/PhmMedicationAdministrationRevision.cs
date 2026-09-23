using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Nilai dosis MAR sebelum dikoreksi atau sebelum cek gandanya ditolak — <c>BE-RWI-115</c>, <c>BE-RWI-116</c>, kamus data 0.4 bagian 11.13. Hanya ditambah.
    /// </summary>
    [Table("PhmMedicationAdministrationRevision", Schema = "public")]
    public class PhmMedicationAdministrationRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AdministrationId { get; set; }

        /// <summary>Nomor revisi yang menghasilkan baris berlaku sesudah perubahan.</summary>
        public int RevisionNumber { get; set; }

        /// <summary><c>Correction</c> atau <c>DoubleCheckRejected</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string RevisionKind { get; set; } = string.Empty;

        public MedicationDoseStatus PreviousDoseStatus { get; set; }

        public decimal? PreviousActualDose { get; set; }

        [MaxLength(100)]
        public string? PreviousActualRouteSnapshot { get; set; }

        public DateTime? PreviousAdministeredAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? PreviousStatusReason { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? PreviousDeviationNote { get; set; }

        public Guid? PreviousRecordedByUserId { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(500)]
        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public DateTime CorrectedAt { get; set; }
    }
}
