using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models
{
    /// <summary>
    /// Satu baris jurnal. Sumber tunggal buku besar.
    ///
    /// Sengaja <b>tidak</b> membawa <c>LegalEntityId</c> sendiri (<c>ACC-DEC-037</c>). Badan
    /// hukumnya diturunkan dari akun yang ditunjuk, dan kesamaannya dengan badan hukum jurnal
    /// ditegakkan di service.
    /// </summary>
    [Table("AccJournalLine", Schema = "public")]
    public class AccJournalLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JournalId { get; set; }

        /// <summary>
        /// Urutan baris, mulai dari 1. Unik dalam satu jurnal.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Wajib menunjuk akun yang menerima transaksi dan masih aktif; penegakannya di service.
        /// </summary>
        [Required]
        public Guid AccountId { get; set; }

        /// <summary>
        /// <b>Wajib bila akun berjenis <c>Expense</c></b> (<c>ACC-DEC-019</c>). Kewajiban itu
        /// diturunkan dari jenis akun, bukan disimpan sebagai kolom, dan ditegakkan di service.
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// SENSITIF — keterangan baris.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// SENSITIF. Nol bila baris ini kredit. Tepat satu sisi yang boleh lebih besar dari nol;
        /// dijaga check constraint pada tabel dan diperiksa ulang di service.
        /// </summary>
        public decimal DebitAmount { get; set; }

        /// <summary>
        /// SENSITIF. Nol bila baris ini debit.
        /// </summary>
        public decimal CreditAmount { get; set; }

        public AccJournal? Journal { get; set; }

        public AccChartOfAccount? Account { get; set; }

        public MstCostCenter? CostCenter { get; set; }
    }
}
