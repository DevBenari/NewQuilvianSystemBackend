using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Kategori katalog barang/jasa Pengadaan, juga kunci pagu (PBJ-DEC-009). Contoh: `ALKES` Alat Kesehatan.
    /// Kode diisi pengguna, unik di antara data yang belum dihapus.
    /// </summary>
    [Table("MstProcurementItemCategory", Schema = "public")]
    public class MstProcurementItemCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
