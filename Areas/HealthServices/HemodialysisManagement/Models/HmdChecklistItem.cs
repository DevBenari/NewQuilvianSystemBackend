using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Master butir checklist Pra-HD (<c>FEAT-010</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="IsOverridable"/> adalah syarat 1 dan wujud <c>HMD-ASM-001</c>: bawaannya
    /// <c>false</c> untuk seluruh butir, dan hanya pemegang hak akses
    /// <c>HemodialysisChecklistItem : SetOverridable</c> yang dapat mengubahnya. Setiap perubahan
    /// menyimpan alasan, pelaku, dan waktunya pada baris ini — alasan adalah bagian dari data,
    /// bukan bagian dari log.
    /// </para>
    /// <para>
    /// <see cref="CheckSequence"/> adalah urutan pemeriksaan butir, bukan <c>SortOrder</c>
    /// presentasi generik yang dilarang untuk kode baru.
    /// </para>
    /// </remarks>
    [Table("HmdChecklistItem", Schema = "public")]
    public class HmdChecklistItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public HmdChecklistCategory Category { get; set; }

        public bool IsMandatory { get; set; } = true;

        public bool IsOverridable { get; set; }

        [MaxLength(1000)]
        public string? OverridableDecisionNote { get; set; }

        public Guid? OverridableDecidedByUserId { get; set; }

        public DateTime? OverridableDecidedAt { get; set; }

        public int CheckSequence { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
