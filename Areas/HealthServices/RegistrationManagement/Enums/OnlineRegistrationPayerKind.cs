using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Jenis kartu penjamin milik pasien yang ditampilkan tersamar di halaman publik (<c>KSK-DSN-PO-012</c>).</summary>
    public enum OnlineRegistrationPayerKind
    {
        [Display(Name = "Asuransi")]
        Insurance = 1,

        [Display(Name = "Perusahaan")]
        Company = 2
    }
}
