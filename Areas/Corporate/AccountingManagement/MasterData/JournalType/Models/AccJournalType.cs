using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models
{
    /// <summary>
    /// Jenis jurnal beserta aturan alurnya.
    ///
    /// Sengaja <b>tanpa</b> <c>LegalEntityId</c>: jenis jurnal bersifat struktural dan berlaku
    /// sama untuk semua badan hukum.
    /// </summary>
    [Table("AccJournalType", Schema = "public")]
    public class AccJournalType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Contoh <c>JU</c>, <c>JP</c>, <c>JB</c>, <c>SA</c>. Unik global.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string JournalTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string JournalTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Awalan nomor jurnal. Wajib diambil dari master ini; penomoran jurnal tidak boleh
        /// menuliskan awalannya di kode.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string NumberPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Mewujudkan <c>ACC-DEC-010</c> — jurnal manual melewati persetujuan.
        /// </summary>
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Jenis bawaan sistem tidak dapat dihapus pengguna.
        /// </summary>
        public bool IsSystemType { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
