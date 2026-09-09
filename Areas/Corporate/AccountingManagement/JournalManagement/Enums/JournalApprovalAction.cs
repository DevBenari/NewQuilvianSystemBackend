using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums
{
    /// <summary>
    /// Jenis tindakan yang tercatat pada riwayat persetujuan sebuah jurnal.
    ///
    /// Riwayat ini adalah data bisnis yang ditampilkan kepada pengguna di layar rincian jurnal,
    /// berbeda dari log teknis yang ditulis `LoggerService`. Barisnya tidak pernah diubah maupun
    /// dihapus, karena ia menjawab pertanyaan audit "siapa menyetujui apa dan kapan".
    ///
    /// <see cref="Rejected"/> dan <see cref="Reversed"/> wajib menyertakan alasan.
    ///
    /// Nilai integer mengikuti contract `ACC-STATE-0.1`.
    /// </summary>
    public enum JournalApprovalAction
    {
        [Display(Name = "Diajukan")]
        Submitted = 1,

        [Display(Name = "Disetujui")]
        Approved = 2,

        [Display(Name = "Ditolak")]
        Rejected = 3,

        [Display(Name = "Disahkan")]
        Posted = 4,

        [Display(Name = "Dibalik")]
        Reversed = 5
    }
}
