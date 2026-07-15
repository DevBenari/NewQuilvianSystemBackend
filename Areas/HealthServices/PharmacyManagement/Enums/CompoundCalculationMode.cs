using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum CompoundCalculationMode
    {
        [Display(Name = "Legacy / Jumlah Sumber")]
        LegacySourceUnit = 0,

        [Display(Name = "Dosis Terbagi")]
        DividedDose = 1,

        [Display(Name = "Jumlah Akhir Berat")]
        FinalWeight = 2,

        [Display(Name = "Jumlah Akhir Volume")]
        FinalVolume = 3,

        [Display(Name = "Manual")]
        Manual = 99
    }

}
