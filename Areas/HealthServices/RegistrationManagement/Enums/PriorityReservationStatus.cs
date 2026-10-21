using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Status Reservasi Prioritas (rawat-jalan Amendment RP, state-transition-matrix RP-A).
    /// Status 3 s.d. 6 adalah status akhir dan tidak dapat dibuka kembali (INV-RP-07).
    /// </summary>
    public enum PriorityReservationStatus
    {
        [Display(Name = "Menunggu Kedatangan")]
        AwaitingArrival = 1,

        [Display(Name = "Sudah Tiba")]
        Arrived = 2,

        [Display(Name = "Menjadi Kunjungan")]
        Converted = 3,

        [Display(Name = "Dibatalkan")]
        Cancelled = 4,

        [Display(Name = "Tidak Datang")]
        NoShow = 5,

        [Display(Name = "Didaftarkan lewat Jalur Lain")]
        RegisteredElsewhere = 6
    }
}
