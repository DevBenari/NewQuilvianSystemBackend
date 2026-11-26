using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Hasil pengiriman email berisi kode (<c>KSK-DEC-042</c>). Gagal kirim tidak membatalkan
    /// pendaftaran; pasien tetap melihat kode di layar dan dapat memakai Lupa Kode.
    /// </summary>
    public enum OnlineRegistrationEmailStatus
    {
        [Display(Name = "Tidak Diminta")]
        NotRequested = 0,

        [Display(Name = "Terkirim")]
        Sent = 1,

        [Display(Name = "Gagal")]
        Failed = 2
    }
}
