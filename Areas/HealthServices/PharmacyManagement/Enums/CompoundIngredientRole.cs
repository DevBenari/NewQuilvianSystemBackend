using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum CompoundIngredientRole
    {
        [Display(Name = "Bahan Aktif")]
        ActiveIngredient = 1,

        [Display(Name = "Bahan Tambahan")]
        Excipient = 2,

        [Display(Name = "Basis / Vehicle")]
        Vehicle = 3
    }

}
