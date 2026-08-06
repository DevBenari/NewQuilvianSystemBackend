using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ShiftSwapService
    {
        private readonly ApplicationDbContext _dbContext;

        public ShiftSwapService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ShiftSwapFilterMetadataResponse GetFilterMetadata()
        {
            return new ShiftSwapFilterMetadataResponse
            {
                RequestStatusOptions = new List<string>
                {
                    SchedulingRequestValueConstants.ShiftSwapStatus.Draft,
                    SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget,
                    SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted,
                    SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected,
                    SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval,
                    SchedulingRequestValueConstants.ShiftSwapStatus.NeedRevision,
                    SchedulingRequestValueConstants.ShiftSwapStatus.Approved,
                    SchedulingRequestValueConstants.ShiftSwapStatus.Applied,
                    SchedulingRequestValueConstants.ShiftSwapStatus.Rejected,
                    SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled
                },
                ViewModeOptions = new List<string> { "all", "requested", "incoming" },
                SortDirections = SchedulingRequestValueConstants.SortDirections.ToList(),
                PageSizeOptions = SchedulingRequestValueConstants.PageSizes.ToList()
            };
        }

        public async Task<ShiftSwapSummaryResponse> GetSummaryAsync(
            Guid? workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.WfpShiftSwapRequests
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue)
            {
                query = query.Where(x =>
                    x.RequesterWorkforceProfileId == workforceProfileId.Value ||
                    x.TargetWorkforceProfileId == workforceProfileId.Value);
            }

            return new ShiftSwapSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Draft, cancellationToken),
                WaitingTarget = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget, cancellationToken),
                WaitingApproval = await query.CountAsync(x =>
                    x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted ||
                    x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval,
                    cancellationToken),
                NeedRevision = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.NeedRevision, cancellationToken),
                Approved = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Approved, cancellationToken),
                Applied = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Applied, cancellationToken),
                Rejected = await query.CountAsync(x =>
                    x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Rejected ||
                    x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected,
                    cancellationToken),
                Cancelled = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled, cancellationToken)
            };
        }

        public async Task<PagedResult<ShiftSwapListResponse>> GetPagedAsync(
            Guid? workforceProfileId,
            string? viewMode,
            string? requestStatus,
            DateOnly? startDate,
            DateOnly? endDate,
            string? search,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = SchedulingRequestValueConstants.PageSizes.Contains(pageSize) ? pageSize : 25;

            var query = BaseQuery().Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue)
            {
                var mode = string.IsNullOrWhiteSpace(viewMode) ? "all" : viewMode.Trim().ToLowerInvariant();
                query = mode switch
                {
                    "requested" => query.Where(x => x.RequesterWorkforceProfileId == workforceProfileId.Value),
                    "incoming" => query.Where(x => x.TargetWorkforceProfileId == workforceProfileId.Value),
                    _ => query.Where(x =>
                        x.RequesterWorkforceProfileId == workforceProfileId.Value ||
                        x.TargetWorkforceProfileId == workforceProfileId.Value)
                };
            }

            if (!string.IsNullOrWhiteSpace(requestStatus))
            {
                var status = requestStatus.Trim();
                query = query.Where(x => x.RequestStatus == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(x =>
                    x.RequesterShiftDate >= startDate.Value ||
                    x.TargetShiftDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x =>
                    x.RequesterShiftDate <= endDate.Value ||
                    x.TargetShiftDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    (x.RequesterWorkforceProfile != null && x.RequesterWorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.TargetWorkforceProfile != null && x.TargetWorkforceProfile.DisplayName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);
            query = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.CreateDateTime)
                : query.OrderByDescending(x => x.CreateDateTime);

            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ShiftSwapListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapList(x, workforceProfileId)).ToList()
            };
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> GetByIdAsync(
            Guid id,
            Guid? viewerWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return entity == null
                ? SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan.")
                : SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Ok(
                    MapDetail(entity, viewerWorkforceProfileId),
                    "Detail pengajuan tukar shift berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapValidationResponse>> ValidatePreviewAsync(
            Guid requesterWorkforceProfileId,
            CreateShiftSwapSelfServiceRequest request,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            var result = new ShiftSwapValidationResponse();

            if (requesterWorkforceProfileId == Guid.Empty)
            {
                result.Errors.Add("Requester workforce profile tidak valid.");
            }

            if (request.TargetWorkforceProfileId == Guid.Empty ||
                request.TargetWorkforceProfileId == requesterWorkforceProfileId)
            {
                result.Errors.Add("Target workforce profile harus berbeda dengan requester.");
            }

            var requesterAssignment = await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .Include(x => x.Shift)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.RequesterShiftAssignmentId &&
                    x.WorkforceProfileId == requesterWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel,
                    cancellationToken);

            var targetAssignment = await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .Include(x => x.Shift)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.TargetShiftAssignmentId &&
                    x.WorkforceProfileId == request.TargetWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel,
                    cancellationToken);

            if (requesterAssignment == null)
            {
                result.Errors.Add("Shift assignment requester tidak ditemukan atau bukan milik user login.");
            }

            if (targetAssignment == null)
            {
                result.Errors.Add("Shift assignment target tidak ditemukan atau tidak sesuai target employee.");
            }

            if (requesterAssignment != null && targetAssignment != null)
            {
                result.RequesterShiftSummary = BuildAssignmentSummary(requesterAssignment);
                result.TargetShiftSummary = BuildAssignmentSummary(targetAssignment);

                if (requesterAssignment.Id == targetAssignment.Id)
                {
                    result.Errors.Add("Requester dan target shift assignment tidak boleh sama.");
                }

                if (requesterAssignment.AssignmentStatus == "Cancelled" ||
                    targetAssignment.AssignmentStatus == "Cancelled")
                {
                    result.Errors.Add("Shift assignment yang sudah dibatalkan tidak dapat ditukar.");
                }

                var requesterConflict = await _dbContext.TrxShiftAssignments
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == requesterWorkforceProfileId &&
                        x.ShiftDate == targetAssignment.ShiftDate &&
                        x.Id != requesterAssignment.Id &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.AssignmentStatus != "Cancelled",
                        cancellationToken);

                var targetConflict = await _dbContext.TrxShiftAssignments
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == request.TargetWorkforceProfileId &&
                        x.ShiftDate == requesterAssignment.ShiftDate &&
                        x.Id != targetAssignment.Id &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.AssignmentStatus != "Cancelled",
                        cancellationToken);

                if (requesterConflict)
                {
                    result.Errors.Add("Requester sudah memiliki shift lain pada tanggal shift target.");
                }

                if (targetConflict)
                {
                    result.Errors.Add("Target employee sudah memiliki shift lain pada tanggal shift requester.");
                }

                if (requesterAssignment.RosterAssignmentId == Guid.Empty ||
                    targetAssignment.RosterAssignmentId == Guid.Empty)
                {
                    result.Errors.Add("Shift assignment harus terhubung dengan roster assignment.");
                }
            }

            var targetExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.TargetWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (!targetExists)
            {
                result.Errors.Add("Target workforce profile tidak ditemukan atau tidak aktif.");
            }

            var targetHasLogin = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == request.TargetWorkforceProfileId &&
                    x.IsActive,
                    cancellationToken);

            if (!targetHasLogin)
            {
                result.Errors.Add("Target employee belum mempunyai akun aktif untuk memberikan acknowledgement.");
            }

            var openStatuses = new[]
            {
                SchedulingRequestValueConstants.ShiftSwapStatus.Draft,
                SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget,
                SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted,
                SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval,
                SchedulingRequestValueConstants.ShiftSwapStatus.NeedRevision,
                SchedulingRequestValueConstants.ShiftSwapStatus.Approved
            };

            var duplicate = await _dbContext.WfpShiftSwapRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    openStatuses.Contains(x.RequestStatus) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value) &&
                    (x.RequesterShiftAssignmentId == request.RequesterShiftAssignmentId ||
                     x.TargetShiftAssignmentId == request.RequesterShiftAssignmentId ||
                     x.RequesterShiftAssignmentId == request.TargetShiftAssignmentId ||
                     x.TargetShiftAssignmentId == request.TargetShiftAssignmentId),
                    cancellationToken);

            if (duplicate)
            {
                result.Errors.Add("Salah satu shift assignment masih digunakan pada pengajuan tukar shift aktif.");
            }

            if (request.RequestReasonId.HasValue)
            {
                var reasonExists = await _dbContext.MstRequestReasons
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.RequestReasonId.Value && x.IsActive && !x.IsDelete, cancellationToken);

                if (!reasonExists)
                {
                    result.Errors.Add("Request reason tidak ditemukan atau tidak aktif.");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return SchedulingRequestServiceResult<ShiftSwapValidationResponse>.Ok(
                result,
                result.IsValid ? "Pengajuan tukar shift valid." : "Pengajuan tukar shift belum valid.");
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> CreateDraftAsync(
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            CreateShiftSwapSelfServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidatePreviewAsync(
                requesterWorkforceProfileId,
                request,
                null,
                cancellationToken);

            if (validation.Data == null || !validation.Data.IsValid)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", validation.Data?.Errors ?? new List<string> { "Data pengajuan tidak valid." }));
            }

            var requesterAssignment = await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.RequesterShiftAssignmentId, cancellationToken);
            var targetAssignment = await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.TargetShiftAssignmentId, cancellationToken);

            var now = DateTime.UtcNow;
            var entity = new WfpShiftSwapRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = GenerateRequestNumber(),
                RequesterWorkforceProfileId = requesterWorkforceProfileId,
                TargetWorkforceProfileId = request.TargetWorkforceProfileId,
                RosterPeriodId = await ResolveCommonRosterPeriodIdAsync(
                    requesterAssignment.RosterAssignmentId,
                    targetAssignment.RosterAssignmentId,
                    cancellationToken),
                RequesterShiftAssignmentId = requesterAssignment.Id,
                TargetShiftAssignmentId = targetAssignment.Id,
                RequestReasonId = NormalizeGuid(request.RequestReasonId),
                RequesterShiftDate = requesterAssignment.ShiftDate,
                TargetShiftDate = targetAssignment.ShiftDate,
                Reason = request.Reason.Trim(),
                AttachmentPath = NormalizeText(request.AttachmentPath),
                RequestStatus = SchedulingRequestValueConstants.ShiftSwapStatus.Draft,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.WfpShiftSwapRequests.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, requesterWorkforceProfileId, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> UpdateDraftAsync(
            Guid id,
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            UpdateShiftSwapSelfServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsRequester(entity, requesterWorkforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan milik user login.");
            }

            if (!CanEdit(entity.RequestStatus))
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan hanya dapat diubah pada status Draft, TargetRejected, atau NeedRevision.");
            }

            var validation = await ValidatePreviewAsync(
                requesterWorkforceProfileId,
                request,
                id,
                cancellationToken);

            if (validation.Data == null || !validation.Data.IsValid)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", validation.Data?.Errors ?? new List<string> { "Data pengajuan tidak valid." }));
            }

            var requesterAssignment = await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.RequesterShiftAssignmentId, cancellationToken);
            var targetAssignment = await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.TargetShiftAssignmentId, cancellationToken);

            entity.TargetWorkforceProfileId = request.TargetWorkforceProfileId;
            entity.RequesterShiftAssignmentId = requesterAssignment.Id;
            entity.TargetShiftAssignmentId = targetAssignment.Id;
            entity.RosterPeriodId = await ResolveCommonRosterPeriodIdAsync(
                requesterAssignment.RosterAssignmentId,
                targetAssignment.RosterAssignmentId,
                cancellationToken);
            entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
            entity.RequesterShiftDate = requesterAssignment.ShiftDate;
            entity.TargetShiftDate = targetAssignment.ShiftDate;
            entity.Reason = request.Reason.Trim();
            entity.AttachmentPath = NormalizeText(request.AttachmentPath);
            entity.RequestStatus = SchedulingRequestValueConstants.ShiftSwapStatus.Draft;
            entity.RequestedAt = null;
            entity.TargetRespondedAt = null;
            entity.IsAcceptedByTarget = null;
            entity.TargetResponseNotes = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(entity.Id, requesterWorkforceProfileId, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> SubmitToTargetAsync(
            Guid id,
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            string? note,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsRequester(entity, requesterWorkforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan milik user login.");
            }

            if (!CanEdit(entity.RequestStatus))
            {
                if (entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget)
                {
                    return await GetByIdAsync(entity.Id, requesterWorkforceProfileId, cancellationToken);
                }

                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak dapat dikirim ke target dari status saat ini.");
            }

            var validationRequest = new CreateShiftSwapSelfServiceRequest
            {
                TargetWorkforceProfileId = entity.TargetWorkforceProfileId,
                RequesterShiftAssignmentId = entity.RequesterShiftAssignmentId,
                TargetShiftAssignmentId = entity.TargetShiftAssignmentId,
                RequestReasonId = entity.RequestReasonId,
                Reason = entity.Reason,
                AttachmentPath = entity.AttachmentPath
            };

            var validation = await ValidatePreviewAsync(
                requesterWorkforceProfileId,
                validationRequest,
                entity.Id,
                cancellationToken);

            if (validation.Data == null || !validation.Data.IsValid)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", validation.Data?.Errors ?? new List<string> { "Data pengajuan tidak valid." }));
            }

            entity.RequestStatus = SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget;
            entity.RequestedAt = DateTime.UtcNow;
            entity.TargetRespondedAt = null;
            entity.IsAcceptedByTarget = null;
            entity.ApprovalNotes = NormalizeText(note);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, requesterWorkforceProfileId, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> RespondAsTargetAsync(
            Guid id,
            Guid targetWorkforceProfileId,
            Guid actorUserId,
            bool accept,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || entity.TargetWorkforceProfileId != targetWorkforceProfileId)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan ditujukan kepada user login.");
            }

            if (entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted &&
                entity.IsAcceptedByTarget == true && accept)
            {
                return await GetByIdAsync(entity.Id, targetWorkforceProfileId, cancellationToken);
            }

            if (entity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Target hanya dapat merespons pengajuan berstatus PendingTarget.");
            }

            entity.IsAcceptedByTarget = accept;
            entity.TargetRespondedAt = DateTime.UtcNow;
            entity.TargetResponseNotes = NormalizeText(notes);
            entity.RequestStatus = accept
                ? SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted
                : SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, targetWorkforceProfileId, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> CancelAsync(
            Guid id,
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsRequester(entity, requesterWorkforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Applied ||
                entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Approved ||
                entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Rejected ||
                entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak dapat dibatalkan dari status saat ini.");
            }

            entity.RequestStatus = SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled;
            entity.ApprovalNotes = NormalizeText(reason);
            entity.IsCancel = true;
            entity.CancelDateTime = DateTime.UtcNow;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, requesterWorkforceProfileId, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<object>> DeleteAsync(
            Guid id,
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsRequester(entity, requesterWorkforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.Draft &&
                entity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected)
            {
                return SchedulingRequestServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya draft atau pengajuan yang ditolak target yang dapat dihapus.");
            }

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return SchedulingRequestServiceResult<object>.Ok(
                new { entity.Id },
                "Pengajuan tukar shift berhasil dihapus.");
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapDetailResponse>> ApplyAsync(
            Guid id,
            Guid actorUserId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan.");
            }

            if (entity.IsAppliedToRoster || entity.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Applied)
            {
                await transaction.RollbackAsync(cancellationToken);
                return await GetByIdAsync(id, null, cancellationToken);
            }

            if (entity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.Approved ||
                entity.IsAcceptedByTarget != true)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tukar shift hanya dapat diterapkan setelah target menerima dan workflow manager selesai.");
            }

            var requesterAssignment = await _dbContext.TrxShiftAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.RequesterShiftAssignmentId &&
                    x.WorkforceProfileId == entity.RequesterWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            var targetAssignment = await _dbContext.TrxShiftAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.TargetShiftAssignmentId &&
                    x.WorkforceProfileId == entity.TargetWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (requesterAssignment == null || targetAssignment == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Shift assignment requester atau target tidak lagi tersedia.");
            }

            var requesterConflict = await HasDestinationConflictAsync(
                requesterAssignment.WorkforceProfileId,
                targetAssignment.ShiftDate,
                requesterAssignment.Id,
                cancellationToken);
            var targetConflict = await HasDestinationConflictAsync(
                targetAssignment.WorkforceProfileId,
                requesterAssignment.ShiftDate,
                targetAssignment.Id,
                cancellationToken);

            if (requesterConflict || targetConflict)
            {
                return SchedulingRequestServiceResult<ShiftSwapDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tukar shift tidak dapat diterapkan karena salah satu employee sudah memiliki assignment pada tanggal tujuan.");
            }

            var requesterPayload = ShiftAssignmentPayload.From(requesterAssignment);
            var targetPayload = ShiftAssignmentPayload.From(targetAssignment);
            var now = DateTime.UtcNow;

            targetPayload.ApplyTo(requesterAssignment, entity.Id, actorUserId, now);
            requesterPayload.ApplyTo(targetAssignment, entity.Id, actorUserId, now);

            entity.IsAppliedToRoster = true;
            entity.AppliedAt = now;
            entity.RequestStatus = SchedulingRequestValueConstants.ShiftSwapStatus.Applied;
            entity.ApprovalNotes = NormalizeText(notes) ?? entity.ApprovalNotes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, null, cancellationToken);
        }

        public async Task<List<ShiftSwapTargetOptionResponse>> GetEligibleTargetOptionsAsync(
            Guid requesterWorkforceProfileId,
            string? search,
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 100);
            var now = DateTime.UtcNow;
            var requesterOrganization = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == requesterWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (requesterOrganization == null)
            {
                return new List<ShiftSwapTargetOptionResponse>();
            }

            var query = _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Position)
                .Where(x =>
                    x.WorkforceProfileId != requesterWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now) &&
                    x.WorkforceProfile != null &&
                    x.WorkforceProfile.IsActive &&
                    !x.WorkforceProfile.IsDelete &&
                    (x.OrganizationUnitId == requesterOrganization.OrganizationUnitId ||
                     x.DepartmentId == requesterOrganization.DepartmentId));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkforceProfile!.ProfileCode.ToLower().Contains(keyword) ||
                    x.WorkforceProfile.DisplayName.ToLower().Contains(keyword));
            }

            return await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.WorkforceProfile!.DisplayName)
                .Select(x => new ShiftSwapTargetOptionResponse
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    PositionName = x.Position != null ? x.Position.PositionName : null
                })
                .Distinct()
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ShiftSwapAssignmentOptionResponse>> GetAssignmentOptionsAsync(
            Guid workforceProfileId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            if (endDate < startDate)
            {
                return new List<ShiftSwapAssignmentOptionResponse>();
            }

            return await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Shift)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ShiftDate >= startDate &&
                    x.ShiftDate <= endDate &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.AssignmentStatus != "Cancelled")
                .OrderBy(x => x.ShiftDate)
                .ThenBy(x => x.ScheduledStartAt)
                .Select(x => new ShiftSwapAssignmentOptionResponse
                {
                    ShiftAssignmentId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    ShiftDate = x.ShiftDate,
                    ShiftId = x.ShiftId,
                    ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                    ScheduledStartAt = x.ScheduledStartAt,
                    ScheduledEndAt = x.ScheduledEndAt
                })
                .ToListAsync(cancellationToken);
        }

        private IQueryable<WfpShiftSwapRequest> BaseQuery()
        {
            return _dbContext.WfpShiftSwapRequests
                .AsNoTracking()
                .Include(x => x.RequesterWorkforceProfile)
                .Include(x => x.TargetWorkforceProfile)
                .Include(x => x.RequesterShiftAssignment)
                    .ThenInclude(x => x!.Shift)
                .Include(x => x.TargetShiftAssignment)
                    .ThenInclude(x => x!.Shift);
        }

        private static ShiftSwapListResponse MapList(
            WfpShiftSwapRequest x,
            Guid? viewerWorkforceProfileId)
        {
            var isRequester = viewerWorkforceProfileId.HasValue &&
                              x.RequesterWorkforceProfileId == viewerWorkforceProfileId.Value;
            var isTarget = viewerWorkforceProfileId.HasValue &&
                           x.TargetWorkforceProfileId == viewerWorkforceProfileId.Value;
            var canEdit = isRequester && CanEdit(x.RequestStatus);

            return new ShiftSwapListResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                RequesterWorkforceProfileId = x.RequesterWorkforceProfileId,
                RequesterName = x.RequesterWorkforceProfile?.DisplayName ?? string.Empty,
                TargetWorkforceProfileId = x.TargetWorkforceProfileId,
                TargetName = x.TargetWorkforceProfile?.DisplayName ?? string.Empty,
                RequesterShiftAssignmentId = x.RequesterShiftAssignmentId,
                TargetShiftAssignmentId = x.TargetShiftAssignmentId,
                RequesterShiftDate = x.RequesterShiftDate,
                TargetShiftDate = x.TargetShiftDate,
                RequesterShiftName = x.RequesterShiftAssignment?.Shift?.ShiftName,
                TargetShiftName = x.TargetShiftAssignment?.Shift?.ShiftName,
                Reason = x.Reason,
                RequestStatus = x.RequestStatus,
                IsAcceptedByTarget = x.IsAcceptedByTarget,
                WorkflowInstanceId = x.WorkflowInstanceId,
                IsAppliedToRoster = x.IsAppliedToRoster,
                RequestedAt = x.RequestedAt,
                TargetRespondedAt = x.TargetRespondedAt,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                IsRequester = isRequester,
                IsTarget = isTarget,
                CanEdit = canEdit,
                CanSubmitToTarget = canEdit,
                CanRespondAsTarget = isTarget && x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget,
                CanCancel = isRequester &&
                            x.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.Approved &&
                            x.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.Applied &&
                            x.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.Rejected &&
                            x.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled,
                CanDelete = isRequester &&
                            (x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Draft ||
                             x.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected)
            };
        }

        private static ShiftSwapDetailResponse MapDetail(
            WfpShiftSwapRequest x,
            Guid? viewerWorkforceProfileId)
        {
            var list = MapList(x, viewerWorkforceProfileId);
            return new ShiftSwapDetailResponse
            {
                Id = list.Id,
                RequestNumber = list.RequestNumber,
                RequesterWorkforceProfileId = list.RequesterWorkforceProfileId,
                RequesterName = list.RequesterName,
                TargetWorkforceProfileId = list.TargetWorkforceProfileId,
                TargetName = list.TargetName,
                RequesterShiftAssignmentId = list.RequesterShiftAssignmentId,
                TargetShiftAssignmentId = list.TargetShiftAssignmentId,
                RequesterShiftDate = list.RequesterShiftDate,
                TargetShiftDate = list.TargetShiftDate,
                RequesterShiftName = list.RequesterShiftName,
                TargetShiftName = list.TargetShiftName,
                Reason = list.Reason,
                RequestStatus = list.RequestStatus,
                IsAcceptedByTarget = list.IsAcceptedByTarget,
                WorkflowInstanceId = list.WorkflowInstanceId,
                IsAppliedToRoster = list.IsAppliedToRoster,
                RequestedAt = list.RequestedAt,
                TargetRespondedAt = list.TargetRespondedAt,
                CreateDateTime = list.CreateDateTime,
                UpdateDateTime = list.UpdateDateTime,
                IsRequester = list.IsRequester,
                IsTarget = list.IsTarget,
                CanEdit = list.CanEdit,
                CanSubmitToTarget = list.CanSubmitToTarget,
                CanRespondAsTarget = list.CanRespondAsTarget,
                CanCancel = list.CanCancel,
                CanDelete = list.CanDelete,
                RosterPeriodId = x.RosterPeriodId,
                RequestReasonId = x.RequestReasonId,
                RejectionReasonId = x.RejectionReasonId,
                WorkflowDefinitionId = x.WorkflowDefinitionId,
                AttachmentPath = x.AttachmentPath,
                TargetResponseNotes = x.TargetResponseNotes,
                ApprovalNotes = x.ApprovalNotes,
                ApprovedAt = x.ApprovedAt,
                RejectedAt = x.RejectedAt,
                AppliedAt = x.AppliedAt
            };
        }

        private async Task<Guid?> ResolveCommonRosterPeriodIdAsync(
            Guid requesterRosterAssignmentId,
            Guid targetRosterAssignmentId,
            CancellationToken cancellationToken)
        {
            var periods = await _dbContext.TrxRosterAssignments
                .AsNoTracking()
                .Where(x => x.Id == requesterRosterAssignmentId || x.Id == targetRosterAssignmentId)
                .Select(x => x.RosterPeriodId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return periods.Count == 1 ? periods[0] : null;
        }

        private async Task<bool> HasDestinationConflictAsync(
            Guid workforceProfileId,
            DateOnly date,
            Guid excludedAssignmentId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.TrxShiftAssignments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ShiftDate == date &&
                    x.Id != excludedAssignmentId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.AssignmentStatus != "Cancelled",
                    cancellationToken);
        }

        private static bool CanEdit(string status)
        {
            return status == SchedulingRequestValueConstants.ShiftSwapStatus.Draft ||
                   status == SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected ||
                   status == SchedulingRequestValueConstants.ShiftSwapStatus.NeedRevision;
        }

        private static bool IsRequester(
            WfpShiftSwapRequest entity,
            Guid workforceProfileId,
            Guid actorUserId)
        {
            return entity.RequesterWorkforceProfileId == workforceProfileId &&
                   (entity.CreateBy == actorUserId || entity.CreateBy == Guid.Empty);
        }

        private static string BuildAssignmentSummary(TrxShiftAssignment assignment)
        {
            return $"{assignment.ShiftDate:yyyy-MM-dd} | " +
                   $"{assignment.Shift?.ShiftName ?? "Shift"} | " +
                   $"{assignment.ScheduledStartAt:HH:mm}-{assignment.ScheduledEndAt:HH:mm}";
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty ? value : null;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string GenerateRequestNumber()
        {
            var value = $"SSW-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            return value.Length <= 50 ? value : value[..50];
        }

        private sealed class ShiftAssignmentPayload
        {
            public Guid? WorkScheduleId { get; set; }
            public Guid? ShiftId { get; set; }
            public Guid? HospitalSiteId { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public Guid? DepartmentId { get; set; }
            public Guid? WorkLocationId { get; set; }
            public DateOnly ShiftDate { get; set; }
            public DateTime ScheduledStartAt { get; set; }
            public DateTime ScheduledEndAt { get; set; }
            public int BreakDurationMinutes { get; set; }
            public int PlannedWorkMinutes { get; set; }
            public string AssignmentType { get; set; } = string.Empty;
            public string AssignmentStatus { get; set; } = string.Empty;
            public bool IsNightShift { get; set; }
            public bool IsOnCall { get; set; }
            public bool IsDayOff { get; set; }

            public static ShiftAssignmentPayload From(TrxShiftAssignment source)
            {
                return new ShiftAssignmentPayload
                {
                    WorkScheduleId = source.WorkScheduleId,
                    ShiftId = source.ShiftId,
                    HospitalSiteId = source.HospitalSiteId,
                    OrganizationUnitId = source.OrganizationUnitId,
                    DepartmentId = source.DepartmentId,
                    WorkLocationId = source.WorkLocationId,
                    ShiftDate = source.ShiftDate,
                    ScheduledStartAt = source.ScheduledStartAt,
                    ScheduledEndAt = source.ScheduledEndAt,
                    BreakDurationMinutes = source.BreakDurationMinutes,
                    PlannedWorkMinutes = source.PlannedWorkMinutes,
                    AssignmentType = source.AssignmentType,
                    AssignmentStatus = source.AssignmentStatus,
                    IsNightShift = source.IsNightShift,
                    IsOnCall = source.IsOnCall,
                    IsDayOff = source.IsDayOff
                };
            }

            public void ApplyTo(
                TrxShiftAssignment target,
                Guid shiftSwapRequestId,
                Guid actorUserId,
                DateTime now)
            {
                target.WorkScheduleId = WorkScheduleId;
                target.ShiftId = ShiftId;
                target.HospitalSiteId = HospitalSiteId;
                target.OrganizationUnitId = OrganizationUnitId;
                target.DepartmentId = DepartmentId;
                target.WorkLocationId = WorkLocationId;
                target.ShiftDate = ShiftDate;
                target.ScheduledStartAt = ScheduledStartAt;
                target.ScheduledEndAt = ScheduledEndAt;
                target.BreakDurationMinutes = BreakDurationMinutes;
                target.PlannedWorkMinutes = PlannedWorkMinutes;
                target.AssignmentType = AssignmentType;
                target.AssignmentStatus = AssignmentStatus;
                target.IsNightShift = IsNightShift;
                target.IsOnCall = IsOnCall;
                target.IsDayOff = IsDayOff;
                target.AssignmentSource = "ShiftSwap";
                target.ShiftSwapRequestId = shiftSwapRequestId;
                target.IsManualOverride = true;
                target.UpdateDateTime = now;
                target.UpdateBy = actorUserId;
            }
        }
    }
}
