using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Layanan penegakan gerbang pemulangan rawat inap: memvalidasi status clearance kasir,
    /// mengeksekusi auto-reblock seketika saat tagihan susulan muncul, otorisasi supervisor override,
    /// dan konfirmasi kepulangan fisik dengan pelepasan bed serta penerbitan outbox BED_RELEASED.
    /// </summary>
    public class InpatientClearanceGateService : IInpatientClearanceGateService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpBedOccupancyService _bedOccupancyService;
        private readonly IInpIntegrationOutboxService _outboxService;

        public InpatientClearanceGateService(
            ApplicationDbContext dbContext,
            InpBedOccupancyService bedOccupancyService,
            IInpIntegrationOutboxService outboxService)
        {
            _dbContext = dbContext;
            _bedOccupancyService = bedOccupancyService;
            _outboxService = outboxService;
        }

        /// <inheritdoc />
        public async Task<InpEpisodeOperationResult> HandleClearanceSignalAsync(
            Guid episodeId,
            ClearanceSignalWebhookDto webhookDto,
            CancellationToken cancellationToken = default)
        {
            if (webhookDto == null)
            {
                return InpEpisodeOperationResult.Invalid("Payload webhook tidak boleh kosong.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null && Guid.TryParse(webhookDto.EncounterId, out var encounterGuid))
            {
                episode = await _dbContext.Set<InpEpisode>()
                    .FirstOrDefaultAsync(x => x.EncounterId == encounterGuid && !x.IsDelete, cancellationToken);
            }

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            var action = (webhookDto.Action ?? string.Empty).Trim().ToUpperInvariant();

            if (action == "CLEARANCE_APPROVED" || action == "CLEARANCEAPPROVED")
            {
                episode.ClearanceStatus = BillingClearanceStatus.Cleared;
                episode.ClearanceRevokedReason = null;
                episode.UpdateDateTime = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Sinyal persetujuan kelayakan kasir berhasil diterima. Status clearance berubah menjadi Cleared.");
            }
            else if (action == "CLEARANCE_REVOKED" || action == "CLEARANCEREVOKED")
            {
                // Auto-Reblock: kunci seketika tombol kepulangan fisik
                episode.ClearanceStatus = BillingClearanceStatus.Revoked;
                episode.ClearanceRevokedReason = !string.IsNullOrWhiteSpace(webhookDto.Reason)
                    ? webhookDto.Reason.Trim()
                    : "Pencabutan clearance oleh kasir akibat tagihan susulan belum diselesaikan.";
                episode.IsSupervisorOverridden = false;
                episode.UpdateDateTime = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Sinyal pencabutan kelayakan kasir berhasil diproses. Auto-Reblock aktif, tombol kepulangan fisik dikunci.");
            }
            else
            {
                return InpEpisodeOperationResult.Invalid(
                    $"Aksi sinyal clearance '{webhookDto.Action}' tidak dikenali. Gunakan CLEARANCE_APPROVED atau CLEARANCE_REVOKED.");
            }
        }

        /// <inheritdoc />
        public async Task<InpEpisodeOperationResult> ExecuteSupervisorOverrideAsync(
            Guid episodeId,
            SupervisorOverrideRequestDto request,
            Guid actorUserId,
            bool isSupervisor,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            // VAL-INT-004: Alasan supervisor override wajib diisi minimal 20 karakter
            if (string.IsNullOrWhiteSpace(request?.Reason) || request.Reason.Trim().Length < 20)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Alasan supervisor override wajib diisi minimal 20 karakter dengan menyebutkan kondisi darurat medis atau rumah sakit rujukan secara jelas.");
            }

            // VAL-INT-005: Wewenang Supervisor Bangsal & PIN otorisasi
            if (!isSupervisor || string.IsNullOrWhiteSpace(request?.SupervisorPin))
            {
                return InpEpisodeOperationResult.Forbidden(
                    "Otorisasi ditolak: Anda tidak memiliki hak wewenang Supervisor Rawat Inap atau PIN otorisasi yang dimasukkan salah.");
            }

            var now = DateTime.UtcNow;
            episode.IsSupervisorOverridden = true;
            episode.ClearanceStatus = BillingClearanceStatus.Overridden;
            episode.SupervisorOverrideReason = request.Reason.Trim();
            episode.SupervisorOverriddenByUserId = actorUserId;
            episode.SupervisorOverriddenAtUtc = now;
            episode.UpdateDateTime = now;
            episode.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpEpisodeOperationResult.Success(
                episode,
                "Supervisor override berhasil disahkan. Tombol pelepasan fisik pasien kini aktif atas izin darurat medis.");
        }

        /// <inheritdoc />
        public async Task<InpEpisodeOperationResult> ConfirmPhysicalDischargeAsync(
            Guid episodeId,
            ConfirmPhysicalDischargeRequestDto request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            // VAL-INT-001: Pelepasan fisik ditolak jika clearance kasir masih Pending atau Revoked tanpa otorisasi override
            if (episode.ClearanceStatus != BillingClearanceStatus.Cleared &&
                episode.ClearanceStatus != BillingClearanceStatus.Overridden &&
                !episode.IsSupervisorOverridden)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Pelepasan fisik pasien ditolak: Tagihan kasir belum disetujui (Clearance Pending/Revoked). Pasien hanya dapat dilepaskan setelah kasir menerbitkan persetujuan lunas atau melalui otorisasi Supervisor Override.",
                    episode);
            }

            var activePlacement = await _dbContext.Set<InpBedPlacement>()
                .FirstOrDefaultAsync(
                    x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete,
                    cancellationToken);

            if (activePlacement == null)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Pasien tidak memiliki catatan penempatan tempat tidur aktif yang sedang ditempati.",
                    episode);
            }

            var dischargeTime = request?.PhysicalDischargeDateTime ?? DateTime.UtcNow;
            var now = DateTime.UtcNow;

            if (dischargeTime > now.AddMinutes(5))
            {
                return InpEpisodeOperationResult.Invalid(
                    "Waktu kepulangan fisik tidak boleh mendahului waktu saat ini ke masa depan.");
            }

            // VAL-INT-008: Jam kepulangan fisik tidak boleh mendahului jam mulai hunian kamar terakhir
            if (dischargeTime < activePlacement.OccupancyStartAt)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Jam kepulangan fisik tidak valid: Waktu keluar fisik tidak boleh mendahului waktu pasien mulai menempati tempat tidur.",
                    episode);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Lepas tempat tidur aktif
                await _bedOccupancyService.ReleaseActivePlacementAsync(
                    episode.Id,
                    InpBedPlacementEndReason.PatientDeparted,
                    actorUserId,
                    dischargeTime,
                    cancellationToken);

                // Update kolom tracking kepergian fisik pada placement & episode
                activePlacement.PhysicallyLeftAt = dischargeTime;
                episode.PhysicallyLeftAt = dischargeTime;
                episode.PhysicallyLeftByUserId = actorUserId;
                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                // Terbitkan event BED_RELEASED via Transactional Outbox
                var idempotencyKey = $"INPATIENT:DISCHARGE:{episode.Id}:{activePlacement.Version}";
                var payload = new
                {
                    EpisodeId = episode.Id,
                    EncounterId = episode.EncounterId,
                    BedId = activePlacement.BedId,
                    RoomId = activePlacement.RoomId,
                    RoomClassId = activePlacement.RoomClassId,
                    OccupancyStartAt = activePlacement.OccupancyStartAt,
                    OccupancyEndAt = activePlacement.OccupancyEndAt ?? dischargeTime,
                    PhysicallyLeftAt = dischargeTime,
                    ClearanceStatus = episode.ClearanceStatus.ToString(),
                    IsSupervisorOverridden = episode.IsSupervisorOverridden,
                    SupervisorOverrideReason = episode.SupervisorOverrideReason,
                    ReleasedByUserId = actorUserId,
                    Notes = request?.Notes
                };

                await _outboxService.EnqueueEventAsync(
                    eventType: "BED_RELEASED",
                    idempotencyKey: idempotencyKey,
                    sourceDomain: "INPATIENT",
                    sourceType: "DISCHARGE",
                    sourceDetailId: episode.Id.ToString(),
                    payload: payload,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Pasien berhasil dipulangkan secara fisik. Jam hunian tempat tidur ditutup presisi dan event kepulangan telah dikirim ke Kasir.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
