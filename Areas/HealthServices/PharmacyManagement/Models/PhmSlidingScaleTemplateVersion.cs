using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Satu versi protokol sliding scale — <c>BE-RWI-102</c>, migration <c>R6</c>, kamus data 0.5
    /// bagian 13.5.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pengesah bukan pengubah terakhir</b> (<c>FR-DOK-095</c>, pola <c>RWI-DEC-136</c>).
    /// <see cref="LastModifiedByUserId"/> diperbarui setiap kali versi <c>Draft</c> diubah, dan
    /// pengesahan oleh akun yang sama ditolak.
    /// </para>
    /// <para>
    /// <b>Tepat satu versi <c>Approved</c> per template</b> dijaga dua lapis: service memensiunkan
    /// versi sah sebelumnya pada transaksi yang sama, dan index unik parsial basis data menolak dua
    /// versi <c>Approved</c> untuk template yang sama.
    /// </para>
    /// </remarks>
    [Table("PhmSlidingScaleTemplateVersion", Schema = "public")]
    public class PhmSlidingScaleTemplateVersion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        /// <summary>Mulai 1, naik setiap draft baru pada template yang sama.</summary>
        public int VersionNumber { get; set; }

        public SlidingScaleVersionStatus VersionStatus { get; set; } = SlidingScaleVersionStatus.Draft;

        /// <summary>Satuan gula darah seluruh rentang pada versi ini. Wajib dipilih — gate <c>G-25</c>.</summary>
        public BloodGlucoseUnit GlucoseUnit { get; set; }

        /// <summary>SHA-256 heksadesimal atas definisi rentang yang disahkan; diisi saat pengesahan.</summary>
        [MaxLength(64)]
        public string? DefinitionHash { get; set; }

        [Required]
        public Guid LastModifiedByUserId { get; set; }

        public DateTime LastModifiedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        /// <summary>Diisi saat versi sah berikutnya disahkan.</summary>
        public DateTime? RetiredAt { get; set; }

        [MaxLength(500)]
        public string? ApprovalNote { get; set; }

        public PhmSlidingScaleTemplate? Template { get; set; }

        public ApplicationUser? LastModifiedByUser { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<PhmSlidingScaleRange> Ranges { get; set; } = new List<PhmSlidingScaleRange>();
    }
}
