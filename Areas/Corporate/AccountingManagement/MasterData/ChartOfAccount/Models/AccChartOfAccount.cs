using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models
{
    [Table("AccChartOfAccount", Schema = "public")]
    public class AccChartOfAccount : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Badan hukum pemilik buku. Kode akun unik per badan hukum, bukan unik global
        /// (<c>ACC-DEC-037</c>).
        /// </summary>
        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Kode akun, contoh <c>1-1001</c>. Tidak boleh diubah setelah dipakai jurnal berstatus
        /// <c>Posted</c> (<c>ACC-DEC-023</c>); penegakannya di service, bukan di sini.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Akun induk pada tabel yang sama. Kosong bila akun tingkat pertama.
        /// </summary>
        public Guid? ParentAccountId { get; set; }

        /// <summary>
        /// Kedalaman susunan, 1 sampai 5.
        /// </summary>
        public int AccountLevel { get; set; } = 1;

        public AccountType AccountType { get; set; }

        /// <summary>
        /// Disimpan tersendiri, tidak diturunkan dari <see cref="AccountType"/>, agar akun kontra
        /// seperti Akumulasi Penyusutan dapat berjenis aset tetapi bersaldo normal kredit.
        /// </summary>
        public NormalBalance NormalBalance { get; set; }

        /// <summary>
        /// Menerima transaksi atau tidak. Wajib <c>false</c> bila akun ini punya anak
        /// (<c>ACC-DEC-022</c>); penegakannya di service, bukan di sini.
        /// </summary>
        public bool IsPostable { get; set; } = false;

        /// <summary>
        /// Tidak boleh dimatikan selama saldonya belum nol (<c>ACC-DEC-024</c>); penegakannya di
        /// service, bukan di sini.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Akun ini hanya boleh dicatat lewat kejadian akuntansi atau subledger, <b>bukan</b>
        /// lewat layar Jurnal Manual (<c>ACC-DEC-064</c>). Berlaku untuk Kas Kasir, Kas Kecil,
        /// Piutang, dan Hutang.
        ///
        /// <para>
        /// <b>Kenapa disimpan, bukan diturunkan.</b> Penandanya tidak dapat disimpulkan dari
        /// data lain: <c>Kas Kasir</c> dan <c>Piutang</c> sama-sama berjenis <c>Asset</c>,
        /// tetapi tidak setiap akun <c>Asset</c> adalah control account. Ini berbeda dari
        /// <c>RequiresCostCenter</c> yang justru ditolak menjadi kolom pada <c>ACC-DEC-019</c>
        /// karena memang dapat diturunkan dari <see cref="AccountType"/>.
        /// </para>
        ///
        /// <para>
        /// <b>Bawaannya <c>false</c>, dan itu mengikat.</b> Akun yang sudah ada di database
        /// tidak boleh berubah perilakunya hanya karena kolom ini ditambahkan.
        /// </para>
        ///
        /// Penolakan jurnal manualnya ditegakkan <c>AccJournalService</c> pada
        /// <c>BE-ACC-P2-012</c>, bukan di sini.
        /// </summary>
        public bool IsControlAccount { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public AccChartOfAccount? ParentAccount { get; set; }

        public ICollection<AccChartOfAccount> ChildAccounts { get; set; }
            = new List<AccChartOfAccount>();
    }
}
