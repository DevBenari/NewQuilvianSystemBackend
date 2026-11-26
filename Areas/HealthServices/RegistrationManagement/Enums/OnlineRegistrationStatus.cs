using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Status Pendaftaran Pasien Online (kiosk Amendment PO, <c>contracts/state-transition-matrix.md</c>
    /// PO-S.1). Status 2 s.d. 5 adalah status akhir dan tidak dapat dibuka kembali (<c>INV-PO-07</c>).
    /// Hanya status 1 yang memakai kuota appointment (<c>INV-PO-02</c>).
    /// </summary>
    public enum OnlineRegistrationStatus
    {
        [Display(Name = "Menunggu Check-in")]
        AwaitingCheckIn = 1,

        [Display(Name = "Menjadi Kunjungan")]
        CheckedIn = 2,

        [Display(Name = "Didaftarkan lewat Jalur Lain")]
        RegisteredElsewhere = 3,

        [Display(Name = "Tidak Datang")]
        NoShow = 4,

        [Display(Name = "Dibatalkan")]
        Cancelled = 5
    }
}
