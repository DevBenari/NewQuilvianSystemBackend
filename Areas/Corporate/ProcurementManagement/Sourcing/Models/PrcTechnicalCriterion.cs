using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Butir kriteria teknis per item paket (BE-PBJ-PS-004; PBJ-DEC-074). Contoh: "rangka baja, beban 120 kg".
    /// </summary>
    /// <remarks>
    /// <see cref="CriterionNo"/> adalah nomor butir kriteria (1, 2, 3, …), bukan urutan tampilan generik — kamus data
    /// menyebutnya <c>SortOrder</c>, diganti field semantik mengikuti kontrak engineering (QBE).
    /// </remarks>
    [Table("PrcTechnicalCriterion", Schema = "public")]
    public class PrcTechnicalCriterion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageItemId { get; set; }

        public PrcPackageItem? PackageItem { get; set; }

        public int CriterionNo { get; set; }

        [Required]
        [MaxLength(500)]
        public string CriterionText { get; set; } = string.Empty;
    }
}
