namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan pengiriman satu tindakan keperawatan ke Billing — <c>BE-RWI-061</c>,
    /// <c>BE-RWI-062</c>, <c>INT-KEP-05</c>, <c>AC-CAP014-02</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Ini mesin status yang terpisah dari keadaan klinis, dan pemisahan itu yang paling
    /// penting di seluruh slice ini.</b> Menggabungkan keduanya adalah kesalahan yang paling
    /// mahal: catatan klinis bisa hilang karena masalah keuangan.
    /// </para>
    /// <para>
    /// <b>Contoh nyata.</b> Ns. Sari memasang infus pukul 02.00 saat sistem Billing sedang mati.
    /// Yang benar: catatan tindakan tersimpan <c>Recorded</c>, penanda pengiriman
    /// <c>Failed</c>, dan percobaan ulang dijalankan terpisah. Yang salah: tindakan ikut gagal
    /// tersimpan, sehingga pukul 08.00 tidak ada bukti infus pernah dipasang.
    /// </para>
    /// <para>
    /// <b><c>Pending</c> adalah keadaan wajar hari ini.</b> <c>BillingManagement</c> belum
    /// memiliki kemampuan transaksi yang menerima pemicu ini (<c>INT-KEP-05</c>), sehingga
    /// tindakan yang dapat ditagih menunggu di <c>Pending</c> dan tidak ada satu pun yang hilang.
    /// </para>
    /// </remarks>
    public enum NursingBillingDispatchStatus
    {
        /// <summary>Tindakan yang memang tidak dapat ditagih.</summary>
        NotApplicable = 0,

        /// <summary>Menunggu dikirim ke Billing.</summary>
        Pending = 1,

        /// <summary>Sudah terkirim dan diterima Billing.</summary>
        Dispatched = 2,

        /// <summary>Pengiriman gagal. Catatan klinisnya <b>tetap</b> tersimpan.</summary>
        Failed = 3
    }
}
