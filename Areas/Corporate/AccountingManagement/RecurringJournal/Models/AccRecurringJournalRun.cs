using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models
{
    /// <summary>
    /// Bukti bahwa sebuah template sudah menerbitkan jurnalnya untuk satu periode.
    ///
    /// <b>Tabel terpenting bagi jurnal berulang.</b> Unique index
    /// <c>(TemplateId, AccountingPeriodId)</c> adalah satu-satunya hal yang mencegah penyusutan
    /// bulan yang sama tercatat dua kali ketika penjadwal berjalan dua kali — misalnya karena
    /// layanan dimuat ulang di tengah siklus.
    ///
    /// Memeriksanya di kode C# saja <b>tidak cukup</b>: dua proses yang berjalan bersamaan dapat
    /// sama-sama lolos pemeriksaan, tetapi tidak dapat sama-sama lolos dari database.
    /// </summary>
    [Table("AccRecurringJournalRun", Schema = "public")]
    public class AccRecurringJournalRun : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Periode tujuan penerbitan. Bersama <c>TemplateId</c> membentuk penjaga terbit ganda.
        /// </summary>
        [Required]
        public Guid AccountingPeriodId { get; set; }

        /// <summary>
        /// Jurnal <c>Draft</c> yang dihasilkan. Wajib terisi — sebuah penerbitan yang tidak
        /// menghasilkan jurnal tidak punya alasan untuk dicatat.
        /// </summary>
        [Required]
        public Guid JournalId { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public AccRecurringJournalTemplate? Template { get; set; }

        public AccountingPeriod.Models.AccAccountingPeriod? AccountingPeriod { get; set; }

        public AccJournal? Journal { get; set; }
    }
}
