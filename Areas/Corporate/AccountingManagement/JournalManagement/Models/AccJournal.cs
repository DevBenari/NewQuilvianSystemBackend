using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models
{
    /// <summary>
    /// Kepala satu catatan transaksi akuntansi. Aggregate root modul Accounting.
    ///
    /// Satu jurnal tidak boleh mencampur dua badan hukum (<c>ACC-DEC-037</c>). Aturan itu
    /// ditegakkan di service, karena melibatkan pemeriksaan setiap akun pada barisnya.
    /// </summary>
    [Table("AccJournal", Schema = "public")]
    public class AccJournal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Contoh <c>JU/2026/09/00001</c>. Nomor terlewat diizinkan, nomor kembar tidak
        /// (<c>ACC-DEC-014</c>).
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string JournalNumber { get; set; } = string.Empty;

        /// <summary>
        /// Menentukan awalan nomor dan aturan alur persetujuan.
        /// </summary>
        [Required]
        public Guid JournalTypeId { get; set; }

        /// <summary>
        /// Ditentukan sistem dari <see cref="AccountingDate"/>, tidak pernah dipilih pengguna.
        /// </summary>
        [Required]
        public Guid AccountingPeriodId { get; set; }

        /// <summary>
        /// Nomor dokumen sumber, misalnya nomor faktur pemasok.
        /// </summary>
        [MaxLength(50)]
        public string? DocumentNumber { get; set; }

        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Menentukan periode. Inilah tanggal yang dipakai laporan.
        /// </summary>
        public DateTime AccountingDate { get; set; }

        /// <summary>
        /// SENSITIF — rahasia bisnis, tidak boleh masuk logger.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public JournalStatus JournalStatus { get; set; } = JournalStatus.Draft;

        /// <summary>
        /// SENSITIF. <b>Salinan</b> dari jumlah baris untuk mempercepat tampilan daftar.
        /// Bukan sumber kebenaran: keputusan boleh atau tidaknya pengajuan dan pengesahan selalu
        /// dihitung ulang dari <see cref="Lines"/>.
        /// </summary>
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// SENSITIF. Salinan, sama seperti <see cref="TotalDebit"/>.
        /// </summary>
        public decimal TotalCredit { get; set; }

        public Guid? SubmittedBy { get; set; }

        public DateTime? SubmittedAt { get; set; }

        /// <summary>
        /// Penyetuju. Tidak boleh sama dengan <c>CreateBy</c> (<c>ACC-DEC-016</c>);
        /// penegakannya di service, karena membandingkan dua kolom yang diisi pada waktu berbeda.
        /// </summary>
        public Guid? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? PostedBy { get; set; }

        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// Wajib diisi saat menolak; penegakannya di service.
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Jurnal yang dikoreksi. Kosong bila jurnal biasa.
        /// </summary>
        public Guid? ReversalOfJournalId { get; set; }

        /// <summary>
        /// Kosong bila bukan koreksi.
        /// </summary>
        public JournalCorrectionType? CorrectionType { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public AccJournalType? JournalType { get; set; }

        public AccAccountingPeriod? AccountingPeriod { get; set; }

        public AccJournal? ReversalOfJournal { get; set; }

        public ICollection<AccJournalLine> Lines { get; set; } = new List<AccJournalLine>();

        public ICollection<AccJournalApproval> Approvals { get; set; }
            = new List<AccJournalApproval>();
    }
}
