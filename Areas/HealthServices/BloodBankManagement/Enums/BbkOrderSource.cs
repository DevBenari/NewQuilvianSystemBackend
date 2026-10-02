using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Dari mana satu order darah masuk ke sistem (<c>DEC-BD-006</c>).
    /// </summary>
    /// <remarks>
    /// <b>Perbedaannya bukan kosmetik.</b> Order <see cref="Electronic"/> dibuat sendiri oleh
    /// unit pelayanan yang berwenang, sehingga pelaku input adalah pengguna terautentikasi yang
    /// mengirim permintaannya. Order <see cref="Manual"/> diinput petugas Bank Darah atas nama
    /// unit lain — biasanya karena ordernya datang di kertas — sehingga
    /// <c>BbkBloodOrder.InputByUserId</c> menjadi <b>wajib</b> dan seluruh rujukannya wajib
    /// lengkap (<c>VAL-BD-010</c>).
    /// </remarks>
    public enum BbkOrderSource
    {
        /// <summary>Dibuat unit pelayanan lewat sistem.</summary>
        [Display(Name = "Elektronik")]
        Electronic = 0,

        /// <summary>Diinput petugas Bank Darah dari order kertas.</summary>
        [Display(Name = "Manual")]
        Manual = 1
    }
}
