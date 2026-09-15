using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models
{
    /// <summary>
    /// Kepala aturan posting: jenis kejadian mana, pada badan hukum mana, menjadi jurnal jenis apa,
    /// dengan perlakuan apa — kamus data bagian 12.
    ///
    /// Akun debit dan kredit <b>tidak</b> ada di sini (<c>ACC-DEC-058</c>). Akunnya berada pada
    /// <see cref="AccPostingRuleLine"/>, karena satu aturan dapat menghasilkan tiga baris atau
    /// lebih — misalnya pendapatan rawat jalan yang disertai jasa medis dokter.
    ///
    /// Aturan lama tidak dihapus. Aturan yang diganti disimpan nonaktif sebagai riwayat, sehingga
    /// jurnal otomatis yang terbentuk sebelumnya tetap dapat ditelusuri ke aturan yang
    /// menghasilkannya.
    /// </summary>
    [Table("AccPostingRule", Schema = "public")]
    public class AccPostingRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Aturan berbeda per badan hukum (<c>ACC-DEC-037</c>).
        /// </summary>
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid EventTypeId { get; set; }

        /// <summary>
        /// Jenis jurnal yang dihasilkan aturan ini (<c>ACC-DEC-074</c>) — menentukan awalan nomor
        /// jurnal otomatisnya. Pola yang sama dengan <c>AccRecurringJournalTemplate.JournalTypeId</c>.
        /// </summary>
        [Required]
        public Guid JournalTypeId { get; set; }

        /// <summary>
        /// <b>Bawaan <see cref="AccountingEventTreatment.BuatDraft"/>, sengaja yang lebih aman.</b>
        /// Bila petugas lupa menetapkannya, akibat terburuknya jurnal menumpuk menunggu pemeriksaan
        /// — bukan angka salah yang langsung masuk buku besar.
        /// </summary>
        public AccountingEventTreatment Treatment { get; set; } = AccountingEventTreatment.BuatDraft;

        /// <summary>
        /// Satu jenis kejadian hanya boleh punya satu aturan aktif per badan hukum; dijaga unique
        /// index berfilter.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public AccEventType? EventType { get; set; }

        public AccJournalType? JournalType { get; set; }

        public ICollection<AccPostingRuleLine> Lines { get; set; } = new List<AccPostingRuleLine>();
    }
}
