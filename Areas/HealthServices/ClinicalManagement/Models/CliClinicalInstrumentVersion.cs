using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Satu versi definisi instrumen klinis — <c>BE-RWI-107</c>, <c>BE-RWI-108</c>, kamus data 0.4 bagian 11.5.
    /// </summary>
    [Table("CliClinicalInstrumentVersion", Schema = "public")]
    public class CliClinicalInstrumentVersion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InstrumentId { get; set; }

        /// <summary>Mulai 1, naik per versi baru pada instrumen yang sama.</summary>
        public int VersionNumber { get; set; }

        public ClinicalInstrumentVersionStatus VersionStatus { get; set; } = ClinicalInstrumentVersionStatus.Draft;

        /// <summary>Definisi bagian, isian, pita, dan isian wajib. Beku setelah <c>Approved</c>.</summary>
        [Required]
        public string DefinitionJson { get; set; } = string.Empty;

        /// <summary>SHA-256 heksadesimal huruf kecil atas <c>DefinitionJson</c> ternormalisasi.</summary>
        [Required]
        [MaxLength(64)]
        public string DefinitionHash { get; set; } = string.Empty;

        /// <summary>Pengubah terakhir.</summary>
        public Guid LastModifiedByUserId { get; set; }

        public DateTime LastModifiedAt { get; set; }

        /// <summary>Pengesah; wajib berbeda dari pengubah terakhir.</summary>
        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? ApprovalNote { get; set; }

        public DateTime? RetiredAt { get; set; }

        public Guid? RetiredByUserId { get; set; }

        /// <summary>Wajib bila dipensiunkan tanpa pengganti.</summary>
        [MaxLength(500)]
        public string? RetireReason { get; set; }
    }
}
