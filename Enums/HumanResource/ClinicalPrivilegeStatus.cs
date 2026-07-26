using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Enums.HumanResource
{
    public enum ClinicalPrivilegeStatus
    {
        [Display(Name = "Tidak berlaku")]
        NotApplicable = 0,

        [Display(Name = "Menunggu persetujuan")]
        Pending = 1,

        [Display(Name = "Aktif")]
        Active = 2,

        [Display(Name = "Ditangguhkan")]
        Suspended = 3,

        [Display(Name = "Dicabut")]
        Revoked = 4,

        [Display(Name = "Kedaluwarsa")]
        Expired = 5
    }

}
