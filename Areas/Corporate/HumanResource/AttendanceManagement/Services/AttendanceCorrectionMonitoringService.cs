using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Query dan operational recovery untuk monitoring attendance correction.
    /// Service ini tidak melakukan approve/reject. Keputusan approval tetap dilakukan
    /// melalui Workflow Engine dan Approval Inbox.
    /// </summary>
    public class AttendanceCorrectionMonitoringService
    {
        private static readonly string[] WorkflowReferenceAliases =
        {
            "ATTENDANCE_CORRECTION",
            "AttendanceCorrection",
            "HrdAttendanceCorrectionRequest"
        };

        private static readonly string[] OpenAssignmentStatuses =
        {
            "Pending",
            "Available",
            "InProgress"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AttendanceCorrectionService _attendanceCorrectionService;
        private readonly ILogger<AttendanceCorrectionMonitoringService> _logger;

        public AttendanceCorrectionMonitoringService(
            ApplicationDbContext dbContext,
            AttendanceCorrectionService attendanceCorrectionService,
            ILogger<AttendanceCorrectionMonitoringService> logger)
        {
            _dbContext = dbContext;
            _attendanceCorrectionService = attendanceCorrectionService;
            _logger = logger;
        }

        public AttendanceCorrectionMonitoringFilterMetadataResponse GetMetadata()
        {
            return new AttendanceCorrectionMonitoringFilterMetadataResponse
            {
                DefaultFilter = new AttendanceCorrectionMonitoringDefaultFilterResponse(),
                CustomPeriods = new()
                {
                    Option("today", "Hari ini"),
                    Option("last7days", "7 hari terakhir"),
                    Option("thismonth", "Bulan ini"),
                    Option("lastmonth", "Bulan lalu")
                },
                CorrectionTypeOptions = new()
                {
                    Option("AttendanceTime", "Waktu kehadiran"),
                    Option("MissingPunch", "Punch tidak lengkap"),
                    Option("Location", "Lokasi"),
                    Option("Schedule", "Jadwal"),
                    Option("Status", "Status kehadiran"),
                    Option("BusinessTrip", "Perjalanan dinas"),
                    Option("RemoteAttendance", "Kehadiran remote"),
                    Option("Other", "Lainnya")
                },
                RequestStatusOptions = new()
                {
                    Option("Draft", "Draft"),
                    Option("Submitted", "Diajukan"),
                    Option("UnderReview", "Dalam peninjauan"),
                    Option("NeedRevision", "Perlu revisi"),
                    Option("Approved", "Disetujui"),
                    Option("Rejected", "Ditolak"),
                    Option("Applied", "Sudah diterapkan"),
                    Option("Cancelled", "Dibatalkan")
                },
                WorkflowStatusOptions = new()
                {
                    Option("Draft", "Draft"),
                    Option("Submitted", "Diajukan"),
                    Option("InProgress", "Sedang berjalan"),
                    Option("RevisionRequested", "Perlu revisi"),
                    Option("Returned", "Dikembalikan"),
                    Option("Approved", "Disetujui"),
                    Option("Completed", "Selesai"),
                    Option("Rejected", "Ditolak"),
                    Option("Cancelled", "Dibatalkan"),
                    Option("Withdrawn", "Ditarik")
                },
                MonitoringStatusOptions = new()
                {
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Draft, "Draft"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.WaitingApproval, "Menunggu approval"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.NeedRevision, "Perlu revisi"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.ApprovedPendingApply, "Disetujui, belum diterapkan"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Applied, "Sudah diterapkan"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Rejected, "Ditolak"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Cancelled, "Dibatalkan"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.MissingWorkflow, "Workflow tidak ditemukan"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.WorkflowMismatch, "Status tidak sinkron"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Overdue, "Approval melewati jatuh tempo"),
                    Option(AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Stale, "Tidak ada aktivitas")
                },
                DueStatusOptions = new()
                {
                    Option(AttendanceCorrectionMonitoringValueConstants.DueStatus.Overdue, "Terlambat"),
                    Option(AttendanceCorrectionMonitoringValueConstants.DueStatus.DueToday, "Jatuh tempo hari ini"),
                    Option(AttendanceCorrectionMonitoringValueConstants.DueStatus.Upcoming, "Akan datang"),
                    Option(AttendanceCorrectionMonitoringValueConstants.DueStatus.Completed, "Selesai"),
                    Option(AttendanceCorrectionMonitoringValueConstants.DueStatus.NoDueDate, "Tanpa jatuh tempo")
                },
                SortOptions = new()
                {
                    Option("createDateTime", "Tanggal dibuat"),
                    Option("attendanceDate", "Tanggal kehadiran"),
                    Option("requestNumber", "Nomor pengajuan"),
                    Option("workforceDisplayName", "Nama workforce"),
                    Option("requestStatus", "Status pengajuan"),
                    Option("workflowStatus", "Status workflow"),
                    Option("monitoringStatus", "Status monitoring"),
                    Option("workflowDueAt", "Jatuh tempo workflow"),
                    Option("ageHours", "Usia pengajuan")
                },
                SortDirections = new() { "asc", "desc" },
                PageSizeOptions = new() { 10, 25, 50, 100 }
            };
        }

        public async Task<AttendanceCorrectionMonitoringSummaryResponse> GetSummaryAsync(
            AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var rows = await LoadRowsAsync(request, cancellationToken);
            rows = ApplyAdvancedFilters(rows, request);
            var today = DateTime.UtcNow.Date;

            return new AttendanceCorrectionMonitoringSummaryResponse
            {
                TotalRequest = rows.Count,
                DraftRequest = rows.Count(x => IsStatus(x.RequestStatus, "Draft")),
                WaitingApproval = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.WaitingApproval),
                NeedRevision = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.NeedRevision),
                ApprovedPendingApply = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.ApprovedPendingApply),
                Applied = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Applied),
                Rejected = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Rejected),
                Cancelled = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Cancelled),
                MissingWorkflow = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.MissingWorkflow),
                WorkflowMismatch = rows.Count(x => x.MonitoringStatus == AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.WorkflowMismatch),
                AutoApplyPending = rows.Count(x => x.IsAutoApplyPending),
                StaleRequest = rows.Count(x => x.IsStale),
                OverdueApproval = rows.Count(x => x.OverdueAssignmentCount > 0),
                PayrollBlocking = rows.Count(x => x.PayrollBlockingExceptionCount > 0),
                RequiresAttention = rows.Count(x => x.RequiresAttention),
                CreatedToday = rows.Count(x => x.CreateDateTime.Date == today)
            };
        }

        public async Task<AttendanceCorrectionMonitoringPagedResponse> GetPagedAsync(
            AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var rows = await LoadRowsAsync(request, cancellationToken);
            rows = ApplyAdvancedFilters(rows, request);
            var ordered = ApplySorting(rows, request.SortBy, request.SortDirection);
            var totalData = ordered.Count;

            return new AttendanceCorrectionMonitoringPagedResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = ordered
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList()
            };
        }

        public async Task<AttendanceCorrectionMonitoringPagedResponse> GetAttentionAsync(
            AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            request.RequiresAttention = true;
            return await GetPagedAsync(request, cancellationToken);
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringDetailResponse>>
            GetDetailAsync(
                Guid id,
                CancellationToken cancellationToken = default)
        {
            var correctionResult = await _attendanceCorrectionService.GetDetailAsync(
                id,
                ownerUserId: null,
                cancellationToken);

            if (!correctionResult.Success || correctionResult.Data == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringDetailResponse>.Fail(
                    correctionResult.StatusCode,
                    correctionResult.Message);
            }

            var monitoringRequest = new AttendanceCorrectionMonitoringQueryRequest
            {
                CustomPeriod = null,
                PageNumber = 1,
                PageSize = 1,
                StaleAfterHours = 24
            };

            var rows = await LoadRowsAsync(monitoringRequest, cancellationToken, id);
            var monitoring = rows.FirstOrDefault();
            if (monitoring == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction monitoring tidak ditemukan.");
            }

            AttendanceCorrectionWorkflowLinkResponse? workflowLink = null;
            var workflowResult = await _attendanceCorrectionService.GetWorkflowAsync(
                id,
                ownerUserId: null,
                cancellationToken);
            if (workflowResult.Success)
                workflowLink = workflowResult.Data;

            var assignments = monitoring.WorkflowInstanceId.HasValue
                ? await LoadAssignmentsAsync(monitoring.WorkflowInstanceId.Value, cancellationToken)
                : new List<AttendanceCorrectionMonitoringAssignmentResponse>();

            var histories = monitoring.WorkflowInstanceId.HasValue
                ? await LoadStatusHistoriesAsync(monitoring.WorkflowInstanceId.Value, cancellationToken)
                : new List<AttendanceCorrectionMonitoringStatusHistoryResponse>();

            var issues = BuildIssues(monitoring);

            return AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringDetailResponse>.Ok(
                new AttendanceCorrectionMonitoringDetailResponse
                {
                    Correction = correctionResult.Data,
                    Monitoring = monitoring,
                    WorkflowLink = workflowLink,
                    Issues = issues,
                    Assignments = assignments,
                    StatusHistories = histories,
                    AvailableAdminActions = BuildAvailableAdminActions(monitoring)
                },
                "Detail monitoring attendance correction berhasil diambil.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringWorkflowHealthResponse>>
            GetWorkflowHealthAsync(
                Guid id,
                CancellationToken cancellationToken = default)
        {
            var request = new AttendanceCorrectionMonitoringQueryRequest
            {
                CustomPeriod = null,
                PageNumber = 1,
                PageSize = 1,
                StaleAfterHours = 24
            };

            var rows = await LoadRowsAsync(request, cancellationToken, id);
            var row = rows.FirstOrDefault();
            if (row == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringWorkflowHealthResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            var assignments = row.WorkflowInstanceId.HasValue
                ? await LoadAssignmentsAsync(row.WorkflowInstanceId.Value, cancellationToken)
                : new List<AttendanceCorrectionMonitoringAssignmentResponse>();

            var histories = row.WorkflowInstanceId.HasValue
                ? await LoadStatusHistoriesAsync(row.WorkflowInstanceId.Value, cancellationToken)
                : new List<AttendanceCorrectionMonitoringStatusHistoryResponse>();

            return AttendanceCorrectionServiceResult<AttendanceCorrectionMonitoringWorkflowHealthResponse>.Ok(
                new AttendanceCorrectionMonitoringWorkflowHealthResponse
                {
                    AttendanceCorrectionRequestId = row.Id,
                    RequestNumber = row.RequestNumber,
                    RequestStatus = row.RequestStatus,
                    WorkflowInstanceId = row.WorkflowInstanceId,
                    WorkflowRequestNumber = row.WorkflowRequestNumber,
                    WorkflowStatus = row.WorkflowStatus,
                    HasWorkflow = row.HasWorkflow,
                    IsSynchronized = row.IsSynchronized,
                    IsAutoApplyPending = row.IsAutoApplyPending,
                    IsStale = row.IsStale,
                    RequiresAttention = row.RequiresAttention,
                    MonitoringStatus = row.MonitoringStatus,
                    OpenAssignmentCount = row.OpenAssignmentCount,
                    OverdueAssignmentCount = row.OverdueAssignmentCount,
                    Issues = BuildIssues(row),
                    Assignments = assignments,
                    StatusHistories = histories
                },
                "Kesehatan workflow attendance correction berhasil diambil.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionSynchronizationResponse>>
            SynchronizeAsync(
                Guid id,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            return await _attendanceCorrectionService.SynchronizeAsync(
                id,
                actorUserId,
                cancellationToken);
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>>
            RetryApplyAsync(
                Guid id,
                string? note,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            return await _attendanceCorrectionService.ApplyAsync(
                id,
                new AttendanceCorrectionApplyRequest
                {
                    Note = string.IsNullOrWhiteSpace(note)
                        ? "Retry apply dari Attendance Correction Monitoring."
                        : note.Trim()
                },
                actorUserId,
                cancellationToken);
        }

        public async Task<AttendanceCorrectionMonitoringBatchResponse> BulkSynchronizeAsync(
            AttendanceCorrectionMonitoringBatchRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var ids = NormalizeBatchIds(request.AttendanceCorrectionRequestIds);
            var response = new AttendanceCorrectionMonitoringBatchResponse
            {
                TotalItem = ids.Count
            };

            foreach (var id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var result = await SynchronizeAsync(id, actorUserId, cancellationToken);
                    response.Items.Add(new AttendanceCorrectionMonitoringOperationItemResponse
                    {
                        AttendanceCorrectionRequestId = id,
                        Success = result.Success,
                        StatusCode = result.StatusCode,
                        Message = result.Message,
                        PreviousRequestStatus = result.Data?.PreviousAttendanceCorrectionStatus,
                        CurrentRequestStatus = result.Data?.CurrentAttendanceCorrectionStatus,
                        WorkflowStatus = result.Data?.WorkflowStatus,
                        AutoApplyAttempted = result.Data?.AutoApplyAttempted,
                        AutoApplySucceeded = result.Data?.AutoApplySucceeded
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bulk synchronize attendance correction {CorrectionRequestId} gagal.", id);
                    response.Items.Add(new AttendanceCorrectionMonitoringOperationItemResponse
                    {
                        AttendanceCorrectionRequestId = id,
                        Success = false,
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = $"Sinkronisasi gagal: {ex.Message}"
                    });
                }
            }

            CompleteBatchResponse(response);
            return response;
        }

        public async Task<AttendanceCorrectionMonitoringBatchResponse> BulkRetryApplyAsync(
            AttendanceCorrectionMonitoringBatchRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var ids = NormalizeBatchIds(request.AttendanceCorrectionRequestIds);
            var response = new AttendanceCorrectionMonitoringBatchResponse
            {
                TotalItem = ids.Count
            };

            foreach (var id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var result = await RetryApplyAsync(id, request.Note, actorUserId, cancellationToken);
                    response.Items.Add(new AttendanceCorrectionMonitoringOperationItemResponse
                    {
                        AttendanceCorrectionRequestId = id,
                        Success = result.Success,
                        StatusCode = result.StatusCode,
                        Message = result.Message,
                        PreviousRequestStatus = result.Data?.PreviousRequestStatus,
                        CurrentRequestStatus = result.Data?.CurrentRequestStatus,
                        AutoApplyAttempted = true,
                        AutoApplySucceeded = result.Success
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bulk retry apply attendance correction {CorrectionRequestId} gagal.", id);
                    response.Items.Add(new AttendanceCorrectionMonitoringOperationItemResponse
                    {
                        AttendanceCorrectionRequestId = id,
                        Success = false,
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = $"Apply gagal: {ex.Message}",
                        AutoApplyAttempted = true,
                        AutoApplySucceeded = false
                    });
                }
            }

            CompleteBatchResponse(response);
            return response;
        }

        private async Task<List<AttendanceCorrectionMonitoringListResponse>> LoadRowsAsync(
            AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken,
            Guid? correctionRequestId = null)
        {
            var query = _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (correctionRequestId.HasValue)
                query = query.Where(x => x.Id == correctionRequestId.Value);

            var range = ResolveDateRange(request.StartDate, request.EndDate, request.CustomPeriod);
            if (range.Start.HasValue)
                query = query.Where(x => x.AttendanceDate >= range.Start.Value);
            if (range.End.HasValue)
                query = query.Where(x => x.AttendanceDate <= range.End.Value);

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.AttendanceDaily != null && x.AttendanceDaily.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != Guid.Empty)
                query = query.Where(x => x.AttendanceDaily != null && x.AttendanceDaily.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
                query = query.Where(x => x.AttendanceDaily != null && x.AttendanceDaily.DepartmentId == request.DepartmentId.Value);
            if (!string.IsNullOrWhiteSpace(request.CorrectionType))
                query = query.Where(x => x.CorrectionType == request.CorrectionType.Trim());
            if (!string.IsNullOrWhiteSpace(request.RequestStatus))
                query = query.Where(x => x.RequestStatus == request.RequestStatus.Trim());
            if (request.HasEvidence.HasValue)
            {
                query = request.HasEvidence.Value
                    ? query.Where(x => x.EvidenceFilePath != null && x.EvidenceFilePath != string.Empty)
                    : query.Where(x => x.EvidenceFilePath == null || x.EvidenceFilePath == string.Empty);
            }
            if (request.IsPayrollBlocking.HasValue)
            {
                query = request.IsPayrollBlocking.Value
                    ? query.Where(x => x.AttendanceDailyId.HasValue &&
                        _dbContext.Set<TrxAttendanceException>().Any(e =>
                            e.AttendanceDailyId == x.AttendanceDailyId.Value &&
                            e.IsActive && !e.IsDelete &&
                            e.IsPayrollBlocking &&
                            (e.ExceptionStatus == "Open" || e.ExceptionStatus == "UnderReview")))
                    : query.Where(x => !x.AttendanceDailyId.HasValue ||
                        !_dbContext.Set<TrxAttendanceException>().Any(e =>
                            e.AttendanceDailyId == x.AttendanceDailyId.Value &&
                            e.IsActive && !e.IsDelete &&
                            e.IsPayrollBlocking &&
                            (e.ExceptionStatus == "Open" || e.ExceptionStatus == "UnderReview")));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    x.CorrectionType.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null &&
                        (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                         x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))) ||
                    (x.RequestedByUser != null &&
                        ((x.RequestedByUser.DisplayName != null && x.RequestedByUser.DisplayName.ToLower().Contains(keyword)) ||
                         (x.RequestedByUser.UserName != null && x.RequestedByUser.UserName.ToLower().Contains(keyword)) ||
                         (x.RequestedByUser.Email != null && x.RequestedByUser.Email.ToLower().Contains(keyword)) ||
                         (x.RequestedByUser.UserCode != null && x.RequestedByUser.UserCode.ToLower().Contains(keyword)))));
            }

            var corrections = await query
                .Select(x => new CorrectionSnapshot
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    AttendanceDailyId = x.AttendanceDailyId,
                    AttendanceDate = x.AttendanceDate,
                    CorrectionType = x.CorrectionType,
                    RequestStatus = x.RequestStatus,
                    Reason = x.Reason,
                    HasEvidence = x.EvidenceFilePath != null && x.EvidenceFilePath != string.Empty,
                    DetailCount = x.Details.Count(d => d.IsActive && !d.IsDelete),
                    LinkedExceptionCount = x.Exceptions.Count(e => e.IsActive && !e.IsDelete),
                    SubmittedAt = x.SubmittedAt,
                    ApprovedAt = x.ApprovedAt,
                    RejectedAt = x.RejectedAt,
                    AppliedAt = x.AppliedAt,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime,
                    RequestedByUserName = x.RequestedByUser != null
                        ? x.RequestedByUser.DisplayName ?? x.RequestedByUser.UserName ?? x.RequestedByUser.Email ?? x.RequestedByUser.UserCode
                        : null,
                    HospitalSiteId = x.AttendanceDaily != null ? x.AttendanceDaily.HospitalSiteId : null,
                    HospitalSiteName = x.AttendanceDaily != null && x.AttendanceDaily.HospitalSite != null
                        ? x.AttendanceDaily.HospitalSite.SiteName
                        : null,
                    OrganizationUnitId = x.AttendanceDaily != null ? x.AttendanceDaily.OrganizationUnitId : null,
                    OrganizationUnitName = x.AttendanceDaily != null && x.AttendanceDaily.OrganizationUnit != null
                        ? x.AttendanceDaily.OrganizationUnit.UnitName
                        : null,
                    DepartmentId = x.AttendanceDaily != null ? x.AttendanceDaily.DepartmentId : null,
                    DepartmentName = x.AttendanceDaily != null && x.AttendanceDaily.Department != null
                        ? x.AttendanceDaily.Department.DepartmentName
                        : null,
                    AttendanceStatus = x.AttendanceDaily != null ? x.AttendanceDaily.AttendanceStatus : null,
                    AttendanceProcessingStatus = x.AttendanceDaily != null ? x.AttendanceDaily.ProcessingStatus : null,
                    PayrollInputStatus = x.AttendanceDaily != null ? x.AttendanceDaily.PayrollInputStatus : null,
                    IsAttendanceLocked = x.AttendanceDaily != null && x.AttendanceDaily.IsLocked,
                    IsAttendanceCorrected = x.AttendanceDaily != null && x.AttendanceDaily.IsCorrected,
                    PayrollBlockingExceptionCount = x.AttendanceDailyId.HasValue
                        ? _dbContext.Set<TrxAttendanceException>().Count(e =>
                            e.AttendanceDailyId == x.AttendanceDailyId.Value &&
                            e.IsActive && !e.IsDelete &&
                            e.IsPayrollBlocking &&
                            (e.ExceptionStatus == "Open" || e.ExceptionStatus == "UnderReview"))
                        : 0
                })
                .ToListAsync(cancellationToken);

            if (corrections.Count == 0)
                return new List<AttendanceCorrectionMonitoringListResponse>();

            var correctionIds = corrections.Select(x => x.Id).ToList();
            var workflows = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    correctionIds.Contains(x.ReferenceId) &&
                    WorkflowReferenceAliases.Contains(x.ReferenceType) &&
                    !x.IsDelete)
                .Select(x => new WorkflowSnapshot
                {
                    Id = x.Id,
                    ReferenceId = x.ReferenceId,
                    RequestNumber = x.RequestNumber,
                    WorkflowStatus = x.WorkflowStatus,
                    CurrentStepCode = x.CurrentStepCode,
                    CurrentStepOrder = x.CurrentStepOrder,
                    SubmittedAt = x.SubmittedAt,
                    DueAt = x.DueAt,
                    LastActionAt = x.LastActionAt,
                    CompletedAt = x.CompletedAt,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var latestWorkflowByReferenceId = workflows
                .GroupBy(x => x.ReferenceId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(w => w.CreateDateTime)
                          .ThenByDescending(w => w.Id)
                          .First());

            var workflowIds = latestWorkflowByReferenceId.Values.Select(x => x.Id).ToList();
            var assignmentSummaryByWorkflowId = new Dictionary<Guid, AssignmentSummary>();

            if (workflowIds.Count > 0)
            {
                var assignments = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                    .AsNoTracking()
                    .Where(x => workflowIds.Contains(x.WorkflowInstanceId) && x.IsActive && !x.IsDelete)
                    .Select(x => new
                    {
                        x.WorkflowInstanceId,
                        x.AssignmentStatus,
                        x.DueAt,
                        x.IsCurrentAssignment
                    })
                    .ToListAsync(cancellationToken);

                assignmentSummaryByWorkflowId = assignments
                    .GroupBy(x => x.WorkflowInstanceId)
                    .ToDictionary(
                        x => x.Key,
                        x => new AssignmentSummary
                        {
                            OpenCount = x.Count(a => OpenAssignmentStatuses.Contains(a.AssignmentStatus)),
                            OverdueCount = x.Count(a =>
                                OpenAssignmentStatuses.Contains(a.AssignmentStatus) &&
                                a.DueAt.HasValue &&
                                a.DueAt.Value < DateTime.UtcNow),
                            CurrentDueAt = x
                                .Where(a => a.IsCurrentAssignment && OpenAssignmentStatuses.Contains(a.AssignmentStatus))
                                .OrderBy(a => a.DueAt)
                                .Select(a => a.DueAt)
                                .FirstOrDefault()
                        });
            }

            var staleAfterHours = Math.Clamp(request.StaleAfterHours, 1, 720);
            var now = DateTime.UtcNow;
            var rows = new List<AttendanceCorrectionMonitoringListResponse>(corrections.Count);

            foreach (var correction in corrections)
            {
                latestWorkflowByReferenceId.TryGetValue(correction.Id, out var workflow);
                var assignmentSummary = workflow != null && assignmentSummaryByWorkflowId.TryGetValue(workflow.Id, out var summary)
                    ? summary
                    : new AssignmentSummary();

                var activityAt = workflow?.LastActionAt
                    ?? workflow?.SubmittedAt
                    ?? correction.UpdateDateTime
                    ?? correction.SubmittedAt
                    ?? correction.CreateDateTime;

                var ageHours = Math.Max(0, (int)Math.Floor((now - activityAt).TotalHours));
                var isSynchronized = IsSynchronized(correction.RequestStatus, workflow?.WorkflowStatus);
                var isAutoApplyPending = IsAutoApplyPending(correction.RequestStatus, workflow?.WorkflowStatus);
                var isStale = IsWaitingApproval(correction.RequestStatus, workflow?.WorkflowStatus) && ageHours >= staleAfterHours;
                var dueAt = assignmentSummary.CurrentDueAt ?? workflow?.DueAt;
                var dueStatus = ResolveDueStatus(dueAt, workflow?.WorkflowStatus);

                var row = new AttendanceCorrectionMonitoringListResponse
                {
                    Id = correction.Id,
                    RequestNumber = correction.RequestNumber,
                    WorkforceProfileId = correction.WorkforceProfileId,
                    WorkforceProfileCode = correction.WorkforceProfileCode,
                    WorkforceDisplayName = correction.WorkforceDisplayName,
                    AttendanceDailyId = correction.AttendanceDailyId,
                    AttendanceDate = correction.AttendanceDate,
                    CorrectionType = correction.CorrectionType,
                    RequestStatus = correction.RequestStatus,
                    Reason = correction.Reason,
                    HasEvidence = correction.HasEvidence,
                    DetailCount = correction.DetailCount,
                    LinkedExceptionCount = correction.LinkedExceptionCount,
                    SubmittedAt = correction.SubmittedAt,
                    ApprovedAt = correction.ApprovedAt,
                    RejectedAt = correction.RejectedAt,
                    AppliedAt = correction.AppliedAt,
                    CreateDateTime = correction.CreateDateTime,
                    UpdateDateTime = correction.UpdateDateTime,
                    RequestedByUserName = correction.RequestedByUserName,
                    HospitalSiteId = correction.HospitalSiteId,
                    HospitalSiteName = correction.HospitalSiteName,
                    OrganizationUnitId = correction.OrganizationUnitId,
                    OrganizationUnitName = correction.OrganizationUnitName,
                    DepartmentId = correction.DepartmentId,
                    DepartmentName = correction.DepartmentName,
                    AttendanceStatus = correction.AttendanceStatus,
                    AttendanceProcessingStatus = correction.AttendanceProcessingStatus,
                    PayrollInputStatus = correction.PayrollInputStatus,
                    IsAttendanceLocked = correction.IsAttendanceLocked,
                    IsAttendanceCorrected = correction.IsAttendanceCorrected,
                    PayrollBlockingExceptionCount = correction.PayrollBlockingExceptionCount,
                    HasWorkflow = workflow != null,
                    WorkflowInstanceId = workflow?.Id,
                    WorkflowRequestNumber = workflow?.RequestNumber,
                    WorkflowStatus = workflow?.WorkflowStatus,
                    CurrentStepCode = workflow?.CurrentStepCode,
                    CurrentStepOrder = workflow?.CurrentStepOrder ?? 0,
                    WorkflowSubmittedAt = workflow?.SubmittedAt,
                    WorkflowDueAt = workflow?.DueAt,
                    WorkflowLastActionAt = workflow?.LastActionAt,
                    WorkflowCompletedAt = workflow?.CompletedAt,
                    OpenAssignmentCount = assignmentSummary.OpenCount,
                    OverdueAssignmentCount = assignmentSummary.OverdueCount,
                    IsSynchronized = isSynchronized,
                    IsAutoApplyPending = isAutoApplyPending,
                    IsStale = isStale,
                    DueStatus = dueStatus,
                    AgeHours = ageHours
                };

                row.AttentionReasonCodes = BuildAttentionReasonCodes(row);
                row.RequiresAttention = row.AttentionReasonCodes.Count > 0;
                row.MonitoringStatus = ResolveMonitoringStatus(row);
                rows.Add(row);
            }

            return rows;
        }

        private async Task<List<AttendanceCorrectionMonitoringAssignmentResponse>> LoadAssignmentsAsync(
            Guid workflowInstanceId,
            CancellationToken cancellationToken)
        {
            var assignments = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId && x.IsActive && !x.IsDelete)
                .OrderBy(x => x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepOrder : int.MaxValue)
                .ThenBy(x => x.AssignmentOrder)
                .Select(x => new AttendanceCorrectionMonitoringAssignmentResponse
                {
                    AssignmentId = x.Id,
                    WorkflowStepInstanceId = x.WorkflowStepInstanceId,
                    StepOrder = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepOrder : 0,
                    StepCode = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepCodeSnapshot : string.Empty,
                    StepName = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepNameSnapshot : string.Empty,
                    StepType = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepTypeSnapshot : string.Empty,
                    AssignmentStatus = x.AssignmentStatus,
                    AssignmentOrder = x.AssignmentOrder,
                    AssignedApproverUserId = x.AssignedApproverUserId,
                    AssignedApproverName = x.AssignedApproverUser != null
                        ? x.AssignedApproverUser.DisplayName ?? x.AssignedApproverUser.UserName ?? x.AssignedApproverUser.Email ?? x.AssignedApproverUser.UserCode
                        : null,
                    OriginalApproverUserId = x.OriginalApproverUserId,
                    OriginalApproverName = x.OriginalApproverUser != null
                        ? x.OriginalApproverUser.DisplayName ?? x.OriginalApproverUser.UserName ?? x.OriginalApproverUser.Email ?? x.OriginalApproverUser.UserCode
                        : null,
                    IsDelegated = x.IsDelegated,
                    IsCurrentAssignment = x.IsCurrentAssignment,
                    AssignedAt = x.AssignedAt,
                    AvailableAt = x.AvailableAt,
                    StartedAt = x.StartedAt,
                    DueAt = x.DueAt,
                    CompletedAt = x.CompletedAt
                })
                .ToListAsync(cancellationToken);

            foreach (var assignment in assignments)
                assignment.DueStatus = ResolveDueStatus(assignment.DueAt, assignment.AssignmentStatus);

            return assignments;
        }

        private async Task<List<AttendanceCorrectionMonitoringStatusHistoryResponse>> LoadStatusHistoriesAsync(
            Guid workflowInstanceId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxWorkflowStatusHistory>()
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId && x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.SequenceNumber)
                .ThenByDescending(x => x.ChangedAt)
                .Select(x => new AttendanceCorrectionMonitoringStatusHistoryResponse
                {
                    Id = x.Id,
                    SequenceNumber = x.SequenceNumber,
                    FromWorkflowStatus = x.FromWorkflowStatus,
                    ToWorkflowStatus = x.ToWorkflowStatus,
                    FromStepStatus = x.FromStepStatus,
                    ToStepStatus = x.ToStepStatus,
                    ActionType = x.ActionType,
                    ChangedAt = x.ChangedAt,
                    ChangedByName = x.ChangedByUser != null
                        ? x.ChangedByUser.DisplayName ?? x.ChangedByUser.UserName ?? x.ChangedByUser.Email ?? x.ChangedByUser.UserCode
                        : null,
                    Comment = x.Comment,
                    IsSystemGenerated = x.IsSystemGenerated
                })
                .ToListAsync(cancellationToken);
        }

        private static List<AttendanceCorrectionMonitoringListResponse> ApplyAdvancedFilters(
            List<AttendanceCorrectionMonitoringListResponse> rows,
            AttendanceCorrectionMonitoringQueryRequest request)
        {
            IEnumerable<AttendanceCorrectionMonitoringListResponse> result = rows;

            if (!string.IsNullOrWhiteSpace(request.WorkflowStatus))
                result = result.Where(x => IsStatus(x.WorkflowStatus, request.WorkflowStatus));
            if (!string.IsNullOrWhiteSpace(request.MonitoringStatus))
                result = result.Where(x => IsStatus(x.MonitoringStatus, request.MonitoringStatus));
            if (!string.IsNullOrWhiteSpace(request.DueStatus))
                result = result.Where(x => IsStatus(x.DueStatus, request.DueStatus));
            if (request.HasWorkflow.HasValue)
                result = result.Where(x => x.HasWorkflow == request.HasWorkflow.Value);
            if (request.IsSynchronized.HasValue)
                result = result.Where(x => x.IsSynchronized == request.IsSynchronized.Value);
            if (request.IsAutoApplyPending.HasValue)
                result = result.Where(x => x.IsAutoApplyPending == request.IsAutoApplyPending.Value);
            if (request.RequiresAttention.HasValue)
                result = result.Where(x => x.RequiresAttention == request.RequiresAttention.Value);

            return result.ToList();
        }

        private static List<AttendanceCorrectionMonitoringListResponse> ApplySorting(
            List<AttendanceCorrectionMonitoringListResponse> rows,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var key = (sortBy ?? "createDateTime").Trim().ToLowerInvariant();

            IOrderedEnumerable<AttendanceCorrectionMonitoringListResponse> ordered = key switch
            {
                "attendancedate" => desc ? rows.OrderByDescending(x => x.AttendanceDate) : rows.OrderBy(x => x.AttendanceDate),
                "requestnumber" => desc ? rows.OrderByDescending(x => x.RequestNumber) : rows.OrderBy(x => x.RequestNumber),
                "workforcedisplayname" => desc ? rows.OrderByDescending(x => x.WorkforceDisplayName) : rows.OrderBy(x => x.WorkforceDisplayName),
                "requeststatus" => desc ? rows.OrderByDescending(x => x.RequestStatus) : rows.OrderBy(x => x.RequestStatus),
                "workflowstatus" => desc ? rows.OrderByDescending(x => x.WorkflowStatus) : rows.OrderBy(x => x.WorkflowStatus),
                "monitoringstatus" => desc ? rows.OrderByDescending(x => x.MonitoringStatus) : rows.OrderBy(x => x.MonitoringStatus),
                "workflowdueat" => desc ? rows.OrderByDescending(x => x.WorkflowDueAt) : rows.OrderBy(x => x.WorkflowDueAt),
                "agehours" => desc ? rows.OrderByDescending(x => x.AgeHours) : rows.OrderBy(x => x.AgeHours),
                _ => desc ? rows.OrderByDescending(x => x.CreateDateTime) : rows.OrderBy(x => x.CreateDateTime)
            };

            return ordered.ThenByDescending(x => x.RequestNumber).ToList();
        }

        private static List<string> BuildAttentionReasonCodes(AttendanceCorrectionMonitoringListResponse row)
        {
            var reasons = new List<string>();

            if (!row.HasWorkflow && !IsStatus(row.RequestStatus, "Draft") && !IsStatus(row.RequestStatus, "Cancelled"))
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.MissingWorkflow);
            if (row.HasWorkflow && !row.IsSynchronized)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.WorkflowStatusMismatch);
            if (row.IsAutoApplyPending)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.AutoApplyPending);
            if (row.IsStale)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.StaleWorkflow);
            if (row.OverdueAssignmentCount > 0)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.OverdueAssignment);
            if (row.PayrollBlockingExceptionCount > 0)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.PayrollBlocking);
            if (row.IsAutoApplyPending && row.IsAttendanceLocked)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.AttendanceLocked);
            if (row.IsAutoApplyPending && IsStatus(row.PayrollInputStatus, "Processed"))
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.PayrollAlreadyProcessed);
            if (IsStatus(row.RequestStatus, "Applied") && !row.IsAttendanceCorrected)
                reasons.Add(AttendanceCorrectionMonitoringValueConstants.IssueCode.AppliedFlagMismatch);

            return reasons.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static List<AttendanceCorrectionMonitoringIssueResponse> BuildIssues(
            AttendanceCorrectionMonitoringListResponse row)
        {
            return row.AttentionReasonCodes.Select(code => code switch
            {
                AttendanceCorrectionMonitoringValueConstants.IssueCode.MissingWorkflow => Issue(
                    code,
                    "Critical",
                    "Pengajuan bukan Draft tetapi workflow tidak ditemukan.",
                    "Periksa proses submit dan buat ulang workflow jika submit sebelumnya gagal."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.WorkflowStatusMismatch => Issue(
                    code,
                    "High",
                    "Status attendance correction tidak selaras dengan status workflow.",
                    "Jalankan Synchronize Workflow."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.AutoApplyPending => Issue(
                    code,
                    "High",
                    "Workflow telah selesai, tetapi koreksi belum berhasil diterapkan.",
                    "Jalankan Retry Apply setelah memastikan attendance belum terkunci atau masuk payroll."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.StaleWorkflow => Issue(
                    code,
                    "Warning",
                    "Workflow tidak memiliki aktivitas dalam batas waktu monitoring.",
                    "Periksa assignment aktif dan approver pada Approval Inbox."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.OverdueAssignment => Issue(
                    code,
                    "High",
                    $"Terdapat {row.OverdueAssignmentCount} assignment yang melewati jatuh tempo.",
                    "Periksa approver, delegation, dan escalation policy."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.PayrollBlocking => Issue(
                    code,
                    "High",
                    $"Terdapat {row.PayrollBlockingExceptionCount} exception aktif yang memblokir payroll.",
                    "Selesaikan koreksi dan pastikan exception ditutup."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.AttendanceLocked => Issue(
                    code,
                    "Critical",
                    "Attendance terkunci sehingga koreksi yang disetujui tidak dapat diterapkan.",
                    "Pastikan lock dibuka melalui prosedur HR/payroll yang berwenang sebelum retry apply."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.PayrollAlreadyProcessed => Issue(
                    code,
                    "Critical",
                    "Attendance telah diproses ke payroll sehingga koreksi tidak boleh diterapkan langsung.",
                    "Gunakan prosedur payroll adjustment atau reversal yang berlaku."),

                AttendanceCorrectionMonitoringValueConstants.IssueCode.AppliedFlagMismatch => Issue(
                    code,
                    "High",
                    "Request berstatus Applied tetapi AttendanceDaily belum ditandai IsCorrected.",
                    "Audit detail apply dan sinkronkan data attendance."),

                _ => Issue(code, "Warning", "Ditemukan kondisi monitoring yang memerlukan pemeriksaan.", null)
            }).ToList();
        }

        private static List<string> BuildAvailableAdminActions(AttendanceCorrectionMonitoringListResponse row)
        {
            var actions = new List<string> { "ViewAttendance" };
            if (row.HasWorkflow)
                actions.Add("OpenWorkflow");
            if (row.HasEvidence)
                actions.Add("ViewEvidence");
            if (row.HasWorkflow && (!row.IsSynchronized || row.IsAutoApplyPending))
                actions.Add("Synchronize");
            if (row.IsAutoApplyPending ||
                (IsStatus(row.RequestStatus, "Approved") &&
                 (IsStatus(row.WorkflowStatus, "Completed") || IsStatus(row.WorkflowStatus, "Approved"))))
            {
                actions.Add("RetryApply");
            }
            return actions;
        }

        private static string ResolveMonitoringStatus(AttendanceCorrectionMonitoringListResponse row)
        {
            if (!row.HasWorkflow && !IsStatus(row.RequestStatus, "Draft") && !IsStatus(row.RequestStatus, "Cancelled"))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.MissingWorkflow;
            if (row.HasWorkflow && !row.IsSynchronized)
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.WorkflowMismatch;
            if (row.IsAutoApplyPending)
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.ApprovedPendingApply;
            if (row.OverdueAssignmentCount > 0)
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Overdue;
            if (row.IsStale)
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Stale;
            if (IsStatus(row.RequestStatus, "NeedRevision"))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.NeedRevision;
            if (IsWaitingApproval(row.RequestStatus, row.WorkflowStatus))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.WaitingApproval;
            if (IsStatus(row.RequestStatus, "Applied"))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Applied;
            if (IsStatus(row.RequestStatus, "Rejected"))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Rejected;
            if (IsStatus(row.RequestStatus, "Cancelled"))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Cancelled;
            if (IsStatus(row.RequestStatus, "Draft"))
                return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Draft;
            return AttendanceCorrectionMonitoringValueConstants.MonitoringStatus.Completed;
        }

        private static bool IsSynchronized(string requestStatus, string? workflowStatus)
        {
            if (string.IsNullOrWhiteSpace(workflowStatus))
                return IsStatus(requestStatus, "Draft") || IsStatus(requestStatus, "Cancelled");

            if (IsStatus(requestStatus, "Draft"))
                return IsStatus(workflowStatus, "Draft");
            if (IsStatus(requestStatus, "Submitted") || IsStatus(requestStatus, "UnderReview") || IsStatus(requestStatus, "PartiallyApproved"))
                return IsStatus(workflowStatus, "Submitted") || IsStatus(workflowStatus, "InProgress");
            if (IsStatus(requestStatus, "NeedRevision"))
                return IsStatus(workflowStatus, "RevisionRequested") || IsStatus(workflowStatus, "Returned");
            if (IsStatus(requestStatus, "Approved") || IsStatus(requestStatus, "Applied"))
                return IsStatus(workflowStatus, "Approved") || IsStatus(workflowStatus, "Completed");
            if (IsStatus(requestStatus, "Rejected"))
                return IsStatus(workflowStatus, "Rejected");
            if (IsStatus(requestStatus, "Cancelled"))
                return IsStatus(workflowStatus, "Cancelled") || IsStatus(workflowStatus, "Withdrawn");
            return false;
        }

        private static bool IsAutoApplyPending(string requestStatus, string? workflowStatus)
        {
            return !IsStatus(requestStatus, "Applied") &&
                   (IsStatus(workflowStatus, "Completed") || IsStatus(workflowStatus, "Approved"));
        }

        private static bool IsWaitingApproval(string requestStatus, string? workflowStatus)
        {
            return IsStatus(requestStatus, "Submitted") ||
                   IsStatus(requestStatus, "UnderReview") ||
                   IsStatus(requestStatus, "PartiallyApproved") ||
                   IsStatus(workflowStatus, "Submitted") ||
                   IsStatus(workflowStatus, "InProgress");
        }

        private static string ResolveDueStatus(DateTime? dueAt, string? terminalStatus)
        {
            if (IsStatus(terminalStatus, "Completed") ||
                IsStatus(terminalStatus, "Approved") ||
                IsStatus(terminalStatus, "Rejected") ||
                IsStatus(terminalStatus, "Cancelled") ||
                IsStatus(terminalStatus, "Withdrawn"))
            {
                return AttendanceCorrectionMonitoringValueConstants.DueStatus.Completed;
            }

            if (!dueAt.HasValue)
                return AttendanceCorrectionMonitoringValueConstants.DueStatus.NoDueDate;

            var today = DateTime.UtcNow.Date;
            if (dueAt.Value < DateTime.UtcNow)
                return AttendanceCorrectionMonitoringValueConstants.DueStatus.Overdue;
            if (dueAt.Value.Date == today)
                return AttendanceCorrectionMonitoringValueConstants.DueStatus.DueToday;
            return AttendanceCorrectionMonitoringValueConstants.DueStatus.Upcoming;
        }

        private static (DateOnly? Start, DateOnly? End) ResolveDateRange(
            DateOnly? startDate,
            DateOnly? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
                return (startDate, endDate);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today),
                "last7days" => (today.AddDays(-6), today),
                "thismonth" => (new DateOnly(today.Year, today.Month, 1), new DateOnly(today.Year, today.Month, 1).AddMonths(1).AddDays(-1)),
                "lastmonth" => (new DateOnly(today.Year, today.Month, 1).AddMonths(-1), new DateOnly(today.Year, today.Month, 1).AddDays(-1)),
                _ => (null, null)
            };
        }

        private static void NormalizePaging(AttendanceCorrectionMonitoringQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            request.StaleAfterHours = Math.Clamp(request.StaleAfterHours, 1, 720);
        }

        private static List<Guid> NormalizeBatchIds(IEnumerable<Guid> ids)
        {
            return ids
                .Where(x => x != Guid.Empty)
                .Distinct()
                .Take(100)
                .ToList();
        }

        private static void CompleteBatchResponse(AttendanceCorrectionMonitoringBatchResponse response)
        {
            response.SuccessCount = response.Items.Count(x => x.Success);
            response.FailedCount = response.Items.Count - response.SuccessCount;
        }

        private static AttendanceCorrectionMonitoringOptionResponse Option(string value, string label) =>
            new() { Value = value, Label = label };

        private static AttendanceCorrectionMonitoringIssueResponse Issue(
            string code,
            string severity,
            string message,
            string? suggestedAction) =>
            new()
            {
                Code = code,
                Severity = severity,
                Message = message,
                SuggestedAction = suggestedAction
            };

        private static bool IsStatus(string? actual, string? expected) =>
            string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

        private sealed class CorrectionSnapshot
        {
            public Guid Id { get; set; }
            public string RequestNumber { get; set; } = string.Empty;
            public Guid WorkforceProfileId { get; set; }
            public string? WorkforceProfileCode { get; set; }
            public string? WorkforceDisplayName { get; set; }
            public Guid? AttendanceDailyId { get; set; }
            public DateOnly AttendanceDate { get; set; }
            public string CorrectionType { get; set; } = string.Empty;
            public string RequestStatus { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public bool HasEvidence { get; set; }
            public int DetailCount { get; set; }
            public int LinkedExceptionCount { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public DateTime? RejectedAt { get; set; }
            public DateTime? AppliedAt { get; set; }
            public DateTime CreateDateTime { get; set; }
            public DateTime? UpdateDateTime { get; set; }
            public string? RequestedByUserName { get; set; }
            public Guid? HospitalSiteId { get; set; }
            public string? HospitalSiteName { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public string? OrganizationUnitName { get; set; }
            public Guid? DepartmentId { get; set; }
            public string? DepartmentName { get; set; }
            public string? AttendanceStatus { get; set; }
            public string? AttendanceProcessingStatus { get; set; }
            public string? PayrollInputStatus { get; set; }
            public bool IsAttendanceLocked { get; set; }
            public bool IsAttendanceCorrected { get; set; }
            public int PayrollBlockingExceptionCount { get; set; }
        }

        private sealed class WorkflowSnapshot
        {
            public Guid Id { get; set; }
            public Guid ReferenceId { get; set; }
            public string RequestNumber { get; set; } = string.Empty;
            public string WorkflowStatus { get; set; } = string.Empty;
            public string? CurrentStepCode { get; set; }
            public int CurrentStepOrder { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public DateTime? DueAt { get; set; }
            public DateTime? LastActionAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime CreateDateTime { get; set; }
        }

        private sealed class AssignmentSummary
        {
            public int OpenCount { get; set; }
            public int OverdueCount { get; set; }
            public DateTime? CurrentDueAt { get; set; }
        }
    }
}
