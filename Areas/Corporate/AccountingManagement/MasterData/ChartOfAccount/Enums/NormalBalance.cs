using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums
{
    /// <summary>
    /// Sisi saldo normal sebuah akun, yaitu sisi mana yang membuat saldonya bertambah.
    ///
    /// Disimpan sebagai kolom tersendiri dan tidak diturunkan dari <see cref="AccountType"/>,
    /// karena ada akun kontra yang menyimpang dari golongannya. Contoh: `Akumulasi Penyusutan`
    /// bergolongan aset tetapi bersaldo normal kredit, sebab ia mengurangi nilai aset.
    ///
    /// Nilai integer mengikuti contract `ACC-STATE-0.1`.
    /// </summary>
    public enum NormalBalance
    {
        [Display(Name = "Debit")]
        Debit = 1,

        [Display(Name = "Kredit")]
        Credit = 2
    }
}
