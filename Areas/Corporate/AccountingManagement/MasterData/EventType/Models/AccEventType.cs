using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models
{
    /// <summary>
    /// Jenis kejadian keuangan yang dikenal Accounting — kamus data bagian 11.
    ///
    /// Sengaja <b>tanpa</b> <c>LegalEntityId</c>: jenis kejadian bersifat struktural dan berlaku
    /// sama untuk semua badan hukum, sama seperti <c>AccJournalType</c>. Yang berbeda per badan
    /// hukum adalah aturan posting-nya.
    ///
    /// <b>Isinya belum ditetapkan.</b> Daftar jenis kejadian yang benar-benar diterbitkan Finance
    /// menunggu <c>DEC-ACC-P2-002</c>, jadi tabel ini tidak diisi seeder. Bentuknya tidak menunggu.
    /// </summary>
    [Table("AccEventType", Schema = "public")]
    public class AccEventType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode jenis kejadian, contoh <c>PENGAKUAN-PIUTANG</c>. Unik global. Kode inilah yang
        /// dibawa pesan kejadian, dan yang disimpan apa adanya pada kejadian berjenis belum
        /// terdaftar (<c>ACC-DEC-075</c>).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string EventTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Modul yang diharapkan menerbitkannya, contoh <c>Finance</c>.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SourceModule { get; set; } = string.Empty;

        /// <summary>
        /// Tidak boleh dimatikan selama masih ada aturan posting aktif yang memakainya; penegakannya
        /// di service.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
