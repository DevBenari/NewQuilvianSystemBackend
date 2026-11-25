using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums
{
    public enum FinancialStatementRowKind
    {
        [Display(Name = "Akun")]
        Account = 1,

        [Display(Name = "Akun Induk")]
        Group = 2,

        [Display(Name = "Baris Hitungan")]
        Computed = 3
    }
}
