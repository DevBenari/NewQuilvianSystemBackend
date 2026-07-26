using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Enums.HumanResource
{
    public enum CredentialingStatus
    {
        [Display(Name = "Belum dimulai")]
        NotStarted = 0,

        [Display(Name = "Menunggu proses")]
        Pending = 1,

        [Display(Name = "Sedang ditinjau")]
        UnderReview = 2,

        [Display(Name = "Disetujui")]
        Approved = 3,

        [Display(Name = "Ditolak")]
        Rejected = 4,

        [Display(Name = "Kedaluwarsa")]
        Expired = 5,

        [Display(Name = "Ditangguhkan")]
        Suspended = 6,

        [Display(Name = "Dicabut")]
        Revoked = 7
    }
}
