using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Penanda yang dipakai pasien lama untuk mengenali dirinya di halaman publik (<c>KSK-DEC-031</c>).</summary>
    public enum OnlineRegistrationIdentityType
    {
        [Display(Name = "NIK")]
        IdentityNumber = 1,

        [Display(Name = "No. Rekam Medis")]
        MedicalRecordNumber = 2
    }
}
