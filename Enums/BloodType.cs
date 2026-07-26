using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Enums
{
    public enum BloodType
    {
        [Display(Name = "Tidak diketahui")]
        Unknown = 0,

        [Display(Name = "A Positif")]
        APositive = 1,

        [Display(Name = "A Negatif")]
        ANegative = 2,

        [Display(Name = "B Positif")]
        BPositive = 3,

        [Display(Name = "B Negatif")]
        BNegative = 4,

        [Display(Name = "AB Positif")]
        ABPositive = 5,

        [Display(Name = "AB Negatif")]
        ABNegative = 6,

        [Display(Name = "O Positif")]
        OPositive = 7,

        [Display(Name = "O Negatif")]
        ONegative = 8,

        [Display(Name = "Tidak diinformasikan")]
        NotDisclosed = 99
    }
}
