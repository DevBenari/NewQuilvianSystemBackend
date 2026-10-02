using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models
{
    /// <summary>
    /// Satu baris aturan posting — kamus data bagian 12b. Bentuknya meniru
    /// <c>AccJournalLine</c>, tetapi <b>tanpa nominal</b>: nominalnya diambil dari komponen
    /// kejadian yang ditunjuk <see cref="ComponentCode"/>.
    ///
    /// Contoh aturan pendapatan rawat jalan dengan jasa medis dokter: baris 1 <c>TOTAL</c> debit
    /// Piutang Penjamin, baris 2 <c>TOTAL</c> kredit Pendapatan Rawat Jalan, baris 3
    /// <c>JASA_MEDIS</c> debit Beban Jasa Medis, baris 4 <c>JASA_MEDIS</c> kredit Utang Jasa Medis
    /// Dokter. Kejadian Rp 10.000.000 berkomponen jasa medis Rp 3.000.000 menjadi jurnal empat
    /// baris, debit Rp 13.000.000 lawan kredit Rp 13.000.000.
    ///
    /// Sengaja <b>tidak</b> membawa <c>LegalEntityId</c> sendiri; badan hukumnya diturunkan dari
    /// akun, dan kesamaannya dengan badan hukum aturan ditegakkan di service.
    /// </summary>
    [Table("AccPostingRuleLine", Schema = "public")]
    public class AccPostingRuleLine : IdentityModel
    {
        /// <summary>
        /// Komponen bawaan yang selalu tersedia: nilai total kejadian.
        /// </summary>
        public const string KomponenTotal = "TOTAL";

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PostingRuleId { get; set; }

        /// <summary>
        /// Urutan baris, mulai dari 1. Unik dalam satu aturan.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Komponen nilai yang dipakai baris ini, contoh <c>JASA_MEDIS</c>. <c>TOTAL</c> berarti
        /// nilai total kejadian.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ComponentCode { get; set; } = KomponenTotal;

        /// <summary>
        /// Wajib menunjuk akun yang menerima transaksi dan sebadan hukum dengan aturannya;
        /// penegakannya di service. Control account <b>boleh</b> dipakai — aturan posting adalah
        /// jalur otomatis yang sah (<c>ACC-DEC-064</c>).
        /// </summary>
        [Required]
        public Guid AccountId { get; set; }

        /// <summary>
        /// <b>Wajib bila akun berjenis <c>Expense</c></b> (<c>ACC-DEC-019</c>); ditegakkan di service.
        /// </summary>
        public Guid? CostCenterId { get; set; }

        public PostingSide Side { get; set; }

        /// <summary>
        /// Keterangan yang disalin ke baris jurnal yang dihasilkan.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        public AccPostingRule? PostingRule { get; set; }

        public AccChartOfAccount? Account { get; set; }

        public MstCostCenter? CostCenter { get; set; }
    }
}
