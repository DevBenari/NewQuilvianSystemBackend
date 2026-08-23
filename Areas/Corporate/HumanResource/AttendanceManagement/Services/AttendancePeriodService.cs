using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendancePeriodService
    {
        private static readonly string[] ActiveCorrectionStatuses =
        {
            AttendanceValueConstants.CorrectionRequestStatus.Draft,
            AttendanceValueConstants.CorrectionRequestStatus.Submitted,
            AttendanceValueConstants.CorrectionRequestStatus.UnderReview,
            AttendanceValueConstants.CorrectionRequestStatus.NeedRevision,
            AttendanceValueConstants.CorrectionRequestStatus.Approved,
            AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved
        };

        private static readonly string[] OpenSchedulerStatuses =
        {
            AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending,
            AttendanceValueConstants.AttendanceSchedulerJobStatus.Running,
            AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AttendancePeriodService> _logger;

        public AttendancePeriodService(
            ApplicationDbContext dbContext,
            ILogger<AttendancePeriodService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public AttendancePeriodMetadataResponse GetMetadata()
        {
            return new AttendancePeriodMetadataResponse
            {
                PeriodStatusOptions = new List<AttendanceStringOptionResponse>
                {
                    Option(AttendanceValueConstants.AttendancePeriodStatus.Open, "Open"),
                    Option(AttendanceValueConstants.AttendancePeriodStatus.Closing, "Closing"),
                    Option(AttendanceValueConstants.AttendancePeriodStatus.Closed, "Closed"),
                    Option(AttendanceValueConstants.AttendancePeriodStatus.Reopened, "Reopened"),
                    Option(AttendanceValueConstants.AttendancePeriodStatus.Cancelled, "Cancelled")
                },
                SortOptions = new List<AttendanceSortOptionResponse>
                {
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "endDate", Label = "Tanggal selesai" },
                    new() { Value = "periodCode", Label = "Kode periode" },
                    new() { Value = "periodName", Label = "Nama periode" },
                    new() { Value = "periodStatus", Label = "Status" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<AttendancePeriodSummaryResponse> GetSummaryAsync(
            AttendancePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            var now = DateTime.UtcNow;
            return new AttendancePeriodSummaryResponse
            {
                TotalPeriod = await query.CountAsync(cancellationToken),
                OpenPeriod = await query.CountAsync(x => x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Open, cancellationToken),
                ClosedPeriod = await query.CountAsync(x => x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed, cancellationToken),
                ReopenedPeriod = await query.CountAsync(x => x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Reopened, cancellationToken),
                CancelledPeriod = await query.CountAsync(x => x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Cancelled, cancellationToken),
                ScheduledToClose = await query.CountAsync(x => x.ScheduledCloseAt.HasValue && x.ScheduledCloseAt <= now && x.PeriodStatus != AttendanceValueConstants.AttendancePeriodStatus.Closed, cancellationToken)
            };
        }

        public async Task<AttendancePeriodPagedResponse> GetPagedAsync(
            AttendancePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplyFilter(BuildBaseQuery(), request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AttendancePeriodResponse
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TotalDays = x.EndDate.DayNumber - x.StartDate.DayNumber + 1,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PeriodStatus = x.PeriodStatus,
                    RequirePayrollHandoff = x.RequirePayrollHandoff,
                    ScheduledCloseAt = x.ScheduledCloseAt,
                    LastValidatedAt = x.LastValidatedAt,
                    ClosedAt = x.ClosedAt,
                    ReopenedAt = x.ReopenedAt,
                    ReopenCount = x.ReopenCount,
                    AttendanceDailyCount = x.AttendanceDailies.Count(a => !a.IsDelete),
                    SchedulerJobCount = x.SchedulerJobs.Count(j => !j.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new AttendancePeriodPagedResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<List<AttendancePeriodOptionResponse>> GetOptionsAsync(
            string? search,
            bool onlyOpen,
            int take,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 200);
            var query = BuildBaseQuery();
            if (onlyOpen)
            {
                query = query.Where(x =>
                    x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Open ||
                    x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Reopened);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PeriodCode.ToLower().Contains(keyword) ||
                    x.PeriodName.ToLower().Contains(keyword));
            }

            return await query
                .OrderByDescending(x => x.StartDate)
                .Take(take)
                .Select(x => new AttendancePeriodOptionResponse
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PeriodStatus = x.PeriodStatus
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<AttendancePeriodDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new AttendancePeriodDetailResponse
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TotalDays = x.EndDate.DayNumber - x.StartDate.DayNumber + 1,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PeriodStatus = x.PeriodStatus,
                    RequirePayrollHandoff = x.RequirePayrollHandoff,
                    ScheduledCloseAt = x.ScheduledCloseAt,
                    LastProcessingRunId = x.LastProcessingRunId,
                    LastProcessingRunNumber = x.LastProcessingRun != null ? x.LastProcessingRun.RunNumber : null,
                    LastValidatedAt = x.LastValidatedAt,
                    ValidationSnapshotJson = x.ValidationSnapshotJson,
                    ClosedAt = x.ClosedAt,
                    ClosedByUserId = x.ClosedByUserId,
                    ClosedByUserName = x.ClosedByUser != null ? x.ClosedByUser.DisplayName ?? x.ClosedByUser.UserName ?? x.ClosedByUser.Email ?? x.ClosedByUser.UserCode : null,
                    CloseReason = x.CloseReason,
                    ReopenedAt = x.ReopenedAt,
                    ReopenedByUserId = x.ReopenedByUserId,
                    ReopenedByUserName = x.ReopenedByUser != null ? x.ReopenedByUser.DisplayName ?? x.ReopenedByUser.UserName ?? x.ReopenedByUser.Email ?? x.ReopenedByUser.UserCode : null,
                    ReopenReason = x.ReopenReason,
                    ReopenCount = x.ReopenCount,
                    AttendanceDailyCount = x.AttendanceDailies.Count(a => !a.IsDelete),
                    SchedulerJobCount = x.SchedulerJobs.Count(j => !j.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>> CreateAsync(
            CreateAttendancePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateRequestAsync(null, request, cancellationToken);
            if (validation != null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>.Fail(StatusCodes.Status400BadRequest, validation);
            }

            var entity = new HrdAttendancePeriod
            {
                Id = Guid.NewGuid(),
                PeriodCode = await GeneratePeriodCodeAsync(request.StartDate, cancellationToken),
                PeriodName = request.PeriodName.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Open,
                RequirePayrollHandoff = request.RequirePayrollHandoff,
                ScheduledCloseAt = NormalizeUtc(request.ScheduledCloseAt),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                UpdateBy = actorUserId
            };

            _dbContext.Set<HrdAttendancePeriod>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>.Ok(detail!, "Attendance period berhasil dibuat.");
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>> UpdateAsync(
            Guid id,
            UpdateAttendancePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendancePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
            }

            if (!IsEditableStatus(entity.PeriodStatus))
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>.Fail(StatusCodes.Status409Conflict, "Hanya attendance period Open atau Reopened yang dapat diubah.");
            }

            var validation = await ValidateRequestAsync(id, request, cancellationToken);
            if (validation != null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>.Fail(StatusCodes.Status400BadRequest, validation);
            }

            entity.PeriodName = request.PeriodName.Trim();
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId);
            entity.RequirePayrollHandoff = request.RequirePayrollHandoff;
            entity.ScheduledCloseAt = NormalizeUtc(request.ScheduledCloseAt);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return AttendancePeriodSchedulerServiceResult<AttendancePeriodDetailResponse>.Ok(detail!, "Attendance period berhasil diperbarui.");
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodClosePreviewResponse>> PreviewCloseAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var period = await _dbContext.Set<HrdAttendancePeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (period == null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodClosePreviewResponse>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
            }

            var preview = await BuildClosePreviewAsync(period, cancellationToken);
            return AttendancePeriodSchedulerServiceResult<AttendancePeriodClosePreviewResponse>.Ok(preview, "Validasi penutupan attendance period berhasil dijalankan.");
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>> CloseAsync(
            Guid id,
            CloseAttendancePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan penutupan wajib diisi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _dbContext.Set<HrdAttendancePeriod>()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
                if (period == null)
                {
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
                }

                if (!IsEditableStatus(period.PeriodStatus))
                {
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status409Conflict, "Attendance period hanya dapat ditutup dari status Open atau Reopened.");
                }

                var preview = await BuildClosePreviewAsync(period, cancellationToken);
                if (!preview.CanClose)
                {
                    var firstIssue = preview.Issues.FirstOrDefault(x => x.IsBlocking)?.Message ?? "Attendance period belum memenuhi validasi penutupan.";
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status409Conflict, firstIssue);
                }

                var now = DateTime.UtcNow;
                var previousStatus = period.PeriodStatus;
                period.PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Closing;
                period.LastValidatedAt = preview.ValidatedAt;
                period.ValidationSnapshotJson = JsonSerializer.Serialize(preview);
                period.UpdateDateTime = now;
                period.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var dailies = await ApplyScope(
                        _dbContext.Set<HrdAttendanceDaily>().Where(x => !x.IsDelete && x.AttendanceDate >= period.StartDate && x.AttendanceDate <= period.EndDate),
                        period)
                    .ToListAsync(cancellationToken);

                foreach (var daily in dailies)
                {
                    daily.AttendancePeriodId = period.Id;
                    daily.IsLocked = true;
                    daily.UpdateDateTime = now;
                    daily.UpdateBy = actorUserId;
                }

                period.PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Closed;
                period.ClosedAt = now;
                period.ClosedByUserId = actorUserId;
                period.CloseReason = request.Reason.Trim();
                period.ReopenedAt = null;
                period.ReopenedByUserId = null;
                period.ReopenReason = null;
                period.UpdateDateTime = now;
                period.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Ok(
                    new AttendancePeriodActionResponse
                    {
                        AttendancePeriodId = period.Id,
                        PeriodCode = period.PeriodCode,
                        PreviousStatus = previousStatus,
                        CurrentStatus = period.PeriodStatus,
                        AffectedAttendanceDailyCount = dailies.Count,
                        ActionAt = now
                    },
                    "Attendance period berhasil ditutup dan attendance daily berhasil dikunci.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Penutupan attendance period {AttendancePeriodId} gagal.", id);
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status500InternalServerError, "Penutupan attendance period gagal diproses.");
            }
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>> ReopenAsync(
            Guid id,
            ReopenAttendancePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan reopen wajib diisi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _dbContext.Set<HrdAttendancePeriod>()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
                if (period == null)
                {
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
                }

                if (period.PeriodStatus != AttendanceValueConstants.AttendancePeriodStatus.Closed)
                {
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status409Conflict, "Hanya attendance period Closed yang dapat dibuka kembali.");
                }

                var dailies = await _dbContext.Set<HrdAttendanceDaily>()
                    .Where(x => x.AttendancePeriodId == period.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                var payrollLinked = dailies.Count(x =>
                    x.PayrollPeriodId.HasValue ||
                    x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed);
                if (payrollLinked > 0)
                {
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Terdapat {payrollLinked} attendance yang sudah terhubung ke payroll. Jalankan rollback Attendance Payroll Handoff terlebih dahulu.");
                }

                var activeJobs = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                    .AsNoTracking()
                    .CountAsync(x => x.AttendancePeriodId == period.Id && !x.IsDelete && OpenSchedulerStatuses.Contains(x.JobStatus), cancellationToken);
                if (activeJobs > 0)
                {
                    return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status409Conflict, "Attendance period masih memiliki scheduler job aktif.");
                }

                var now = DateTime.UtcNow;
                foreach (var daily in dailies)
                {
                    daily.AttendancePeriodId = null;
                    daily.IsLocked = false;
                    daily.UpdateDateTime = now;
                    daily.UpdateBy = actorUserId;
                }

                var previousStatus = period.PeriodStatus;
                period.PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Reopened;
                period.ReopenedAt = now;
                period.ReopenedByUserId = actorUserId;
                period.ReopenReason = request.Reason.Trim();
                period.ReopenCount += 1;
                period.UpdateDateTime = now;
                period.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Ok(
                    new AttendancePeriodActionResponse
                    {
                        AttendancePeriodId = period.Id,
                        PeriodCode = period.PeriodCode,
                        PreviousStatus = previousStatus,
                        CurrentStatus = period.PeriodStatus,
                        AffectedAttendanceDailyCount = dailies.Count,
                        ActionAt = now
                    },
                    "Attendance period berhasil dibuka kembali.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Reopen attendance period {AttendancePeriodId} gagal.", id);
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status500InternalServerError, "Reopen attendance period gagal diproses.");
            }
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>> CancelAsync(
            Guid id,
            CancelAttendancePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");
            }

            var period = await _dbContext.Set<HrdAttendancePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (period == null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
            }

            if (!IsEditableStatus(period.PeriodStatus))
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status409Conflict, "Attendance period Closed tidak dapat dibatalkan.");
            }

            var activeJobs = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .AsNoTracking()
                .CountAsync(x => x.AttendancePeriodId == id && !x.IsDelete && OpenSchedulerStatuses.Contains(x.JobStatus), cancellationToken);
            if (activeJobs > 0)
            {
                return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Fail(StatusCodes.Status409Conflict, "Batalkan scheduler job aktif sebelum membatalkan attendance period.");
            }

            var previousStatus = period.PeriodStatus;
            var now = DateTime.UtcNow;
            period.PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Cancelled;
            period.IsCancel = true;
            period.CancelDateTime = now;
            period.CancelBy = actorUserId;
            period.CloseReason = request.Reason.Trim();
            period.UpdateDateTime = now;
            period.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>.Ok(
                new AttendancePeriodActionResponse
                {
                    AttendancePeriodId = period.Id,
                    PeriodCode = period.PeriodCode,
                    PreviousStatus = previousStatus,
                    CurrentStatus = period.PeriodStatus,
                    ActionAt = now
                },
                "Attendance period berhasil dibatalkan.");
        }

        public async Task<AttendancePeriodSchedulerServiceResult<object>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _dbContext.Set<HrdAttendancePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (period == null)
            {
                return AttendancePeriodSchedulerServiceResult<object>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
            }

            if (!IsEditableStatus(period.PeriodStatus) && period.PeriodStatus != AttendanceValueConstants.AttendancePeriodStatus.Cancelled)
            {
                return AttendancePeriodSchedulerServiceResult<object>.Fail(StatusCodes.Status409Conflict, "Attendance period Closed tidak dapat dihapus.");
            }

            var isUsed = await _dbContext.Set<HrdAttendanceDaily>().AsNoTracking().AnyAsync(x => x.AttendancePeriodId == id && !x.IsDelete, cancellationToken) ||
                         await _dbContext.Set<HrdAttendanceSchedulerJob>().AsNoTracking().AnyAsync(x => x.AttendancePeriodId == id && !x.IsDelete, cancellationToken);
            if (isUsed)
            {
                return AttendancePeriodSchedulerServiceResult<object>.Fail(StatusCodes.Status409Conflict, "Attendance period tidak dapat dihapus karena sudah digunakan.");
            }

            var now = DateTime.UtcNow;
            period.IsDelete = true;
            period.IsActive = false;
            period.DeleteDateTime = now;
            period.DeleteBy = actorUserId;
            period.UpdateDateTime = now;
            period.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return AttendancePeriodSchedulerServiceResult<object>.Ok(new { period.Id, period.PeriodCode }, "Attendance period berhasil dihapus.");
        }

        private async Task<AttendancePeriodClosePreviewResponse> BuildClosePreviewAsync(
            HrdAttendancePeriod period,
            CancellationToken cancellationToken)
        {
            var dailyQuery = ApplyScope(
                _dbContext.Set<HrdAttendanceDaily>().AsNoTracking().Where(x => !x.IsDelete && x.AttendanceDate >= period.StartDate && x.AttendanceDate <= period.EndDate),
                period);

            var totalDaily = await dailyQuery.CountAsync(cancellationToken);
            var processedDaily = await dailyQuery.CountAsync(x =>
                x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed ||
                x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Skipped,
                cancellationToken);
            var unprocessedDaily = totalDaily - processedDaily;
            var pendingPayroll = period.RequirePayrollHandoff
                ? await dailyQuery.CountAsync(x =>
                    x.IsPayrollEligible &&
                    x.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Processed &&
                    x.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Excluded,
                    cancellationToken)
                : 0;

            var dailyIdsQuery = dailyQuery.Select(x => x.Id);
            var blockingException = await _dbContext.Set<HrdAttendanceException>()
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.IsPayrollBlocking &&
                    (x.ExceptionStatus == AttendanceValueConstants.AttendanceExceptionStatus.Open ||
                     x.ExceptionStatus == AttendanceValueConstants.AttendanceExceptionStatus.UnderReview) &&
                    dailyIdsQuery.Contains(x.AttendanceDailyId),
                    cancellationToken);

            var activeCorrection = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.AttendanceDailyId.HasValue &&
                    dailyIdsQuery.Contains(x.AttendanceDailyId.Value) &&
                    ActiveCorrectionStatuses.Contains(x.RequestStatus),
                    cancellationToken);

            var startUtc = DateTime.SpecifyKind(period.StartDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtcExclusive = DateTime.SpecifyKind(period.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var rawQuery = _dbContext.Set<HrdAttendanceRawLog>().AsNoTracking().Where(x => !x.IsDelete && x.EventAt >= startUtc && x.EventAt < endUtcExclusive);
            if (period.HospitalSiteId.HasValue)
            {
                rawQuery = rawQuery.Where(x => x.HospitalSiteId == period.HospitalSiteId);
            }
            var pendingRawLog = await rawQuery.CountAsync(x =>
                x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Pending ||
                x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Matched ||
                x.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Error,
                cancellationToken);

            var processingQuery = _dbContext.Set<HrdAttendanceProcessingRun>().AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.StartDate <= period.EndDate &&
                x.EndDate >= period.StartDate &&
                (x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Pending ||
                 x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Running));
            processingQuery = ApplyProcessingScope(processingQuery, period);
            var runningProcessing = await processingQuery.CountAsync(cancellationToken);

            var schedulerQuery = _dbContext.Set<HrdAttendanceSchedulerJob>().AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.StartDate <= period.EndDate &&
                x.EndDate >= period.StartDate &&
                OpenSchedulerStatuses.Contains(x.JobStatus));
            schedulerQuery = ApplySchedulerScope(schedulerQuery, period);
            var runningScheduler = await schedulerQuery.CountAsync(cancellationToken);

            var linkedToOtherPeriod = await dailyQuery.CountAsync(x => x.AttendancePeriodId.HasValue && x.AttendancePeriodId != period.Id, cancellationToken);
            var lockedWithoutKnownSource = await dailyQuery.CountAsync(x =>
                x.IsLocked &&
                !x.AttendancePeriodId.HasValue &&
                x.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Processed,
                cancellationToken);

            var issues = new List<AttendancePeriodValidationIssueResponse>();
            AddIssue(issues, "NO_ATTENDANCE_DATA", "Critical", "Belum ada attendance daily pada periode dan scope ini.", totalDaily == 0 ? 1 : 0, totalDaily == 0);
            AddIssue(issues, "UNPROCESSED_ATTENDANCE", "Critical", "Masih terdapat attendance yang belum selesai diproses.", unprocessedDaily, unprocessedDaily > 0);
            AddIssue(issues, "PENDING_PAYROLL_HANDOFF", "Critical", "Masih terdapat attendance payroll eligible yang belum dikirim ke payroll.", pendingPayroll, pendingPayroll > 0);
            AddIssue(issues, "PAYROLL_BLOCKING_EXCEPTION", "Critical", "Masih terdapat attendance exception yang memblokir payroll.", blockingException, blockingException > 0);
            AddIssue(issues, "ACTIVE_CORRECTION_REQUEST", "Critical", "Masih terdapat attendance correction request aktif.", activeCorrection, activeCorrection > 0);
            AddIssue(issues, "PENDING_RAW_LOG", "Warning", "Masih terdapat raw log Pending, Matched, atau Error pada periode ini.", pendingRawLog, pendingRawLog > 0);
            AddIssue(issues, "RUNNING_PROCESSING", "Critical", "Masih terdapat attendance processing run aktif.", runningProcessing, runningProcessing > 0);
            AddIssue(issues, "RUNNING_SCHEDULER_JOB", "Critical", "Masih terdapat scheduler job aktif.", runningScheduler, runningScheduler > 0);
            AddIssue(issues, "LINKED_TO_OTHER_PERIOD", "Critical", "Terdapat attendance yang sudah terhubung ke attendance period lain.", linkedToOtherPeriod, linkedToOtherPeriod > 0);
            AddIssue(issues, "UNKNOWN_LOCK_SOURCE", "Critical", "Terdapat attendance terkunci yang bukan berasal dari payroll handoff atau attendance period.", lockedWithoutKnownSource, lockedWithoutKnownSource > 0);

            return new AttendancePeriodClosePreviewResponse
            {
                AttendancePeriodId = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodStatus = period.PeriodStatus,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                CanClose = issues.All(x => !x.IsBlocking),
                TotalAttendanceDaily = totalDaily,
                ProcessedAttendanceDaily = processedDaily,
                UnprocessedAttendanceDaily = unprocessedDaily,
                PendingPayrollHandoff = pendingPayroll,
                OpenPayrollBlockingException = blockingException,
                ActiveCorrectionRequest = activeCorrection,
                PendingRawLog = pendingRawLog,
                RunningProcessingRun = runningProcessing,
                RunningSchedulerJob = runningScheduler,
                LinkedToOtherPeriod = linkedToOtherPeriod,
                ValidatedAt = DateTime.UtcNow,
                Issues = issues
            };
        }

        private async Task<string?> ValidateRequestAsync(
            Guid? excludeId,
            CreateAttendancePeriodRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PeriodName))
            {
                return "Nama attendance period wajib diisi.";
            }
            if (request.EndDate < request.StartDate)
            {
                return "Tanggal selesai tidak boleh lebih kecil dari tanggal mulai.";
            }
            if (request.EndDate.DayNumber - request.StartDate.DayNumber + 1 > 366)
            {
                return "Rentang attendance period maksimal 366 hari.";
            }

            var legalEntityId = NormalizeGuid(request.LegalEntityId);
            var hospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            var organizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            var departmentId = NormalizeGuid(request.DepartmentId);

            if (legalEntityId.HasValue && !hospitalSiteId.HasValue)
            {
                return "Jika LegalEntityId diisi, HospitalSiteId juga wajib diisi karena attendance daily menyimpan scope hospital site.";
            }

            if (legalEntityId.HasValue && !await _dbContext.MstLegalEntities.AsNoTracking().AnyAsync(x => x.Id == legalEntityId && !x.IsDelete, cancellationToken))
            {
                return "Legal entity tidak ditemukan atau tidak aktif.";
            }
            if (hospitalSiteId.HasValue && !await _dbContext.MstHospitalSites.AsNoTracking().AnyAsync(x => x.Id == hospitalSiteId && !x.IsDelete, cancellationToken))
            {
                return "Hospital site tidak ditemukan atau tidak aktif.";
            }
            if (organizationUnitId.HasValue && !await _dbContext.MstOrganizationUnits.AsNoTracking().AnyAsync(x => x.Id == organizationUnitId && !x.IsDelete, cancellationToken))
            {
                return "Organization unit tidak ditemukan atau tidak aktif.";
            }
            if (departmentId.HasValue && !await _dbContext.MstDepartments.AsNoTracking().AnyAsync(x => x.Id == departmentId && !x.IsDelete, cancellationToken))
            {
                return "Department tidak ditemukan atau tidak aktif.";
            }

            var overlapQuery = _dbContext.Set<HrdAttendancePeriod>().AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.PeriodStatus != AttendanceValueConstants.AttendancePeriodStatus.Cancelled &&
                x.StartDate <= request.EndDate &&
                x.EndDate >= request.StartDate &&
                x.LegalEntityId == legalEntityId &&
                x.HospitalSiteId == hospitalSiteId &&
                x.OrganizationUnitId == organizationUnitId &&
                x.DepartmentId == departmentId);
            if (excludeId.HasValue)
            {
                overlapQuery = overlapQuery.Where(x => x.Id != excludeId.Value);
            }
            if (await overlapQuery.AnyAsync(cancellationToken))
            {
                return "Rentang attendance period bertabrakan dengan period lain pada scope organisasi yang sama.";
            }

            return null;
        }

        private IQueryable<HrdAttendancePeriod> BuildBaseQuery() =>
            _dbContext.Set<HrdAttendancePeriod>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<HrdAttendancePeriod> ApplyFilter(
            IQueryable<HrdAttendancePeriod> query,
            AttendancePeriodQueryRequest request)
        {
            if (request.StartDate.HasValue) query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.StartDate <= request.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.PeriodStatus)) query = query.Where(x => x.PeriodStatus == request.PeriodStatus.Trim());
            if (request.LegalEntityId.HasValue) query = query.Where(x => x.LegalEntityId == request.LegalEntityId);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (request.RequirePayrollHandoff.HasValue) query = query.Where(x => x.RequirePayrollHandoff == request.RequirePayrollHandoff.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x => x.PeriodCode.ToLower().Contains(keyword) || x.PeriodName.ToLower().Contains(keyword));
            }
            return query;
        }

        private static IOrderedQueryable<HrdAttendancePeriod> ApplySorting(
            IQueryable<HrdAttendancePeriod> query,
            string? sortBy,
            string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "enddate" => desc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                "periodcode" => desc ? query.OrderByDescending(x => x.PeriodCode) : query.OrderBy(x => x.PeriodCode),
                "periodname" => desc ? query.OrderByDescending(x => x.PeriodName) : query.OrderBy(x => x.PeriodName),
                "periodstatus" => desc ? query.OrderByDescending(x => x.PeriodStatus) : query.OrderBy(x => x.PeriodStatus),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate)
            };
        }

        private static IQueryable<HrdAttendanceDaily> ApplyScope(
            IQueryable<HrdAttendanceDaily> query,
            HrdAttendancePeriod period)
        {
            if (period.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == period.HospitalSiteId);
            if (period.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == period.OrganizationUnitId);
            if (period.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == period.DepartmentId);
            return query;
        }

        private static IQueryable<HrdAttendanceProcessingRun> ApplyProcessingScope(
            IQueryable<HrdAttendanceProcessingRun> query,
            HrdAttendancePeriod period)
        {
            if (period.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == period.HospitalSiteId);
            if (period.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == period.OrganizationUnitId);
            if (period.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == period.DepartmentId);
            return query;
        }

        private static IQueryable<HrdAttendanceSchedulerJob> ApplySchedulerScope(
            IQueryable<HrdAttendanceSchedulerJob> query,
            HrdAttendancePeriod period)
        {
            if (period.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == period.HospitalSiteId);
            if (period.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == period.OrganizationUnitId);
            if (period.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == period.DepartmentId);
            return query;
        }

        private async Task<string> GeneratePeriodCodeAsync(DateOnly startDate, CancellationToken cancellationToken)
        {
            var prefix = $"ATP-{startDate:yyyyMM}-";
            var existing = await _dbContext.Set<HrdAttendancePeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PeriodCode.StartsWith(prefix))
                .Select(x => x.PeriodCode)
                .ToListAsync(cancellationToken);
            var used = existing
                .Select(x => x.Substring(prefix.Length))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return prefix + next.ToString("D4");
        }

        private static void NormalizePaging(AttendancePeriodQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
        }

        private static bool IsEditableStatus(string status) =>
            status == AttendanceValueConstants.AttendancePeriodStatus.Open ||
            status == AttendanceValueConstants.AttendancePeriodStatus.Reopened;

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static DateTime? NormalizeUtc(DateTime? value)
        {
            if (!value.HasValue) return null;
            return value.Value.Kind == DateTimeKind.Utc
                ? value.Value
                : value.Value.ToUniversalTime();
        }

        private static AttendanceStringOptionResponse Option(string value, string label) =>
            new() { Value = value, Label = label };

        private static void AddIssue(
            ICollection<AttendancePeriodValidationIssueResponse> issues,
            string code,
            string severity,
            string message,
            int count,
            bool isBlocking)
        {
            if (count <= 0) return;
            issues.Add(new AttendancePeriodValidationIssueResponse
            {
                Code = code,
                Severity = severity,
                Message = message,
                Count = count,
                IsBlocking = isBlocking
            });
        }
    }
}
