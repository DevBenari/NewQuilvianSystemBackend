using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums
{
    /// <summary>
    /// Jenis tindakan yang tercatat pada riwayat penutupan sebuah periode akuntansi.
    ///
    /// Bentuknya meniru <c>JournalApprovalAction</c> milik jurnal, dan alasannya sama: riwayat ini
    /// adalah data bisnis yang ditampilkan kepada pengguna, berbeda dari log teknis yang ditulis
    /// <c>LoggerService</c>. Barisnya tidak pernah diubah maupun dihapus, karena ia menjawab
    /// pertanyaan audit "siapa menyatakan angka bulan ini final, dan kapan".
    ///
    /// <see cref="Rejected"/> wajib menyertakan alasan; penegakannya di service.
    ///
    /// Nilai integer mengikuti contract <c>ACC-STATE-0.2</c>.
    /// </summary>
    public enum PeriodClosingAction
    {
        [Display(Name = "Diajukan")]
        Submitted = 1,

        [Display(Name = "Disetujui")]
        Approved = 2,

        [Display(Name = "Ditolak")]
        Rejected = 3
    }
}
