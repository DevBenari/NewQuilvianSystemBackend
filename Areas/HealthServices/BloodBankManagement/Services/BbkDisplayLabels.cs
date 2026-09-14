using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Label berbahasa Indonesia untuk nilai enum Bank Darah, dibaca dari
    /// <see cref="DisplayAttribute"/> pada enum-nya sendiri.
    /// </summary>
    /// <remarks>
    /// Satu sumber label: teksnya hidup di enum, bukan disalin ke setiap service. Nilai tanpa
    /// <see cref="DisplayAttribute"/> jatuh ke nama teknisnya.
    /// </remarks>
    internal static class BbkDisplayLabels
    {
        public static string Of<TEnum>(TEnum value)
            where TEnum : struct, Enum
            => typeof(TEnum)
                   .GetMember(value.ToString())
                   .FirstOrDefault()?
                   .GetCustomAttribute<DisplayAttribute>()?
                   .Name
               ?? value.ToString();
    }
}
