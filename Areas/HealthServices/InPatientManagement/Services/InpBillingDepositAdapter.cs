using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Implementasi adapter pembaca posisi deposit dan tagihan episode dari BillingDepositService.
    /// Memenuhi kebutuhan integrasi BE-RWI-071 dan BE-RWI-072 dengan pertahanan fail-safe.
    /// </summary>
    public class InpBillingDepositAdapter : IInpBillingDepositAdapter
    {
        private readonly BillingDepositService _billingDepositService;
        private readonly ILogger<InpBillingDepositAdapter> _logger;

        public InpBillingDepositAdapter(
            BillingDepositService billingDepositService,
            ILogger<InpBillingDepositAdapter> logger)
        {
            _billingDepositService = billingDepositService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<InpEpisodeDepositSummaryDto> GetDepositSummaryAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            if (episodeId == Guid.Empty)
            {
                return new InpEpisodeDepositSummaryDto
                {
                    EpisodeId = episodeId,
                    IsDataAvailable = false,
                    UnavailableReason = "EpisodeId tidak valid (Guid kosong)."
                };
            }

            try
            {
                var summary = await _billingDepositService.GetEpisodeDepositSummaryAsync(
                    episodeId,
                    cancellationToken);

                return new InpEpisodeDepositSummaryDto
                {
                    EpisodeId = summary.EpisodeId,
                    EncounterId = summary.EncounterId,
                    HasDepositAccount = summary.HasDepositAccount,
                    IsPolicyRequired = summary.IsPolicyRequired,
                    MinimumPolicyAmount = summary.MinimumPolicyAmount,
                    TotalReceived = summary.TotalReceived,
                    TotalAllocated = summary.TotalAllocated,
                    TotalRefunded = summary.TotalRefunded,
                    AvailableBalance = summary.AvailableBalance,
                    PolicyShortfallAmount = summary.PolicyShortfallAmount,
                    FinalBillAmount = summary.FinalBillAmount,
                    FinalBillShortfallAmount = summary.FinalBillShortfallAmount,
                    OutstandingTopUp = summary.OutstandingTopUp,
                    FollowUpIntervalDays = summary.FollowUpIntervalDays,
                    IsDataAvailable = true
                };
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Data ringkasan deposit untuk episode {EpisodeId} tidak ditemukan di Billing.", episodeId);
                return new InpEpisodeDepositSummaryDto
                {
                    EpisodeId = episodeId,
                    IsDataAvailable = false,
                    UnavailableReason = $"Data deposit episode tidak ditemukan di Billing: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengakses layanan Billing untuk episode {EpisodeId}.", episodeId);
                return new InpEpisodeDepositSummaryDto
                {
                    EpisodeId = episodeId,
                    IsDataAvailable = false,
                    UnavailableReason = $"Layanan Billing tidak dapat diakses: {ex.Message}"
                };
            }
        }
    }
}
