namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Antarmuka untuk pendaftaran event outbox integrasi Rawat Inap ke Billing dalam satu scope transaksi DbContext.
    /// </summary>
    /// <remarks>
    /// Sejak kontrak <c>integrasi-billing</c> <c>1.1.0</c> (<c>BE-RWI-151</c>, <c>INV-RWF-05</c>) isi
    /// pesan disusun dari parameter bertipe, bukan dari objek bebas. Tanda tangan lama yang menerima
    /// <c>object payload</c> dihapus, sehingga ruang, kelas, waktu hunian, tarif, dan status kasir
    /// tidak dapat lagi ikut terbawa ke Billing.
    /// </remarks>
    public interface IInpIntegrationOutboxService
    {
        /// <summary>
        /// Mendaftarkan satu ketukan pintu ke tabel outbox lokal, di dalam transaksi pemanggil.
        /// </summary>
        /// <param name="eventType"><c>ADMISSION_CONFIRMED</c>, <c>BED_OCCUPIED</c>, <c>OCCUPANCY_CORRECTED</c>, atau <c>BED_RELEASED</c>.</param>
        /// <param name="episodeId">Episode rawat inap.</param>
        /// <param name="encounterId">Kunjungan rawat inap; kunci invoice di Billing.</param>
        /// <param name="sourceType"><c>ADMISSION</c>, <c>ROOM_STAY</c>, atau <c>DISCHARGE</c>.</param>
        /// <param name="sourceId">Entitas sumber: episode untuk admisi, penempatan untuk hunian dan pelepasan bed.</param>
        /// <param name="version">Versi entitas sumber; bagian kunci idempotensi.</param>
        /// <param name="occurredAtUtc">Waktu kejadian di Rawat Inap.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Kunci idempotensi disusun di sini: <c>INPATIENT:&lt;SourceType&gt;:&lt;SourceId&gt;:&lt;Version&gt;</c>
        /// (<c>RWI-DEC-161</c> butir 4). Pemanggil wajib menyimpan perubahannya sendiri.
        /// </remarks>
        Task EnqueueEventAsync(
            string eventType,
            Guid episodeId,
            Guid encounterId,
            string sourceType,
            Guid sourceId,
            int version,
            DateTime occurredAtUtc,
            CancellationToken cancellationToken = default);
    }
}
