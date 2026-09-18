using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Keadaan satu permintaan koreksi pencatatan pemberian (<c>DEC-BD-041</c>,
    /// <c>02-backend-architecture.md</c> §F.6).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Koreksi berlaku hanya pada <see cref="Approved"/></b> (<c>INV-BD-033</c>). Selama
    /// <see cref="Requested"/>, angka pemenuhan order tidak bergerak dan rekam tidak berubah.
    /// </para>
    /// <para>
    /// <b><see cref="Rejected"/> ada karena permintaan yang ditolak tetap terbaca.</b> Menghapusnya
    /// atau membiarkannya menggantung di <see cref="Requested"/> sama-sama menghilangkan fakta bahwa
    /// seseorang pernah menyatakan catatan itu keliru dan pemutus tidak sependapat.
    /// </para>
    /// <para>
    /// Keputusan bersifat sekali: <see cref="Approved"/> dan <see cref="Rejected"/> final
    /// (<c>VAL-BD-075</c>).
    /// </para>
    /// </remarks>
    public enum BbkCorrectionStatus
    {
        /// <summary>Diajukan petugas BDRS, menunggu keputusan Dokter BDRS. Belum berlaku.</summary>
        [Display(Name = "Menunggu persetujuan")]
        Requested = 0,

        /// <summary>Disetujui Dokter BDRS; sejak saat ini koreksi berlaku.</summary>
        [Display(Name = "Disetujui")]
        Approved = 1,

        /// <summary>Ditolak Dokter BDRS; rekam tidak berubah, permintaan tetap terbaca.</summary>
        [Display(Name = "Ditolak")]
        Rejected = 2
    }
}
