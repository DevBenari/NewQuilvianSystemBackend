using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums
{
    /// <summary>
    /// Tahap sebuah jurnal sejak disusun sampai sah masuk buku besar.
    ///
    /// <see cref="Posted"/> bersifat permanen: jurnal yang sudah disahkan tidak boleh diubah
    /// maupun dihapus lewat jalur mana pun, termasuk penandaan `IsDelete`. Koreksi dilakukan
    /// lewat pembalikan atau jurnal penyesuaian. Aturan itu berasal dari `ACC-DEC-006` dan
    /// ditegakkan pada lapis service, bukan diserahkan pada kebiasaan pemanggil.
    ///
    /// Nilai integer mengikuti contract `ACC-STATE-0.1`.
    /// </summary>
    public enum JournalStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Menunggu Persetujuan")]
        PendingApproval = 2,

        [Display(Name = "Disetujui")]
        Approved = 3,

        [Display(Name = "Disahkan")]
        Posted = 4,

        [Display(Name = "Ditolak")]
        Rejected = 5
    }
}
