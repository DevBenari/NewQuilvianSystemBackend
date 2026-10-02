using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Kontrak layanan gerbang pemulangan rawat inap: menangani webhook clearance kasir,
    /// auto-reblock, otorisasi supervisor override darurat, dan konfirmasi kepulangan fisik.
    /// </summary>
    public interface IInpatientClearanceGateService
    {
        /// <summary>
        /// Memproses sinyal kelayakan kasir dari webhook (ClearanceApproved / ClearanceRevoked dengan Auto-Reblock).
        /// </summary>
        Task<InpEpisodeOperationResult> HandleClearanceSignalAsync(
            Guid episodeId,
            ClearanceSignalWebhookDto webhookDto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengeksekusi supervisor override darurat untuk pemulangan pasien rujukan/kritis saat clearance belum disetujui.
        /// </summary>
        Task<InpEpisodeOperationResult> ExecuteSupervisorOverrideAsync(
            Guid episodeId,
            SupervisorOverrideRequestDto request,
            Guid actorUserId,
            bool isSupervisor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengonfirmasi pelepasan fisik pasien dari bangsal, menutup jam hunian bed presisi, dan menerbitkan event BED_RELEASED.
        /// </summary>
        Task<InpEpisodeOperationResult> ConfirmPhysicalDischargeAsync(
            Guid episodeId,
            ConfirmPhysicalDischargeRequestDto request,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
