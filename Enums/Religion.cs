using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Enums
{
    public enum Religion
    {
        [Display(Name = "Tidak diketahui")]
        Unknown = 0,

        [Display(Name = "Islam")]
        Islam = 1,

        [Display(Name = "Kristen Protestan")]
        ProtestantChristian = 2,

        [Display(Name = "Katolik")]
        CatholicChristian = 3,

        [Display(Name = "Hindu")]
        Hindu = 4,

        [Display(Name = "Buddha")]
        Buddhist = 5,

        [Display(Name = "Konghucu")]
        Confucian = 6,

        [Display(Name = "Lainnya")]
        Other = 99
    }
}
