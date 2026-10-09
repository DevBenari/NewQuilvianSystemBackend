using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInpatientClearanceItem", Schema = "public")]
    public class MstInpatientClearanceItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsMandatory { get; set; } = true;

        /// <remarks>
        /// <c>TOUCHED LEGACY</c> (<c>BE-RWI-185</c>): kolom lama ini tetap menjadi urutan butir per
        /// jenis daftar periksa. Tidak ada kolom urutan baru yang ditambahkan.
        /// </remarks>
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Jenis daftar periksa (<c>BE-RWI-185</c>, kamus data 20.15). Bawaan
        /// <see cref="MstClearanceChecklistType.EpisodeClosure"/>, sehingga butir lama tetap menjadi
        /// butir penutupan episode.
        /// </summary>
        public MstClearanceChecklistType ChecklistType { get; set; } = MstClearanceChecklistType.EpisodeClosure;

        /// <summary>
        /// Induk sub-butir, misalnya "Laboratorium" di bawah butir 2 serah terima. Induk wajib satu
        /// jenis dan bukan sub-butir (kedalaman satu). Butir penutupan tidak pernah punya induk.
        /// </summary>
        public Guid? ParentItemId { get; set; }

        /// <summary>
        /// Sumber saran sistem untuk butir serah terima. Butir penutupan selalu
        /// <see cref="MstHandoverSuggestionSource.None"/>.
        /// </summary>
        public MstHandoverSuggestionSource HandoverSuggestionSource { get; set; } = MstHandoverSuggestionSource.None;

        public MstInpatientClearanceItem? ParentItem { get; set; }
    }
}
