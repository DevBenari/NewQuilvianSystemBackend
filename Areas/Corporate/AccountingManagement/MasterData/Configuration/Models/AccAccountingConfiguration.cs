using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Models
{
    /// <summary>
    /// Pengaturan akuntansi satu badan hukum. Untuk sekarang isinya hanya satu hal: akun laba
    /// ditahan yang dituju jurnal penutup tahun (<c>ACC-DEC-054</c>).
    ///
    /// <para>
    /// <b>Kenapa tabel tersendiri, bukan penanda pada daftar akun.</b> Akun laba ditahan tidak
    /// dapat diturunkan dari data lain. <c>AccountType == Equity</c> saja tidak cukup, karena
    /// satu badan hukum lazim punya lebih dari satu akun ekuitas — modal disetor, laba ditahan,
    /// dan laba tahun berjalan semuanya berjenis ekuitas. Menaruhnya sebagai penanda pada
    /// <c>AccChartOfAccount</c> akan menuntut penjagaan "hanya boleh satu akun bertanda per badan
    /// hukum", yang justru lebih rumit daripada satu baris pengaturan yang dijaga unique index.
    /// </para>
    ///
    /// <para>
    /// Aturan yang ditegakkan <b>service</b> pada <c>BE-ACC-P2-009</c>, bukan di sini:
    /// akun yang ditunjuk wajib berjenis <c>Equity</c>, wajib menerima transaksi
    /// (<c>IsPostable</c>), dan wajib milik badan hukum yang sama.
    /// </para>
    /// </summary>
    [Table("AccAccountingConfiguration", Schema = "public")]
    public class AccAccountingConfiguration : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Badan hukum pemilik pengaturan. <b>Satu pengaturan per badan hukum</b>, dijaga unique
        /// index (<c>ACC-DEC-037</c>).
        /// </summary>
        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Akun laba ditahan tujuan jurnal penutup tahun. Seluruh selisih pendapatan dikurangi
        /// beban masuk ke satu akun ini, tanpa pembagian ke akun lain lebih dahulu
        /// (<c>ACC-DEC-054</c>).
        /// </summary>
        [Required]
        public Guid RetainedEarningsAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public AccChartOfAccount? RetainedEarningsAccount { get; set; }
    }
}
