using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan pengelolaan permintaan penambahan tenaga kerja / headcount request (BE-HRD-040, BE-HRD-042).
    /// </summary>
    public sealed class HrdHeadcountRequestService : IHrdHeadcountRequestService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdWorkforcePlanningCostService _costService;
        private readonly IHrdStaffingGapSnapshotService _snapshotService;
        private readonly IHrdWorkforcePlanningWorkflowIntegrationService _workflowService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public HrdHeadcountRequestService(
            ApplicationDbContext dbContext,
            IHrdWorkforcePlanningCostService costService,
            IHrdStaffingGapSnapshotService snapshotService,
            IHrdWorkforcePlanningWorkflowIntegrationService workflowService,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _costService = costService;
            _snapshotService = snapshotService;
            _workflowService = workflowService;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        public async Task<WorkforcePlanningResult<PagedResult<HeadcountRequestListItemResponse>>> GetPagedListAsync(
            HeadcountRequestPagedQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(userId, cancellationToken);
            var isCostVisible = _costService.HasCostReadPermission(user);

            var dbQuery = _dbContext.HrdHeadcountRequests
                .AsNoTracking()
                .Include(r => r.HospitalSite)
                .Include(r => r.OrganizationUnit)
                .Include(r => r.Position)
                .Include(r => r.Seats)
                .Where(r => !r.IsDelete);

            if (query.HospitalSiteId.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                if (scopedUnitIds != null && !scopedUnitIds.Contains(query.OrganizationUnitId.Value))
                {
                    return WorkforcePlanningResult<PagedResult<HeadcountRequestListItemResponse>>.Fail("Data tidak ditemukan.", 404);
                }
                dbQuery = dbQuery.Where(r => r.OrganizationUnitId == query.OrganizationUnitId.Value);
            }
            else if (scopedUnitIds != null)
            {
                dbQuery = dbQuery.Where(r => scopedUnitIds.Contains(r.OrganizationUnitId));
            }

            if (query.PositionId.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.PositionId == query.PositionId.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.RequestStatus == query.Status.Value);
            }

            if (query.RequestType.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.RequestType == query.RequestType.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(r => r.RequestNumber.ToLower().Contains(term) ||
                                             (r.Justification != null && r.Justification.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var items = await dbQuery
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new HeadcountRequestListItemResponse
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    HospitalSiteId = r.HospitalSiteId,
                    HospitalSiteName = r.HospitalSite != null ? r.HospitalSite.SiteName : null,
                    OrganizationUnitId = r.OrganizationUnitId,
                    OrganizationUnitName = r.OrganizationUnit != null ? r.OrganizationUnit.UnitName : null,
                    PositionId = r.PositionId,
                    PositionName = r.Position != null ? r.Position.PositionName : null,
                    RequestType = r.RequestType,
                    RequestStatus = r.RequestStatus,
                    RequiredStartDate = r.RequiredStartDate,
                    RequestedQuantity = r.RequestedQuantity,
                    TotalSeats = r.Seats.Count(s => !s.IsDelete),
                    EstimatedCost = isCostVisible ? r.EstimatedCost : null,
                    FinanceAdjustedCost = isCostVisible ? r.FinanceAdjustedCost : null,
                    CostVisible = isCostVisible,
                    CreatedAt = r.CreateDateTime,
                    RowVersion = r.RowVersion
                })
                .ToListAsync(cancellationToken);

            return WorkforcePlanningResult<PagedResult<HeadcountRequestListItemResponse>>.Ok(
                new PagedResult<HeadcountRequestListItemResponse>
                {
                    Items = items,
                    TotalData = totalCount,
                    TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                "Daftar permintaan penambahan tenaga kerja berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<HeadcountRequestResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(userId, cancellationToken);
            var isCostVisible = _costService.HasCostReadPermission(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .AsNoTracking()
                .Include(r => r.HospitalSite)
                .Include(r => r.OrganizationUnit)
                .Include(r => r.Position)
                .Include(r => r.SalaryGrade)
                .Include(r => r.TemporaryReasonWorkforceProfile)
                .Include(r => r.Seats.Where(s => !s.IsDelete))
                    .ThenInclude(s => s.ReplacedWorkforceProfile)
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (scopedUnitIds != null && !scopedUnitIds.Contains(entity.OrganizationUnitId))
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Data tidak ditemukan.", 404);
            }

            return WorkforcePlanningResult<HeadcountRequestResponse>.Ok(
                MapToResponse(entity, isCostVisible),
                "Detail permintaan penambahan tenaga kerja berhasil diambil.");
        }

        public async Task<RemainingSeatsResponse> GetRemainingSeatsAsync(
            Guid hospitalSiteId,
            Guid organizationUnitId,
            Guid positionId,
            DateOnly? asOfDate,
            CancellationToken cancellationToken = default)
        {
            var effectiveDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var formation = await _dbContext.HrdPositionFormations
                .AsNoTracking()
                .Where(f => !f.IsDelete &&
                            f.OrganizationUnitId == organizationUnitId &&
                            f.PositionId == positionId &&
                            f.EffectiveStartDate <= effectiveDate &&
                            (!f.EffectiveEndDate.HasValue || f.EffectiveEndDate.Value >= effectiveDate))
                .OrderByDescending(f => f.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            decimal formationQty = formation?.FormationQuantity ?? 0m;
            bool hasFormation = formation != null;

            var effectiveDateTime = effectiveDate.ToDateTime(TimeOnly.MinValue);

            var orgAssignments = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(oa => !oa.IsDelete &&
                             oa.OrganizationUnitId == organizationUnitId &&
                             oa.IsPrimary &&
                             oa.EffectiveStartDate <= effectiveDateTime &&
                             (!oa.EffectiveEndDate.HasValue || oa.EffectiveEndDate.Value >= effectiveDateTime))
                .Select(oa => oa.WorkforceProfileId)
                .ToListAsync(cancellationToken);

            var filledCount = await _dbContext.WfpPositionAssignments
                .AsNoTracking()
                .Where(pa => !pa.IsDelete &&
                             pa.PositionId == positionId &&
                             pa.IsPrimary &&
                             orgAssignments.Contains(pa.WorkforceProfileId) &&
                             pa.EffectiveStartDate <= effectiveDateTime &&
                             (!pa.EffectiveEndDate.HasValue || pa.EffectiveEndDate.Value >= effectiveDateTime))
                .CountAsync(cancellationToken);

            decimal filledFte = filledCount;
            decimal willBeVacantFte = 0m;

            var activeStatuses = new[]
            {
                HeadcountRequestStatus.Submitted,
                HeadcountRequestStatus.ReadyForRecruitment,
                HeadcountRequestStatus.PartiallyFulfilled
            };

            var activeSeatStatuses = new[]
            {
                HeadcountSeatStatus.Open,
                HeadcountSeatStatus.Linked
            };

            var claimedSeatsFte = await _dbContext.HrdHeadcountRequestSeats
                .AsNoTracking()
                .Where(s => !s.IsDelete &&
                            activeSeatStatuses.Contains(s.SeatStatus) &&
                            s.HeadcountRequest != null &&
                            !s.HeadcountRequest.IsDelete &&
                            s.HeadcountRequest.OrganizationUnitId == organizationUnitId &&
                            s.HeadcountRequest.PositionId == positionId &&
                            activeStatuses.Contains(s.HeadcountRequest.RequestStatus))
                .SumAsync(s => (decimal?)s.SeatFte, cancellationToken) ?? 0m;

            decimal remaining = formationQty - (filledFte - willBeVacantFte) - claimedSeatsFte;
            if (remaining < 0m) remaining = 0m;

            return new RemainingSeatsResponse
            {
                HospitalSiteId = hospitalSiteId,
                OrganizationUnitId = organizationUnitId,
                PositionId = positionId,
                FormationQuantity = formationQty,
                FilledFte = filledFte,
                WillBeVacantFte = willBeVacantFte,
                ClaimedSeatsFte = claimedSeatsFte,
                RemainingSeats = remaining,
                HasActiveFormation = hasFormation,
                Note = hasFormation
                    ? $"Sisa kursi tersedia: {remaining:N2}"
                    : "Unit tidak memiliki formasi aktif; permintaan diproses sebagai Di Luar Formasi."
            };
        }

        public async Task<WorkforcePlanningResult<HeadcountRequestResponse>> CreateDraftAsync(
            CreateHeadcountRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            if (request.Seats == null || request.Seats.Count == 0)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Rincian kursi permintaan tenaga wajib diisi.", 400);
            }

            if (request.ExternalSeatCount + request.InternalSeatCount != request.Seats.Count)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Jumlah kursi rekrut luar dan internal harus sama dengan total kursi.", 422);
            }

            foreach (var seat in request.Seats)
            {
                if (seat.SeatFte < 0.10m || seat.SeatFte > 1.00m)
                {
                    return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Porsi kerja per kursi harus berada di antara 0,10 hingga 1,00 FTE.", 400);
                }
            }

            string requestNumber;
            try
            {
                requestNumber = await _numberSeriesAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.HeadcountRequest,
                    Prefix: "REQ-",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: userId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch
            {
                requestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            var totalFte = request.Seats.Sum(s => s.SeatFte);

            var costResult = await _costService.CalculateEstimatedCostAsync(
                request.SalaryGradeId,
                totalFte,
                12,
                request.RequiredStartDate,
                user,
                cancellationToken);

            var entity = new HrdHeadcountRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = requestNumber,
                HospitalSiteId = request.HospitalSiteId,
                OrganizationUnitId = request.OrganizationUnitId,
                PositionId = request.PositionId,
                SalaryGradeId = request.SalaryGradeId,
                RequestType = request.RequestType,
                RequestStatus = HeadcountRequestStatus.Draft,
                RequiredStartDate = request.RequiredStartDate,
                TemporaryEndDate = request.TemporaryEndDate,
                TemporaryReasonWorkforceProfileId = request.TemporaryReasonWorkforceProfileId,
                Justification = request.Justification,
                RequestedQuantity = totalFte,
                ExternalSeatCount = request.ExternalSeatCount,
                InternalSeatCount = request.InternalSeatCount,
                EstimatedMonthlyRate = costResult.MidpointSalary,
                EstimatedCost = costResult.EstimatedCost,
                CostStatus = costResult.RateStatus == CostRateStatus.Available
                    ? CostCalculationStatus.Calculated
                    : CostCalculationStatus.RateUnavailable,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            int seatNum = 1;
            foreach (var seat in request.Seats)
            {
                entity.Seats.Add(new HrdHeadcountRequestSeat
                {
                    Id = Guid.NewGuid(),
                    HeadcountRequestId = entity.Id,
                    SeatNumber = seatNum++,
                    SeatFte = seat.SeatFte,
                    FulfilmentSource = seat.FulfilmentSource,
                    SeatStatus = HeadcountSeatStatus.Open,
                    ReplacedWorkforceProfileId = seat.ReplacedWorkforceProfileId,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                });
            }

            _dbContext.HrdHeadcountRequests.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<HeadcountRequestResponse>> UpdateDraftAsync(
            Guid id,
            UpdateHeadcountRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (entity.RequestStatus != HeadcountRequestStatus.Draft &&
                entity.RequestStatus != HeadcountRequestStatus.Returned)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Hanya draf atau permintaan yang dikembalikan yang dapat diubah.", 400);
            }

            if (request.ExternalSeatCount + request.InternalSeatCount != request.Seats.Count)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Jumlah kursi rekrut luar dan internal harus sama dengan total kursi.", 422);
            }

            foreach (var seat in request.Seats)
            {
                if (seat.SeatFte < 0.10m || seat.SeatFte > 1.00m)
                {
                    return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Porsi kerja per kursi harus berada di antara 0,10 hingga 1,00 FTE.", 400);
                }
            }

            var totalFte = request.Seats.Sum(s => s.SeatFte);

            var costResult = await _costService.CalculateEstimatedCostAsync(
                request.SalaryGradeId,
                totalFte,
                12,
                request.RequiredStartDate,
                user,
                cancellationToken);

            entity.SalaryGradeId = request.SalaryGradeId;
            entity.RequestType = request.RequestType;
            entity.RequiredStartDate = request.RequiredStartDate;
            entity.TemporaryEndDate = request.TemporaryEndDate;
            entity.TemporaryReasonWorkforceProfileId = request.TemporaryReasonWorkforceProfileId;
            entity.Justification = request.Justification;
            entity.RequestedQuantity = totalFte;
            entity.ExternalSeatCount = request.ExternalSeatCount;
            entity.InternalSeatCount = request.InternalSeatCount;
            entity.EstimatedMonthlyRate = costResult.MidpointSalary;
            entity.EstimatedCost = costResult.EstimatedCost;
            entity.CostStatus = costResult.RateStatus == CostRateStatus.Available
                ? CostCalculationStatus.Calculated
                : CostCalculationStatus.RateUnavailable;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;
            entity.RowVersion = Guid.NewGuid();

            foreach (var existingSeat in entity.Seats)
            {
                existingSeat.IsDelete = true;
            }

            int seatNum = 1;
            foreach (var seat in request.Seats)
            {
                entity.Seats.Add(new HrdHeadcountRequestSeat
                {
                    Id = Guid.NewGuid(),
                    HeadcountRequestId = entity.Id,
                    SeatNumber = seatNum++,
                    SeatFte = seat.SeatFte,
                    FulfilmentSource = seat.FulfilmentSource,
                    SeatStatus = HeadcountSeatStatus.Open,
                    ReplacedWorkforceProfileId = seat.ReplacedWorkforceProfileId,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<HeadcountRequestResponse>> SubmitAsync(
            Guid id,
            SubmitHeadcountRequestDto request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await _dbContext.HrdHeadcountRequests
                    .Include(r => r.Seats.Where(s => !s.IsDelete))
                    .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

                if (entity == null)
                {
                    return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Data tidak ditemukan.", 404);
                }

                if (entity.RequestStatus != HeadcountRequestStatus.Draft &&
                    entity.RequestStatus != HeadcountRequestStatus.Returned)
                {
                    return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Hanya draf atau permintaan yang dikembalikan yang dapat diajukan.", 400);
                }

                if (entity.RequestType == HeadcountRequestType.Replacement)
                {
                    if (entity.Seats.Any(s => !s.ReplacedWorkforceProfileId.HasValue))
                    {
                        return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Setiap kursi pengganti wajib menentukan pegawai yang digantikan.", 422);
                    }
                }

                if (entity.RequestType == HeadcountRequestType.Temporary)
                {
                    if (!entity.TemporaryEndDate.HasValue || !entity.TemporaryReasonWorkforceProfileId.HasValue)
                    {
                        return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Permintaan sementara wajib menentukan tanggal selesai dan pegawai yang digantikan sementara.", 422);
                    }
                }

                if (entity.RequestType == HeadcountRequestType.AdditionalBeyondFormation)
                {
                    if (string.IsNullOrWhiteSpace(entity.Justification))
                    {
                        return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Permintaan di luar formasi wajib mencantumkan alasan pengajuan.", 422);
                    }
                }

                var remainingInfo = await GetRemainingSeatsAsync(
                    entity.HospitalSiteId,
                    entity.OrganizationUnitId,
                    entity.PositionId,
                    entity.RequiredStartDate,
                    cancellationToken);

                if (entity.RequestType == HeadcountRequestType.AdditionalWithinFormation || entity.RequestType == HeadcountRequestType.Replacement)
                {
                    if (entity.RequestedQuantity > remainingInfo.RemainingSeats)
                    {
                        return WorkforcePlanningResult<HeadcountRequestResponse>.Fail(
                            $"Permintaan sebesar {entity.RequestedQuantity:N2} FTE melebihi sisa kursi yang tersedia ({remainingInfo.RemainingSeats:N2} FTE).", 422);
                    }
                }

                entity.RemainingSeatsAtSubmission = remainingInfo.RemainingSeats;

                await _snapshotService.CaptureSubmissionSnapshotAsync(
                    entity.HospitalSiteId,
                    entity.OrganizationUnitId,
                    PositionFormationSourceType.HeadcountRequest,
                    entity.Id,
                    entity.RequiredStartDate,
                    userId,
                    cancellationToken);

                var workflowCode = entity.RequestType == HeadcountRequestType.AdditionalBeyondFormation ? "T12" : "T11";

                var workflowResult = await _workflowService.StartWorkflowAsync(new StartWorkforcePlanningWorkflowRequest
                {
                    WorkflowDefinitionCode = workflowCode,
                    ReferenceType = WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest,
                    ReferenceId = entity.Id,
                    DocumentNumber = entity.RequestNumber,
                    SubmitComment = "Pengajuan Permintaan Penambahan Tenaga Kerja",
                    AutoSubmit = true
                }, cancellationToken);

                if (!workflowResult.Success)
                {
                    return WorkforcePlanningResult<HeadcountRequestResponse>.Fail(
                        $"Gagal mendaftarkan alur persetujuan: {workflowResult.Message}", workflowResult.StatusCode);
                }

                entity.WorkflowInstanceId = workflowResult.Data?.Id;
                entity.RequestStatus = HeadcountRequestStatus.Submitted;
                entity.SubmittedByUserId = userId;
                entity.SubmittedAt = DateTime.UtcNow;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = userId;
                entity.RowVersion = Guid.NewGuid();

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetByIdAsync(entity.Id, user, cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<WorkforcePlanningResult<HeadcountRequestResponse>> WithdrawAsync(
            Guid id,
            WithdrawHeadcountRequestDto request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (entity.RequestStatus == HeadcountRequestStatus.ReadyForRecruitment ||
                entity.RequestStatus == HeadcountRequestStatus.PartiallyFulfilled ||
                entity.RequestStatus == HeadcountRequestStatus.Fulfilled)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Permintaan yang sudah disetujui untuk rekrutmen tidak dapat ditarik kembali.", 409);
            }

            entity.RequestStatus = HeadcountRequestStatus.Withdrawn;
            entity.WithdrawnAt = DateTime.UtcNow;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;
            entity.UpdateBy = userId;
            entity.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<HeadcountRequestResponse>> FinanceAdjustmentAsync(
            Guid id,
            FinanceAdjustmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            if (!_costService.HasCostReadPermission(user))
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Hanya peninjau Keuangan (Finance) yang berwenang mengoreksi estimasi biaya.", 403);
            }

            var entity = await _dbContext.HrdHeadcountRequests
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (entity.RequestStatus != HeadcountRequestStatus.Submitted)
            {
                return WorkforcePlanningResult<HeadcountRequestResponse>.Fail("Koreksi biaya Finance hanya dapat dilakukan saat dokumen sedang ditinjau dalam persetujuan.", 400);
            }

            entity.FinanceAdjustedCost = request.AdjustedCost;
            entity.FinanceAdjustmentReason = request.Reason;
            entity.FinanceAdjustedByUserId = userId;
            entity.FinanceAdjustedAt = DateTime.UtcNow;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;
            entity.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<FulfilmentPlanChangeResponse>> ChangeFulfilmentPlanAsync(
            Guid id,
            ChangeFulfilmentPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .Include(r => r.Seats.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<FulfilmentPlanChangeResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (entity.CreateBy == userId)
            {
                return WorkforcePlanningResult<FulfilmentPlanChangeResponse>.Fail("Perubahan rencana pemenuhan harus dilakukan oleh HR Manager yang berbeda dari pembuat permintaan.", 403);
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkforcePlanningResult<FulfilmentPlanChangeResponse>.Fail("Alasan perubahan rencana pemenuhan wajib diisi.", 422);
            }

            var seat = entity.Seats.FirstOrDefault(s => s.Id == request.SeatId);
            if (seat == null)
            {
                return WorkforcePlanningResult<FulfilmentPlanChangeResponse>.Fail("Kursi tidak ditemukan pada permintaan ini.", 404);
            }

            if (seat.SeatStatus != HeadcountSeatStatus.Open)
            {
                return WorkforcePlanningResult<FulfilmentPlanChangeResponse>.Fail("Hanya kursi yang masih terbuka (Open) yang dapat diubah sumber pemenuhannya.", 422);
            }

            var oldSource = seat.FulfilmentSource;
            seat.FulfilmentSource = request.NewSource;
            seat.UpdateDateTime = DateTime.UtcNow;
            seat.UpdateBy = userId;

            entity.ExternalSeatCount = entity.Seats.Count(s => !s.IsDelete && s.FulfilmentSource == HeadcountFulfilmentSource.External);
            entity.InternalSeatCount = entity.Seats.Count(s => !s.IsDelete && s.FulfilmentSource == HeadcountFulfilmentSource.Internal);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            var changeRecord = new HrdFulfilmentPlanChange
            {
                Id = Guid.NewGuid(),
                HeadcountRequestId = entity.Id,
                SeatId = seat.Id,
                FromSource = oldSource,
                ToSource = request.NewSource,
                Reason = request.Reason,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            _dbContext.HrdFulfilmentPlanChanges.Add(changeRecord);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return WorkforcePlanningResult<FulfilmentPlanChangeResponse>.Ok(
                new FulfilmentPlanChangeResponse
                {
                    Id = changeRecord.Id,
                    SeatId = seat.Id,
                    FromSource = oldSource,
                    ToSource = request.NewSource,
                    Reason = request.Reason,
                    ChangedAt = changeRecord.ChangedAt,
                    ChangedByUserName = user.Identity?.Name
                },
                "Rencana pemenuhan kursi berhasil diubah.");
        }

        public async Task<WorkforcePlanningResult<CancellationResponse>> RequestCancellationAsync(
            Guid id,
            CreateCancellationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .Include(r => r.Seats.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<CancellationResponse>.Fail("Permintaan penambahan tenaga kerja tidak ditemukan.", 404);
            }

            var seat = entity.Seats.FirstOrDefault(s => s.Id == request.SeatId);
            if (seat == null)
            {
                return WorkforcePlanningResult<CancellationResponse>.Fail("Kursi tidak ditemukan pada permintaan ini.", 404);
            }

            // Validasi status kursi yang boleh dibatalkan (VR-WFP-35)
            if (seat.SeatStatus != HeadcountSeatStatus.Open && seat.SeatStatus != HeadcountSeatStatus.Frozen)
            {
                return WorkforcePlanningResult<CancellationResponse>.Fail("Hanya kursi yang belum terisi (Open atau Frozen) yang dapat diajukan pembatalan.", 422);
            }

            // Validasi penjaga "tutup lowongan dulu" (VR-WFP-36 / HRD-DEC-071, BE-HRD-045)
            var hasOpenVacancy = await _dbContext.HrdJobRequisitions
                .Where(r => r.HeadcountRequestId == entity.Id && !r.IsDelete)
                .Join(_dbContext.HrdJobVacancies.Where(v => !v.IsDelete && v.Status == JobVacancyStatus.Published),
                      r => r.Id,
                      v => v.RequisitionId,
                      (r, v) => v)
                .AnyAsync(cancellationToken);

            if (hasOpenVacancy)
            {
                return WorkforcePlanningResult<CancellationResponse>.Fail(
                    "Pembatalan kursi tidak dapat dilakukan karena masih terdapat lowongan kerja aktif. Tutup lowongan terlebih dahulu (VR-WFP-36).", 409);
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkforcePlanningResult<CancellationResponse>.Fail("Alasan pembatalan kursi wajib diisi.", 422);
            }

            string cancellationNumber;
            try
            {
                cancellationNumber = await _numberSeriesAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.HeadcountCancellation,
                    Prefix: "CAN-",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: userId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch
            {
                cancellationNumber = $"CAN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            var now = DateTime.UtcNow;
            var cancellation = new HrdHeadcountRequestCancellation
            {
                Id = Guid.NewGuid(),
                CancellationNumber = cancellationNumber,
                HeadcountRequestId = entity.Id,
                CancellationStatus = HeadcountCancellationStatus.Submitted,
                Reason = request.Reason,
                RequestedByUserId = userId,
                CreateDateTime = now,
                CreateBy = userId
            };

            // Inisiasi workflow persetujuan T13
            var workflowResult = await _workflowService.StartWorkflowAsync(new StartWorkforcePlanningWorkflowRequest
            {
                WorkflowDefinitionCode = "T13",
                ReferenceType = WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountCancellation,
                ReferenceId = cancellation.Id,
                DocumentNumber = cancellation.CancellationNumber,
                SubmitComment = request.Reason,
                AutoSubmit = true
            }, cancellationToken);

            if (!workflowResult.Success)
            {
                return WorkforcePlanningResult<CancellationResponse>.Fail(
                    $"Gagal mendaftarkan alur persetujuan pembatalan: {workflowResult.Message}", workflowResult.StatusCode);
            }

            cancellation.WorkflowInstanceId = workflowResult.Data?.Id;
            seat.CancellationId = cancellation.Id;
            seat.UpdateDateTime = now;
            seat.UpdateBy = userId;

            _dbContext.HrdHeadcountRequestCancellations.Add(cancellation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return WorkforcePlanningResult<CancellationResponse>.Ok(
                new CancellationResponse
                {
                    Id = cancellation.Id,
                    CancellationNumber = cancellation.CancellationNumber,
                    HeadcountRequestId = entity.Id,
                    SeatId = seat.Id,
                    CancellationStatus = cancellation.CancellationStatus,
                    Reason = cancellation.Reason,
                    RequestedByUserId = cancellation.RequestedByUserId,
                    WorkflowInstanceId = cancellation.WorkflowInstanceId,
                    CreatedAt = cancellation.CreateDateTime
                },
                "Pengajuan pembatalan kursi berhasil didaftarkan ke workflow T13.");
        }

        public HeadcountRequestFilterMetadataResponse GetFilterMetadata()
        {
            return new HeadcountRequestFilterMetadataResponse
            {
                Statuses = Enum.GetNames<HeadcountRequestStatus>().ToList(),
                RequestTypes = Enum.GetNames<HeadcountRequestType>().ToList(),
                FulfilmentSources = Enum.GetNames<HeadcountFulfilmentSource>().ToList()
            };
        }

        private HeadcountRequestResponse MapToResponse(HrdHeadcountRequest entity, bool isCostVisible)
        {
            return new HeadcountRequestResponse
            {
                Id = entity.Id,
                RequestNumber = entity.RequestNumber,
                HospitalSiteId = entity.HospitalSiteId,
                HospitalSiteName = entity.HospitalSite?.SiteName,
                OrganizationUnitId = entity.OrganizationUnitId,
                OrganizationUnitName = entity.OrganizationUnit?.UnitName,
                PositionId = entity.PositionId,
                PositionName = entity.Position?.PositionName,
                SalaryGradeId = entity.SalaryGradeId,
                SalaryGradeName = entity.SalaryGrade?.SalaryGradeName,
                RequestType = entity.RequestType,
                RequestStatus = entity.RequestStatus,
                RequiredStartDate = entity.RequiredStartDate,
                TemporaryEndDate = entity.TemporaryEndDate,
                TemporaryReasonWorkforceProfileId = entity.TemporaryReasonWorkforceProfileId,
                TemporaryReasonEmployeeName = entity.TemporaryReasonWorkforceProfile?.DisplayName,
                Justification = entity.Justification,
                RequestedQuantity = entity.RequestedQuantity,
                ExternalSeatCount = entity.ExternalSeatCount,
                InternalSeatCount = entity.InternalSeatCount,
                RemainingSeatsAtSubmission = entity.RemainingSeatsAtSubmission,
                EstimatedMonthlyRate = isCostVisible ? entity.EstimatedMonthlyRate : null,
                EstimatedCost = isCostVisible ? entity.EstimatedCost : null,
                CostStatus = entity.CostStatus,
                FinanceAdjustedCost = isCostVisible ? entity.FinanceAdjustedCost : null,
                FinanceAdjustmentReason = isCostVisible ? entity.FinanceAdjustmentReason : null,
                FinanceAdjustedAt = entity.FinanceAdjustedAt,
                CostVisible = isCostVisible,
                WorkflowInstanceId = entity.WorkflowInstanceId,
                SubmittedAt = entity.SubmittedAt,
                ReadyForRecruitmentAt = entity.ReadyForRecruitmentAt,
                WithdrawnAt = entity.WithdrawnAt,
                ClosedAt = entity.ClosedAt,
                Seats = entity.Seats
                    .Where(s => !s.IsDelete)
                    .Select(s => new HeadcountSeatResponse
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        SeatFte = s.SeatFte,
                        FulfilmentSource = s.FulfilmentSource,
                        SeatStatus = s.SeatStatus,
                        ReplacedWorkforceProfileId = s.ReplacedWorkforceProfileId,
                        ReplacedEmployeeName = s.ReplacedWorkforceProfile?.DisplayName,
                        LinkedPositionAssignmentId = s.LinkedPositionAssignmentId,
                        LinkedPersonnelActionOrderId = s.LinkedPersonnelActionOrderId,
                        FulfilledAt = s.FulfilledAt
                    })
                    .ToList(),
                RowVersion = entity.RowVersion
            };
        }

        private async Task<List<Guid>?> GetScopedOrganizationUnitIdsAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            var managerialUnitIds = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(a => !a.IsDelete &&
                            a.WorkforceProfileId == userId &&
                            a.IsManagerialAssignment &&
                            (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= nowUtc))
                .Select(a => a.OrganizationUnitId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var validUnitIds = managerialUnitIds.Where(id => id.HasValue).Select(id => id!.Value).ToList();
            if (validUnitIds.Count == 0) return null;

            var allUnits = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .Where(u => !u.IsDelete)
                .Select(u => new { u.Id, u.ParentOrganizationUnitId })
                .ToListAsync(cancellationToken);

            var scopedIds = new HashSet<Guid>(validUnitIds);
            var queue = new Queue<Guid>(validUnitIds);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var children = allUnits.Where(u => u.ParentOrganizationUnitId == currentId);
                foreach (var child in children)
                {
                    if (scopedIds.Add(child.Id)) queue.Enqueue(child.Id);
                }
            }

            return scopedIds.ToList();
        }

        private static Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
