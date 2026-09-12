using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Lima keadaan yang dapat disandang satu order darah, sesuai
    /// <c>contracts/state-transition-matrix.md</c> bagian 1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tiga di antaranya terminal</b> — <see cref="FullyFulfilled"/>,
    /// <see cref="Cancelled"/>, dan <see cref="Expired"/>. Order yang sudah
    /// <see cref="Expired"/> <b>tidak pernah</b> dihidupkan kembali (<c>ASM-BD-002</c>,
    /// <c>VAL-BD-004</c>): pasien yang masih membutuhkan darah dibuatkan order baru pada
    /// kunjungan yang berjalan.
    /// </para>
    ///
    /// <para>
    /// <b><see cref="PartiallyFulfilled"/> dan <see cref="FullyFulfilled"/> tidak disetel
    /// tangan.</b> Keduanya lahir dari pemberian kantong yang nyata, dan angka pemenuhannya
    /// dihitung dari transaksi — bukan dari kolom yang disunting (<c>BD-DOM-17</c>). Karena
    /// pemberian kantong baru lahir pada <c>BE-BD-006</c>/<c>BE-BD-007</c>, kedua nilai ini
    /// sudah didefinisikan di sini tetapi belum ada jalur yang menghasilkannya pada slice
    /// <c>BE-BD-003</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Tidak ada nilai "ditutup administratif".</b> Order yang kunjungannya berakhir
    /// menjadi <see cref="Expired"/>, dan sinyal berakhirnya kunjungan dibaca dari modul
    /// pemiliknya lewat <c>BbkEncounterStatusReader</c> (<c>DEC-BD-014</c>) — Bank Darah tidak
    /// pernah menulis ke sana.
    /// </para>
    /// </remarks>
    public enum BbkBloodOrderStatus
    {
        /// <summary>Order berlaku dan masih menunggu pemenuhan.</summary>
        [Display(Name = "Aktif")]
        Active = 0,

        /// <summary>Sebagian kantong yang diminta sudah diberikan.</summary>
        [Display(Name = "Terpenuhi sebagian")]
        PartiallyFulfilled = 1,

        /// <summary>Seluruh kantong yang diminta sudah diberikan. Terminal.</summary>
        [Display(Name = "Terpenuhi penuh")]
        FullyFulfilled = 2,

        /// <summary>
        /// Order dibatalkan dengan alasan terkendali oleh dokter peminta atau petugas BDRS.
        /// Terminal.
        /// </summary>
        /// <remarks>
        /// Pembatalan <b>selalu</b> menyimpan alasan berkategori, pelaku, dan waktu
        /// (<c>INV-BD-035</c>). Tidak ada pembatalan order tanpa audit.
        /// </remarks>
        [Display(Name = "Dibatalkan")]
        Cancelled = 3,

        /// <summary>Kunjungan asal berakhir sementara order belum terpenuhi. Terminal.</summary>
        [Display(Name = "Kedaluwarsa")]
        Expired = 4
    }
}
