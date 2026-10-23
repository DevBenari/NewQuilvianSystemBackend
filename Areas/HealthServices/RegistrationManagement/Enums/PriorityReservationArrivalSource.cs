using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Asal konfirmasi kedatangan Reservasi Prioritas (RJ-PRI-DEC-012, RJ-PRI-DEC-023).</summary>
    public enum PriorityReservationArrivalSource
    {
        [Display(Name = "Link Pasien")]
        PatientLink = 1,

        [Display(Name = "Petugas")]
        Staff = 2
    }
}
