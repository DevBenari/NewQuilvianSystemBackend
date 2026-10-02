using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Tiga keadaan yang dilalui satu pemeriksaan golongan darah Bank Darah, berurutan dan
    /// tidak pernah mundur.
    /// </summary>
    /// <remarks>
    /// <b>Keadaan "konflik" sengaja TIDAK ada di sini.</b> Konflik bukan status pemeriksaan —
    /// ia keadaan milik <i>pasien</i>, yang muncul ketika dua hasil tervalidasi berselisih.
    /// Menjadikannya nilai enum keempat akan memaksa satu pemeriksaan menyandang status yang
    /// sebenarnya menggambarkan hubungan antara dua pemeriksaan. Penandanya karena itu berupa
    /// <c>IsConflictHeld</c> pada
    /// <see cref="Models.BbkBloodGroupExam"/> (<c>02-backend-architecture.md</c> §F.6).
    ///
    /// <b>Tidak ada nilai batal/tolak.</b> Hasil yang sudah <c>Validated</c> tidak pernah
    /// ditimpa dan tidak pernah dibatalkan (<c>DEC-BD-026</c>); hasil yang keliru diselesaikan
    /// lewat pemeriksaan ulang, bukan lewat penghapusan (<c>DEC-BD-031</c>).
    /// </remarks>
    public enum BbkBloodGroupExamStatus
    {
        /// <summary>Sampel sudah diambil, hasil belum dicatat.</summary>
        [Display(Name = "Sampel diambil")]
        SampleTaken = 0,

        /// <summary>Hasil ABO dan Rhesus tercatat, belum divalidasi.</summary>
        /// <remarks>
        /// Hasil pada keadaan ini <b>tidak boleh dipakai untuk keperluan klinis</b>
        /// (<c>BD-DOM-09</c>). Ia belum menjadi golongan darah sah pasien.
        /// </remarks>
        [Display(Name = "Hasil tercatat")]
        ResultRecorded = 1,

        /// <summary>Hasil sudah divalidasi.</summary>
        /// <remarks>
        /// <c>Validated</c> menyatakan hasilnya sudah diperiksa manusia berwenang — <b>bukan</b>
        /// bahwa hasil itu yang berlaku bagi pasien. Yang berlaku ditandai
        /// <c>IsValidResult</c>, dan sebuah hasil tervalidasi dapat berhenti berlaku ketika
        /// muncul hasil tervalidasi lain yang berselisih (<c>BD-XINV-04</c>).
        /// </remarks>
        [Display(Name = "Tervalidasi")]
        Validated = 2
    }
}
