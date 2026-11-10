using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Edukasi rencana dan status persetujuan pasien/keluarga — dokumen klinis
    /// <c>ClinicalDocumentKind.RehabEducation</c> (<c>data-dictionary.md</c> bagian 2.11, FEAT-015).
    /// </summary>
    /// <remarks>
    /// Persetujuan <c>Refused</c> pada edukasi terakhir sebuah program memblokir aktivasinya
    /// (<c>RHB-VAL-045</c>). Formulir informed consent hukum tetap milik modul consent existing.
    /// </remarks>
    [Table("RhbEducationRecord", Schema = "public")]
    public class RhbEducationRecord : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        public Guid? ProgramId { get; set; }

        public RhbProgram? Program { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Tujuan, manfaat, risiko, alternatif, peran pasien. SENSITIF.</summary>
        [Required]
        [MaxLength(2000)]
        public string MaterialText { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(150)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RecipientRelation { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string MethodText { get; set; } = string.Empty;

        public RehabUnderstandingLevel UnderstandingLevel { get; set; } = RehabUnderstandingLevel.NotAssessed;

        /// <summary>Hambatan bahasa, kognitif, atau sensorik. SENSITIF.</summary>
        [MaxLength(500)]
        public string? BarrierText { get; set; }

        public RehabConsentStatus ConsentStatus { get; set; }

        /// <summary>Wajib bila menolak atau setuju sebagian. SENSITIF.</summary>
        [MaxLength(500)]
        public string? ConsentNote { get; set; }

        [Required]
        public Guid EducatorUserId { get; set; }

        public DateTime EducatedAt { get; set; }

        public RehabRecordStatus RecordStatus { get; set; } = RehabRecordStatus.Draft;

        public int Version { get; set; }
    }
}
