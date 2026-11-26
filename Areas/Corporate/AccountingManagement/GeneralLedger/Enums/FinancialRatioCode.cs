using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums
{
    public enum FinancialRatioCode
    {
        [Display(Name = "Current Ratio")]
        CurrentRatio = 1,

        [Display(Name = "Quick Ratio")]
        QuickRatio = 2,

        [Display(Name = "Cash Ratio")]
        CashRatio = 3,

        [Display(Name = "Net Profit Margin")]
        NetProfitMargin = 4,

        [Display(Name = "Return on Assets")]
        ReturnOnAssets = 5,

        [Display(Name = "Return on Equity")]
        ReturnOnEquity = 6,

        [Display(Name = "Debt to Asset Ratio")]
        DebtToAssetRatio = 7,

        [Display(Name = "Debt to Equity Ratio")]
        DebtToEquityRatio = 8
    }
}
