namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Jenis resep menurut peruntukannya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BE-RWI-042</c>, <c>RWI-RULE-024</c>, <c>RWI-DEC-046</c>. Obat pulang menjadi jenis
    /// resep yang <b>eksplisit</b>, bukan disimpulkan dari waktu penulisan atau dari status
    /// perawatan: petugas farmasi harus dapat menyaringnya di layar mereka sendiri —
    /// <c>AC-CAP023-03</c>.
    /// </para>
    /// <para>
    /// Bawaannya <c>Routine</c> supaya seluruh baris resep yang sudah ada terbaca sebagai resep
    /// biasa dan tidak perlu disentuh.
    /// </para>
    /// </remarks>
    public enum PrescriptionOrderType
    {
        /// <summary>Resep biasa. Bawaan bagi seluruh baris yang sudah ada.</summary>
        Routine = 0,

        /// <summary>Resep harian selama pasien dirawat.</summary>
        Daily = 1,

        /// <summary>Obat yang dibawa pulang pasien.</summary>
        Discharge = 2
    }
}
