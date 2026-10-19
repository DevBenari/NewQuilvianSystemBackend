using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Template Hasil — menu v1 Setup → Template Hasil (<c>BE-RAD-26</c>, <c>RAD-DEC-021</c>).
    /// </summary>
    /// <remarks>
    /// Isian awal draf bacaan, bukan bacaan. Memakai template tidak melewati pengesahan dokter radiolog
    /// (<c>RAD-DEC-003</c>): isinya tetap draf yang wajib diperiksa dan disahkan. Boleh dikaitkan ke satu
    /// alat dan/atau satu pemeriksaan; kosong = berlaku umum.
    /// </remarks>
    public class RadReportTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        public Guid? ModalityId { get; set; }

        public Guid? ProcedureId { get; set; }

        public string? Findings { get; set; }

        [Required]
        public string Impression { get; set; } = string.Empty;

        public string? Recommendation { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
