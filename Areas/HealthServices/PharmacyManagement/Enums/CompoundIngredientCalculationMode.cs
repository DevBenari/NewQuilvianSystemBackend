using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum CompoundIngredientCalculationMode
    {
        [Display(Name = "Jumlah Sumber Legacy")]
        LegacySourceQuantity = 0,

        [Display(Name = "Target Dosis per Unit")]
        TargetDosePerUnit = 1,

        [Display(Name = "Target Persentase")]
        TargetPercentage = 2,

        [Display(Name = "Target Konsentrasi")]
        TargetConcentration = 3,

        [Display(Name = "q.s. ad Jumlah Akhir")]
        QuantitySufficient = 4,

        [Display(Name = "Jumlah Sumber Manual")]
        ManualSourceQuantity = 99
    }

}
