using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Jenis pasien pada langkah Jenis Pasien, sama dengan menu Pendaftaran Pasien RJ (<c>KSK-DEC-029</c>).</summary>
    public enum OnlineRegistrationPatientType
    {
        [Display(Name = "Pribadi")]
        Personal = 1,

        [Display(Name = "Asuransi")]
        Insurance = 2,

        [Display(Name = "Rujukan")]
        Referral = 3
    }
}
