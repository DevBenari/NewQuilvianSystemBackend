using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Pihak yang menandatangani General Consent Rawat Jalan (<c>RJ-REG-REV-BE-007</c>,
    /// <c>RJ-REG-DEC-012</c>).
    /// </summary>
    /// <remarks>
    /// Pilihannya sama dengan persetujuan Rawat Inap (<c>BE-RWI-205</c>) supaya kedua layar
    /// memakai istilah yang sama. API menerima dan mengirim namanya (<c>"Self"</c>,
    /// <c>"Parent"</c>, ...), bukan angkanya.
    /// </remarks>
    public enum GeneralConsentSignerType
    {
        [Display(Name = "Pasien Sendiri")]
        Self = 1,

        [Display(Name = "Orang Tua")]
        Parent = 2,

        [Display(Name = "Wali yang Sah")]
        Guardian = 3,

        [Display(Name = "Suami / Istri")]
        Spouse = 4,

        [Display(Name = "Keluarga Lainnya")]
        Family = 5
    }
}
