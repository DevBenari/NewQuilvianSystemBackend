using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Enums
{
    public enum Gender
    {
        [Display(Name = "Tidak diketahui")]
        Unknown = 0,

        [Display(Name = "Laki-laki")]
        Male = 1,

        [Display(Name = "Perempuan")]
        Female = 2,

        [Display(Name = "Tidak diinformasikan")]
        NotDisclosed = 99
    }
}
