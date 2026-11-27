using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums
{
    public enum FinancialRatioUnit
    {
        [Display(Name = "Kali")]
        Times = 1,

        [Display(Name = "Persen")]
        Percent = 2
    }
}
