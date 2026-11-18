using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Sumber dana pagu dan permintaan pengadaan (G-10). Contoh: `OPERASIONAL`, `INVESTASI`.
    /// Kode diisi pengguna, unik di antara data yang belum dihapus.
    /// </summary>
    [Table("MstProcurementFundSource", Schema = "public")]
    public class MstProcurementFundSource : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string FundSourceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FundSourceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
