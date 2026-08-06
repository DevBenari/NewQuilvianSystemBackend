using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class ResignationRequestService
    {
        private readonly ApplicationDbContext _dbContext;

        public ResignationRequestService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ResignationFilterMetadataResponse GetFilterMetadata()
        {
            return new ResignationFilterMetadataResponse
            {
                RequestStatusOptions = new List<string>
                {
                    ResignationValueConstants.Status.Draft,
                    ResignationValueConstants.Status.Submitted,
                    ResignationValueConstants.Status.UnderReview,
                    ResignationValueConstants.Status.NeedRevision,
                    ResignationValueConstants.Status.Approved,
                    ResignationValueConstants.Status.HandoffCompleted,
                    ResignationValueConstants.Status.Rejected,
                    ResignationValueConstants.Status.Cancelled
                },
                SortDirections = ResignationValueConstants.SortDirections.ToList(),
                PageSizeOptions = ResignationValueConstants.PageSizes.ToList()
            };
        }

        public async Task<ResignationSummaryResponse> GetSummaryAsync(
            Guid? workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxResignationRequests
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            return new ResignationSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(x => x.RequestStatus == ResignationValueConstants.Status.Draft, cancellationToken),
                WaitingApproval = await query.CountAsync(x =>
                    x.RequestStatus == ResignationValueConstants.Status.Submitted ||
                    x.RequestStatus == ResignationValueConstants.Status.UnderReview,
                    cancellationToken),
                NeedRevision = await query.CountAsync(x => x.RequestStatus == ResignationValueConstants.Status.NeedRevision, cancellationToken),
                Approved = await query.CountAsync(x => x.RequestStatus == ResignationValueConstants.Status.Approved, cancellationToken),
                HandoffCompleted = await query.CountAsync(x => x.RequestStatus == ResignationValueConstants.Status.HandoffCompleted, cancellationToken),
                Rejected = await query.CountAsync(x => x.RequestStatus == ResignationValueConstants.Status.Rejected, cancellationToken),
                Cancelled = await query.CountAsync(x => x.RequestStatus == ResignationValueConstants.Status.Cancelled, cancellationToken)
            };
        }

        public async Task<PagedResult<ResignationListResponse>> GetPagedAsync(
            Guid? workforceProfileId,
            string? requestStatus,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = ResignationValueConstants.PageSizes.Contains(pageSize) ? pageSize : 25;

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

            if (startDate.HasValue)
            {
                query = query.Where(x => x.ProposedLastWorkingDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.ProposedLastWorkingDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.ResignationReason.ToLower().Contains(keyword) ||
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

            return new PagedResult<ResignationListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(MapList).ToList()
            };
        }

        public async Task<ResignationServiceResult<ResignationDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return entity == null
                ? ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan.")
                : ResignationServiceResult<ResignationDetailResponse>.Ok(
                    MapDetail(entity),
                    "Detail pengajuan resign berhasil diambil.");
        }

        public async Task<ResignationServiceResult<ResignationDetailResponse>> CreateDraftAsync(
            Guid workforceProfileId,
            Guid? employeeId,
            Guid actorUserId,
            CreateResignationSelfServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateAsync(
                workforceProfileId,
                employeeId,
                request,
                null,
                cancellationToken);

            if (!validation.Success)
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var now = DateTime.UtcNow;
            var entity = new TrxResignationRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = GenerateRequestNumber(),
                WorkforceProfileId = workforceProfileId,
                EmployeeId = NormalizeGuid(employeeId),
                RequestReasonId = NormalizeGuid(request.RequestReasonId),
                RequestDate = now,
                ProposedLastWorkingDate = NormalizeUtcDate(request.ProposedLastWorkingDate),
                NoticePeriodDays = CalculateNoticePeriodDays(now, request.ProposedLastWorkingDate),
                ResignationReason = request.ResignationReason.Trim(),
                HandoverPlan = NormalizeText(request.HandoverPlan),
                RequestStatus = ResignationValueConstants.Status.Draft,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.TrxResignationRequests.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ResignationServiceResult<ResignationDetailResponse>> UpdateDraftAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            UpdateResignationSelfServiceRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxResignationRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan atau bukan milik user login.");
            }

            if (!CanEdit(entity.RequestStatus))
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan hanya dapat diubah pada status Draft atau NeedRevision.");
            }

            var validation = await ValidateAsync(
                workforceProfileId,
                entity.EmployeeId,
                request,
                id,
                cancellationToken);

            if (!validation.Success)
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
            entity.ProposedLastWorkingDate = NormalizeUtcDate(request.ProposedLastWorkingDate);
            entity.NoticePeriodDays = CalculateNoticePeriodDays(DateTime.UtcNow, request.ProposedLastWorkingDate);
            entity.ResignationReason = request.ResignationReason.Trim();
            entity.HandoverPlan = NormalizeText(request.HandoverPlan);
            entity.RequestStatus = ResignationValueConstants.Status.Draft;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ResignationServiceResult<ResignationDetailResponse>> PrepareSubmitAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxResignationRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus == ResignationValueConstants.Status.Submitted ||
                entity.RequestStatus == ResignationValueConstants.Status.UnderReview)
            {
                return await GetByIdAsync(entity.Id, cancellationToken);
            }

            if (!CanEdit(entity.RequestStatus))
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak dapat di-submit dari status saat ini.");
            }

            var validationRequest = new CreateResignationSelfServiceRequest
            {
                RequestReasonId = entity.RequestReasonId,
                ProposedLastWorkingDate = entity.ProposedLastWorkingDate,
                ResignationReason = entity.ResignationReason,
                HandoverPlan = entity.HandoverPlan
            };

            var validation = await ValidateAsync(
                entity.WorkforceProfileId,
                entity.EmployeeId,
                validationRequest,
                entity.Id,
                cancellationToken);

            if (!validation.Success)
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            entity.RequestStatus = ResignationValueConstants.Status.Submitted;
            entity.SubmittedAt ??= DateTime.UtcNow;
            entity.SubmittedByUserId = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ResignationServiceResult<ResignationDetailResponse>> CancelAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxResignationRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus == ResignationValueConstants.Status.Approved ||
                entity.RequestStatus == ResignationValueConstants.Status.HandoffCompleted ||
                entity.RequestStatus == ResignationValueConstants.Status.Rejected ||
                entity.RequestStatus == ResignationValueConstants.Status.Cancelled)
            {
                return ResignationServiceResult<ResignationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan tidak dapat dibatalkan dari status saat ini.");
            }

            entity.RequestStatus = ResignationValueConstants.Status.Cancelled;
            entity.WithdrawnAt = DateTime.UtcNow;
            entity.WithdrawalReason = NormalizeText(reason);
            entity.IsCancel = true;
            entity.CancelDateTime = DateTime.UtcNow;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ResignationServiceResult<object>> DeleteAsync(
            Guid id,
            Guid workforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxResignationRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null || !IsOwnedBy(entity, workforceProfileId, actorUserId))
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan atau bukan milik user login.");
            }

            if (entity.RequestStatus != ResignationValueConstants.Status.Draft)
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya draft yang dapat dihapus.");
            }

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResignationServiceResult<object>.Ok(
                new { entity.Id },
                "Draft pengajuan resign berhasil dihapus.");
        }

        private async Task<ResignationServiceResult<object>> ValidateAsync(
            Guid workforceProfileId,
            Guid? employeeId,
            CreateResignationSelfServiceRequest request,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile tidak valid.");
            }

            if (string.IsNullOrWhiteSpace(request.ResignationReason))
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan resign wajib diisi.");
            }

            var proposedDate = NormalizeUtcDate(request.ProposedLastWorkingDate);
            if (proposedDate.Date <= DateTime.UtcNow.Date)
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Proposed last working date harus lebih besar dari tanggal hari ini.");
            }

            var workforceExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && x.IsActive && !x.IsDelete, cancellationToken);

            if (!workforceExists)
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan atau tidak aktif.");
            }

            if (employeeId.HasValue)
            {
                var employeeExists = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == employeeId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!employeeExists)
                {
                    return ResignationServiceResult<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Employee yang terhubung dengan akun tidak ditemukan atau tidak aktif.");
                }
            }

            if (request.RequestReasonId.HasValue)
            {
                var reasonExists = await _dbContext.MstRequestReasons
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.RequestReasonId.Value && x.IsActive && !x.IsDelete, cancellationToken);

                if (!reasonExists)
                {
                    return ResignationServiceResult<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Request reason tidak ditemukan atau tidak aktif.");
                }
            }

            var openStatuses = new[]
            {
                ResignationValueConstants.Status.Draft,
                ResignationValueConstants.Status.Submitted,
                ResignationValueConstants.Status.UnderReview,
                ResignationValueConstants.Status.NeedRevision,
                ResignationValueConstants.Status.Approved,
                ResignationValueConstants.Status.HandoffCompleted
            };

            var hasOpenRequest = await _dbContext.TrxResignationRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    openStatuses.Contains(x.RequestStatus) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value),
                    cancellationToken);

            if (hasOpenRequest)
            {
                return ResignationServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Masih terdapat pengajuan resign aktif untuk employee tersebut.");
            }

            return ResignationServiceResult<object>.Ok(
                new { },
                "Data pengajuan resign valid.");
        }

        private IQueryable<TrxResignationRequest> BaseQuery()
        {
            return _dbContext.TrxResignationRequests
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee);
        }

        private static ResignationListResponse MapList(TrxResignationRequest x)
        {
            var canEdit = CanEdit(x.RequestStatus);
            return new ResignationListResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                WorkforceProfileId = x.WorkforceProfileId,
                EmployeeId = x.EmployeeId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                RequestDate = x.RequestDate,
                ProposedLastWorkingDate = x.ProposedLastWorkingDate,
                NoticePeriodDays = x.NoticePeriodDays,
                ResignationReason = x.ResignationReason,
                RequestStatus = x.RequestStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                EmployeeSeparationId = x.EmployeeSeparationId,
                SubmittedAt = x.SubmittedAt,
                ApprovedAt = x.ApprovedAt,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                CanEdit = canEdit,
                CanSubmit = canEdit,
                CanCancel = x.RequestStatus != ResignationValueConstants.Status.Approved &&
                            x.RequestStatus != ResignationValueConstants.Status.HandoffCompleted &&
                            x.RequestStatus != ResignationValueConstants.Status.Rejected &&
                            x.RequestStatus != ResignationValueConstants.Status.Cancelled,
                CanDelete = x.RequestStatus == ResignationValueConstants.Status.Draft
            };
        }

        private static ResignationDetailResponse MapDetail(TrxResignationRequest x)
        {
            var list = MapList(x);
            return new ResignationDetailResponse
            {
                Id = list.Id,
                RequestNumber = list.RequestNumber,
                WorkforceProfileId = list.WorkforceProfileId,
                EmployeeId = list.EmployeeId,
                WorkforceProfileCode = list.WorkforceProfileCode,
                WorkforceDisplayName = list.WorkforceDisplayName,
                RequestDate = list.RequestDate,
                ProposedLastWorkingDate = list.ProposedLastWorkingDate,
                NoticePeriodDays = list.NoticePeriodDays,
                ResignationReason = list.ResignationReason,
                RequestStatus = list.RequestStatus,
                WorkflowInstanceId = list.WorkflowInstanceId,
                EmployeeSeparationId = list.EmployeeSeparationId,
                SubmittedAt = list.SubmittedAt,
                ApprovedAt = list.ApprovedAt,
                CreateDateTime = list.CreateDateTime,
                UpdateDateTime = list.UpdateDateTime,
                CanEdit = list.CanEdit,
                CanSubmit = list.CanSubmit,
                CanCancel = list.CanCancel,
                CanDelete = list.CanDelete,
                RequestReasonId = x.RequestReasonId,
                RejectionReasonId = x.RejectionReasonId,
                WorkflowDefinitionId = x.WorkflowDefinitionId,
                HandoverPlan = x.HandoverPlan,
                ManagerComment = x.ManagerComment,
                RejectedAt = x.RejectedAt,
                WithdrawnAt = x.WithdrawnAt,
                WithdrawalReason = x.WithdrawalReason
            };
        }

        private static bool CanEdit(string status)
        {
            return status == ResignationValueConstants.Status.Draft ||
                   status == ResignationValueConstants.Status.NeedRevision;
        }

        private static bool IsOwnedBy(
            TrxResignationRequest entity,
            Guid workforceProfileId,
            Guid actorUserId)
        {
            return entity.WorkforceProfileId == workforceProfileId &&
                   (entity.CreateBy == actorUserId ||
                    entity.SubmittedByUserId == actorUserId ||
                    entity.CreateBy == Guid.Empty);
        }

        private static int CalculateNoticePeriodDays(DateTime requestDate, DateTime proposedLastWorkingDate)
        {
            return Math.Max(0, (NormalizeUtcDate(proposedLastWorkingDate).Date - requestDate.Date).Days);
        }

        private static DateTime NormalizeUtcDate(DateTime value)
        {
            var date = value.Date;
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
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
            var value = $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            return value.Length <= 50 ? value : value[..50];
        }
    }
}
