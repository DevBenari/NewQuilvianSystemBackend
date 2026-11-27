using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Pasien lama (sudah punya No. RM, dikenali NIK/No. RM + tanggal lahir, <c>KSK-DEC-031</c>) atau
    /// pasien baru (master dibuat saat check-in dari hasil pindai KTP/KIA, <c>KSK-DEC-032</c>, <c>048</c>).
    /// </summary>
    public enum OnlineRegistrationPatientMode
    {
        [Display(Name = "Pasien Lama")]
        Existing = 1,

        [Display(Name = "Pasien Baru")]
        New = 2
    }
}
