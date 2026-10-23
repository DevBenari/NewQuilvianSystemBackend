using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Nilai satu isian pada hasil Pemeriksaan MCU. Satu isian paling banyak satu nilai per hasil.
    /// </summary>
    /// <remarks>
    /// Angka disimpan sebagai teks berformat titik desimal (<c>6.4</c>) dan divalidasi service menurut
    /// <c>FieldType</c> isiannya. Nilai yang direvisi sesudah resume disetujui meninggalkan jejak di
    /// <see cref="McuExaminationResultValueRevision"/>.
    /// </remarks>
    [Table("McuExaminationResultValue", Schema = "public")]
    public class McuExaminationResultValue : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExaminationResultId { get; set; }

        public McuExaminationResult? ExaminationResult { get; set; }

        [Required]
        public Guid ExaminationFieldId { get; set; }

        public McuExaminationField? ExaminationField { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? ValueText { get; set; }

        public ICollection<McuExaminationResultValueRevision> Revisions { get; set; } =
            new List<McuExaminationResultValueRevision>();
    }
}
