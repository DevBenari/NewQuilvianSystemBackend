namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Ringkasan posisi deposit dan tagihan episode dari Billing untuk kebutuhan modul Rawat Inap.
    /// Memastikan Rawat Inap membaca data otoritatif Billing tanpa menghitung ulang (BE-RWI-071, BE-RWI-072).
    /// </summary>
    public class InpEpisodeDepositSummaryDto
    {
        public Guid EpisodeId { get; set; }
        public Guid EncounterId { get; set; }
        public bool HasDepositAccount { get; set; }
        public bool IsPolicyRequired { get; set; }
        public decimal MinimumPolicyAmount { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PolicyShortfallAmount { get; set; }
        public decimal FinalBillAmount { get; set; }
        public decimal FinalBillShortfallAmount { get; set; }
        public decimal OutstandingTopUp { get; set; }
        public int FollowUpIntervalDays { get; set; }

        /// <summary>
        /// Menunjukkan apakah data posisi deposit dari Billing berhasil diakses.
        /// Jika false, sistem tidak boleh mengasumsikan nol atau lunas.
        /// </summary>
        public bool IsDataAvailable { get; set; } = true;

        /// <summary>Alasan bila data ringkasan Billing tidak dapat diakses.</summary>
        public string? UnavailableReason { get; set; }
    }

    /// <summary>
    /// Adapter untuk membaca posisi deposit dan tagihan episode dari BillingManagement.
    /// Menyediakan graceful degradation / fail-safe bila modul billing belum tersedia atau terjadi galat.
    /// </summary>
    public interface IInpBillingDepositAdapter
    {
        /// <summary>
        /// Mengambil ringkasan posisi deposit per episode rawat inap.
        /// </summary>
        Task<InpEpisodeDepositSummaryDto> GetDepositSummaryAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default);
    }
}
