using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Lima keadaan yang dapat disandang satu permintaan darah ke PMI, sesuai
    /// <c>contracts/state-transition-matrix.md</c> bagian 2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><see cref="PartiallyFulfilled"/> dan <see cref="Fulfilled"/> tidak disetel tangan.</b>
    /// Keduanya lahir dari penerimaan kantong yang nyata: sisa permintaan dihitung dari kantong
    /// yang benar-benar diterima, dengan batas bawah nol (<c>INV-BD-017</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Terminal: <see cref="Fulfilled"/> dan <see cref="Cancelled"/>.</b>
    /// <see cref="ClosedEncounter"/> berbeda: ia tidak pernah kembali aktif, tetapi tetap
    /// menerima penerimaan susulan karena kantong yang sudah di jalan tetap datang
    /// (<c>DEC-BD-020</c>).
    /// </para>
    /// </remarks>
    public enum BbkProviderRequestStatus
    {
        /// <summary>Permintaan dicatat dan menunggu kiriman PMI.</summary>
        [Display(Name = "Diminta")]
        Requested = 0,

        /// <summary>Sebagian kantong yang diminta sudah diterima fisik.</summary>
        [Display(Name = "Diterima sebagian")]
        PartiallyFulfilled = 1,

        /// <summary>Seluruh kantong yang diminta sudah diterima. Sisa berhenti di nol. Terminal.</summary>
        [Display(Name = "Terpenuhi")]
        Fulfilled = 2,

        /// <summary>Permintaan dibatalkan dengan alasan terkendali. Terminal.</summary>
        [Display(Name = "Dibatalkan")]
        Cancelled = 3,

        /// <summary>
        /// Kunjungan asal berakhir sementara permintaan masih kurang. Ditutup administratif
        /// tanpa menghapus riwayat (<c>DEC-BD-020</c>).
        /// </summary>
        [Display(Name = "Ditutup — kunjungan berakhir")]
        ClosedEncounter = 4
    }
}
