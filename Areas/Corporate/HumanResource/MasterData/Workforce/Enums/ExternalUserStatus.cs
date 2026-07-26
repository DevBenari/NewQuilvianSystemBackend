using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Enums
{
    public enum ExternalUserStatus
    {
        [Display(Name = "Tidak diketahui")]
        Unknown = 0,

        [Display(Name = "Aktif")]
        Active = 1,

        [Display(Name = "Tidak aktif")]
        Inactive = 2,

        [Display(Name = "Menunggu persetujuan")]
        PendingApproval = 3,

        [Display(Name = "Ditangguhkan")]
        Suspended = 4,

        [Display(Name = "Masuk daftar hitam")]
        Blacklisted = 5,

        [Display(Name = "Kontrak berakhir")]
        ContractEnded = 6,

        [Display(Name = "Akses kedaluwarsa")]
        AccessExpired = 7
    }
}
