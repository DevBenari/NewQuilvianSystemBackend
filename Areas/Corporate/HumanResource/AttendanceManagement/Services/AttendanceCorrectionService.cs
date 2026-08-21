using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceCorrectionService
    {
        public const string DefaultWorkflowCode = "ATTENDANCE_CORRECTION";

        private static readonly string[] EditableStatuses =
        {
            AttendanceValueConstants.CorrectionRequestStatus.Draft,
            AttendanceValueConstants.CorrectionRequestStatus.NeedRevision
        };

        private static readonly string[] SupportedWorkflowReferenceTypes =
        {
            WorkflowReferenceLifecycleService.AttendanceCorrectionReferenceType,
            "AttendanceCorrection",
            "HrdAttendanceCorrectionRequest",
            "ATTENDANCE_CORRECTION_REQUEST"
        };

        private static readonly string[] ActiveRequestStatuses =
        {
            AttendanceValueConstants.CorrectionRequestStatus.Draft,
            AttendanceValueConstants.CorrectionRequestStatus.Submitted,
            AttendanceValueConstants.CorrectionRequestStatus.UnderReview,
            AttendanceValueConstants.CorrectionRequestStatus.NeedRevision,
            AttendanceValueConstants.CorrectionRequestStatus.Approved,
            AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly WorkflowService _workflowService;
        private readonly AttendanceCorrectionWorkflowLifecycleService _lifecycleService;
        private readonly WorkflowFileStorageService _fileStorageService;

        public AttendanceCorrectionService(
            ApplicationDbContext dbContext,
            WorkflowService workflowService,
            AttendanceCorrectionWorkflowLifecycleService lifecycleService,
            WorkflowFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _workflowService = workflowService;
            _lifecycleService = lifecycleService;
            _fileStorageService = fileStorageService;
        }

        public AttendanceCorrectionFilterMetadataResponse GetMetadata()
        {
            return new AttendanceCorrectionFilterMetadataResponse
            {
                DefaultFilter = new AttendanceCorrectionDefaultFilterResponse(),
                CustomPeriods = new List<AttendanceCorrectionStringOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                CorrectionTypeOptions = new List<AttendanceCorrectionStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.CorrectionType.AttendanceTime, Label = "Waktu kehadiran" },
                    new() { Value = AttendanceValueConstants.CorrectionType.MissingPunch, Label = "Punch tidak lengkap" },
                    new() { Value = AttendanceValueConstants.CorrectionType.Location, Label = "Lokasi kehadiran" },
                    new() { Value = AttendanceValueConstants.CorrectionType.Schedule, Label = "Jadwal kerja" },
                    new() { Value = AttendanceValueConstants.CorrectionType.Status, Label = "Status kehadiran" },
                    new() { Value = AttendanceValueConstants.CorrectionType.BusinessTrip, Label = "Perjalanan dinas" },
                    new() { Value = AttendanceValueConstants.CorrectionType.RemoteAttendance, Label = "Kehadiran remote" },
                    new() { Value = AttendanceValueConstants.CorrectionType.Other, Label = "Lainnya" }
                },
                RequestStatusOptions = new List<AttendanceCorrectionStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.Draft, Label = "Draft" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.Submitted, Label = "Diajukan" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.UnderReview, Label = "Sedang direview" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.NeedRevision, Label = "Perlu revisi" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.Approved, Label = "Disetujui" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved, Label = "Disetujui sebagian" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.Rejected, Label = "Ditolak" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.Applied, Label = "Diterapkan" },
                    new() { Value = AttendanceValueConstants.CorrectionRequestStatus.Cancelled, Label = "Dibatalkan" }
                },
                SortOptions = new List<AttendanceCorrectionStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "attendanceDate", Label = "Tanggal attendance" },
                    new() { Value = "requestNumber", Label = "Nomor pengajuan" },
                    new() { Value = "requestStatus", Label = "Status pengajuan" },
                    new() { Value = "workforceName", Label = "Nama workforce" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EditableFields = AttendanceCorrectionFieldCatalog.ToOptions()
            };
        }

        public async Task<AttendanceCorrectionSummaryResponse> GetSummaryAsync(
            AttendanceCorrectionQueryRequest request,
            Guid? requestedByUserId = null,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            if (requestedByUserId.HasValue)
            {
                query = query.Where(x => x.RequestedByUserId == requestedByUserId.Value);
            }

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return new AttendanceCorrectionSummaryResponse
            {
                TotalRequest = await query.CountAsync(cancellationToken),
                DraftRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Draft, cancellationToken),
                SubmittedRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Submitted, cancellationToken),
                UnderReviewRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.UnderReview, cancellationToken),
                NeedRevisionRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.NeedRevision, cancellationToken),
                ApprovedRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Approved || x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved, cancellationToken),
                RejectedRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Rejected, cancellationToken),
                AppliedRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Applied, cancellationToken),
                CancelledRequest = await query.CountAsync(x => x.RequestStatus == AttendanceValueConstants.CorrectionRequestStatus.Cancelled, cancellationToken),
                RequestWithEvidence = await query.CountAsync(x => x.EvidenceFilePath != null, cancellationToken),
                RequestCreatedToday = await query.CountAsync(x => x.CreateDateTime >= today && x.CreateDateTime < tomorrow, cancellationToken)
            };
        }

        public async Task<AttendanceCorrectionPagedResponse> GetPagedAsync(
            AttendanceCorrectionQueryRequest request,
            Guid? requestedByUserId = null,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);

            var query = ApplyFilter(BuildBaseQuery(), request);
            if (requestedByUserId.HasValue)
            {
                query = query.Where(x => x.RequestedByUserId == requestedByUserId.Value);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.RequestReason)
                .Include(x => x.RequestedByUser)
                .Include(x => x.Details)
                .Include(x => x.Exceptions)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            var ids = entities.Select(x => x.Id).ToList();
            var workflows = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    ids.Contains(x.ReferenceId) &&
                    SupportedWorkflowReferenceTypes.Contains(x.ReferenceType) &&
                    !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var latestWorkflow = workflows
                .GroupBy(x => x.ReferenceId)
                .ToDictionary(x => x.Key, x => x.First());

            return new AttendanceCorrectionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x =>
                    MapList(x, latestWorkflow.GetValueOrDefault(x.Id))).ToList()
            };
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>>
            GetDetailAsync(
                Guid id,
                Guid? ownerUserId = null,
                CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.RequestReason)
                .Include(x => x.RequestedByUser)
                .Include(x => x.Details)
                .Include(x => x.Exceptions)
                .AsSplitQuery()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete,
                    cancellationToken);

            if (request == null ||
                (ownerUserId.HasValue && request.RequestedByUserId != ownerUserId.Value))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            var workflow = await FindLatestWorkflowAsync(id, cancellationToken);
            WorkflowInstanceDetailResponse? workflowDetail = null;
            if (workflow != null)
            {
                var workflowResult = await _workflowService.GetByIdAsync(
                    workflow.Id,
                    cancellationToken);
                if (workflowResult.Success)
                {
                    workflowDetail = workflowResult.Data;
                }
            }

            return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Ok(
                MapDetail(request, workflow, workflowDetail),
                "Detail attendance correction berhasil diambil.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>>
            CreateAsync(
                CreateAttendanceCorrectionRequest request,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty || request.AttendanceDailyId == Guid.Empty)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Attendance daily id atau actor user id tidak valid.");
            }

            var actorWorkforceProfileId = await ResolveActorWorkforceProfileIdAsync(
                actorUserId,
                cancellationToken);
            if (!actorWorkforceProfileId.HasValue)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Akun login belum terhubung dengan workforce profile.");
            }

            var daily = await _dbContext.Set<TrxAttendanceDaily>()
                .Include(x => x.AttendancePolicy)
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(
                    x => x.Id == request.AttendanceDailyId && !x.IsDelete,
                    cancellationToken);

            if (daily == null || daily.WorkforceProfileId != actorWorkforceProfileId)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan untuk workforce user login.");
            }

            var eligibilityError = ValidateCorrectionEligibility(daily);
            if (eligibilityError != null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    eligibilityError);
            }

            var duplicate = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.AttendanceDailyId == daily.Id &&
                    ActiveRequestStatuses.Contains(x.RequestStatus) &&
                    !x.IsDelete,
                    cancellationToken);

            if (duplicate)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Masih terdapat attendance correction aktif untuk attendance daily tersebut.");
            }

            var validation = await ValidateRequestContentAsync(
                daily,
                request.CorrectionType,
                request.RequestReasonId,
                request.Details,
                request.ExceptionIds,
                null,
                cancellationToken);
            if (validation != null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var now = DateTime.UtcNow;
                var entity = new HrdAttendanceCorrectionRequest
                {
                    Id = Guid.NewGuid(),
                    RequestNumber = GenerateRequestNumber(),
                    WorkforceProfileId = actorWorkforceProfileId.Value,
                    AttendanceDailyId = daily.Id,
                    RequestReasonId = NormalizeGuid(request.RequestReasonId),
                    RequestedByWorkforceProfileId = actorWorkforceProfileId.Value,
                    RequestedByUserId = actorUserId,
                    AttendanceDate = daily.AttendanceDate,
                    CorrectionType = request.CorrectionType.Trim(),
                    RequestStatus = AttendanceValueConstants.CorrectionRequestStatus.Draft,
                    Reason = request.Reason.Trim(),
                    OriginalSummaryJson = BuildDailySummaryJson(daily),
                    RequestedSummaryJson = BuildRequestedSummaryJson(request.Details),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                AddDetails(entity, daily, request.Details, actorUserId, now);
                _dbContext.Set<HrdAttendanceCorrectionRequest>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await LinkExceptionsAsync(
                    entity.Id,
                    daily.Id,
                    request.ExceptionIds,
                    actorUserId,
                    now,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Ok(
                    new AttendanceCorrectionCreateResponse
                    {
                        Id = entity.Id,
                        RequestNumber = entity.RequestNumber,
                        RequestStatus = entity.RequestStatus,
                        AttendanceDailyId = daily.Id,
                        AttendanceDate = daily.AttendanceDate
                    },
                    "Draft attendance correction berhasil dibuat.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AttendanceCorrectionServiceResult<AttendanceCorrectionCreateResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance correction gagal dibuat karena terjadi konflik data.");
            }
        }

        public async Task<AttendanceCorrectionServiceResult<object?>> UpdateAsync(
            Guid id,
            UpdateAttendanceCorrectionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .Include(x => x.Details)
                .Include(x => x.AttendanceDaily)
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon yang dapat mengubah attendance correction.");
            }

            if (!EditableStatuses.Contains(entity.RequestStatus))
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance correction hanya dapat diubah dari status Draft atau NeedRevision.");
            }

            var daily = entity.AttendanceDaily;
            if (daily == null)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily sumber koreksi tidak ditemukan.");
            }

            var validation = await ValidateRequestContentAsync(
                daily,
                request.CorrectionType,
                request.RequestReasonId,
                request.Details,
                request.ExceptionIds,
                entity.Id,
                cancellationToken);
            if (validation != null)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var now = DateTime.UtcNow;
                entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
                entity.CorrectionType = request.CorrectionType.Trim();
                entity.Reason = request.Reason.Trim();
                entity.RequestedSummaryJson = BuildRequestedSummaryJson(request.Details);
                entity.RequestStatus = AttendanceValueConstants.CorrectionRequestStatus.Draft;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                foreach (var detail in entity.Details.Where(x => !x.IsDelete))
                {
                    detail.IsDelete = true;
                    detail.IsActive = false;
                    detail.DeleteDateTime = now;
                    detail.DeleteBy = actorUserId;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                }

                AddDetails(entity, daily, request.Details, actorUserId, now);

                var previousLinks = await _dbContext.Set<TrxAttendanceException>()
                    .Where(x => x.CorrectionRequestId == entity.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                foreach (var exception in previousLinks)
                {
                    exception.CorrectionRequestId = null;
                    exception.ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.Open;
                    exception.UpdateDateTime = now;
                    exception.UpdateBy = actorUserId;
                }

                await LinkExceptionsAsync(
                    entity.Id,
                    daily.Id,
                    request.ExceptionIds,
                    actorUserId,
                    now,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return AttendanceCorrectionServiceResult<object?>.Ok(
                    null,
                    "Attendance correction berhasil diperbarui.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance correction gagal diperbarui karena terjadi konflik data.");
            }
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>>
            SubmitAsync(
                Guid id,
                AttendanceCorrectionSubmitRequest? request,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .Include(x => x.Details)
                .Include(x => x.RequestReason)
                .Include(x => x.AttendanceDaily)
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon yang dapat submit attendance correction.");
            }

            if (!EditableStatuses.Contains(entity.RequestStatus))
            {
                var running = await FindLatestWorkflowAsync(id, cancellationToken);
                if (running != null && IsWorkflowRunning(running.WorkflowStatus))
                {
                    var existingDetail = await _workflowService.GetByIdAsync(
                        running.Id,
                        cancellationToken);
                    if (existingDetail.Success && existingDetail.Data != null)
                    {
                        return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Ok(
                            BuildWorkflowResponse(entity, existingDetail.Data),
                            "Attendance correction sudah berada dalam proses workflow.");
                    }
                }

                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance correction hanya dapat di-submit dari status Draft atau NeedRevision.");
            }

            if (!entity.Details.Any(x => !x.IsDelete && x.IsActive))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Attendance correction harus mempunyai minimal satu detail aktif.");
            }

            if (entity.RequestReason?.IsAttachmentRequired == true &&
                string.IsNullOrWhiteSpace(entity.EvidenceFilePath))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan pengajuan yang dipilih mewajibkan evidence attachment.");
            }

            var existingWorkflow = await FindLatestWorkflowAsync(id, cancellationToken);
            WorkflowInstanceDetailResponse workflowDetail;
            var workflowCreatedNow = false;

            if (existingWorkflow != null)
            {
                if (string.Equals(existingWorkflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Draft, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(existingWorkflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.RevisionRequested, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(existingWorkflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Returned, StringComparison.OrdinalIgnoreCase))
                {
                    var submitExisting = await _workflowService.SubmitAsync(
                        existingWorkflow.Id,
                        new WorkflowSubmitRequest
                        {
                            Comment = NormalizeOptionalText(request?.Comment),
                            IdempotencyKey = NormalizeOptionalText(request?.IdempotencyKey)
                        },
                        cancellationToken);

                    if (!submitExisting.Success || submitExisting.Data == null)
                    {
                        return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                            submitExisting.StatusCode,
                            submitExisting.Message);
                    }

                    workflowDetail = submitExisting.Data;
                }
                else if (IsWorkflowRunning(existingWorkflow.WorkflowStatus))
                {
                    var existingDetail = await _workflowService.GetByIdAsync(
                        existingWorkflow.Id,
                        cancellationToken);
                    if (!existingDetail.Success || existingDetail.Data == null)
                    {
                        return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                            existingDetail.StatusCode,
                            existingDetail.Message);
                    }
                    workflowDetail = existingDetail.Data;
                }
                else
                {
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Workflow attendance correction sebelumnya sudah berstatus {existingWorkflow.WorkflowStatus} dan tidak dapat digunakan kembali.");
                }
            }
            else
            {
                var workflowCodeResult = await ResolveWorkflowCodeAsync(entity, cancellationToken);
                if (!workflowCodeResult.Success)
                {
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                        workflowCodeResult.StatusCode,
                        workflowCodeResult.Message);
                }

                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = workflowCodeResult.WorkflowCode!,
                        ReferenceType = WorkflowReferenceLifecycleService.AttendanceCorrectionReferenceType,
                        ReferenceId = entity.Id,
                        ExternalReferenceNumber = entity.RequestNumber,
                        SourceChannel = NormalizeSourceChannel(request?.SourceChannel),
                        RequestCorrelationId = NormalizeOptionalText(request?.RequestCorrelationId),
                        IdempotencyKey = $"attendance-correction:{entity.Id:N}:workflow",
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            attendanceCorrectionRequestId = entity.Id,
                            entity.RequestNumber,
                            entity.WorkforceProfileId,
                            entity.AttendanceDailyId,
                            entity.AttendanceDate,
                            entity.CorrectionType,
                            entity.RequestReasonId,
                            entity.Reason,
                            detailCount = entity.Details.Count(x => !x.IsDelete && x.IsActive),
                            hasEvidence = !string.IsNullOrWhiteSpace(entity.EvidenceFilePath),
                            requestedByUserId = entity.RequestedByUserId
                        }),
                        SelectedApproverUserIds = request?.SelectedApproverUserIds?
                            .Where(x => x != Guid.Empty)
                            .Distinct()
                            .ToList() ?? new List<Guid>()
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                        createResult.StatusCode,
                        createResult.Message);
                }

                workflowCreatedNow = true;
                workflowDetail = createResult.Data;
                entity.WorkflowDefinitionId = workflowDetail.WorkflowDefinitionId;
                entity.WorkflowInstanceId = workflowDetail.Id;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var submitResult = await _workflowService.SubmitAsync(
                    workflowDetail.Id,
                    new WorkflowSubmitRequest
                    {
                        Comment = NormalizeOptionalText(request?.Comment),
                        IdempotencyKey = NormalizeOptionalText(request?.IdempotencyKey)
                    },
                    cancellationToken);

                if (!submitResult.Success || submitResult.Data == null)
                {
                    if (workflowCreatedNow)
                    {
                        await SoftDeleteFailedDraftWorkflowAsync(
                            workflowDetail.Id,
                            actorUserId,
                            cancellationToken);
                    }

                    return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                        submitResult.StatusCode,
                        submitResult.Message);
                }

                workflowDetail = submitResult.Data;
            }

            var latestWorkflow = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == workflowDetail.Id, cancellationToken);

            await _lifecycleService.SynchronizeAsync(
                latestWorkflow,
                actorUserId,
                allowAutoApply: false,
                cancellationToken);

            entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == id, cancellationToken);

            return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Ok(
                BuildWorkflowResponse(entity, workflowDetail),
                "Attendance correction berhasil di-submit ke workflow.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>>
            CancelAsync(
                Guid id,
                AttendanceCorrectionCancelRequest request,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon yang dapat membatalkan attendance correction.");
            }

            if (string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Applied, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance correction sudah berada pada status terminal dan tidak dapat dibatalkan.");
            }

            var workflow = await FindLatestWorkflowAsync(id, cancellationToken);
            if (workflow == null)
            {
                var now = DateTime.UtcNow;
                entity.RequestStatus = AttendanceValueConstants.CorrectionRequestStatus.Cancelled;
                entity.IsCancel = true;
                entity.CancelDateTime = now;
                entity.CancelBy = actorUserId;
                entity.FinalNote = request.Reason.Trim();
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Ok(
                    new AttendanceCorrectionWorkflowResponse
                    {
                        AttendanceCorrectionRequestId = entity.Id,
                        AttendanceCorrectionRequestNumber = entity.RequestNumber,
                        AttendanceCorrectionStatus = entity.RequestStatus,
                        IsSynchronized = true,
                        IsAutoApplyPending = false
                    },
                    "Attendance correction berhasil dibatalkan sebelum workflow dibuat.");
            }

            WorkflowServiceResult<WorkflowInstanceDetailResponse> workflowResult;
            if (string.Equals(workflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase))
            {
                workflowResult = await _workflowService.WithdrawAsync(
                    workflow.Id,
                    new WorkflowWithdrawRequest
                    {
                        Reason = request.Reason.Trim(),
                        IdempotencyKey = NormalizeOptionalText(request.IdempotencyKey)
                    },
                    cancellationToken);
            }
            else
            {
                workflowResult = await _workflowService.CancelAsync(
                    workflow.Id,
                    new WorkflowCancelRequest
                    {
                        Reason = request.Reason.Trim(),
                        IdempotencyKey = NormalizeOptionalText(request.IdempotencyKey)
                    },
                    cancellationToken);
            }

            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            var trackedWorkflow = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == workflow.Id, cancellationToken);
            await _lifecycleService.SynchronizeAsync(
                trackedWorkflow,
                actorUserId,
                allowAutoApply: false,
                cancellationToken);

            entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == id, cancellationToken);

            return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowResponse>.Ok(
                BuildWorkflowResponse(entity, workflowResult.Data),
                "Attendance correction dan workflow berhasil dibatalkan.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowLinkResponse>>
            GetWorkflowAsync(
                Guid id,
                Guid? ownerUserId = null,
                CancellationToken cancellationToken = default)
        {
            var source = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (source == null ||
                (ownerUserId.HasValue && source.RequestedByUserId != ownerUserId.Value))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowLinkResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            var workflow = await FindLatestWorkflowAsync(id, cancellationToken);
            if (workflow == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowLinkResponse>.Ok(
                    new AttendanceCorrectionWorkflowLinkResponse
                    {
                        AttendanceCorrectionRequestId = source.Id,
                        AttendanceCorrectionRequestNumber = source.RequestNumber,
                        AttendanceCorrectionStatus = source.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = string.Equals(source.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Draft, StringComparison.OrdinalIgnoreCase),
                        IsAutoApplyPending = false
                    },
                    "Attendance correction belum mempunyai workflow instance.");
            }

            var detailResult = await _workflowService.GetByIdAsync(
                workflow.Id,
                cancellationToken);
            if (!detailResult.Success || detailResult.Data == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowLinkResponse>.Fail(
                    detailResult.StatusCode,
                    detailResult.Message);
            }

            return AttendanceCorrectionServiceResult<AttendanceCorrectionWorkflowLinkResponse>.Ok(
                new AttendanceCorrectionWorkflowLinkResponse
                {
                    AttendanceCorrectionRequestId = source.Id,
                    AttendanceCorrectionRequestNumber = source.RequestNumber,
                    AttendanceCorrectionStatus = source.RequestStatus,
                    HasWorkflow = true,
                    IsSynchronized = IsSynchronized(source.RequestStatus, workflow.WorkflowStatus),
                    IsAutoApplyPending = IsAutoApplyPending(source.RequestStatus, workflow.WorkflowStatus),
                    Workflow = detailResult.Data
                },
                "Relasi workflow attendance correction berhasil diambil.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionSynchronizationResponse>>
            SynchronizeAsync(
                Guid id,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var workflow = await FindLatestWorkflowAsync(id, cancellationToken);
            if (workflow == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionSynchronizationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow attendance correction tidak ditemukan.");
            }

            var synchronization = await _lifecycleService.SynchronizeAsync(
                workflow,
                actorUserId,
                allowAutoApply: true,
                cancellationToken);

            return AttendanceCorrectionServiceResult<AttendanceCorrectionSynchronizationResponse>.Ok(
                new AttendanceCorrectionSynchronizationResponse
                {
                    AttendanceCorrectionRequestId = id,
                    WorkflowInstanceId = workflow.Id,
                    PreviousAttendanceCorrectionStatus = synchronization.PreviousReferenceStatus,
                    CurrentAttendanceCorrectionStatus = synchronization.CurrentReferenceStatus,
                    WorkflowStatus = synchronization.WorkflowStatus,
                    StatusChanged = synchronization.StatusChanged,
                    AutoApplyAttempted = synchronization.AutoApplyAttempted,
                    AutoApplySucceeded = synchronization.AutoApplySucceeded,
                    WarningMessage = synchronization.WarningMessage
                },
                synchronization.WarningMessage ??
                "Status attendance correction berhasil disinkronkan dengan workflow.");
        }

        public Task<AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>>
            ApplyAsync(
                Guid id,
                AttendanceCorrectionApplyRequest? request,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            return _lifecycleService.ApplyApprovedRequestAsync(
                id,
                actorUserId,
                request?.Note,
                cancellationToken);
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceResponse>>
            UploadEvidenceAsync(
                Guid id,
                Microsoft.AspNetCore.Http.IFormFile file,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon yang dapat mengunggah evidence.");
            }

            if (!EditableStatuses.Contains(entity.RequestStatus))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Evidence hanya dapat diubah dari status Draft atau NeedRevision.");
            }

            var saveResult = await _fileStorageService.SaveAsync(
                entity.Id,
                file,
                cancellationToken);
            if (!saveResult.Success || saveResult.Data == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceResponse>.Fail(
                    saveResult.StatusCode,
                    saveResult.Message);
            }

            var oldPath = entity.EvidenceFilePath;
            entity.EvidenceFilePath = saveResult.Data.RelativePath;
            entity.EvidenceFileName = Path.GetFileName(file.FileName);
            entity.EvidenceContentType = saveResult.Data.ContentType;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _fileStorageService.DeletePhysicalFileAsync(oldPath, cancellationToken);

            return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceResponse>.Ok(
                BuildEvidenceResponse(entity),
                "Evidence attendance correction berhasil diunggah.");
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceDownload>>
            ResolveEvidenceDownloadAsync(
                Guid id,
                Guid? ownerUserId = null,
                CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null ||
                (ownerUserId.HasValue && entity.RequestedByUserId != ownerUserId.Value))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceDownload>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(entity.EvidenceFilePath))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceDownload>.Fail(
                    StatusCodes.Status404NotFound,
                    "Evidence attendance correction belum tersedia.");
            }

            var pathResult = _fileStorageService.ResolveDownloadPath(entity.EvidenceFilePath);
            if (!pathResult.Success || string.IsNullOrWhiteSpace(pathResult.Data))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceDownload>.Fail(
                    pathResult.StatusCode,
                    pathResult.Message);
            }

            return AttendanceCorrectionServiceResult<AttendanceCorrectionEvidenceDownload>.Ok(
                new AttendanceCorrectionEvidenceDownload
                {
                    PhysicalPath = pathResult.Data,
                    FileName = entity.EvidenceFileName ?? "attendance-correction-evidence",
                    ContentType = entity.EvidenceContentType ?? "application/octet-stream"
                },
                "Evidence attendance correction berhasil ditemukan.");
        }

        public async Task<AttendanceCorrectionServiceResult<object?>> DeleteEvidenceAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon yang dapat menghapus evidence.");
            }

            if (!EditableStatuses.Contains(entity.RequestStatus))
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status409Conflict,
                    "Evidence hanya dapat dihapus dari status Draft atau NeedRevision.");
            }

            var path = entity.EvidenceFilePath;
            entity.EvidenceFilePath = null;
            entity.EvidenceFileName = null;
            entity.EvidenceContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _fileStorageService.DeletePhysicalFileAsync(path, cancellationToken);

            return AttendanceCorrectionServiceResult<object?>.Ok(
                null,
                "Evidence attendance correction berhasil dihapus.");
        }

        public async Task<AttendanceCorrectionServiceResult<object?>> DeleteDraftAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance correction tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon yang dapat menghapus attendance correction.");
            }

            if (!string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceCorrectionServiceResult<object?>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya attendance correction berstatus Draft yang dapat dihapus.");
            }

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            foreach (var detail in entity.Details.Where(x => !x.IsDelete))
            {
                detail.IsDelete = true;
                detail.IsActive = false;
                detail.DeleteDateTime = now;
                detail.DeleteBy = actorUserId;
            }

            var exceptions = await _dbContext.Set<TrxAttendanceException>()
                .Where(x => x.CorrectionRequestId == entity.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var exception in exceptions)
            {
                exception.CorrectionRequestId = null;
                exception.ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.Open;
                exception.UpdateDateTime = now;
                exception.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _fileStorageService.DeletePhysicalFileAsync(
                entity.EvidenceFilePath,
                cancellationToken);

            return AttendanceCorrectionServiceResult<object?>.Ok(
                null,
                "Draft attendance correction berhasil dihapus.");
        }

        private IQueryable<HrdAttendanceCorrectionRequest> BuildBaseQuery() =>
            _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

        private static IQueryable<HrdAttendanceCorrectionRequest> ApplyFilter(
            IQueryable<HrdAttendanceCorrectionRequest> query,
            AttendanceCorrectionQueryRequest request)
        {
            var range = ResolveDateRange(request.StartDate, request.EndDate, request.CustomPeriod);
            if (range.Start.HasValue)
                query = query.Where(x => x.AttendanceDate >= range.Start.Value);
            if (range.End.HasValue)
                query = query.Where(x => x.AttendanceDate <= range.End.Value);
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.AttendanceDailyId.HasValue && request.AttendanceDailyId != Guid.Empty)
                query = query.Where(x => x.AttendanceDailyId == request.AttendanceDailyId.Value);
            if (request.WorkflowDefinitionId.HasValue && request.WorkflowDefinitionId != Guid.Empty)
                query = query.Where(x => x.WorkflowDefinitionId == request.WorkflowDefinitionId.Value);
            if (!string.IsNullOrWhiteSpace(request.CorrectionType))
                query = query.Where(x => x.CorrectionType == request.CorrectionType.Trim());
            if (!string.IsNullOrWhiteSpace(request.RequestStatus))
                query = query.Where(x => x.RequestStatus == request.RequestStatus.Trim());
            if (request.HasEvidence.HasValue)
                query = request.HasEvidence.Value
                    ? query.Where(x => x.EvidenceFilePath != null)
                    : query.Where(x => x.EvidenceFilePath == null);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    x.CorrectionType.ToLower().Contains(keyword) ||
                    x.WorkforceProfile != null &&
                    (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                     x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<HrdAttendanceCorrectionRequest> ApplySorting(
            IQueryable<HrdAttendanceCorrectionRequest> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "attendancedate" => desc ? query.OrderByDescending(x => x.AttendanceDate) : query.OrderBy(x => x.AttendanceDate),
                "requestnumber" => desc ? query.OrderByDescending(x => x.RequestNumber) : query.OrderBy(x => x.RequestNumber),
                "requeststatus" => desc ? query.OrderByDescending(x => x.RequestStatus) : query.OrderBy(x => x.RequestStatus),
                "workforcename" => desc
                    ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                    : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private AttendanceCorrectionListResponse MapList(
            HrdAttendanceCorrectionRequest entity,
            TrxWorkflowInstance? workflow)
        {
            return new AttendanceCorrectionListResponse
            {
                Id = entity.Id,
                RequestNumber = entity.RequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName,
                AttendanceDailyId = entity.AttendanceDailyId,
                AttendanceDate = entity.AttendanceDate,
                CorrectionType = entity.CorrectionType,
                RequestStatus = entity.RequestStatus,
                Reason = entity.Reason,
                RequestReasonId = entity.RequestReasonId,
                RequestReasonCode = entity.RequestReason?.ReasonCode,
                RequestReasonName = entity.RequestReason?.ReasonName,
                WorkflowInstanceId = workflow?.Id ?? entity.WorkflowInstanceId,
                WorkflowRequestNumber = workflow?.RequestNumber,
                WorkflowStatus = workflow?.WorkflowStatus,
                DetailCount = entity.Details.Count(x => !x.IsDelete && x.IsActive),
                LinkedExceptionCount = entity.Exceptions.Count(x => !x.IsDelete && x.IsActive),
                HasEvidence = !string.IsNullOrWhiteSpace(entity.EvidenceFilePath),
                SubmittedAt = entity.SubmittedAt,
                ApprovedAt = entity.ApprovedAt,
                RejectedAt = entity.RejectedAt,
                AppliedAt = entity.AppliedAt,
                CreateDateTime = entity.CreateDateTime,
                RequestedByUserName = entity.RequestedByUser?.DisplayName ??
                                      entity.RequestedByUser?.UserName ??
                                      entity.RequestedByUser?.Email ??
                                      entity.RequestedByUser?.UserCode
            };
        }

        private AttendanceCorrectionDetailResponse MapDetail(
            HrdAttendanceCorrectionRequest entity,
            TrxWorkflowInstance? workflow,
            WorkflowInstanceDetailResponse? workflowDetail)
        {
            var list = MapList(entity, workflow);
            var editable = EditableStatuses.Contains(entity.RequestStatus);
            var applied = string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Applied, StringComparison.OrdinalIgnoreCase);
            var terminal = applied ||
                           string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Cancelled, StringComparison.OrdinalIgnoreCase);

            var response = new AttendanceCorrectionDetailResponse
            {
                Id = list.Id,
                RequestNumber = list.RequestNumber,
                WorkforceProfileId = list.WorkforceProfileId,
                WorkforceProfileCode = list.WorkforceProfileCode,
                WorkforceDisplayName = list.WorkforceDisplayName,
                AttendanceDailyId = list.AttendanceDailyId,
                AttendanceDate = list.AttendanceDate,
                CorrectionType = list.CorrectionType,
                RequestStatus = list.RequestStatus,
                Reason = list.Reason,
                RequestReasonId = list.RequestReasonId,
                RequestReasonCode = list.RequestReasonCode,
                RequestReasonName = list.RequestReasonName,
                WorkflowInstanceId = list.WorkflowInstanceId,
                WorkflowRequestNumber = list.WorkflowRequestNumber,
                WorkflowStatus = list.WorkflowStatus,
                DetailCount = list.DetailCount,
                LinkedExceptionCount = list.LinkedExceptionCount,
                HasEvidence = list.HasEvidence,
                SubmittedAt = list.SubmittedAt,
                ApprovedAt = list.ApprovedAt,
                RejectedAt = list.RejectedAt,
                AppliedAt = list.AppliedAt,
                CreateDateTime = list.CreateDateTime,
                RequestedByUserName = list.RequestedByUserName,
                RequestedByWorkforceProfileId = entity.RequestedByWorkforceProfileId,
                RequestedByUserId = entity.RequestedByUserId,
                OriginalSummaryJson = entity.OriginalSummaryJson,
                RequestedSummaryJson = entity.RequestedSummaryJson,
                ApprovedSummaryJson = entity.ApprovedSummaryJson,
                FinalNote = entity.FinalNote,
                UpdateDateTime = entity.UpdateDateTime,
                CanEdit = editable,
                CanSubmit = editable,
                CanCancel = !terminal,
                CanDelete = string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Draft, StringComparison.OrdinalIgnoreCase),
                CanUploadEvidence = editable,
                CanApply = string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(entity.RequestStatus, AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved, StringComparison.OrdinalIgnoreCase),
                Details = entity.Details
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.CreateDateTime)
                    .Select(x => new AttendanceCorrectionDetailItemResponse
                    {
                        Id = x.Id,
                        FieldName = x.FieldName,
                        FieldLabel = AttendanceCorrectionFieldCatalog.GetLabel(x.FieldName),
                        DataType = x.DataType,
                        OriginalValue = x.OriginalValue,
                        RequestedValue = x.RequestedValue,
                        ApprovedValue = x.ApprovedValue,
                        DetailStatus = x.DetailStatus,
                        Reason = x.Reason,
                        IsApplied = x.IsApplied,
                        AppliedAt = x.AppliedAt,
                        SortOrder = x.SortOrder
                    }).ToList(),
                Exceptions = entity.Exceptions
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderByDescending(x => x.DetectedAt)
                    .Select(x => new AttendanceCorrectionExceptionResponse
                    {
                        Id = x.Id,
                        ExceptionCode = x.ExceptionCode,
                        ExceptionType = x.ExceptionType,
                        Severity = x.Severity,
                        ExceptionStatus = x.ExceptionStatus,
                        Message = x.Message,
                        IsPayrollBlocking = x.IsPayrollBlocking,
                        DetectedAt = x.DetectedAt,
                        ResolvedAt = x.ResolvedAt,
                        ResolutionNote = x.ResolutionNote
                    }).ToList(),
                Evidence = BuildEvidenceResponse(entity)
            };

            if (editable) response.AvailableActions.AddRange(new[] { "Update", "UploadEvidence", "Submit", "Cancel", "Delete" });
            else if (!terminal) response.AvailableActions.Add("Cancel");
            if (response.CanApply) response.AvailableActions.Add("Apply");
            if (workflow != null)
            {
                response.WorkflowLink = new AttendanceCorrectionWorkflowLinkResponse
                {
                    AttendanceCorrectionRequestId = entity.Id,
                    AttendanceCorrectionRequestNumber = entity.RequestNumber,
                    AttendanceCorrectionStatus = entity.RequestStatus,
                    HasWorkflow = true,
                    IsSynchronized = IsSynchronized(entity.RequestStatus, workflow.WorkflowStatus),
                    IsAutoApplyPending = IsAutoApplyPending(entity.RequestStatus, workflow.WorkflowStatus),
                    Workflow = workflowDetail
                };
            }

            return response;
        }

        private static AttendanceCorrectionEvidenceResponse BuildEvidenceResponse(
            HrdAttendanceCorrectionRequest entity)
        {
            return new AttendanceCorrectionEvidenceResponse
            {
                HasEvidence = !string.IsNullOrWhiteSpace(entity.EvidenceFilePath),
                FileName = entity.EvidenceFileName,
                ContentType = entity.EvidenceContentType,
                DownloadUrl = string.IsNullOrWhiteSpace(entity.EvidenceFilePath)
                    ? null
                    : $"/api/v1/corporate/human-resource/attendance/correction-requests/{entity.Id}/evidence/download"
            };
        }

        private async Task<string?> ValidateRequestContentAsync(
            TrxAttendanceDaily daily,
            string correctionType,
            Guid? requestReasonId,
            List<AttendanceCorrectionDetailInputRequest> details,
            List<Guid> exceptionIds,
            Guid? currentRequestId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(correctionType))
                return "Correction type wajib diisi.";
            if (details == null || details.Count == 0)
                return "Minimal satu detail koreksi wajib diisi.";

            var supportedTypes = GetMetadata().CorrectionTypeOptions.Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!supportedTypes.Contains(correctionType.Trim()))
                return "Correction type tidak didukung.";

            if (requestReasonId.HasValue && requestReasonId.Value != Guid.Empty)
            {
                var reasonExists = await _dbContext.Set<MstRequestReason>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == requestReasonId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (!reasonExists)
                    return "Request reason tidak ditemukan atau tidak aktif.";
            }

            var duplicateFields = details
                .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
                .GroupBy(x => x.FieldName.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(x => x.Count() > 1);
            if (duplicateFields != null)
                return $"Field {duplicateFields.Key} tidak boleh dikirim lebih dari satu kali.";

            foreach (var detail in details)
            {
                if (!AttendanceCorrectionFieldCatalog.TryGet(detail.FieldName, out var definition))
                    return $"Field koreksi {detail.FieldName} tidak didukung.";
                if (!AttendanceCorrectionFieldCatalog.IsAllowedForCorrectionType(definition, correctionType.Trim()))
                    return $"Field {definition.Label} tidak dapat digunakan untuk correction type {correctionType}.";

                var valueError = AttendanceCorrectionFieldCatalog.ValidateRequestedValue(definition, detail.RequestedValue);
                if (valueError != null)
                    return valueError;

                var originalValue = AttendanceCorrectionFieldCatalog.GetValue(daily, definition.FieldName);
                if (string.Equals(originalValue, detail.RequestedValue?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return $"Nilai baru field {definition.Label} sama dengan nilai attendance saat ini.";
            }

            var requestedValues = details
                .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
                .ToDictionary(
                    x => x.FieldName.Trim(),
                    x => NormalizeNullableValue(x.RequestedValue),
                    StringComparer.OrdinalIgnoreCase);

            var requestedCheckIn = ResolveRequestedDateTime(
                requestedValues,
                "FirstCheckInAt",
                daily.FirstCheckInAt);
            var requestedCheckOut = ResolveRequestedDateTime(
                requestedValues,
                "LastCheckOutAt",
                daily.LastCheckOutAt);
            if (requestedCheckIn.HasValue && requestedCheckOut.HasValue &&
                requestedCheckOut <= requestedCheckIn)
            {
                return "Waktu check-out harus lebih besar daripada waktu check-in.";
            }

            var requestedScheduledIn = ResolveRequestedDateTime(
                requestedValues,
                "ScheduledCheckInAt",
                daily.ScheduledCheckInAt);
            var requestedScheduledOut = ResolveRequestedDateTime(
                requestedValues,
                "ScheduledCheckOutAt",
                daily.ScheduledCheckOutAt);
            if (requestedScheduledIn.HasValue && requestedScheduledOut.HasValue &&
                requestedScheduledOut <= requestedScheduledIn)
            {
                return "Jadwal pulang harus lebih besar daripada jadwal masuk. Untuk shift malam, kirim tanggal pulang pada hari berikutnya.";
            }

            if (TryResolveRequestedBoolean(requestedValues, "IsPresent", out var isPresent) &&
                TryResolveRequestedBoolean(requestedValues, "IsAbsent", out var isAbsent) &&
                isPresent && isAbsent)
            {
                return "IsPresent dan IsAbsent tidak boleh sama-sama bernilai true.";
            }

            if (TryResolveRequestedBoolean(requestedValues, "IsBusinessTrip", out var isBusinessTrip) &&
                TryResolveRequestedBoolean(requestedValues, "IsRemoteAttendance", out var isRemoteAttendance) &&
                isBusinessTrip && isRemoteAttendance)
            {
                return "Business trip dan remote attendance tidak boleh aktif secara bersamaan.";
            }

            if (requestedValues.TryGetValue("WorkScheduleId", out var workScheduleValue) &&
                !string.IsNullOrWhiteSpace(workScheduleValue))
            {
                var workScheduleId = Guid.Parse(workScheduleValue);
                var exists = await _dbContext.Set<MstWorkSchedule>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == workScheduleId && x.IsActive && !x.IsDelete, cancellationToken);
                if (!exists)
                    return "Work schedule tujuan tidak ditemukan atau tidak aktif.";
            }

            if (requestedValues.TryGetValue("ShiftId", out var shiftValue) &&
                !string.IsNullOrWhiteSpace(shiftValue))
            {
                var shiftId = Guid.Parse(shiftValue);
                var exists = await _dbContext.Set<MstShift>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == shiftId && x.IsActive && !x.IsDelete, cancellationToken);
                if (!exists)
                    return "Shift tujuan tidak ditemukan atau tidak aktif.";
            }

            var normalizedExceptionIds = exceptionIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (normalizedExceptionIds.Count > 0)
            {
                var validCount = await _dbContext.Set<TrxAttendanceException>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        normalizedExceptionIds.Contains(x.Id) &&
                        x.AttendanceDailyId == daily.Id &&
                        !x.IsDelete &&
                        x.IsActive &&
                        (x.CorrectionRequestId == null || x.CorrectionRequestId == currentRequestId),
                        cancellationToken);
                if (validCount != normalizedExceptionIds.Count)
                    return "Satu atau lebih attendance exception tidak valid atau sudah terhubung ke correction request lain.";
            }

            return null;
        }

        private static string? ValidateCorrectionEligibility(TrxAttendanceDaily daily)
        {
            if (!daily.WorkforceProfileId.HasValue)
                return "Attendance daily belum terhubung dengan workforce profile.";
            if (daily.AttendanceDate > DateOnly.FromDateTime(DateTime.UtcNow))
                return "Attendance pada tanggal yang belum terjadi tidak dapat dikoreksi.";
            if (daily.IsLocked)
                return "Attendance daily sedang dikunci.";
            if (string.Equals(daily.PayrollInputStatus, AttendanceValueConstants.PayrollInputStatus.Processed, StringComparison.OrdinalIgnoreCase))
                return "Attendance daily sudah diproses ke payroll.";
            if (daily.AttendancePolicy?.AllowManualCorrection == false)
                return "Attendance policy tidak mengizinkan koreksi manual.";

            var limitDays = daily.AttendancePolicy?.CorrectionRequestLimitDays ?? 7;
            if (limitDays > 0 && daily.AttendanceDate < DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-limitDays))
                return $"Batas pengajuan koreksi attendance adalah {limitDays} hari.";

            return null;
        }

        private void AddDetails(
            HrdAttendanceCorrectionRequest entity,
            TrxAttendanceDaily daily,
            IEnumerable<AttendanceCorrectionDetailInputRequest> details,
            Guid actorUserId,
            DateTime now)
        {
            var order = 1;
            foreach (var input in details)
            {
                AttendanceCorrectionFieldCatalog.TryGet(input.FieldName, out var definition);
                entity.Details.Add(new HrdAttendanceCorrectionDetail
                {
                    Id = Guid.NewGuid(),
                    AttendanceCorrectionRequestId = entity.Id,
                    FieldName = definition.FieldName,
                    DataType = definition.DataType,
                    OriginalValue = AttendanceCorrectionFieldCatalog.GetValue(daily, definition.FieldName),
                    RequestedValue = NormalizeNullableValue(input.RequestedValue),
                    DetailStatus = "Requested",
                    Reason = NormalizeOptionalText(input.Reason),
                    SortOrder = order++,
                    IsApplied = false,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                });
            }
        }

        private async Task LinkExceptionsAsync(
            Guid requestId,
            Guid attendanceDailyId,
            IEnumerable<Guid> exceptionIds,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var ids = exceptionIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
                return;

            var exceptions = await _dbContext.Set<TrxAttendanceException>()
                .Where(x => ids.Contains(x.Id) && x.AttendanceDailyId == attendanceDailyId && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var exception in exceptions)
            {
                exception.CorrectionRequestId = requestId;
                exception.ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.UnderReview;
                exception.UpdateDateTime = now;
                exception.UpdateBy = actorUserId;
            }
        }

        private async Task<Guid?> ResolveActorWorkforceProfileIdAsync(
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<TrxWorkflowInstance?> FindLatestWorkflowAsync(
            Guid requestId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    x.ReferenceId == requestId &&
                    SupportedWorkflowReferenceTypes.Contains(x.ReferenceType) &&
                    !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<WorkflowCodeResolutionResult> ResolveWorkflowCodeAsync(
            HrdAttendanceCorrectionRequest source,
            CancellationToken cancellationToken)
        {
            if (!source.WorkflowDefinitionId.HasValue || source.WorkflowDefinitionId == Guid.Empty)
                return WorkflowCodeResolutionResult.Ok(DefaultWorkflowCode);

            var definition = await _dbContext.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == source.WorkflowDefinitionId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);
            return definition == null
                ? WorkflowCodeResolutionResult.Fail(StatusCodes.Status400BadRequest, "Workflow definition attendance correction tidak ditemukan atau tidak aktif.")
                : WorkflowCodeResolutionResult.Ok(definition.WorkflowCode);
        }

        private async Task SoftDeleteFailedDraftWorkflowAsync(
            Guid workflowInstanceId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.Set<TrxWorkflowInstance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == workflowInstanceId &&
                    x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Draft &&
                    !x.IsDelete,
                    cancellationToken);
            if (workflow == null)
                return;

            var now = DateTime.UtcNow;
            workflow.IsDelete = true;
            workflow.DeleteDateTime = now;
            workflow.DeleteBy = actorUserId;
            workflow.UpdateDateTime = now;
            workflow.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static AttendanceCorrectionWorkflowResponse BuildWorkflowResponse(
            HrdAttendanceCorrectionRequest source,
            WorkflowInstanceDetailResponse workflow)
        {
            return new AttendanceCorrectionWorkflowResponse
            {
                AttendanceCorrectionRequestId = source.Id,
                AttendanceCorrectionRequestNumber = source.RequestNumber,
                AttendanceCorrectionStatus = source.RequestStatus,
                WorkflowInstanceId = workflow.Id,
                WorkflowRequestNumber = workflow.RequestNumber,
                WorkflowStatus = workflow.WorkflowStatus,
                CurrentStepCode = workflow.CurrentStepCode,
                CurrentStepOrder = workflow.CurrentStepOrder,
                IsSynchronized = IsSynchronized(source.RequestStatus, workflow.WorkflowStatus),
                IsAutoApplyPending = IsAutoApplyPending(source.RequestStatus, workflow.WorkflowStatus),
                Workflow = workflow
            };
        }

        private static bool IsSynchronized(string requestStatus, string workflowStatus) =>
            string.Equals(
                requestStatus,
                AttendanceCorrectionWorkflowLifecycleService.MapRequestStatus(workflowStatus),
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(requestStatus, AttendanceValueConstants.CorrectionRequestStatus.Applied, StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase));

        private static bool IsAutoApplyPending(string requestStatus, string workflowStatus) =>
            (string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase)) &&
            !string.Equals(requestStatus, AttendanceValueConstants.CorrectionRequestStatus.Applied, StringComparison.OrdinalIgnoreCase);

        private static bool IsWorkflowRunning(string workflowStatus) =>
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Submitted, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase);

        private static string BuildDailySummaryJson(TrxAttendanceDaily daily) =>
            JsonSerializer.Serialize(new
            {
                daily.Id,
                daily.WorkforceProfileId,
                daily.AttendanceDate,
                daily.AttendanceStatus,
                daily.ScheduledCheckInAt,
                daily.ScheduledCheckOutAt,
                daily.FirstCheckInAt,
                daily.LastCheckOutAt,
                daily.BreakMinutes,
                daily.ActualWorkMinutes,
                daily.PayableWorkMinutes,
                daily.LateMinutes,
                daily.EarlyLeaveMinutes,
                daily.OvertimeMinutes,
                daily.IsPresent,
                daily.IsAbsent,
                daily.IsLate,
                daily.IsEarlyLeave,
                daily.HasMissingPunch,
                daily.IsBusinessTrip,
                daily.IsRemoteAttendance,
                daily.WorkScheduleId,
                daily.ShiftId,
                daily.ScheduleSource,
                daily.ProcessingVersion
            });

        private static string BuildRequestedSummaryJson(
            IEnumerable<AttendanceCorrectionDetailInputRequest> details) =>
            JsonSerializer.Serialize(details.Select(x => new
            {
                fieldName = x.FieldName.Trim(),
                requestedValue = NormalizeNullableValue(x.RequestedValue),
                reason = NormalizeOptionalText(x.Reason)
            }));

        private static DateTime? ResolveRequestedDateTime(
            IReadOnlyDictionary<string, string?> requestedValues,
            string fieldName,
            DateTime? currentValue)
        {
            if (!requestedValues.TryGetValue(fieldName, out var requestedValue))
                return currentValue;
            if (string.IsNullOrWhiteSpace(requestedValue))
                return null;

            return DateTimeOffset.Parse(
                requestedValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal)
                .UtcDateTime;
        }

        private static bool TryResolveRequestedBoolean(
            IReadOnlyDictionary<string, string?> requestedValues,
            string fieldName,
            out bool value)
        {
            value = false;
            return requestedValues.TryGetValue(fieldName, out var rawValue) &&
                   !string.IsNullOrWhiteSpace(rawValue) &&
                   bool.TryParse(rawValue, out value);
        }

        private static string GenerateRequestNumber() =>
            $"ACR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        private static string NormalizeSourceChannel(string? value)
        {
            var normalized = NormalizeOptionalText(value) ?? "Web";
            return normalized.Length <= 30 ? normalized : normalized[..30];
        }

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeOptionalText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeNullableValue(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
                "thismonth" => (new DateOnly(today.Year, today.Month, 1), today),
                "lastmonth" =>
                    (new DateOnly(today.Year, today.Month, 1).AddMonths(-1),
                     new DateOnly(today.Year, today.Month, 1).AddDays(-1)),
                _ => (null, null)
            };
        }

        private sealed class WorkflowCodeResolutionResult
        {
            public bool Success { get; private set; }
            public int StatusCode { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public string? WorkflowCode { get; private set; }

            public static WorkflowCodeResolutionResult Ok(string workflowCode) =>
                new()
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    WorkflowCode = workflowCode
                };

            public static WorkflowCodeResolutionResult Fail(int statusCode, string message) =>
                new()
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
        }
    }

    public class AttendanceCorrectionEvidenceDownload
    {
        public string PhysicalPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
