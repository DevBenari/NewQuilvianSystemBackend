using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>Jenis perubahan pada riwayat rujukan (<c>RJ-DOC-DEC-078</c>).</summary>
    public enum ReferralRevisionType
    {
        [Display(Name = "Dilengkapi")]
        Completed = 1,

        [Display(Name = "Dikoreksi")]
        Corrected = 2,

        [Display(Name = "Surat Ditambahkan")]
        DocumentAdded = 3,

        [Display(Name = "Surat Dihapus")]
        DocumentRemoved = 4
    }
}
