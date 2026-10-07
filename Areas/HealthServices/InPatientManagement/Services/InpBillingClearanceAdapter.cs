using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Implementasi pembaca status kasir dari <see cref="IInpatientClearanceService.GetLatestStatusAsync"/>
    /// milik Billing, dengan pertahanan fail-safe yang sama dengan <see cref="InpBillingDepositAdapter"/>.
    /// </summary>
    public sealed class InpBillingClearanceAdapter : IInpBillingClearanceAdapter
    {
        private readonly IInpatientClearanceService _clearanceService;
        private readonly ILogger<InpBillingClearanceAdapter> _logger;

        public InpBillingClearanceAdapter(
            IInpatientClearanceService clearanceService,
            ILogger<InpBillingClearanceAdapter> logger)
        {
            _clearanceService = clearanceService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<InpClearanceReadResult> GetStatusAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
            {
                return new InpClearanceReadResult { IsReadable = false };
            }

            try
            {
                var view = await _clearanceService.GetLatestStatusAsync(encounterId, cancellationToken);

                return new InpClearanceReadResult
                {
                    IsReadable = true,
                    Status = view.Status,
                    EvaluatedAt = view.EvaluatedAt,
                    InvoiceStatus = view.InvoiceStatus,
                    Reasons = view.Reasons
                        .Select(x => new InpClearanceReasonItem { Code = x.Code, Label = x.Label })
                        .ToList()
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Gagal baca tidak pernah dianggap CLEARED; keputusan diserahkan pemanggil.
                _logger.LogError(ex, "Status kasir untuk kunjungan {EncounterId} tidak dapat dibaca dari Billing.", encounterId);
                return new InpClearanceReadResult { IsReadable = false };
            }
        }
    }
}
