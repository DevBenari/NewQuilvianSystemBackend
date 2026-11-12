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
    /// Penerap keputusan akhir workflow persetujuan T10 untuk rencana formasi tahunan dan addendum (BE-HRD-047).
    /// Menerbitkan versi formasi baru per baris dan membekukan kursi permintaan yang melebihi formasi.
    /// </summary>
    public sealed class HrdManpowerPlanDecisionApplier : IWorkforcePlanningDecisionApplier
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdPositionFormationService _formationService;
        private readonly ILogger<HrdManpowerPlanDecisionApplier> _logger;

        public HrdManpowerPlanDecisionApplier(
            ApplicationDbContext dbContext,
            IHrdPositionFormationService formationService,
            ILogger<HrdManpowerPlanDecisionApplier> logger)
        {
            _dbContext = dbContext;
            _formationService = formationService;
            _logger = logger;
        }

        public string ReferenceType => WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceManpowerPlan;

        public async Task<WorkforcePlanningApplyResult> ApplyDecisionAsync(
            WorkforcePlanningDecisionContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var plan = await _dbContext.HrdManpowerPlans
                .Include(p => p.Lines.Where(l => !l.IsDelete))
                .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == context.ReferenceId, cancellationToken);

            if (plan == null)
            {
                _logger.LogWarning("Rencana kebutuhan tenaga {ReferenceId} tidak ditemukan untuk penerapan keputusan workflow.", context.ReferenceId);
                return WorkforcePlanningApplyResult.Unhandled($"Rencana kebutuhan tenaga {context.ReferenceId} tidak ditemukan.");
            }

            var previousStatus = plan.PlanStatus.ToString();
            var now = DateTime.UtcNow;
            var normalizedStatus = (context.WorkflowStatus ?? string.Empty).Trim();

            if (string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "FinalApproved", StringComparison.OrdinalIgnoreCase))
            {
                // Idempoten (kriteria 5)
                if (plan.PlanStatus == ManpowerPlanStatus.Approved)
                {
                    _logger.LogInformation("Rencana formasi {PlanNumber} sudah disetujui sebelumnya. Penerapan dilewati (idempoten).", plan.PlanNumber);
                    return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
                }

                // Validasi pra-persetujuan VR-WFP-21 (BE-HRD-049):
                // Finance tidak dapat menyetujui rencana yang masih memiliki baris RateUnavailable tanpa koreksi FinanceAdjustedTotal
                if (plan.Lines.Any(l => l.CostStatus == CostCalculationStatus.RateUnavailable) && !plan.FinanceAdjustedTotal.HasValue)
                {
                    _logger.LogWarning("Persetujuan rencana {PlanNumber} ditolak karena terdapat baris dengan tarif belum tersedia tanpa koreksi Finance (VR-WFP-21).", plan.PlanNumber);
                    return WorkforcePlanningApplyResult.Unhandled(
                        "Persetujuan rencana kebutuhan tenaga tidak dapat diproses karena masih terdapat baris dengan tarif belum tersedia tanpa koreksi Finance (VR-WFP-21).");
                }

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var sourceType = plan.PlanKind == ManpowerPlanKind.Addendum
                        ? PositionFormationSourceType.PlanAddendum
                        : PositionFormationSourceType.ManpowerPlan;

                    // Kriteria 1: Terbitkan versi formasi baru per baris dalam transaksi yang sama
                    foreach (var line in plan.Lines)
                    {
                        var newFormation = await _formationService.CreateNewVersionAsync(
                            hospitalSiteId: plan.HospitalSiteId,
                            organizationUnitId: line.OrganizationUnitId,
                            positionId: line.PositionId,
                            formationQuantity: line.ProposedFormationQuantity,
                            effectiveStartDate: line.EffectiveStartDate,
                            sourceType: sourceType,
                            sourceDocumentId: plan.Id,
                            sourceDocumentNumber: plan.PlanNumber,
                            approvedByUserId: context.ActorUserId,
                            approvedAt: now,
                            cancellationToken: cancellationToken);

                        line.CreatedFormationId = newFormation.Id;

                        // Kriteria 2: Periksa apakah ada permintaan berjalan yang kini melebihi formasi baru
                        var activeRequests = await _dbContext.HrdHeadcountRequests
                            .Include(r => r.Seats.Where(s => !s.IsDelete))
                            .Where(r => !r.IsDelete &&
                                        r.OrganizationUnitId == line.OrganizationUnitId &&
                                        r.PositionId == line.PositionId &&
                                        (r.RequestStatus == HeadcountRequestStatus.Submitted ||
                                         r.RequestStatus == HeadcountRequestStatus.ReadyForRecruitment ||
                                         r.RequestStatus == HeadcountRequestStatus.PartiallyFulfilled))
                            .ToListAsync(cancellationToken);

                        decimal totalActiveFte = activeRequests.Sum(r => r.Seats.Where(s => s.SeatStatus == HeadcountSeatStatus.Open || s.SeatStatus == HeadcountSeatStatus.Linked).Sum(s => s.SeatFte));

                        if (totalActiveFte > line.ProposedFormationQuantity)
                        {
                            _logger.LogWarning(
                                "Permintaan berjalan pada unit {UnitId} jabatan {PosId} melebihi formasi baru ({TotalActive} > {ProposedFormation}). Membekukan kursi berlebih.",
                                line.OrganizationUnitId,
                                line.PositionId,
                                totalActiveFte,
                                line.ProposedFormationQuantity);

                            decimal excessToFreeze = totalActiveFte - line.ProposedFormationQuantity;
                            foreach (var req in activeRequests.OrderByDescending(r => r.CreateDateTime))
                            {
                                req.IsOverFormationAfterAddendum = true;
                                req.UpdateDateTime = now;
                                req.UpdateBy = context.ActorUserId;

                                foreach (var seat in req.Seats.Where(s => s.SeatStatus == HeadcountSeatStatus.Open).OrderByDescending(s => s.SeatNumber))
                                {
                                    if (excessToFreeze <= 0m) break;
                                    seat.SeatStatus = HeadcountSeatStatus.Frozen;
                                    seat.UpdateDateTime = now;
                                    seat.UpdateBy = context.ActorUserId;
                                    excessToFreeze -= seat.SeatFte;
                                }
                            }
                        }
                    }

                    plan.PlanStatus = ManpowerPlanStatus.Approved;
                    plan.ApprovedByUserId = context.ActorUserId;
                    plan.ApprovedAt = now;
                    plan.UpdateDateTime = now;
                    plan.UpdateBy = context.ActorUserId;
                    plan.RowVersion = Guid.NewGuid();

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Rencana formasi {PlanNumber} berhasil disahkan menjadi Approved", plan.PlanNumber);
                    return WorkforcePlanningApplyResult.Success(previousStatus, plan.PlanStatus.ToString(), statusChanged: true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Gagal menerapkan persetujuan rencana formasi {PlanNumber}", plan.PlanNumber);
                    throw;
                }
            }

            if (string.Equals(normalizedStatus, "Returned", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "ReturnedForRevision", StringComparison.OrdinalIgnoreCase))
            {
                plan.PlanStatus = ManpowerPlanStatus.Returned;
                plan.UpdateDateTime = now;
                plan.UpdateBy = context.ActorUserId;
                plan.RowVersion = Guid.NewGuid();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return WorkforcePlanningApplyResult.Success(previousStatus, plan.PlanStatus.ToString(), statusChanged: true);
            }

            if (string.Equals(normalizedStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                plan.PlanStatus = ManpowerPlanStatus.Rejected;
                plan.UpdateDateTime = now;
                plan.UpdateBy = context.ActorUserId;
                plan.RowVersion = Guid.NewGuid();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return WorkforcePlanningApplyResult.Success(previousStatus, plan.PlanStatus.ToString(), statusChanged: true);
            }

            return WorkforcePlanningApplyResult.Unhandled($"Status workflow {context.WorkflowStatus} tidak dikenal untuk rujukan ManpowerPlan.");
        }
    }
}
