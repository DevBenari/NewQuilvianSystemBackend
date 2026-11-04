using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Siklus pelayanan tiket antrean admisi Rawat Jalan (blueprint Cetak Antrean Admisi v1.1
    /// bagian 7). Terpisah dari <see cref="QueueStatus"/> milik antrean poliklinik internal dan
    /// dari <see cref="AdmissionTicketPrintStatus"/>.
    /// </summary>
    public enum AdmissionTicketStatus
    {
        [Display(Name = "Menunggu")]
        Waiting = 1,

        [Display(Name = "Dipanggil")]
        Called = 2,

        [Display(Name = "Dilayani")]
        Serving = 3,

        [Display(Name = "Selesai")]
        Completed = 4,

        [Display(Name = "Dilewati")]
        Skipped = 5,

        [Display(Name = "Dibatalkan")]
        Cancelled = 6
    }
}
