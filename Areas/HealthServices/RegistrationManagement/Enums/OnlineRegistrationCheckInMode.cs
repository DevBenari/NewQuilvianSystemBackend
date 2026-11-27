using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Cara pasien menemukan pendaftaran online di kiosk (<c>KSK-DEC-033</c>, <c>040</c>).</summary>
    public enum OnlineRegistrationCheckInMode
    {
        [Display(Name = "Kode Pendaftaran")]
        Code = 1,

        [Display(Name = "Lupa Kode")]
        Identity = 2
    }
}
