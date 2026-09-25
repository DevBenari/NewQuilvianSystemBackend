using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Satu versi order sliding scale per pasien — <c>BE-RWI-103</c>, migration <c>R6</c>, kamus data
    /// 0.5 bagian 13.8.
    /// </summary>
    /// <remarks>
    /// Penyesuaian membuat versi baru; pelaksanaan lama tetap menunjuk versi lamanya
    /// (<c>RWI-DEC-146</c> butir 6). <see cref="TemplateVersionId"/> mencatat versi template asal yang
    /// wajib berstatus <c>Approved</c> saat dipesan.
    /// </remarks>
    [Table("PhmSlidingScaleOrderVersion", Schema = "public")]
    public class PhmSlidingScaleOrderVersion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        public int VersionNumber { get; set; }

        [Required]
        public Guid TemplateVersionId { get; set; }

        /// <summary><c>true</c> bila rentang atau dosis berbeda dari versi template asal.</summary>
        public bool IsAdjusted { get; set; }

        /// <summary>Wajib bila <see cref="IsAdjusted"/>. SENSITIF — keadaan klinis pasien.</summary>
        [MaxLength(500)]
        public string? AdjustmentReason { get; set; }

        [Required]
        public Guid OrderedByDoctorId { get; set; }

        [Required]
        public Guid OrderedByUserId { get; set; }

        public DateTime OrderedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public PhmSlidingScaleOrder? Order { get; set; }

        public PhmSlidingScaleTemplateVersion? TemplateVersion { get; set; }

        public MstDoctor? OrderedByDoctor { get; set; }

        public ApplicationUser? OrderedByUser { get; set; }

        public ICollection<PhmSlidingScaleRange> Ranges { get; set; } = new List<PhmSlidingScaleRange>();
    }
}
