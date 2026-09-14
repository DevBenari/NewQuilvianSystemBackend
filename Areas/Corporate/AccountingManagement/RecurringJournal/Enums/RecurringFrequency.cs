using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Enums
{
    /// <summary>
    /// Seberapa sering sebuah template jurnal berulang menerbitkan jurnalnya.
    ///
    /// Rilis pertama Phase 2 hanya membangun <see cref="Bulanan"/> (<c>ACC-DEC-050</c>). Nilai
    /// triwulanan dan tahunan sengaja <b>tidak</b> ditambahkan sekarang: enum yang memuat nilai
    /// yang belum ada penanganannya akan lolos validasi lalu gagal diam-diam saat penjadwal
    /// mencoba memakainya.
    ///
    /// Nilai integer mengikuti contract <c>ACC-STATE-0.2</c>.
    /// </summary>
    public enum RecurringFrequency
    {
        [Display(Name = "Bulanan")]
        Bulanan = 1
    }
}
