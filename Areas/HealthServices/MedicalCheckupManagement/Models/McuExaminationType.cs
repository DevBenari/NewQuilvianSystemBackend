using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Jenis Pemeriksaan MCU yang diisi di modul MCU sendiri, contoh <c>FISIK</c> (Pemeriksaan Fisik)
    /// atau <c>VISUS</c>. Isiannya ada di <see cref="McuExaminationField"/>.
    /// </summary>
    [Table("McuExaminationType", Schema = "public")]
    public class McuExaminationType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string ExaminationTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ExaminationTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<McuExaminationField> Fields { get; set; } = new List<McuExaminationField>();
    }
}
