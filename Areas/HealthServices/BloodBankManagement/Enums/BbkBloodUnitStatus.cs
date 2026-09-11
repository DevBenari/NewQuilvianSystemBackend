using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Sembilan keadaan yang dapat disandang satu kantong darah operasional, sesuai
    /// <c>contracts/state-transition-matrix.md</c> bagian 3.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kantong lahir <see cref="Received"/> — belum tersimpan, belum dapat dialokasikan</b>
    /// (<c>DEC-BD-036</c>). Gerbang penyimpanan wajib dilewati lebih dulu: <see cref="Received"/>
    /// → <see cref="Stored"/> → <see cref="Available"/>. Lompatan langsung dari
    /// <see cref="Received"/> ke <see cref="Allocated"/> tidak sah (<c>INV-BD-025</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Seluruh nilai sudah didefinisikan di sini, tetapi baru satu yang dapat dihasilkan.</b>
    /// Slice <c>BE-BD-004</c> hanya melahirkan kantong <see cref="Received"/>. Perpindahan ke
    /// status lain lahir bersama task pemiliknya: penyimpanan pada <c>BE-BD-015</c>, alokasi pada
    /// <c>BE-BD-006</c>, pemberian pada <c>BE-BD-007</c>/<c>BE-BD-008</c>, dan penyelesaian
    /// <see cref="PendingReview"/> pada <c>BE-BD-009</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Kantong berlebih bukan status tersendiri.</b> Ia tetap kantong biasa dengan penanda
    /// <c>IsExcess = true</c>; entity atau status per keadaan dilarang (<c>DEC-BD-025</c>).
    /// </para>
    /// </remarks>
    public enum BbkBloodUnitStatus
    {
        /// <summary>Diterima fisik dari PMI, belum punya lokasi penyimpanan.</summary>
        [Display(Name = "Diterima")]
        Received = 0,

        /// <summary>Sudah ditempatkan pada lokasi penyimpanan yang aktif.</summary>
        [Display(Name = "Tersimpan")]
        Stored = 1,

        /// <summary>Masuk stok yang boleh dialokasikan.</summary>
        [Display(Name = "Tersedia")]
        Available = 2,

        /// <summary>Terikat pada satu baris kebutuhan order.</summary>
        [Display(Name = "Dialokasikan")]
        Allocated = 3,

        /// <summary>Sudah diberikan kepada pasien. Terminal.</summary>
        [Display(Name = "Diberikan")]
        Issued = 4,

        /// <summary>Menunggu keputusan manusia — misalnya kantong berlebih.</summary>
        [Display(Name = "Menunggu keputusan")]
        PendingReview = 5,

        /// <summary>Dialihkan ke pasien lain lewat penyelesaian.</summary>
        [Display(Name = "Dialihkan")]
        Reallocated = 6,

        /// <summary>Dikembalikan kepada PMI. Terminal.</summary>
        [Display(Name = "Dikembalikan ke PMI")]
        ReturnedToProvider = 7,

        /// <summary>Dinyatakan tidak layak pakai. Terminal.</summary>
        [Display(Name = "Tidak layak")]
        NotUsable = 8
    }
}
