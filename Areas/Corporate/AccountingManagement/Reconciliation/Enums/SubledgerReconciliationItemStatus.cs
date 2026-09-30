using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Enums
{
    public enum SubledgerReconciliationItemStatus
    {
        [Display(Name = "Cocok")]
        Cocok = 1,

        [Display(Name = "Berselisih")]
        Berselisih = 2,

        [Display(Name = "Belum Diterima")]
        BelumDiterima = 3,

        [Display(Name = "Cut-off Bukan Akhir Periode")]
        CutOffBukanAkhirPeriode = 4
    }
}
