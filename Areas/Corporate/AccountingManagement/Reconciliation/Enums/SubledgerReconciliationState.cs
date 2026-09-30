using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Enums
{
    public enum SubledgerReconciliationState
    {
        [Display(Name = "Belum Berlaku")]
        BelumBerlaku = 1,

        [Display(Name = "Bersih")]
        Bersih = 2,

        [Display(Name = "Belum Bersih")]
        BelumBersih = 3
    }
}
