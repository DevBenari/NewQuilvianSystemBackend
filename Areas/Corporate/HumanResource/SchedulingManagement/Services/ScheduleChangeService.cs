using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ScheduleChangeService
    {
        private readonly ApplicationDbContext _dbContext;

        public ScheduleChangeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ScheduleChangeFilterMetadataResponse GetFilterMetadata()
        {
            return new ScheduleChangeFilterMetadataResponse
            {
                RequestTypeOptions = SchedulingRequestValueConstants.ScheduleChangeType.All.ToList(),
                RequestStatusOptions = new List<string>
                {
                    SchedulingRequestValueConstants.ScheduleChangeStatus.Draft,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.NeedRevision,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.Approved,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.Applied,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.Rejected,
                    SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled
                },
                SortDirections = SchedulingRequestValueConstants.SortDirections.ToList(),
                PageSizeOptions = SchedulingRequestValueConstants.PageSizes.ToList()
            };
        }

        public async Task<ScheduleChangeSummaryResponse> GetSummaryAsync(
            Guid? workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.WfpScheduleChangeRequests
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            return new ScheduleChangeSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Draft, cancellationToken),
                WaitingApproval = await query.CountAsync(x =>
                    x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted ||
                    x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview,
                    cancellationToken),
                NeedRevision = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.NeedRevision, cancellationToken),
                Approved = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Approved, cancellationToken),
                Applied = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Applied, cancellationToken),
                Rejected = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Rejected, cancellationToken),
                Cancelled = await query.CountAsync(x => x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled, cancellationToken)
            };
        }

        public async Task<PagedResult<ScheduleChangeListResponse>> GetPagedAsync(
            Guid? workforceProfileId,
            string? requestStatus,
            string? requestType,
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
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(requestStatus))
            {
                var status = requestStatus.Trim();
                query = query.Where(x => x.RequestStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(requestType))
            {
                var type = requestType.Trim();
                query = query.Where(x => x.RequestType == type);
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.EffectiveStartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.EffectiveStartDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null &&
                     (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                      x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(cancellationToken);
            query = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.CreateDateTime)
                : query.OrderByDescending(x => x.CreateDateTime);

            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ScheduleChangeListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(MapList).ToList()
            };
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return entity == null
                ? SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan.")
                : SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Ok(
                    MapDetail(entity),
                    "Detail pengajuan perubahan jadwal berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeValidationResponse>> ValidatePreviewAsync(
            Guid workforceProfileId,
            CreateScheduleChangeSelfServiceRequest request,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            var result = new ScheduleChangeValidationResponse();
            var requestType = NormalizeRequestType(request.RequestType);

            if (workforceProfileId == Guid.Empty)
            {
                result.Errors.Add("Workforce profile tidak valid.");
            }

            if (requestType == null)
            {
                result.Errors.Add("Request type tidak valid.");
            }

            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value < request.EffectiveStartDate)
            {
                result.Errors.Add("Effective end date tidak boleh lebih awal dari effective start date.");
            }

            if (request.RequestedDate < request.EffectiveStartDate ||
                (request.EffectiveEndDate.HasValue && request.RequestedDate > request.EffectiveEndDate.Value))
            {
                result.Errors.Add("Requested date harus berada di dalam periode efektif.");
            }

            var workforceExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && x.IsActive && !x.IsDelete, cancellationToken);

            if (!workforceExists)
            {
                result.Errors.Add("Workforce profile tidak ditemukan atau tidak aktif.");
            }

            var currentAssignment = await ResolveCurrentWorkScheduleAssignmentAsync(
                workforceProfileId,
                request.EffectiveStartDate,
                cancellationToken);

            if (currentAssignment != null)
            {
                result.ResolvedCurrentWorkScheduleAssignmentId = currentAssignment.Id;
                result.ResolvedCurrentWorkScheduleId = currentAssignment.WorkScheduleId;
            }

            TrxShiftAssignment? currentShiftAssignment = null;
            if (request.CurrentShiftAssignmentId.HasValue)
            {
                currentShiftAssignment = await _dbContext.TrxShiftAssignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.CurrentShiftAssignmentId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (currentShiftAssignment == null)
                {
                    result.Errors.Add("Current shift assignment tidak ditemukan atau bukan milik user login.");
                }
            }
            else
            {
                currentShiftAssignment = await _dbContext.TrxShiftAssignments
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ShiftDate == request.RequestedDate &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel)
                    .OrderByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (currentShiftAssignment != null)
            {
                result.ResolvedCurrentShiftAssignmentId = currentShiftAssignment.Id;
                result.ResolvedCurrentShiftId = currentShiftAssignment.ShiftId;
            }

            if (request.RequestedWorkScheduleId.HasValue)
            {
                var scheduleExists = await _dbContext.MstWorkSchedules
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.RequestedWorkScheduleId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!scheduleExists)
                {
                    result.Errors.Add("Requested work schedule tidak ditemukan atau tidak aktif.");
                }
            }

            if (request.RequestedShiftId.HasValue)
            {
                var requestedShift = await _dbContext.MstShifts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RequestedShiftId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (requestedShift == null)
                {
                    result.Errors.Add("Requested shift tidak ditemukan atau tidak aktif.");
                }
                else if (request.RequestedWorkScheduleId.HasValue &&
                         requestedShift.WorkScheduleId.HasValue &&
                         requestedShift.WorkScheduleId != request.RequestedWorkScheduleId)
                {
                    result.Errors.Add("Requested shift tidak berada pada requested work schedule.");
                }
            }

            if (requestType == SchedulingRequestValueConstants.ScheduleChangeType.ShiftChange ||
                requestType == SchedulingRequestValueConstants.ScheduleChangeType.DayOffChange)
            {
                if (currentShiftAssignment == null)
                {
                    result.Errors.Add("Shift change membutuhkan current shift assignment pada requested date.");
                }

                if (!request.RequestedShiftId.HasValue)
                {
                    result.Errors.Add("Shift change membutuhkan requested shift.");
                }
            }
            else if (!request.RequestedWorkScheduleId.HasValue && !request.RequestedShiftId.HasValue)
            {
                result.Errors.Add("Requested work schedule atau requested shift wajib dipilih.");
            }

            var openStatuses = new[]
            {
                SchedulingRequestValueConstants.ScheduleChangeStatus.Draft,
                SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted,
                SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview,
                SchedulingRequestValueConstants.ScheduleChangeStatus.NeedRevision,
                SchedulingRequestValueConstants.ScheduleChangeStatus.Approved
            };

            var hasOverlap = await _dbContext.WfpScheduleChangeRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    openStatuses.Contains(x.RequestStatus) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value) &&
                    x.EffectiveStartDate <= (request.EffectiveEndDate ?? request.EffectiveStartDate) &&
                    (x.EffectiveEndDate ?? x.EffectiveStartDate) >= request.EffectiveStartDate,
                    cancellationToken);

            if (hasOverlap)
            {
                result.Errors.Add("Masih terdapat pengajuan perubahan jadwal aktif pada periode yang beririsan.");
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

            if (currentAssignment == null && request.RequestedWorkScheduleId.HasValue)
            {
                result.Warnings.Add("Tidak ditemukan work schedule assignment aktif. Apply akan membuat temporary assignment baru tanpa menutup assignment lama.");
            }

            result.IsValid = result.Errors.Count == 0;
            return SchedulingRequestServiceResult<ScheduleChangeValidationResponse>.Ok(
                result,
                result.IsValid
                    ? "Pengajuan perubahan jadwal valid."
                    : "Pengajuan perubahan jadwal belum valid.");
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeDetailResponse>> CreateDraftAsync(
            Guid workforceProfileId,
            Guid actorUserId,
            CreateScheduleChangeSelfServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidatePreviewAsync(
                workforceProfileId,
                request,
                null,
                cancellationToken);

            if (validation.Data == null || !validation.Data.IsValid)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", validation.Data?.Errors ?? new List<string> { "Data pengajuan tidak valid." }));
            }

            var now = DateTime.UtcNow;
            var entity = new WfpScheduleChangeRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = GenerateRequestNumber("SCH"),
                WorkforceProfileId = workforceProfileId,
                WorkScheduleAssignmentId = validation.Data.ResolvedCurrentWorkScheduleAssignmentId,
                CurrentShiftAssignmentId = validation.Data.ResolvedCurrentShiftAssignmentId,
                CurrentWorkScheduleId = validation.Data.ResolvedCurrentWorkScheduleId,
                CurrentShiftId = validation.Data.ResolvedCurrentShiftId,
                RequestedShiftAssignmentId = NormalizeGuid(request.RequestedShiftAssignmentId),
                RequestedWorkScheduleId = NormalizeGuid(request.RequestedWorkScheduleId),
                RequestedShiftId = NormalizeGuid(request.RequestedShiftId),
                RequestReasonId = NormalizeGuid(request.RequestReasonId),
                RequestType = NormalizeRequestType(request.RequestType)!,
                RequestedDate = request.RequestedDate,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                Reason = request.Reason.Trim(),
                AttachmentPath = NormalizeText(request.AttachmentPath),
                RequestStatus = SchedulingRequestValueConstants.ScheduleChangeStatus.Draft,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.WfpScheduleChangeRequests.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeDetailResponse>> UpdateDraftAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            UpdateScheduleChangeSelfServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan.");
            }

            if (!IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan atau bukan milik user login.");
            }

            if (!CanEdit(entity.RequestStatus))
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan hanya dapat diubah pada status Draft atau NeedRevision.");
            }

            var validation = await ValidatePreviewAsync(
                workforceProfileId,
                request,
                id,
                cancellationToken);

            if (validation.Data == null || !validation.Data.IsValid)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", validation.Data?.Errors ?? new List<string> { "Data pengajuan tidak valid." }));
            }

            entity.WorkScheduleAssignmentId = validation.Data.ResolvedCurrentWorkScheduleAssignmentId;
            entity.CurrentShiftAssignmentId = validation.Data.ResolvedCurrentShiftAssignmentId;
            entity.CurrentWorkScheduleId = validation.Data.ResolvedCurrentWorkScheduleId;
            entity.CurrentShiftId = validation.Data.ResolvedCurrentShiftId;
            entity.RequestedShiftAssignmentId = NormalizeGuid(request.RequestedShiftAssignmentId);
            entity.RequestedWorkScheduleId = NormalizeGuid(request.RequestedWorkScheduleId);
            entity.RequestedShiftId = NormalizeGuid(request.RequestedShiftId);
            entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
            entity.RequestType = NormalizeRequestType(request.RequestType)!;
            entity.RequestedDate = request.RequestedDate;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Reason = request.Reason.Trim();
            entity.AttachmentPath = NormalizeText(request.AttachmentPath);
            entity.RequestStatus = SchedulingRequestValueConstants.ScheduleChangeStatus.Draft;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeDetailResponse>> PrepareSubmitAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan.");
            }

            if (!IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted ||
                entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview)
            {
                return await GetByIdAsync(entity.Id, cancellationToken);
            }

            if (!CanEdit(entity.RequestStatus))
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak dapat di-submit dari status saat ini.");
            }

            var validationRequest = new CreateScheduleChangeSelfServiceRequest
            {
                RequestType = entity.RequestType,
                CurrentShiftAssignmentId = entity.CurrentShiftAssignmentId,
                RequestedShiftAssignmentId = entity.RequestedShiftAssignmentId,
                RequestedWorkScheduleId = entity.RequestedWorkScheduleId,
                RequestedShiftId = entity.RequestedShiftId,
                RequestReasonId = entity.RequestReasonId,
                RequestedDate = entity.RequestedDate,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Reason = entity.Reason,
                AttachmentPath = entity.AttachmentPath
            };

            var validation = await ValidatePreviewAsync(
                workforceProfileId,
                validationRequest,
                entity.Id,
                cancellationToken);

            if (validation.Data == null || !validation.Data.IsValid)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", validation.Data?.Errors ?? new List<string> { "Data pengajuan tidak valid." }));
            }

            entity.RequestStatus = SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted;
            entity.SubmittedAt ??= DateTime.UtcNow;
            entity.SubmittedByUserId = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeDetailResponse>> CancelAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Applied ||
                entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Approved ||
                entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Rejected ||
                entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak dapat dibatalkan dari status saat ini.");
            }

            entity.RequestStatus = SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled;
            entity.ApprovalNotes = NormalizeText(reason);
            entity.IsCancel = true;
            entity.CancelDateTime = DateTime.UtcNow;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<SchedulingRequestServiceResult<object>> DeleteAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return SchedulingRequestServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus != SchedulingRequestValueConstants.ScheduleChangeStatus.Draft)
            {
                return SchedulingRequestServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya draft yang dapat dihapus.");
            }

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return SchedulingRequestServiceResult<object>.Ok(
                new { entity.Id },
                "Draft perubahan jadwal berhasil dihapus.");
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeDetailResponse>> ApplyAsync(
            Guid id,
            Guid actorUserId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan.");
            }

            if (entity.IsAppliedToRoster || entity.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Applied)
            {
                await transaction.RollbackAsync(cancellationToken);
                return await GetByIdAsync(id, cancellationToken);
            }

            if (entity.RequestStatus != SchedulingRequestValueConstants.ScheduleChangeStatus.Approved)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya pengajuan berstatus Approved yang dapat diterapkan.");
            }

            var now = DateTime.UtcNow;
            var applied = false;

            if (entity.RequestedShiftId.HasValue)
            {
                var shiftAssignment = await _dbContext.TrxShiftAssignments
                    .FirstOrDefaultAsync(x =>
                        x.Id == entity.CurrentShiftAssignmentId &&
                        x.WorkforceProfileId == entity.WorkforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                var shift = await _dbContext.MstShifts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == entity.RequestedShiftId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (shiftAssignment == null || shift == null)
                {
                    return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Current shift assignment atau requested shift tidak lagi tersedia.");
                }

                ApplyShiftToAssignment(shiftAssignment, shift, entity.RequestedDate, entity.Id, actorUserId, now);
                applied = true;
            }

            if (entity.RequestedWorkScheduleId.HasValue &&
                (!entity.RequestedShiftId.HasValue ||
                 entity.RequestType == SchedulingRequestValueConstants.ScheduleChangeType.ScheduleChange ||
                 entity.RequestType == SchedulingRequestValueConstants.ScheduleChangeType.TemporarySchedule))
            {
                var requestedSchedule = await _dbContext.MstWorkSchedules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == entity.RequestedWorkScheduleId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (requestedSchedule == null)
                {
                    return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Requested work schedule tidak lagi tersedia.");
                }

                var existingAppliedAssignment = await _dbContext.WfpWorkScheduleAssignments
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == entity.WorkforceProfileId &&
                        x.WorkScheduleId == requestedSchedule.Id &&
                        x.EffectiveStartDate == entity.EffectiveStartDate &&
                        !x.IsDelete &&
                        x.Notes != null &&
                        x.Notes.Contains(entity.RequestNumber),
                        cancellationToken);

                if (existingAppliedAssignment == null)
                {
                    var currentAssignment = entity.WorkScheduleAssignmentId.HasValue
                        ? await _dbContext.WfpWorkScheduleAssignments.FirstOrDefaultAsync(
                            x => x.Id == entity.WorkScheduleAssignmentId.Value && !x.IsDelete,
                            cancellationToken)
                        : await ResolveCurrentWorkScheduleAssignmentAsync(
                            entity.WorkforceProfileId,
                            entity.EffectiveStartDate,
                            cancellationToken);

                    var isTemporary = entity.RequestType == SchedulingRequestValueConstants.ScheduleChangeType.TemporarySchedule;

                    if (currentAssignment != null && !isTemporary && currentAssignment.EffectiveStartDate < entity.EffectiveStartDate)
                    {
                        var dayBefore = entity.EffectiveStartDate.AddDays(-1);
                        if (!currentAssignment.EffectiveEndDate.HasValue || currentAssignment.EffectiveEndDate.Value >= entity.EffectiveStartDate)
                        {
                            currentAssignment.EffectiveEndDate = dayBefore;
                            currentAssignment.UpdateDateTime = now;
                            currentAssignment.UpdateBy = actorUserId;
                        }
                    }

                    existingAppliedAssignment = new WfpWorkScheduleAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = entity.WorkforceProfileId,
                        OrganizationAssignmentId = currentAssignment?.OrganizationAssignmentId,
                        HospitalSiteId = currentAssignment?.HospitalSiteId,
                        OrganizationUnitId = currentAssignment?.OrganizationUnitId,
                        DepartmentId = currentAssignment?.DepartmentId,
                        PositionId = currentAssignment?.PositionId,
                        WorkLocationId = currentAssignment?.WorkLocationId,
                        WorkScheduleId = requestedSchedule.Id,
                        ShiftGroupId = currentAssignment?.ShiftGroupId,
                        ShiftPatternId = currentAssignment?.ShiftPatternId,
                        RosterPolicyId = currentAssignment?.RosterPolicyId,
                        MinimumRestPolicyId = currentAssignment?.MinimumRestPolicyId,
                        AssignmentType = isTemporary ? "Temporary" : "Primary",
                        EffectiveStartDate = entity.EffectiveStartDate,
                        EffectiveEndDate = entity.EffectiveEndDate,
                        WeekStartDay = currentAssignment?.WeekStartDay ?? 1,
                        IsPrimary = !isTemporary,
                        IsRotating = currentAssignment?.IsRotating ?? false,
                        IsTemporary = isTemporary,
                        IsActive = true,
                        Notes = $"Applied from schedule change request {entity.RequestNumber}. {NormalizeText(notes)}".Trim(),
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };

                    _dbContext.WfpWorkScheduleAssignments.Add(existingAppliedAssignment);
                }

                entity.WorkScheduleAssignmentId = existingAppliedAssignment.Id;
                applied = true;
            }

            if (!applied)
            {
                return SchedulingRequestServiceResult<ScheduleChangeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak mempunyai perubahan schedule atau shift yang dapat diterapkan.");
            }

            entity.IsAppliedToRoster = true;
            entity.AppliedAt = now;
            entity.RequestStatus = SchedulingRequestValueConstants.ScheduleChangeStatus.Applied;
            entity.ApprovalNotes = NormalizeText(notes) ?? entity.ApprovalNotes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<List<ScheduleChangeOptionResponse>> GetMyScheduleOptionsAsync(
            Guid workforceProfileId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var assignments = await _dbContext.WfpWorkScheduleAssignments
                .AsNoTracking()
                .Include(x => x.WorkSchedule)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= date &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= date))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ToListAsync(cancellationToken);

            return assignments.Select(x => new ScheduleChangeOptionResponse
            {
                Id = x.Id,
                Code = x.WorkSchedule?.ScheduleCode ?? string.Empty,
                Name = x.WorkSchedule?.ScheduleName ?? string.Empty,
                Date = date,
                AdditionalInfo = x.AssignmentType
            }).ToList();
        }

        public async Task<List<ScheduleChangeOptionResponse>> GetAvailableShiftOptionsAsync(
            Guid? workScheduleId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.MstShifts
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete);

            if (workScheduleId.HasValue)
            {
                query = query.Where(x => x.WorkScheduleId == workScheduleId.Value);
            }

            return await query
                .OrderBy(x => x.StartTime)
                .ThenBy(x => x.ShiftName)
                .Select(x => new ScheduleChangeOptionResponse
                {
                    Id = x.Id,
                    Code = x.ShiftCode,
                    Name = x.ShiftName,
                    AdditionalInfo = x.StartTime.ToString("HH:mm") + " - " + x.EndTime.ToString("HH:mm")
                })
                .ToListAsync(cancellationToken);
        }

        private IQueryable<WfpScheduleChangeRequest> BaseQuery()
        {
            return _dbContext.WfpScheduleChangeRequests
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CurrentWorkSchedule)
                .Include(x => x.RequestedWorkSchedule)
                .Include(x => x.CurrentShift)
                .Include(x => x.RequestedShift);
        }

        private static ScheduleChangeListResponse MapList(WfpScheduleChangeRequest x)
        {
            var canEdit = CanEdit(x.RequestStatus);
            return new ScheduleChangeListResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                RequestType = x.RequestType,
                RequestedDate = x.RequestedDate,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                CurrentScheduleName = x.CurrentWorkSchedule?.ScheduleName,
                RequestedScheduleName = x.RequestedWorkSchedule?.ScheduleName,
                CurrentShiftName = x.CurrentShift?.ShiftName,
                RequestedShiftName = x.RequestedShift?.ShiftName,
                Reason = x.Reason,
                RequestStatus = x.RequestStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                IsAppliedToRoster = x.IsAppliedToRoster,
                SubmittedAt = x.SubmittedAt,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                CanEdit = canEdit,
                CanSubmit = canEdit,
                CanCancel = x.RequestStatus != SchedulingRequestValueConstants.ScheduleChangeStatus.Applied &&
                            x.RequestStatus != SchedulingRequestValueConstants.ScheduleChangeStatus.Approved &&
                            x.RequestStatus != SchedulingRequestValueConstants.ScheduleChangeStatus.Rejected &&
                            x.RequestStatus != SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled,
                CanDelete = x.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Draft
            };
        }

        private static ScheduleChangeDetailResponse MapDetail(WfpScheduleChangeRequest x)
        {
            var list = MapList(x);
            return new ScheduleChangeDetailResponse
            {
                Id = list.Id,
                RequestNumber = list.RequestNumber,
                WorkforceProfileId = list.WorkforceProfileId,
                WorkforceProfileCode = list.WorkforceProfileCode,
                WorkforceDisplayName = list.WorkforceDisplayName,
                RequestType = list.RequestType,
                RequestedDate = list.RequestedDate,
                EffectiveStartDate = list.EffectiveStartDate,
                EffectiveEndDate = list.EffectiveEndDate,
                CurrentScheduleName = list.CurrentScheduleName,
                RequestedScheduleName = list.RequestedScheduleName,
                CurrentShiftName = list.CurrentShiftName,
                RequestedShiftName = list.RequestedShiftName,
                Reason = list.Reason,
                RequestStatus = list.RequestStatus,
                WorkflowInstanceId = list.WorkflowInstanceId,
                IsAppliedToRoster = list.IsAppliedToRoster,
                SubmittedAt = list.SubmittedAt,
                CreateDateTime = list.CreateDateTime,
                UpdateDateTime = list.UpdateDateTime,
                CanEdit = list.CanEdit,
                CanSubmit = list.CanSubmit,
                CanCancel = list.CanCancel,
                CanDelete = list.CanDelete,
                WorkScheduleAssignmentId = x.WorkScheduleAssignmentId,
                RosterPeriodId = x.RosterPeriodId,
                CurrentShiftAssignmentId = x.CurrentShiftAssignmentId,
                RequestedShiftAssignmentId = x.RequestedShiftAssignmentId,
                CurrentWorkScheduleId = x.CurrentWorkScheduleId,
                RequestedWorkScheduleId = x.RequestedWorkScheduleId,
                CurrentShiftId = x.CurrentShiftId,
                RequestedShiftId = x.RequestedShiftId,
                RequestReasonId = x.RequestReasonId,
                RejectionReasonId = x.RejectionReasonId,
                WorkflowDefinitionId = x.WorkflowDefinitionId,
                AttachmentPath = x.AttachmentPath,
                ApprovalNotes = x.ApprovalNotes,
                ApprovedAt = x.ApprovedAt,
                RejectedAt = x.RejectedAt,
                AppliedAt = x.AppliedAt
            };
        }

        private async Task<WfpWorkScheduleAssignment?> ResolveCurrentWorkScheduleAssignmentAsync(
            Guid workforceProfileId,
            DateOnly date,
            CancellationToken cancellationToken)
        {
            return await _dbContext.WfpWorkScheduleAssignments
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= date &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= date))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static void ApplyShiftToAssignment(
            TrxShiftAssignment assignment,
            MstShift shift,
            DateOnly shiftDate,
            Guid requestId,
            Guid actorUserId,
            DateTime now)
        {
            var start = DateTime.SpecifyKind(shiftDate.ToDateTime(shift.StartTime), DateTimeKind.Utc);
            var endDate = shift.IsOvernight || shift.EndTime <= shift.StartTime
                ? shiftDate.AddDays(1)
                : shiftDate;
            var end = DateTime.SpecifyKind(endDate.ToDateTime(shift.EndTime), DateTimeKind.Utc);

            assignment.WorkScheduleId = shift.WorkScheduleId;
            assignment.ShiftId = shift.Id;
            assignment.ShiftDate = shiftDate;
            assignment.ScheduledStartAt = start;
            assignment.ScheduledEndAt = end;
            assignment.BreakDurationMinutes = shift.BreakDurationMinutes;
            assignment.PlannedWorkMinutes = shift.PaidWorkMinutes > 0
                ? shift.PaidWorkMinutes
                : Math.Max(0, (int)(end - start).TotalMinutes - shift.BreakDurationMinutes);
            assignment.IsNightShift = shift.IsNightShift;
            assignment.IsOnCall = shift.IsOnCall;
            assignment.IsDayOff = shift.IsOffShift;
            assignment.AssignmentSource = "ScheduleChange";
            assignment.ScheduleChangeRequestId = requestId;
            assignment.IsManualOverride = true;
            assignment.UpdateDateTime = now;
            assignment.UpdateBy = actorUserId;
        }

        private static bool CanEdit(string status)
        {
            return status == SchedulingRequestValueConstants.ScheduleChangeStatus.Draft ||
                   status == SchedulingRequestValueConstants.ScheduleChangeStatus.NeedRevision;
        }

        private static bool IsOwnedBy(
            WfpScheduleChangeRequest entity,
            Guid workforceProfileId,
            Guid actorUserId)
        {
            return entity.WorkforceProfileId == workforceProfileId &&
                   (entity.CreateBy == actorUserId ||
                    entity.SubmittedByUserId == actorUserId ||
                    entity.CreateBy == Guid.Empty);
        }

        private static string? NormalizeRequestType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return SchedulingRequestValueConstants.ScheduleChangeType.All
                .FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty ? value : null;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string GenerateRequestNumber(string prefix)
        {
            return $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(50, prefix.Length + 1 + 14 + 1 + 32)];
        }
    }
}
