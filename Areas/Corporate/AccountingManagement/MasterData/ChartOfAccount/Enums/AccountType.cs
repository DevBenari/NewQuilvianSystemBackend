using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums
{
    /// <summary>
    /// Golongan akun pada daftar akun. Menentukan akun itu masuk laporan yang mana.
    ///
    /// Nilai integer mengikuti contract `ACC-STATE-0.1` dan disimpan apa adanya lewat
    /// `HasConversion&lt;int&gt;()`. Jangan mengubah nilainya setelah ada data tersimpan.
    /// </summary>
    public enum AccountType
    {
        [Display(Name = "Aset")]
        Asset = 1,

        [Display(Name = "Liabilitas")]
        Liability = 2,

        [Display(Name = "Ekuitas")]
        Equity = 3,

        [Display(Name = "Pendapatan")]
        Revenue = 4,

        [Display(Name = "Beban")]
        Expense = 5
    }
}
