using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models
{
    /// <summary>
    /// Template jurnal yang menerbitkan jurnal baru setiap periode — penyusutan bulanan, sewa
    /// dibayar di muka, amortisasi, dan sejenisnya.
    ///
    /// Jurnal yang dihasilkannya lahir berstatus <c>Draft</c> dan pengesahannya tetap manual
    /// (<c>ACC-DEC-050</c>). Sistem tidak pernah mengesahkan jurnal berulang sendiri, karena
    /// template yang sudah tidak sesuai — misalnya aset yang sudah dijual — akan terus mencatat
    /// beban yang tidak ada, dan karena angkanya wajar tidak ada yang curiga sampai audit.
    /// </summary>
    [Table("AccRecurringJournalTemplate", Schema = "public")]
    public class AccRecurringJournalTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Setiap badan hukum punya templatenya sendiri (<c>ACC-DEC-037</c>).
        /// </summary>
        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Kode template, contoh <c>SUSUT-ALKES</c>. Unik dalam satu badan hukum.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Jenis jurnal yang dihasilkan. Memakai <c>AccJournalType</c> yang sudah terisi sejak
        /// <c>ACC-TD-011</c>; template tidak memperkenalkan jenis jurnal baru.
        /// </summary>
        [Required]
        public Guid JournalTypeId { get; set; }

        public RecurringFrequency Frequency { get; set; } = RecurringFrequency.Bulanan;

        /// <summary>
        /// Tanggal terbit setiap bulan, <b>dibatasi 1 sampai 28</b>. Batas 28 disengaja: tanggal
        /// 29, 30, dan 31 tidak ada di setiap bulan, sehingga template bertanggal itu akan
        /// terlewat pada Februari tanpa menimbulkan error apa pun.
        /// </summary>
        public int DayOfMonth { get; set; } = 1;

        public DateTime StartDate { get; set; }

        /// <summary>
        /// Kosong berarti berlaku tanpa batas waktu.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// <b>Bawaannya tidak aktif.</b> Template baru wajib diperiksa lebih dahulu sebelum
        /// diaktifkan, karena begitu aktif ia menerbitkan jurnal sendiri setiap bulan.
        /// </summary>
        public bool IsActive { get; set; } = false;

        public MstLegalEntity? LegalEntity { get; set; }

        public AccJournalType? JournalType { get; set; }

        public ICollection<AccRecurringJournalTemplateLine> Lines { get; set; } =
            new List<AccRecurringJournalTemplateLine>();

        public ICollection<AccRecurringJournalRun> Runs { get; set; } =
            new List<AccRecurringJournalRun>();
    }
}
