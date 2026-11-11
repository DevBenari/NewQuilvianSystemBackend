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
    /// Penerap keputusan akhir workflow persetujuan T11 dan T12 untuk permintaan penambahan tenaga kerja (BE-HRD-041).
    /// </summary>
    public sealed class HrdHeadcountRequestDecisionApplier : IWorkforcePlanningDecisionApplier
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdPositionFormationService _formationService;
        private readonly ILogger<HrdHeadcountRequestDecisionApplier> _logger;

        public HrdHeadcountRequestDecisionApplier(
            ApplicationDbContext dbContext,
            IHrdPositionFormationService formationService,
            ILogger<HrdHeadcountRequestDecisionApplier> logger)
        {
            _dbContext = dbContext;
            _formationService = formationService;
            _logger = logger;
        }

        public string ReferenceType => WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest;

        public async Task<WorkforcePlanningApplyResult> ApplyDecisionAsync(
            WorkforcePlanningDecisionContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var entity = await _dbContext.HrdHeadcountRequests
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == context.ReferenceId, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Permintaan penambahan tenaga kerja {ReferenceId} tidak ditemukan untuk penerapan keputusan workflow.", context.ReferenceId);
                return WorkforcePlanningApplyResult.Unhandled($"Permintaan penambahan tenaga kerja {context.ReferenceId} tidak ditemukan.");
            }

            var previousStatus = entity.RequestStatus.ToString();
            var now = DateTime.UtcNow;
            var normalizedStatus = (context.WorkflowStatus ?? string.Empty).Trim();

            if (string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "FinalApproved", StringComparison.OrdinalIgnoreCase))
            {
                // Idempoten (kriteria 5)
                if (entity.RequestStatus == HeadcountRequestStatus.ReadyForRecruitment ||
                    entity.RequestStatus == HeadcountRequestStatus.PartiallyFulfilled ||
                    entity.RequestStatus == HeadcountRequestStatus.Fulfilled)
                {
                    _logger.LogInformation("Permintaan tenaga {RequestNumber} sudah siap direkrut / terpenuhi. Penerapan dilewati (idempoten).", entity.RequestNumber);
                    return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
                }

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Kriteria 2: Jika AdditionalBeyondFormation (T12), naikkan formasi unit x jabatan dalam transaksi yang sama
                    if (entity.RequestType == HeadcountRequestType.AdditionalBeyondFormation)
                    {
                        var existingFormation = await _formationService.GetEffectiveFormationForPositionAsync(
                            entity.OrganizationUnitId,
                            entity.PositionId,
                            DateOnly.FromDateTime(now),
                            cancellationToken);

                        decimal oldTarget = existingFormation?.FormationQuantity ?? 0m;
                        decimal newTarget = oldTarget + entity.RequestedQuantity;

                        var newFormation = await _formationService.CreateNewVersionAsync(
                            hospitalSiteId: entity.HospitalSiteId,
                            organizationUnitId: entity.OrganizationUnitId,
                            positionId: entity.PositionId,
                            formationQuantity: newTarget,
                            effectiveStartDate: DateOnly.FromDateTime(now),
                            sourceType: PositionFormationSourceType.HeadcountRequest,
                            sourceDocumentId: entity.Id,
                            sourceDocumentNumber: entity.RequestNumber,
                            approvedByUserId: context.ActorUserId,
                            approvedAt: now,
                            cancellationToken: cancellationToken);

                        entity.CreatedFormationId = newFormation.Id;
                        _logger.LogInformation(
                            "Formasi unit {UnitId} jabatan {PosId} dinaikkan dari {OldTarget} ke {NewTarget} (Versi: {Ver})",
                            entity.OrganizationUnitId,
                            entity.PositionId,
                            oldTarget,
                            newTarget,
                            newFormation.VersionNumber);

                        // Kriteria 3: Buka kembali kursi yang Frozen jika ada
                        var frozenSeats = await _dbContext.HrdHeadcountRequestSeats
                            .Where(s => !s.IsDelete &&
                                        s.SeatStatus == HeadcountSeatStatus.Frozen &&
                                        s.HeadcountRequest != null &&
                                        s.HeadcountRequest.OrganizationUnitId == entity.OrganizationUnitId &&
                                        s.HeadcountRequest.PositionId == entity.PositionId)
                            .ToListAsync(cancellationToken);

                        foreach (var fSeat in frozenSeats)
                        {
                            fSeat.SeatStatus = HeadcountSeatStatus.Open;
                            fSeat.UpdateDateTime = now;
                            fSeat.UpdateBy = context.ActorUserId;
                        }
                    }

                    // Validasi sisa kursi (VR-WFP-23 / BE-HRD-049):
                    // Jika AdditionalWithinFormation, pastikan sisa kursi formasi masih cukup pada persetujuan akhir
                    if (entity.RequestType == HeadcountRequestType.AdditionalWithinFormation)
                    {
                        var effectiveDate = entity.RequiredStartDate;
                        var effectiveDateTime = effectiveDate.ToDateTime(TimeOnly.MinValue);
                        var formation = await _formationService.GetEffectiveFormationForPositionAsync(
                            entity.OrganizationUnitId,
                            entity.PositionId,
                            effectiveDate,
                            cancellationToken);

                        decimal formationQty = formation?.FormationQuantity ?? 0m;
                        var filledFte = (decimal)await _dbContext.WfpOrganizationAssignments
                            .Where(a => !a.IsDelete &&
                                        a.IsActive &&
                                        a.OrganizationUnitId == entity.OrganizationUnitId &&
                                        a.PositionId == entity.PositionId &&
                                        a.IsPrimary &&
                                        a.EffectiveStartDate <= effectiveDateTime &&
                                        (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= effectiveDateTime))
                            .CountAsync(cancellationToken);

                        var claimedSeatsFte = (decimal)await _dbContext.HrdHeadcountRequestSeats
                            .Where(s => !s.IsDelete &&
                                        s.HeadcountRequestId != entity.Id &&
                                        (s.SeatStatus == HeadcountSeatStatus.Open || s.SeatStatus == HeadcountSeatStatus.Linked) &&
                                        s.HeadcountRequest != null &&
                                        s.HeadcountRequest.OrganizationUnitId == entity.OrganizationUnitId &&
                                        s.HeadcountRequest.PositionId == entity.PositionId)
                            .CountAsync(cancellationToken);

                        var availableRemaining = formationQty - filledFte - claimedSeatsFte;
                        if (entity.RequestedQuantity > availableRemaining)
                        {
                            _logger.LogWarning("Persetujuan T11 permintaan {RequestNumber} ditolak karena sisa kursi formasi tidak mencukupi ({Available} < {Requested}). Permintaan dikembalikan (VR-WFP-23).",
                                entity.RequestNumber, availableRemaining, entity.RequestedQuantity);

                            entity.RequestStatus = HeadcountRequestStatus.Returned;
                            entity.UpdateDateTime = now;
                            entity.UpdateBy = context.ActorUserId;
                            await _dbContext.SaveChangesAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);

                            return new WorkforcePlanningApplyResult
                            {
                                IsHandled = true,
                                PreviousStatus = previousStatus,
                                CurrentStatus = HeadcountRequestStatus.Returned.ToString(),
                                StatusChanged = true,
                                WarningMessage = "Sisa kursi formasi tidak lagi mencukupi; permintaan dikembalikan ke pemohon (VR-WFP-23)."
                            };
                        }
                    }

                    // Kriteria 1: Set status ke ReadyForRecruitment
                    entity.RequestStatus = HeadcountRequestStatus.ReadyForRecruitment;
                    entity.ApprovedAt = now;
                    entity.ApprovedByUserId = context.ActorUserId;
                    entity.ReadyForRecruitmentAt = now;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = context.ActorUserId;
                    entity.RowVersion = Guid.NewGuid();

                    // Seluruh kursi yang Open siap diproses rekrutmen
                    foreach (var seat in entity.Seats.Where(s => !s.IsDelete && s.SeatStatus == HeadcountSeatStatus.Open))
                    {
                        seat.UpdateDateTime = now;
                        seat.UpdateBy = context.ActorUserId;
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Permintaan tenaga {RequestNumber} berhasil disahkan menjadi ReadyForRecruitment", entity.RequestNumber);
                    return WorkforcePlanningApplyResult.Success(previousStatus, entity.RequestStatus.ToString(), statusChanged: true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Gagal menerapkan persetujuan permintaan tenaga {RequestNumber}", entity.RequestNumber);
                    throw;
                }
            }

            if (string.Equals(normalizedStatus, "Returned", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "ReturnedForRevision", StringComparison.OrdinalIgnoreCase))
            {
                entity.RequestStatus = HeadcountRequestStatus.Returned;
                entity.UpdateDateTime = now;
                entity.UpdateBy = context.ActorUserId;
                entity.RowVersion = Guid.NewGuid();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return WorkforcePlanningApplyResult.Success(previousStatus, entity.RequestStatus.ToString(), statusChanged: true);
            }

            if (string.Equals(normalizedStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                entity.RequestStatus = HeadcountRequestStatus.Rejected;
                entity.UpdateDateTime = now;
                entity.UpdateBy = context.ActorUserId;
                entity.RowVersion = Guid.NewGuid();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return WorkforcePlanningApplyResult.Success(previousStatus, entity.RequestStatus.ToString(), statusChanged: true);
            }

            return WorkforcePlanningApplyResult.Unhandled($"Status workflow {context.WorkflowStatus} tidak dikenal untuk rujukan HeadcountRequest.");
        }
    }
}
