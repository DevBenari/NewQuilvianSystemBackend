namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Antarmuka untuk pendaftaran event outbox integrasi Rawat Inap ke Billing dalam satu scope transaksi DbContext.
    /// </summary>
    public interface IInpIntegrationOutboxService
    {
        /// <summary>
        /// Mendaftarkan event integrasi ke dalam tabel outbox lokal.
        /// </summary>
        /// <param name="eventType">Jenis event bisnis, e.g. ADMISSION_CONFIRMED, BED_OCCUPIED, OCCUPANCY_CORRECTED, BED_RELEASED.</param>
        /// <param name="idempotencyKey">Kunci keunikan gabungan: SourceDomain:SourceType:SourceDetailId:Version.</param>
        /// <param name="sourceDomain">Domain asal, e.g. INPATIENT.</param>
        /// <param name="sourceType">Tipe entitas pelayanan, e.g. ADMISSION, ROOM_STAY, DISCHARGE.</param>
        /// <param name="sourceDetailId">ID unik entitas sumber.</param>
        /// <param name="payload">Objek payload event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task EnqueueEventAsync(
            string eventType,
            string idempotencyKey,
            string sourceDomain,
            string sourceType,
            string sourceDetailId,
            object payload,
            CancellationToken cancellationToken = default);
    }
}
