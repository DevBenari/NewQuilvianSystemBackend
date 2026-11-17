using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Jenis pengadaan (PBJ-DEC-024): barang stok, aset, barang nonstok, jasa. Diisi lewat data
    /// awal dan hanya dibaca lewat API (PBJ-DEC-037), karena jenis menentukan alur serah terima:
    /// <see cref="IsGoods"/> = true lewat GR BLM, false lewat serah terima pekerjaan dan BAST.
    /// </summary>
    [Table("MstProcurementType", Schema = "public")]
    public class MstProcurementType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>STOCK</c>, <c>ASSET</c>, <c>NON_STOCK</c>, <c>SERVICE</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string TypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;

        public bool IsGoods { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
