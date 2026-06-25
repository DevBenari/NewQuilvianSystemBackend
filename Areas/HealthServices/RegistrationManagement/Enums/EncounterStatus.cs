using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum EncounterStatus
    {
        [Display(Name = "Draft")]
        Draft = 0,

        [Display(Name = "Terdaftar")]
        Registered = 1,

        [Display(Name = "Masuk Antrean")]
        Queued = 2,

        [Display(Name = "Menunggu Perawat")]
        WaitingForNurse = 3,

        [Display(Name = "Screening Perawat")]
        InNurseScreening = 4,

        [Display(Name = "Menunggu Dokter")]
        WaitingForDoctor = 5,

        [Display(Name = "Sedang Konsultasi")]
        InConsultation = 6,

        [Display(Name = "Konsultasi Selesai")]
        ConsultationCompleted = 7,

        [Display(Name = "Proses Billing")]
        Billing = 8,

        [Display(Name = "Selesai")]
        Completed = 9,

        [Display(Name = "Dibatalkan")]
        Cancelled = 10,

        [Display(Name = "Tidak Hadir")]
        NoShow = 11
    }
}
