using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Pelaku sebuah kejadian pada riwayat Reservasi Prioritas.</summary>
    public enum PriorityReservationEventSource
    {
        [Display(Name = "Petugas")]
        Staff = 1,

        [Display(Name = "Link Pasien")]
        PatientLink = 2,

        [Display(Name = "Sistem")]
        System = 3
    }
}
