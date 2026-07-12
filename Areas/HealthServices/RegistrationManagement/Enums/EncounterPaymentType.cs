using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum EncounterPaymentType
    {
        [Display(Name = "Tunai")]
        Cash = 1,

        [Display(Name = "Asuransi")]
        Insurance = 2
    }
}
