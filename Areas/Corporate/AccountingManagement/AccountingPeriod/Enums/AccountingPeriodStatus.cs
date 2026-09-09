using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums
{
    /// <summary>
    /// Keadaan sebuah periode akuntansi, yang menentukan jenis jurnal apa saja yang masih
    /// diterima periode itu.
    ///
    /// <see cref="SoftClosed"/> adalah masa tenggang tutup buku: jurnal umum sudah ditolak,
    /// tetapi jurnal penyesuaian dan pembalikan masih diterima. Inilah yang membedakannya dari
    /// <see cref="Closed"/>, yang menolak semuanya.
    ///
    /// Periode yang dibuka kembali dari <see cref="Closed"/> menjadi <see cref="SoftClosed"/>,
    /// bukan <see cref="Open"/>. Dengan begitu jurnal operasional baru tidak dapat masuk ke bulan
    /// yang laporannya sudah terbit (`ACC-DEC-028`).
    ///
    /// <see cref="PendingClosingApproval"/> adalah keadaan antara sejak `ACC-DEC-052`: Accounting
    /// Manager sudah mengajukan penutupan, dan periode menunggu persetujuan `Accounting Director`.
    /// Nilainya sengaja **ditambahkan di belakang**; menyisipkannya di tengah akan mengubah arti
    /// angka yang sudah tersimpan pada baris periode yang ada.
    ///
    /// Nilai integer mengikuti contract `ACC-STATE-0.2`.
    /// </summary>
    public enum AccountingPeriodStatus
    {
        [Display(Name = "Terbuka")]
        Open = 1,

        [Display(Name = "Tutup Sementara")]
        SoftClosed = 2,

        [Display(Name = "Tutup Permanen")]
        Closed = 3,

        [Display(Name = "Menunggu Persetujuan Penutupan")]
        PendingClosingApproval = 4
    }
}
