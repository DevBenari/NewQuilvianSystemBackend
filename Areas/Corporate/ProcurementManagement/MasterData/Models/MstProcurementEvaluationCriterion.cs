using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Kriteria penilaian vendor per kategori (BE-PBJ-VD-001; PBJ-DEC-050). Total bobot kriteria aktif satu
    /// kategori harus 100% saat penilaian dibuat (PBJ-VAL-108). Contoh kategori Umum: Ketepatan waktu 40%,
    /// Kualitas 40%, Layanan purnajual 20%. Urutan tampil = nama; tidak ada <c>SortOrder</c> tersimpan.
    /// </summary>
    [Table("MstProcurementEvaluationCriterion", Schema = "public")]
    public class MstProcurementEvaluationCriterion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ItemCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CriterionName { get; set; } = string.Empty;

        /// <summary>Bobot persen, 0 &lt; bobot ≤ 100 (PBJ-VAL-113).</summary>
        [Column(TypeName = "numeric(5,2)")]
        public decimal WeightPercent { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProcurementItemCategory? ItemCategory { get; set; }
    }
}
