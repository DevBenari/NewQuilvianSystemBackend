using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Asal pengisian rincian rujukan (<c>RJ-DOC-DEC-076</c>).</summary>
    public enum ReferralCaptureSource
    {
        [Display(Name = "Petugas")]
        Staff = 1,

        [Display(Name = "Kiosk")]
        Kiosk = 2,

        /// <summary>Diisi pasien di halaman Pendaftaran Pasien Online (kiosk Amendment PO, KSK-DSN-PO-014).</summary>
        [Display(Name = "Pendaftaran Online")]
        OnlineRegistration = 3
    }
}
