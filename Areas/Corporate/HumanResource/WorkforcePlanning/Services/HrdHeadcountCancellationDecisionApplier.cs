using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Penerap keputusan akhir workflow persetujuan T13 untuk pembatalan kursi permintaan tenaga kerja (BE-HRD-044).
    /// </summary>
    public sealed class HrdHeadcountCancellationDecisionApplier : IWorkforcePlanningDecisionApplier
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdPositionFormationService _formationService;
        private readonly ILogger<HrdHeadcountCancellationDecisionApplier> _logger;

        public HrdHeadcountCancellationDecisionApplier(
            ApplicationDbContext dbContext,
            IHrdPositionFormationService formationService,
            ILogger<HrdHeadcountCancellationDecisionApplier> logger)
        {
            _dbContext = dbContext;
            _formationService = formationService;
            _logger = logger;
        }

        public string ReferenceType => WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountCancellation;

        public async Task<WorkforcePlanningApplyResult> ApplyDecisionAsync(
            WorkforcePlanningDecisionContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cancellation = await _dbContext.HrdHeadcountRequestCancellations
                .Include(c => c.HeadcountRequest)
                    .ThenInclude(r => r!.Seats)
                .FirstOrDefaultAsync(c => !c.IsDelete && c.Id == context.ReferenceId, cancellationToken);

            if (cancellation == null)
            {
                _logger.LogWarning("Pengajuan pembatalan kursi {ReferenceId} tidak ditemukan untuk penerapan keputusan workflow.", context.ReferenceId);
                return WorkforcePlanningApplyResult.Unhandled($"Pengajuan pembatalan kursi {context.ReferenceId} tidak ditemukan.");
            }

            var previousStatus = cancellation.CancellationStatus.ToString();
            var now = DateTime.UtcNow;
            var normalizedStatus = (context.WorkflowStatus ?? string.Empty).Trim();

            if (string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "FinalApproved", StringComparison.OrdinalIgnoreCase))
            {
                // Idempoten (kriteria 5)
                if (cancellation.CancellationStatus == HeadcountCancellationStatus.Approved)
                {
                    _logger.LogInformation("Pembatalan kursi {CancellationNumber} sudah disetujui sebelumnya. Penerapan dilewati (idempoten).", cancellation.CancellationNumber);
                    return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
                }

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    cancellation.CancellationStatus = HeadcountCancellationStatus.Approved;
                    cancellation.DecidedByUserId = context.ActorUserId;
                    cancellation.DecidedAt = now;
                    cancellation.UpdateDateTime = now;
                    cancellation.UpdateBy = context.ActorUserId;

                    var seat = cancellation.HeadcountRequest?.Seats.FirstOrDefault(s => s.CancellationId == cancellation.Id);
                    if (seat != null)
                    {
                        seat.SeatStatus = HeadcountSeatStatus.Cancelled;
                        seat.CancelledAt = now;
                        seat.UpdateDateTime = now;
                        seat.UpdateBy = context.ActorUserId;

                        // Kriteria 3: Bila permintaan berasal dari AdditionalBeyondFormation (T12), turunkan formasi
                        if (cancellation.HeadcountRequest != null &&
                            cancellation.HeadcountRequest.RequestType == HeadcountRequestType.AdditionalBeyondFormation)
                        {
                            var existingFormation = await _formationService.GetEffectiveFormationForPositionAsync(
                                cancellation.HeadcountRequest.OrganizationUnitId,
                                cancellation.HeadcountRequest.PositionId,
                                DateOnly.FromDateTime(now),
                                cancellationToken);

                            decimal currentTarget = existingFormation?.FormationQuantity ?? 0m;
                            decimal newTarget = Math.Max(0m, currentTarget - seat.SeatFte);

                            var newFormation = await _formationService.CreateNewVersionAsync(
                                hospitalSiteId: cancellation.HeadcountRequest.HospitalSiteId,
                                organizationUnitId: cancellation.HeadcountRequest.OrganizationUnitId,
                                positionId: cancellation.HeadcountRequest.PositionId,
                                formationQuantity: newTarget,
                                effectiveStartDate: DateOnly.FromDateTime(now),
                                sourceType: PositionFormationSourceType.HeadcountRequest,
                                sourceDocumentId: cancellation.Id,
                                sourceDocumentNumber: cancellation.CancellationNumber,
                                approvedByUserId: context.ActorUserId,
                                approvedAt: now,
                                cancellationToken: cancellationToken);

                            cancellation.CreatedFormationId = newFormation.Id;
                            _logger.LogInformation(
                                "Formasi unit {UnitId} jabatan {PosId} diturunkan dari {CurrentTarget} ke {NewTarget} karena pembatalan kursi",
                                cancellation.HeadcountRequest.OrganizationUnitId,
                                cancellation.HeadcountRequest.PositionId,
                                currentTarget,
                                newTarget);
                        }

                        // Hitung ulang status permintaan induk
                        if (cancellation.HeadcountRequest != null)
                        {
                            var activeSeats = cancellation.HeadcountRequest.Seats.Where(s => !s.IsDelete).ToList();
                            int totalSeats = activeSeats.Count;
                            int fulfilledCount = activeSeats.Count(s => s.SeatStatus == HeadcountSeatStatus.Fulfilled);
                            int cancelledCount = activeSeats.Count(s => s.SeatStatus == HeadcountSeatStatus.Cancelled);

                            if (fulfilledCount + cancelledCount == totalSeats && totalSeats > 0)
                            {
                                cancellation.HeadcountRequest.RequestStatus = HeadcountRequestStatus.Closed;
                                cancellation.HeadcountRequest.ClosedAt = now;
                                cancellation.HeadcountRequest.UpdateDateTime = now;
                                cancellation.HeadcountRequest.UpdateBy = context.ActorUserId;
                            }
                        }
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Pembatalan kursi {CancellationNumber} berhasil disetujui", cancellation.CancellationNumber);
                    return WorkforcePlanningApplyResult.Success(previousStatus, cancellation.CancellationStatus.ToString(), statusChanged: true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Gagal menerapkan persetujuan pembatalan kursi {CancellationNumber}", cancellation.CancellationNumber);
                    throw;
                }
            }

            if (string.Equals(normalizedStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                cancellation.CancellationStatus = HeadcountCancellationStatus.Rejected;
                cancellation.DecidedByUserId = context.ActorUserId;
                cancellation.DecidedAt = now;
                cancellation.UpdateDateTime = now;
                cancellation.UpdateBy = context.ActorUserId;

                var rejectedSeat = cancellation.HeadcountRequest?.Seats.FirstOrDefault(s => s.CancellationId == cancellation.Id);
                if (rejectedSeat != null)
                {
                    rejectedSeat.CancellationId = null; // lepas tanda pembatalan (kriteria 4)
                    rejectedSeat.UpdateDateTime = now;
                    rejectedSeat.UpdateBy = context.ActorUserId;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return WorkforcePlanningApplyResult.Success(previousStatus, cancellation.CancellationStatus.ToString(), statusChanged: true);
            }

            return WorkforcePlanningApplyResult.Unhandled($"Status workflow {context.WorkflowStatus} tidak dikenal untuk rujukan HeadcountCancellation.");
        }
    }
}
