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

        public DateTime? EffectiveStartDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public AccChartOfAccount? ParentAccount { get; set; }

        public ICollection<AccChartOfAccount> ChildAccounts { get; set; }
            = new List<AccChartOfAccount>();
    }
}
