using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Status cetak tiket admisi di kiosk, terpisah dari status pelayanan (blueprint Cetak
    /// Antrean Admisi v1.1 bagian 7). Hanya kiosk yang melaporkannya.
    /// </summary>
    public enum AdmissionTicketPrintStatus
    {
        [Display(Name = "Belum Dikonfirmasi")]
        Unconfirmed = 1,

        [Display(Name = "Berhasil")]
        Succeeded = 2,

        [Display(Name = "Gagal")]
        Failed = 3
    }
}
