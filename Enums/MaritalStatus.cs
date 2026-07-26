using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Enums
{
    public enum MaritalStatus
    {
        [Display(Name = "Tidak diketahui")]
        Unknown = 0,

        [Display(Name = "Belum menikah")]
        Single = 1,

        [Display(Name = "Menikah")]
        Married = 2,

        [Display(Name = "Cerai hidup")]
        Divorced = 3,

        [Display(Name = "Cerai mati")]
        Widowed = 4,

        [Display(Name = "Berpisah")]
        Separated = 5
    }
}
