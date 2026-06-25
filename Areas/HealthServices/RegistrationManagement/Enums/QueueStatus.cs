using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum QueueStatus
    {
        [Display(Name = "Menunggu Perawat")]
        WaitingForNurse = 1,

        [Display(Name = "Dipanggil Perawat")]
        CalledByNurse = 2,

        [Display(Name = "Screening Perawat")]
        InNurseScreening = 3,

        [Display(Name = "Menunggu Dokter")]
        WaitingForDoctor = 4,

        [Display(Name = "Dipanggil Dokter")]
        CalledByDoctor = 5,

        [Display(Name = "Sedang Konsultasi")]
        InConsultation = 6,

        [Display(Name = "Dilewati")]
        Skipped = 7,

        [Display(Name = "Tidak Datang")]
        NoShow = 8,

        [Display(Name = "Dibatalkan")]
        Cancelled = 9,

        [Display(Name = "Selesai")]
        Completed = 10
    }
}
