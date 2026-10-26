using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Satu isian formulir Pemeriksaan MCU, contoh <c>TEKANAN_SISTOLIK</c> dalam <c>mmHg</c>.
    /// </summary>
    /// <remarks>
    /// Urutan tampil memakai <see cref="FieldNumber"/> — nomor butir formulir yang bermakna bisnis
    /// dan unik per jenis — bukan kolom urutan presentasi generik.
    /// </remarks>
    [Table("McuExaminationField", Schema = "public")]
    public class McuExaminationField : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExaminationTypeId { get; set; }

        public McuExaminationType? ExaminationType { get; set; }

        [Required]
        [MaxLength(50)]
        public string FieldCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FieldLabel { get; set; } = string.Empty;

        public McuExaminationFieldType FieldType { get; set; }

        [MaxLength(30)]
        public string? UnitName { get; set; }

        /// <summary>Wajib untuk isian Pilihan; dipisah <c>|</c>, contoh <c>Normal|Parsial|Total</c>.</summary>
        [MaxLength(1000)]
        public string? OptionValues { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Nomor butir pada formulir; ≥ 1, unik per jenis pemeriksaan.</summary>
        public int FieldNumber { get; set; }
    }
}
