using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models
{
    /// <summary>
    /// Alokator nomor jurnal milik Accounting.
    ///
    /// Bentuknya meniru <c>BilNumberSeries</c> milik Billing, tetapi tabelnya terpisah:
    /// <c>ACC-DEC-004</c> melarang Accounting menulis tabel Billing, sedangkan
    /// <c>QBE-CODE-006</c> menuntut alokator yang atomik dan ber-scope. Karena itu Accounting
    /// memerlukan tabelnya sendiri, bukan menumpang.
    ///
    /// Tabel ini <b>tidak</b> menyimpan nomor jurnalnya — nomor tersimpan di
    /// <c>AccJournal.JournalNumber</c>. Ia hanya menyimpan penghitungnya.
    ///
    /// Alokasi wajib berada di dalam satu transaction dan didahului
    /// <c>pg_advisory_xact_lock</c>, mengikuti pola repository yang sudah berlaku. Perilaku
    /// alokasinya adalah cakupan <c>BE-ACC-010</c>, bukan task ini.
    /// </summary>
    [Table("AccNumberSeries", Schema = "public")]
    public class AccNumberSeries : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identitas deret, misalnya jenis jurnal per badan hukum.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SequenceKey { get; set; } = string.Empty;

        /// <summary>
        /// Cakupan reset. Untuk nomor jurnal berbentuk <c>yyyyMM</c>.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ScopeKey { get; set; } = string.Empty;

        /// <summary>
        /// <c>NEVER</c>, <c>YEARLY</c>, <c>MONTHLY</c>, atau <c>DAILY</c>. Nomor jurnal memakai
        /// <c>MONTHLY</c>.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ResetPolicy { get; set; } = string.Empty;

        /// <summary>
        /// Nilai terakhir yang dialokasikan.
        /// </summary>
        public long CurrentValue { get; set; }

        public DateTimeOffset LastAllocatedAt { get; set; }
    }
}
