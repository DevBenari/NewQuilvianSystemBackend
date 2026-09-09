using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.DTOs
{
    /// <summary>
    /// Pengaturan akuntansi satu badan hukum. Untuk sekarang isinya satu hal: akun laba ditahan
    /// yang dituju jurnal penutup tahun (<c>ACC-DEC-054</c>).
    /// </summary>
    public class AccountingConfigurationResponse
    {
        /// <summary>
        /// Kosong bila badan hukum ini belum punya pengaturan sama sekali.
        /// </summary>
        public Guid? Id { get; set; }

        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// <b>Salah</b> bila akun laba ditahan belum ditetapkan. Layar memakai ini untuk
        /// membedakan "belum diisi" dari "gagal dibaca" — keduanya sangat berbeda artinya, dan
        /// belum diisi adalah keadaan yang wajar pada rumah sakit yang baru mulai memakai modul.
        /// </summary>
        public bool IsConfigured { get; set; }

        public Guid? RetainedEarningsAccountId { get; set; }

        public string? RetainedEarningsAccountCode { get; set; }

        public string? RetainedEarningsAccountName { get; set; }

        public AccountType? RetainedEarningsAccountType { get; set; }

        public bool IsActive { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// Menetapkan akun laba ditahan sebuah badan hukum.
    /// </summary>
    /// <remarks>
    /// Badan hukumnya diambil dari route, bukan dari badan permintaan — supaya tidak mungkin
    /// terjadi permintaan yang alamatnya satu badan hukum tetapi isinya badan hukum lain.
    /// </remarks>
    public class UpdateAccountingConfigurationRequest
    {
        [Required]
        public Guid RetainedEarningsAccountId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
