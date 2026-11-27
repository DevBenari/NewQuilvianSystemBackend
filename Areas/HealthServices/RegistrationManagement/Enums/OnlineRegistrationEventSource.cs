using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Asal satu kejadian pada riwayat pendaftaran online (<c>GATE-PO-09</c>).</summary>
    public enum OnlineRegistrationEventSource
    {
        [Display(Name = "Halaman Online")]
        PublicPage = 1,

        [Display(Name = "Kiosk")]
        Kiosk = 2,

        [Display(Name = "Petugas")]
        Staff = 3,

        [Display(Name = "Sistem")]
        System = 4
    }
}
