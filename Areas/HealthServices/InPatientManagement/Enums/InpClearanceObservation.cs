using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Kosakata jejak status kasir yang <b>diamati</b> Rawat Inap saat keluar ruangan dan saat
    /// penutupan episode — kontrak <c>integrasi-billing</c> <c>1.1.0</c> bagian 9.7.
    /// </summary>
    /// <remarks>
    /// Ini bukan sumber status kasir. Status izin kasir hanya disimpan di Billing
    /// (<c>BilInpatientClearanceHandoff</c>, <c>INV-RWF-01</c>). Nilai di sini dipetakan dari
    /// kosakata Billing (<c>CLEARED</c>, <c>PENDING</c>, <c>BLOCKED</c>, <c>REVOKED</c>), ditambah
    /// <see cref="Unreadable"/> milik Rawat Inap untuk keadaan Billing tidak dapat dibaca.
    /// Dilarang dibaca untuk mengambil keputusan gerbang.
    /// </remarks>
    public enum InpClearanceObservation
    {
        [Display(Name = "Disetujui kasir")]
        Cleared = 1,

        [Display(Name = "Menunggu kasir")]
        Pending = 2,

        [Display(Name = "Ditahan kasir")]
        Blocked = 3,

        [Display(Name = "Izin kasir dicabut")]
        Revoked = 4,

        [Display(Name = "Status kasir tidak dapat dibaca")]
        Unreadable = 9
    }
}
