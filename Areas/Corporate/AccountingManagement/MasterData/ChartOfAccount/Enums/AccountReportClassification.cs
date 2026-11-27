using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums
{
    public enum AccountReportClassification
    {
        [Display(Name = "Lancar")]
        Current = 1,

        [Display(Name = "Tidak Lancar")]
        NonCurrent = 2
    }
}
