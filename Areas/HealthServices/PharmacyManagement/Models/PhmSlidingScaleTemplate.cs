using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Protokol standar sliding scale rumah sakit, misalnya "Sliding Scale Insulin Dewasa" —
    /// <c>BE-RWI-102</c>, migration <c>R6</c>, kamus data 0.5 bagian 13.4.
    /// </summary>
    /// <remarks>
    /// Isi protokol tidak disimpan di baris ini melainkan pada <see cref="PhmSlidingScaleTemplateVersion"/>.
    /// Setiap perubahan rentang membuat versi baru, dan hanya versi <c>Approved</c> yang dapat dipesan
    /// (<c>RWI-DEC-146</c> butir 1). Prefix <c>Phm</c>, bukan <c>Mst</c>: konfigurasi klinis berversi
    /// milik modul — arsitektur 0.5 bagian 11.5.4.
    /// </remarks>
    [Table("PhmSlidingScaleTemplate", Schema = "public")]
    public class PhmSlidingScaleTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>Template nonaktif tidak dapat dipesan.</summary>
        public bool IsActive { get; set; } = true;

        public ICollection<PhmSlidingScaleTemplateVersion> Versions { get; set; } =
            new List<PhmSlidingScaleTemplateVersion>();
    }
}
