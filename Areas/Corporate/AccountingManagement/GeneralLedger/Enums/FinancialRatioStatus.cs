using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums
{
    public enum FinancialRatioStatus
    {
        [Display(Name = "Dihitung")]
        Calculated = 1,

        [Display(Name = "N/A")]
        NotApplicable = 2,

        [Display(Name = "Tidak dapat dihitung")]
        Unavailable = 3,

        [Display(Name = "Tidak sah")]
        Invalid = 4
    }
}
