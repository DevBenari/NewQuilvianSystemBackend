using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums
{
    public enum LedgerSourceKind
    {
        [Display(Name = "Manual")]
        Manual = 1,

        [Display(Name = "Kejadian")]
        AccountingEvent = 2,

        [Display(Name = "Jurnal Berulang")]
        RecurringJournal = 3,

        [Display(Name = "Tutup Tahun")]
        YearEnd = 4,

        [Display(Name = "Koreksi")]
        Correction = 5
    }
}
