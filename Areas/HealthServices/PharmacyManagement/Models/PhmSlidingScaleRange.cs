using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Satu baris skala: batas bawah inklusif, batas atas eksklusif, dan dosis insulin —
    /// <c>BE-RWI-102</c>, migration <c>R6</c>, kamus data 0.5 bagian 13.6.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tepat satu pemilik</b>: versi template <b>atau</b> versi order, tidak pernah keduanya — check
    /// constraint <c>CK_PhmSlidingScaleRange_SingleOwner</c>. Satu tabel untuk kedua pemilik mencegah dua
    /// definisi rentang yang dapat menyimpang (kamus data 13.14).
    /// </para>
    /// <para>
    /// <b>Contoh.</b> <c>[null, 150)</c> → 0 unit; <c>[150, 200)</c> → 2 unit; <c>[200, null)</c> → 4 unit.
    /// GDS 199,9 jatuh ke <c>[150, 200)</c>; GDS 200 jatuh ke <c>[200, null)</c>. Aturan tidak bertumpuk
    /// dan tidak berlubang dijaga service, karena pemeriksaan antar-baris tidak dapat dinyatakan satu
    /// check constraint.
    /// </para>
    /// <para>
    /// <see cref="SortOrder"/> di sini adalah urutan klinis dari batas terendah yang dibentuk service
    /// setelah rentang divalidasi, bukan urutan tampilan bebas.
    /// </para>
    /// </remarks>
    [Table("PhmSlidingScaleRange", Schema = "public")]
    public class PhmSlidingScaleRange : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? TemplateVersionId { get; set; }

        public Guid? OrderVersionId { get; set; }

        /// <summary>Kosong berarti terbuka ke bawah.</summary>
        public decimal? LowerBoundInclusive { get; set; }

        /// <summary>Kosong berarti terbuka ke atas.</summary>
        public decimal? UpperBoundExclusive { get; set; }

        /// <summary>Dosis insulin dalam unit, <c>≥ 0</c>. <c>0</c> sah untuk rentang tanpa insulin.</summary>
        public decimal DoseUnits { get; set; }

        [MaxLength(300)]
        public string? InstructionText { get; set; }

        /// <summary>Penanda tampilan saja bagi perawat — gate <c>G-24</c>.</summary>
        public bool RequiresPhysicianNotification { get; set; }

        public int SortOrder { get; set; }

        public PhmSlidingScaleTemplateVersion? TemplateVersion { get; set; }

        public PhmSlidingScaleOrderVersion? OrderVersion { get; set; }
    }
}
