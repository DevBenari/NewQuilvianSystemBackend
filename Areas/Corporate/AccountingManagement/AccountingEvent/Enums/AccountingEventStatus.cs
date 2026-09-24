using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Enums
{
    public enum AccountingEventStatus
    {
        [Display(Name = "Diterima")]
        Diterima = 1,

        [Display(Name = "Tertahan")]
        Tertahan = 2,

        [Display(Name = "Gagal")]
        Gagal = 3,

        [Display(Name = "Terjurnal")]
        Terjurnal = 4,

        [Display(Name = "Diabaikan")]
        Diabaikan = 5,

        [Display(Name = "Tercatat")]
        Tercatat = 6
    }
}
