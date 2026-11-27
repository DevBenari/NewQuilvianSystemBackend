using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums
{
    public enum CurrentAssetCategory
    {
        [Display(Name = "Kas dan Setara Kas")]
        CashAndCashEquivalents = 1,

        [Display(Name = "Investasi Jangka Pendek")]
        ShortTermInvestment = 2,

        [Display(Name = "Piutang")]
        Receivable = 3,

        [Display(Name = "Persediaan")]
        Inventory = 4,

        [Display(Name = "Aset Lancar Lainnya")]
        OtherCurrentAsset = 5
    }
}
