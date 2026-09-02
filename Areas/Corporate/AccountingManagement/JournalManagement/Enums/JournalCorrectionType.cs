using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums
{
    /// <summary>
    /// Cara mengoreksi jurnal yang sudah disahkan. Keduanya sah, dan pilihannya bergantung pada
    /// jenis kesalahannya.
    ///
    /// <see cref="FullReversal"/> dipakai ketika akun atau pihaknya yang salah: seluruh baris
    /// dibalik, lalu jurnal yang benar dibuat ulang. <see cref="Adjustment"/> dipakai ketika
    /// hanya nominalnya yang salah: cukup mencatat selisihnya.
    ///
    /// Contoh: beban listrik Rp 12.000.000 yang keliru masuk akun beban air dikoreksi dengan
    /// <see cref="FullReversal"/>. Beban listrik yang tercatat Rp 12.000.000 padahal seharusnya
    /// Rp 12.500.000 cukup dikoreksi dengan <see cref="Adjustment"/> sebesar Rp 500.000.
    ///
    /// Apa pun caranya, jurnal asal tidak pernah berubah (`ACC-DEC-006`).
    ///
    /// Nilai integer mengikuti contract `ACC-STATE-0.1`.
    /// </summary>
    public enum JournalCorrectionType
    {
        [Display(Name = "Pembalikan Penuh")]
        FullReversal = 1,

        [Display(Name = "Jurnal Penyesuaian")]
        Adjustment = 2
    }
}
