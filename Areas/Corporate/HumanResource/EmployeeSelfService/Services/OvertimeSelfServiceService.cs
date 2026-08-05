using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.Services
{
    public class OvertimeSelfServiceService
    {
        private static readonly HashSet<string> DraftBlockingCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "INVALID_INTERVAL",
            "INTERVAL_TOO_LONG",
            "OVERTIME_DATE_MISMATCH",
            "INVALID_CATEGORY",
            "ORGANIZATION_ASSIGNMENT_NOT_FOUND",
            "REQUEST_REASON_INVALID",
            "OVERTIME_PERIOD_CLOSED"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimeSelfServiceContextService _contextService;
        private readonly OvertimeSelfServiceQueryService _queryService;
        private readonly AttendanceScheduleResolverService _scheduleResolver;
        private readonly OvertimePolicyResolverService _policyResolver;
        private readonly OvertimeRateResolverService _rateResolver;
        private readonly OvertimeRequestWorkflowIntegrationService _workflowIntegrationService;
        private readonly OvertimePeriodGuardService _periodGuard;

        public OvertimeSelfServiceService(
            ApplicationDbContext dbContext,
            OvertimeSelfServiceContextService contextService,
            OvertimeSelfServiceQueryService queryService,
            AttendanceScheduleResolverService scheduleResolver,
            OvertimePolicyResolverService policyResolver,
            OvertimeRateResolverService rateResolver,
            OvertimeRequestWorkflowIntegrationService workflowIntegrationService,
            OvertimePeriodGuardService periodGuard)
        {
            _dbContext = dbContext;
            _contextService = contextService;
            _queryService = queryService;
            _scheduleResolver = scheduleResolver;
            _policyResolver = policyResolver;
            _rateResolver = rateResolver;
            _workflowIntegrationService = workflowIntegrationService;
            _periodGuard = periodGuard;
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimePreviewResponse>> PreviewAsync(
            Guid actorUserId,
            PreviewMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimePreviewResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var response = await EvaluateAsync(
                contextResult.Data,
                request,
                NormalizeGuid(request.ExcludeRequestId),
                cancellationToken);

            return OvertimeSelfServiceServiceResult<MyOvertimePreviewResponse>.Ok(
                response,
                response.CanSubmit
                    ? "Preview pengajuan lembur valid dan siap disubmit."
                    : response.CanSaveDraft
                        ? "Preview selesai. Pengajuan masih dapat disimpan sebagai Draft, tetapi belum siap disubmit."
                        : "Preview selesai. Perbaiki data wajib sebelum menyimpan Draft.");
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>> CreateDraftAsync(
            Guid actorUserId,
            CreateMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var previewRequest = ToPreviewRequest(request, null);
            var evaluation = await EvaluateAsync(contextResult.Data, previewRequest, null, cancellationToken);
            if (!evaluation.CanSaveDraft)
            {
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    BuildIssueMessage(evaluation, "Draft belum dapat disimpan."));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var now = DateTime.UtcNow;
                var requestNumber = await GenerateRequestNumberAsync(request.OvertimeDate, cancellationToken);
                var entity = BuildRequestEntity(
                    contextResult.Data,
                    request,
                    evaluation,
                    requestNumber,
                    actorUserId,
                    now);

                var detail = BuildRequestDetail(
                    entity.Id,
                    request,
                    evaluation,
                    actorUserId,
                    now);

                entity.Details.Add(detail);
                _dbContext.WfpOvertimeRequests.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var detailResult = await _queryService.GetDetailAsync(actorUserId, entity.Id, cancellationToken);
                if (!detailResult.Success || detailResult.Data == null)
                    return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(detailResult.StatusCode, detailResult.Message);

                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Ok(
                    detailResult.Data,
                    evaluation.CanSubmit
                        ? "Draft pengajuan lembur berhasil dibuat dan siap disubmit."
                        : "Draft pengajuan lembur berhasil dibuat, tetapi masih memiliki validasi yang harus diselesaikan.",
                    StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>> UpdateDraftAsync(
            Guid actorUserId,
            Guid id,
            UpdateMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var entity = await _dbContext.WfpOvertimeRequests
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == contextResult.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            var editError = ValidateEditableEntity(entity);
            if (editError != null)
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(editError.Value.StatusCode, editError.Value.Message);

            var previewRequest = ToPreviewRequest(request, id);
            var evaluation = await EvaluateAsync(contextResult.Data, previewRequest, id, cancellationToken);
            if (!evaluation.CanSaveDraft)
            {
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    BuildIssueMessage(evaluation, "Draft belum dapat diperbarui."));
            }

            var now = DateTime.UtcNow;
            ApplyRequestEvaluation(entity!, request, evaluation, actorUserId, now);
            var detail = entity!.Details.OrderBy(x => x.SequenceNumber).FirstOrDefault();
            if (detail == null)
            {
                detail = BuildRequestDetail(entity.Id, request, evaluation, actorUserId, now);
                entity.Details.Add(detail);
            }
            else
            {
                ApplyDetailEvaluation(detail, request, evaluation, entity.OvertimeRequestStatus, actorUserId, now);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            var result = await _queryService.GetDetailAsync(actorUserId, id, cancellationToken);
            if (!result.Success || result.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(result.StatusCode, result.Message);

            return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Ok(
                result.Data,
                evaluation.CanSubmit
                    ? "Draft pengajuan lembur berhasil diperbarui dan siap disubmit."
                    : "Draft pengajuan lembur berhasil diperbarui, tetapi masih memiliki validasi yang harus diselesaikan.");
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>> SubmitAsync(
            Guid actorUserId,
            Guid id,
            SubmitMyOvertimeRequest? request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var entity = await _dbContext.WfpOvertimeRequests
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == contextResult.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
                return NotFound<MyOvertimeActionResponse>();

            if (entity.RequestSource != OvertimeValueConstants.RequestSource.EmployeeSelfService)
                return Conflict<MyOvertimeActionResponse>("Request yang berasal dari manager planning bersifat read-only pada employee self service.");

            var canStartOrRecover =
                entity.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Draft ||
                entity.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.NeedRevision ||
                entity.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Submitted;

            if (!canStartOrRecover)
            {
                return Conflict<MyOvertimeActionResponse>(
                    "Request hanya dapat disubmit dari status Draft, NeedRevision, atau Submitted yang belum tersambung sempurna ke workflow.");
            }

            var source = ToUpdateRequest(entity);
            var evaluation = await EvaluateAsync(
                contextResult.Data,
                ToPreviewRequest(source, entity.Id),
                entity.Id,
                cancellationToken);

            if (!evaluation.CanSubmit)
            {
                return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    BuildIssueMessage(evaluation, "Pengajuan lembur belum dapat disubmit."));
            }

            var previous = entity.OvertimeRequestStatus;
            var now = DateTime.UtcNow;
            ApplyRequestEvaluation(entity, source, evaluation, actorUserId, now);

            var primaryDetail = entity.Details.OrderBy(x => x.SequenceNumber).FirstOrDefault();
            if (primaryDetail == null)
            {
                primaryDetail = BuildRequestDetail(entity.Id, source, evaluation, actorUserId, now);
                entity.Details.Add(primaryDetail);
            }
            else
            {
                ApplyDetailEvaluation(primaryDetail, source, evaluation, entity.OvertimeRequestStatus, actorUserId, now);
            }

            if (entity.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Submitted)
            {
                foreach (var detail in entity.Details)
                {
                    detail.DetailStatus = OvertimeValueConstants.RequestStatus.Submitted;
                }
            }

            entity.WorkflowDefinitionId = evaluation.WorkflowDefinitionId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var workflowResult = await _workflowIntegrationService.StartOrResubmitAsync(
                entity.Id,
                new StartOvertimeWorkflowRequest
                {
                    Comment = request?.Comment,
                    IdempotencyKey = request?.IdempotencyKey,
                    SourceChannel = "Web"
                },
                cancellationToken);

            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Ok(
                new MyOvertimeActionResponse
                {
                    OvertimeRequestId = workflowResult.Data.OvertimeRequestId,
                    RequestNumber = workflowResult.Data.RequestNumber,
                    PreviousStatus = previous,
                    CurrentStatus = workflowResult.Data.CurrentRequestStatus,
                    ActionAt = workflowResult.Data.ActionAt,
                    WorkflowDefinitionId = workflowResult.Data.WorkflowDefinitionId,
                    WorkflowInstanceId = workflowResult.Data.WorkflowInstanceId,
                    WorkflowStatus = workflowResult.Data.WorkflowStatus,
                    CurrentApprovalStep = workflowResult.Data.CurrentStepOrder,
                    CurrentWorkflowStepCode = workflowResult.Data.CurrentStepCode,
                    WorkflowCreated = workflowResult.Data.WorkflowCreated,
                    WorkflowSubmitted = workflowResult.Data.WorkflowSubmitted,
                    LifecycleSynchronized = workflowResult.Data.LifecycleSynchronized
                },
                workflowResult.Message);
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>> CancelAsync(
            Guid actorUserId,
            Guid id,
            CancelMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var entity = await _dbContext.WfpOvertimeRequests
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == contextResult.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
                return NotFound<MyOvertimeActionResponse>();

            var periodGuard = await CheckPeriodAsync(entity, cancellationToken);
            if (!periodGuard.IsWritable)
                return Conflict<MyOvertimeActionResponse>(periodGuard.Message);

            if (entity.RequestSource != OvertimeValueConstants.RequestSource.EmployeeSelfService)
                return Conflict<MyOvertimeActionResponse>("Request yang berasal dari manager planning tidak dapat dibatalkan melalui employee self service.");

            if (entity.WorkflowInstanceId.HasValue)
            {
                var workflowResult = await _workflowIntegrationService.CancelOrWithdrawAsync(
                    entity.Id,
                    request.Reason,
                    request.IdempotencyKey,
                    cancellationToken);

                if (!workflowResult.Success || workflowResult.Data == null)
                {
                    return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Fail(
                        workflowResult.StatusCode,
                        workflowResult.Message);
                }

                return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Ok(
                    new MyOvertimeActionResponse
                    {
                        OvertimeRequestId = workflowResult.Data.OvertimeRequestId,
                        RequestNumber = workflowResult.Data.RequestNumber,
                        PreviousStatus = workflowResult.Data.PreviousRequestStatus,
                        CurrentStatus = workflowResult.Data.CurrentRequestStatus,
                        ActionAt = workflowResult.Data.ActionAt,
                        WorkflowDefinitionId = workflowResult.Data.WorkflowDefinitionId,
                        WorkflowInstanceId = workflowResult.Data.WorkflowInstanceId,
                        WorkflowStatus = workflowResult.Data.WorkflowStatus,
                        CurrentApprovalStep = workflowResult.Data.CurrentStepOrder,
                        CurrentWorkflowStepCode = workflowResult.Data.CurrentStepCode,
                        LifecycleSynchronized = workflowResult.Data.LifecycleSynchronized
                    },
                    workflowResult.Message);
            }

            if (entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Draft &&
                entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.NeedRevision &&
                entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Submitted)
            {
                return Conflict<MyOvertimeActionResponse>("Request hanya dapat dibatalkan dari status Draft, NeedRevision, atau Submitted.");
            }

            var previous = entity.OvertimeRequestStatus;
            var now = DateTime.UtcNow;
            entity.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.ApprovalNotes = request.Reason.Trim();
            entity.IsCancel = true;
            entity.IsActive = false;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            foreach (var detail in entity.Details)
            {
                detail.DetailStatus = OvertimeValueConstants.RequestStatus.Cancelled;
                detail.IsCancel = true;
                detail.IsActive = false;
                detail.CancelDateTime = now;
                detail.CancelBy = actorUserId;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Ok(
                new MyOvertimeActionResponse
                {
                    OvertimeRequestId = entity.Id,
                    RequestNumber = entity.RequestNumber,
                    PreviousStatus = previous,
                    CurrentStatus = entity.OvertimeRequestStatus,
                    ActionAt = now,
                    WorkflowDefinitionId = entity.WorkflowDefinitionId,
                    WorkflowInstanceId = entity.WorkflowInstanceId,
                    CurrentApprovalStep = entity.CurrentApprovalStep
                },
                "Pengajuan lembur berhasil dibatalkan.");
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>> DeleteDraftAsync(
            Guid actorUserId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var entity = await _dbContext.WfpOvertimeRequests
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == contextResult.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
                return NotFound<MyOvertimeActionResponse>();

            var periodGuard = await CheckPeriodAsync(entity, cancellationToken);
            if (!periodGuard.IsWritable)
                return Conflict<MyOvertimeActionResponse>(periodGuard.Message);

            if (entity.RequestSource != OvertimeValueConstants.RequestSource.EmployeeSelfService ||
                entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Draft ||
                entity.WorkflowInstanceId.HasValue)
            {
                return Conflict<MyOvertimeActionResponse>("Hanya Draft employee self service yang belum terhubung workflow yang dapat dihapus.");
            }

            var previous = entity.OvertimeRequestStatus;
            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            foreach (var detail in entity.Details)
            {
                detail.IsDelete = true;
                detail.IsActive = false;
                detail.DeleteDateTime = now;
                detail.DeleteBy = actorUserId;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeSelfServiceServiceResult<MyOvertimeActionResponse>.Ok(
                new MyOvertimeActionResponse
                {
                    OvertimeRequestId = entity.Id,
                    RequestNumber = entity.RequestNumber,
                    PreviousStatus = previous,
                    CurrentStatus = previous,
                    ActionAt = now,
                    WorkflowDefinitionId = entity.WorkflowDefinitionId,
                    WorkflowInstanceId = entity.WorkflowInstanceId
                },
                "Draft pengajuan lembur berhasil dihapus.");
        }

        private Task<OvertimePeriodGuardResult> CheckPeriodAsync(
            WfpOvertimeRequest request,
            CancellationToken cancellationToken) =>
            _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                null,
                request.HospitalSiteId,
                request.OrganizationUnitId,
                request.DepartmentId,
                cancellationToken);

        private async Task<MyOvertimePreviewResponse> EvaluateAsync(
            OvertimeSelfServiceEmployeeContext employeeContext,
            PreviewMyOvertimeRequest request,
            Guid? excludeRequestId,
            CancellationToken cancellationToken)
        {
            var startAt = NormalizeUtc(request.PlannedStartAt);
            var endAt = NormalizeUtc(request.PlannedEndAt);
            var rawMinutes = endAt > startAt
                ? (int)Math.Ceiling((endAt - startAt).TotalMinutes)
                : 0;

            var response = new MyOvertimePreviewResponse
            {
                WorkforceProfileId = employeeContext.WorkforceProfileId,
                WorkforceProfileCode = employeeContext.WorkforceProfileCode,
                WorkforceDisplayName = employeeContext.WorkforceDisplayName,
                EmployeeId = employeeContext.EmployeeId,
                EmployeeCode = employeeContext.EmployeeCode,
                EmployeeName = employeeContext.EmployeeName,
                OvertimeDate = request.OvertimeDate,
                PlannedEndDate = endAt > startAt
                    ? DateOnly.FromDateTime(endAt)
                    : request.OvertimeDate,
                PlannedStartAt = startAt,
                PlannedEndAt = endAt,
                RawMinutes = rawMinutes,
                EstimatedBreakMinutes = Math.Max(0, request.EstimatedBreakMinutes),
                OvertimeCategory = request.OvertimeCategory?.Trim() ?? string.Empty,
                EvaluatedChecks = new List<string>
                {
                    "employeeContext",
                    "organizationAssignment",
                    "scheduleResolution",
                    "requestReason",
                    "overtimePolicy",
                    "categoryAlignment",
                    "requestOverlap",
                    "planningOverlap",
                    "dailyWeeklyMonthlyLimit",
                    "ratePreview",
                    "workflowReadiness"
                },
                DeferredChecks = new List<string>
                {
                    "approvedLeaveDeepMatch",
                    "trainingSessionConflict",
                    "minimumRestAcrossPublishedRoster",
                    "attendanceActualMatch"
                }
            };

            if (endAt <= startAt)
                AddIssue(response, "INVALID_INTERVAL", "Planned end harus lebih besar dari planned start.", true, "plannedEndAt");

            if (rawMinutes > 24 * 60)
                AddIssue(response, "INTERVAL_TOO_LONG", "Satu pengajuan lembur tidak boleh melebihi 24 jam.", true, "plannedEndAt");

            if (DateOnly.FromDateTime(startAt) != request.OvertimeDate)
                AddIssue(response, "OVERTIME_DATE_MISMATCH", "OvertimeDate harus sama dengan tanggal PlannedStartAt.", true, "overtimeDate");

            var category = NormalizeToken(request.OvertimeCategory, OvertimeValueConstants.OvertimeCategory.All);
            if (category == null)
                AddIssue(response, "INVALID_CATEGORY", "Overtime category tidak valid.", true, "overtimeCategory");
            else
                response.OvertimeCategory = category;

            var effectiveDate = DateTime.SpecifyKind(
                request.OvertimeDate.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            var assignment = await ResolveOrganizationAssignmentAsync(
                employeeContext.WorkforceProfileId,
                effectiveDate,
                cancellationToken);

            if (assignment == null)
            {
                AddIssue(response, "ORGANIZATION_ASSIGNMENT_NOT_FOUND", "Organization assignment efektif tidak ditemukan.", true, "overtimeDate");
            }
            else
            {
                response.OrganizationAssignmentId = assignment.Id;
                response.LegalEntityId = assignment.LegalEntityId;
                response.HospitalSiteId = assignment.HospitalSiteId;
                response.OrganizationUnitId = assignment.OrganizationUnitId;
                response.DepartmentId = assignment.DepartmentId;
                response.PositionId = assignment.PositionId;
                response.CostCenterId = assignment.CostCenterId;
                response.WorkLocationId = assignment.WorkLocationId;
            }

            var periodGuard = await _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                response.LegalEntityId,
                response.HospitalSiteId,
                response.OrganizationUnitId,
                response.DepartmentId,
                cancellationToken);
            if (!periodGuard.IsWritable)
                AddIssue(response, "OVERTIME_PERIOD_CLOSED", periodGuard.Message, true, "overtimeDate", periodGuard.OvertimePeriodId);

            if (request.RequestReasonId.HasValue && request.RequestReasonId.Value != Guid.Empty)
            {
                var reasonValid = await _dbContext.MstRequestReasons
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.RequestReasonId.Value &&
                        x.RequestType == OvertimeValueConstants.Workflow.RequestType &&
                        x.IsEmployeeSelectable &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate.Date) &&
                        (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate.Date),
                        cancellationToken);

                if (!reasonValid)
                    AddIssue(response, "REQUEST_REASON_INVALID", "Request reason tidak valid untuk OvertimeRequest employee self service.", true, "requestReasonId", request.RequestReasonId);
            }

            var scheduleResult = await _scheduleResolver.ResolveAsync(
                employeeContext.WorkforceProfileId,
                request.OvertimeDate,
                cancellationToken);

            if (!scheduleResult.Success || scheduleResult.Data == null)
            {
                AddIssue(response, "SCHEDULE_RESOLUTION_FAILED", scheduleResult.Message, true, "overtimeDate");
                response.HasScheduleConflict = true;
            }
            else
            {
                var schedule = scheduleResult.Data;
                response.IsScheduleResolved = schedule.IsResolved;
                response.ScheduleSource = schedule.ScheduleSource;
                response.WorkScheduleAssignmentId = schedule.WorkScheduleAssignmentId;
                response.ShiftAssignmentId = schedule.PrimaryShiftAssignmentId;
                response.WorkScheduleId = schedule.WorkScheduleId;
                response.WorkScheduleCode = schedule.WorkScheduleCode;
                response.WorkScheduleName = schedule.WorkScheduleName;
                response.ShiftId = schedule.ShiftId;
                response.ShiftCode = schedule.ShiftCode;
                response.ShiftName = schedule.ShiftName;
                response.ScheduledStartAt = schedule.ScheduledStartAt;
                response.ScheduledEndAt = schedule.ScheduledEndAt;
                response.IsRestDay = schedule.IsRestDay;
                response.IsHoliday = schedule.IsHoliday;
                response.DayType = schedule.IsHoliday
                    ? OvertimeValueConstants.DayType.Holiday
                    : schedule.IsRestDay
                        ? OvertimeValueConstants.DayType.RestDay
                        : OvertimeValueConstants.DayType.Workday;

                if (!schedule.IsResolved)
                    AddIssue(response, "SCHEDULE_NOT_RESOLVED", "Jadwal kerja pada tanggal lembur belum dapat diselesaikan.", true, "overtimeDate");

                if (schedule.HasBlockingConflict)
                {
                    AddIssue(
                        response,
                        "SCHEDULE_BLOCKING_CONFLICT",
                        schedule.ConflictCodes.Count > 0
                            ? "Jadwal memiliki blocking conflict: " + string.Join(", ", schedule.ConflictCodes)
                            : "Jadwal memiliki blocking conflict.",
                        true,
                        "overtimeDate");
                }

                ValidateCategoryAlignment(response, category, startAt, endAt);
            }

            var policyResolution = await _policyResolver.ResolveAsync(
                new OvertimePolicyResolveRequest
                {
                    WorkforceProfileId = employeeContext.WorkforceProfileId,
                    LegalEntityId = assignment?.LegalEntityId,
                    HospitalSiteId = assignment?.HospitalSiteId,
                    OrganizationUnitId = assignment?.OrganizationUnitId,
                    EmployeeCategoryId = employeeContext.EmployeeCategoryId,
                    EmploymentTypeId = employeeContext.EmploymentTypeId,
                    EffectiveDate = effectiveDate
                },
                cancellationToken);

            MstOvertimePolicy? policy = null;
            if (!policyResolution.IsResolved || policyResolution.SelectedPolicy == null)
            {
                AddIssue(response, "POLICY_NOT_RESOLVED", policyResolution.Message, true, "overtimeDate");
            }
            else if (policyResolution.IsAmbiguous)
            {
                AddIssue(response, "POLICY_AMBIGUOUS", policyResolution.Message, true, "overtimeDate", policyResolution.SelectedPolicy.Id);
            }
            else
            {
                policy = await _dbContext.MstOvertimePolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == policyResolution.SelectedPolicy.Id, cancellationToken);
            }

            if (policy != null)
            {
                response.OvertimePolicyId = policy.Id;
                response.OvertimePolicyCode = policy.OvertimePolicyCode;
                response.OvertimePolicyName = policy.OvertimePolicyName;
                response.RequirePreApproval = policy.RequirePreApproval;
                response.RequirePostVerification = policy.RequirePostVerification;
                response.RequireAttendanceMatch = policy.RequireAttendanceMatch;
                response.ApprovalWorkflowCode = policy.ApprovalWorkflowCode;

                if (!IsCategoryAllowed(policy, response.OvertimeCategory))
                    AddIssue(response, "CATEGORY_NOT_ALLOWED", "Kategori lembur tidak diizinkan oleh policy.", true, "overtimeCategory", policy.Id);

                if (!IsDayTypeAllowed(policy, response.DayType))
                    AddIssue(response, "DAY_TYPE_NOT_ALLOWED", "Day type tidak diizinkan oleh policy.", true, "overtimeDate", policy.Id);

                var breakMinutes = policy.DeductBreakMinutes
                    ? Math.Max(request.EstimatedBreakMinutes, policy.BreakDeductionMinutes)
                    : Math.Max(0, request.EstimatedBreakMinutes);

                breakMinutes = Math.Min(Math.Max(0, breakMinutes), rawMinutes);
                var eligible = Math.Max(0, rawMinutes - breakMinutes - Math.Max(0, policy.OvertimeThresholdMinutes));
                var rounded = RoundMinutes(eligible, policy.RoundingIntervalMinutes, policy.RoundingMethod);
                response.EstimatedBreakMinutes = breakMinutes;
                response.EligibleMinutes = eligible;
                response.RoundedMinutes = rounded;

                if (rounded < policy.MinimumOvertimeMinutes)
                    AddIssue(response, "BELOW_MINIMUM", "Menit lembur setelah threshold, break, dan rounding berada di bawah minimum policy.", true);

                if (policy.MaximumOvertimeMinutesPerDay.HasValue && rounded > policy.MaximumOvertimeMinutesPerDay.Value)
                {
                    response.HasWorkHourLimitConflict = true;
                    AddIssue(response, "DAILY_LIMIT_EXCEEDED", "Menit lembur melebihi batas harian policy.", true);
                }

                await ValidateAggregateLimitsAsync(
                    response,
                    employeeContext.WorkforceProfileId,
                    request.OvertimeDate,
                    rounded,
                    policy,
                    excludeRequestId,
                    cancellationToken);

                if (rounded > 0 && !string.IsNullOrWhiteSpace(response.DayType))
                {
                    var rateResolution = await _rateResolver.ResolveAsync(
                        new OvertimeRateResolveRequest
                        {
                            OvertimePolicyId = policy.Id,
                            DayType = response.DayType,
                            EffectiveDate = effectiveDate,
                            MinutePosition = 0,
                            EligibleMinutes = rounded,
                            OccurrenceTime = TimeOnly.FromDateTime(startAt)
                        },
                        cancellationToken);

                    if (!rateResolution.IsResolved || rateResolution.SelectedRate == null)
                    {
                        AddIssue(response, "RATE_NOT_RESOLVED", rateResolution.Message, true);
                    }
                    else if (rateResolution.IsAmbiguous)
                    {
                        AddIssue(response, "RATE_AMBIGUOUS", rateResolution.Message, true, referenceId: rateResolution.SelectedRate.Id);
                    }
                    else
                    {
                        response.PreviewOvertimeRateId = rateResolution.SelectedRate.Id;
                        response.PreviewOvertimeRateCode = rateResolution.SelectedRate.OvertimeRateCode;
                        response.PreviewOvertimeRateName = rateResolution.SelectedRate.OvertimeRateName;
                        response.PreviewRateMultiplier = rateResolution.SelectedRate.RateMultiplier;
                    }
                }

                await ResolveWorkflowReadinessAsync(response, policy, effectiveDate, cancellationToken);
            }
            else
            {
                response.EligibleMinutes = Math.Max(0, rawMinutes - response.EstimatedBreakMinutes);
                response.RoundedMinutes = response.EligibleMinutes;
            }

            response.HasRequestOverlap = await HasRequestOverlapAsync(
                employeeContext.WorkforceProfileId,
                startAt,
                endAt,
                excludeRequestId,
                cancellationToken);

            if (response.HasRequestOverlap)
                AddIssue(response, "REQUEST_OVERLAP", "Terdapat pengajuan lembur lain yang bertabrakan pada interval yang sama.", true, "plannedStartAt");

            response.HasPlanOverlap = await HasPlanningOverlapAsync(
                employeeContext.WorkforceProfileId,
                startAt,
                endAt,
                cancellationToken);

            if (response.HasPlanOverlap)
                AddIssue(response, "PLAN_OVERLAP", "Terdapat overtime plan manager yang bertabrakan pada interval yang sama.", true, "plannedStartAt");

            response.HasScheduleConflict = response.HasScheduleConflict ||
                                           response.HasRequestOverlap ||
                                           response.HasPlanOverlap ||
                                           response.Issues.Any(x =>
                                               x.Code == "SCHEDULE_NOT_RESOLVED" ||
                                               x.Code == "SCHEDULE_BLOCKING_CONFLICT" ||
                                               x.Code == "CATEGORY_SCHEDULE_MISMATCH" ||
                                               x.Code == "REGULAR_WORK_OVERLAP");

            response.CanSaveDraft = !response.Issues.Any(x =>
                x.IsBlocking && DraftBlockingCodes.Contains(x.Code));
            response.CanSubmit = response.Issues.All(x => !x.IsBlocking) &&
                                 response.RoundedMinutes > 0 &&
                                 response.OvertimePolicyId.HasValue &&
                                 response.IsScheduleResolved;
            response.IsPolicyCompliant = response.CanSubmit;

            return response;
        }

        private async Task ResolveWorkflowReadinessAsync(
            MyOvertimePreviewResponse response,
            MstOvertimePolicy policy,
            DateTime effectiveDate,
            CancellationToken cancellationToken)
        {
            if (!policy.RequirePreApproval)
                return;

            if (string.IsNullOrWhiteSpace(policy.ApprovalWorkflowCode))
            {
                AddIssue(response, "WORKFLOW_CODE_REQUIRED", "Policy mewajibkan pre-approval tetapi ApprovalWorkflowCode belum dikonfigurasi.", true, "overtimePolicyId", policy.Id);
                return;
            }

            var workflowId = await _dbContext.MstWorkflowDefinitions
                .AsNoTracking()
                .Where(x =>
                    x.WorkflowCode == policy.ApprovalWorkflowCode &&
                    x.RequestType == OvertimeValueConstants.Workflow.RequestType &&
                    x.WorkflowStatus == OvertimeValueConstants.Workflow.ActiveStatus &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate.Date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate.Date))
                .OrderByDescending(x => x.Version)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!workflowId.HasValue)
            {
                AddIssue(response, "WORKFLOW_NOT_READY", "Workflow OvertimeRequest aktif yang sesuai dengan ApprovalWorkflowCode tidak ditemukan.", true, "overtimePolicyId", policy.Id);
                return;
            }

            response.WorkflowDefinitionId = workflowId.Value;
        }

        private async Task ValidateAggregateLimitsAsync(
            MyOvertimePreviewResponse response,
            Guid workforceProfileId,
            DateOnly overtimeDate,
            int currentMinutes,
            MstOvertimePolicy policy,
            Guid? excludeRequestId,
            CancellationToken cancellationToken)
        {
            var weekStart = GetWeekStart(overtimeDate);
            var weekEnd = weekStart.AddDays(6);
            var monthStart = new DateOnly(overtimeDate.Year, overtimeDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var baseQuery = _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Rejected &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Cancelled);

            if (excludeRequestId.HasValue)
                baseQuery = baseQuery.Where(x => x.Id != excludeRequestId.Value);

            if (policy.MaximumOvertimeMinutesPerWeek.HasValue)
            {
                var weekly = await baseQuery
                    .Where(x => x.OvertimeDate >= weekStart && x.OvertimeDate <= weekEnd)
                    .SumAsync(x => (int?)x.RequestedMinutes, cancellationToken) ?? 0;

                if (weekly + currentMinutes > policy.MaximumOvertimeMinutesPerWeek.Value)
                {
                    response.HasWorkHourLimitConflict = true;
                    AddIssue(response, "WEEKLY_LIMIT_EXCEEDED", "Akumulasi menit lembur melebihi batas mingguan policy.", true);
                }
            }

            if (policy.MaximumOvertimeMinutesPerMonth.HasValue)
            {
                var monthly = await baseQuery
                    .Where(x => x.OvertimeDate >= monthStart && x.OvertimeDate <= monthEnd)
                    .SumAsync(x => (int?)x.RequestedMinutes, cancellationToken) ?? 0;

                if (monthly + currentMinutes > policy.MaximumOvertimeMinutesPerMonth.Value)
                {
                    response.HasWorkHourLimitConflict = true;
                    AddIssue(response, "MONTHLY_LIMIT_EXCEEDED", "Akumulasi menit lembur melebihi batas bulanan policy.", true);
                }
            }
        }

        private async Task<WfpOrganizationAssignment?> ResolveOrganizationAssignmentAsync(
            Guid workforceProfileId,
            DateTime effectiveDate,
            CancellationToken cancellationToken) =>
            await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate.Date <= effectiveDate.Date &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate.Date))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

        private async Task<bool> HasRequestOverlapAsync(
            Guid workforceProfileId,
            DateTime startAt,
            DateTime endAt,
            Guid? excludeRequestId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Rejected &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Cancelled &&
                    x.PlannedStartAt.HasValue &&
                    x.PlannedEndAt.HasValue &&
                    x.PlannedStartAt.Value < endAt &&
                    startAt < x.PlannedEndAt.Value);

            if (excludeRequestId.HasValue)
                query = query.Where(x => x.Id != excludeRequestId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        private async Task<bool> HasPlanningOverlapAsync(
            Guid workforceProfileId,
            DateTime startAt,
            DateTime endAt,
            CancellationToken cancellationToken) =>
            await _dbContext.TrxOvertimePlanDetails
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.DetailStatus != OvertimeValueConstants.PlanDetailStatus.Cancelled &&
                    x.DetailStatus != OvertimeValueConstants.PlanDetailStatus.Skipped &&
                    x.PlannedStartAt < endAt &&
                    startAt < x.PlannedEndAt,
                    cancellationToken);

        private static void ValidateCategoryAlignment(
            MyOvertimePreviewResponse response,
            string? category,
            DateTime startAt,
            DateTime endAt)
        {
            if (category == null) return;

            if (category == OvertimeValueConstants.OvertimeCategory.RestDay && !response.IsRestDay)
                AddIssue(response, "CATEGORY_SCHEDULE_MISMATCH", "Kategori RestDay hanya dapat digunakan pada jadwal hari libur/rest day.", true, "overtimeCategory");

            if (category == OvertimeValueConstants.OvertimeCategory.Holiday && !response.IsHoliday)
                AddIssue(response, "CATEGORY_SCHEDULE_MISMATCH", "Kategori Holiday hanya dapat digunakan pada tanggal holiday.", true, "overtimeCategory");

            if (category == OvertimeValueConstants.OvertimeCategory.BeforeShift)
            {
                if (!response.ScheduledStartAt.HasValue || endAt > response.ScheduledStartAt.Value)
                    AddIssue(response, "CATEGORY_SCHEDULE_MISMATCH", "Lembur BeforeShift harus berakhir sebelum atau tepat pada awal shift reguler.", true, "plannedEndAt");
            }

            if (category == OvertimeValueConstants.OvertimeCategory.AfterShift)
            {
                if (!response.ScheduledEndAt.HasValue || startAt < response.ScheduledEndAt.Value)
                    AddIssue(response, "CATEGORY_SCHEDULE_MISMATCH", "Lembur AfterShift harus dimulai setelah atau tepat pada akhir shift reguler.", true, "plannedStartAt");
            }

            if (!response.IsRestDay &&
                !response.IsHoliday &&
                response.ScheduledStartAt.HasValue &&
                response.ScheduledEndAt.HasValue &&
                category != OvertimeValueConstants.OvertimeCategory.Emergency &&
                category != OvertimeValueConstants.OvertimeCategory.OnCall &&
                startAt < response.ScheduledEndAt.Value &&
                response.ScheduledStartAt.Value < endAt)
            {
                AddIssue(response, "REGULAR_WORK_OVERLAP", "Interval lembur bertabrakan dengan jam kerja reguler.", true, "plannedStartAt");
            }
        }

        private static WfpOvertimeRequest BuildRequestEntity(
            OvertimeSelfServiceEmployeeContext employeeContext,
            CreateMyOvertimeRequest request,
            MyOvertimePreviewResponse evaluation,
            string requestNumber,
            Guid actorUserId,
            DateTime now) => new()
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            WorkforceProfileId = employeeContext.WorkforceProfileId,
            EmployeeId = employeeContext.EmployeeId,
            OrganizationAssignmentId = evaluation.OrganizationAssignmentId,
            HospitalSiteId = evaluation.HospitalSiteId,
            OrganizationUnitId = evaluation.OrganizationUnitId,
            DepartmentId = evaluation.DepartmentId,
            PositionId = evaluation.PositionId,
            CostCenterId = evaluation.CostCenterId,
            OvertimePolicyId = evaluation.OvertimePolicyId,
            RequestSource = OvertimeValueConstants.RequestSource.EmployeeSelfService,
            WorkScheduleAssignmentId = evaluation.WorkScheduleAssignmentId,
            RosterPeriodId = evaluation.RosterPeriodId,
            ShiftAssignmentId = evaluation.ShiftAssignmentId,
            WorkScheduleId = evaluation.WorkScheduleId,
            ShiftId = evaluation.ShiftId,
            RequestReasonId = NormalizeGuid(request.RequestReasonId),
            WorkflowDefinitionId = evaluation.WorkflowDefinitionId,
            OvertimeDate = request.OvertimeDate,
            PlannedEndDate = evaluation.PlannedEndDate,
            PlannedStartAt = evaluation.PlannedStartAt,
            PlannedEndAt = evaluation.PlannedEndAt,
            RequestedStartTime = TimeOnly.FromDateTime(evaluation.PlannedStartAt),
            RequestedEndTime = TimeOnly.FromDateTime(evaluation.PlannedEndAt),
            RequestedMinutes = evaluation.RoundedMinutes,
            ApprovedMinutes = 0,
            EstimatedBreakMinutes = evaluation.EstimatedBreakMinutes,
            CurrencyCode = "IDR",
            Reason = request.Reason.Trim(),
            WorkDescription = request.WorkDescription.Trim(),
            IsUrgent = request.IsUrgent || evaluation.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.Emergency,
            IsBeforeShift = evaluation.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.BeforeShift,
            IsAfterShift = evaluation.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.AfterShift,
            IsRestDay = evaluation.IsRestDay,
            IsHoliday = evaluation.IsHoliday,
            HasScheduleConflict = evaluation.HasScheduleConflict,
            HasLeaveConflict = false,
            HasTrainingConflict = false,
            HasMinimumRestConflict = false,
            HasWorkHourLimitConflict = evaluation.HasWorkHourLimitConflict,
            IsPolicyCompliant = evaluation.IsPolicyCompliant,
            ValidationResultJson = SerializeValidation(evaluation),
            OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.Draft,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId,
            UpdateBy = actorUserId
        };

        private static TrxOvertimeRequestDetail BuildRequestDetail(
            Guid overtimeRequestId,
            CreateMyOvertimeRequest request,
            MyOvertimePreviewResponse evaluation,
            Guid actorUserId,
            DateTime now) => new()
        {
            Id = Guid.NewGuid(),
            OvertimeRequestId = overtimeRequestId,
            SequenceNumber = 1,
            OvertimeDate = request.OvertimeDate,
            WorkScheduleId = evaluation.WorkScheduleId,
            ShiftId = evaluation.ShiftId,
            ShiftAssignmentId = evaluation.ShiftAssignmentId,
            OvertimeRateId = evaluation.PreviewOvertimeRateId,
            PlannedStartAt = evaluation.PlannedStartAt,
            PlannedEndAt = evaluation.PlannedEndAt,
            RequestedMinutes = evaluation.RoundedMinutes,
            ApprovedMinutes = 0,
            BreakMinutes = evaluation.EstimatedBreakMinutes,
            DayType = evaluation.DayType,
            OvertimeCategory = evaluation.OvertimeCategory,
            RateCodeSnapshot = evaluation.PreviewOvertimeRateCode,
            RateMultiplierSnapshot = evaluation.PreviewRateMultiplier <= 0 ? 1 : evaluation.PreviewRateMultiplier,
            BaseHourlyRateSnapshot = 0,
            EstimatedCost = 0,
            ApprovedCost = 0,
            CurrencyCode = "IDR",
            WorkDescription = request.WorkDescription.Trim(),
            Notes = NormalizeText(request.Notes),
            DetailStatus = OvertimeValueConstants.RequestStatus.Draft,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId,
            UpdateBy = actorUserId
        };

        private static void ApplyRequestEvaluation(
            WfpOvertimeRequest entity,
            CreateMyOvertimeRequest request,
            MyOvertimePreviewResponse evaluation,
            Guid actorUserId,
            DateTime now)
        {
            entity.EmployeeId = evaluation.EmployeeId;
            entity.OrganizationAssignmentId = evaluation.OrganizationAssignmentId;
            entity.HospitalSiteId = evaluation.HospitalSiteId;
            entity.OrganizationUnitId = evaluation.OrganizationUnitId;
            entity.DepartmentId = evaluation.DepartmentId;
            entity.PositionId = evaluation.PositionId;
            entity.CostCenterId = evaluation.CostCenterId;
            entity.OvertimePolicyId = evaluation.OvertimePolicyId;
            entity.WorkScheduleAssignmentId = evaluation.WorkScheduleAssignmentId;
            entity.RosterPeriodId = evaluation.RosterPeriodId;
            entity.ShiftAssignmentId = evaluation.ShiftAssignmentId;
            entity.WorkScheduleId = evaluation.WorkScheduleId;
            entity.ShiftId = evaluation.ShiftId;
            entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
            entity.WorkflowDefinitionId = evaluation.WorkflowDefinitionId;
            entity.OvertimeDate = request.OvertimeDate;
            entity.PlannedEndDate = evaluation.PlannedEndDate;
            entity.PlannedStartAt = evaluation.PlannedStartAt;
            entity.PlannedEndAt = evaluation.PlannedEndAt;
            entity.RequestedStartTime = TimeOnly.FromDateTime(evaluation.PlannedStartAt);
            entity.RequestedEndTime = TimeOnly.FromDateTime(evaluation.PlannedEndAt);
            entity.RequestedMinutes = evaluation.RoundedMinutes;
            entity.EstimatedBreakMinutes = evaluation.EstimatedBreakMinutes;
            entity.Reason = request.Reason.Trim();
            entity.WorkDescription = request.WorkDescription.Trim();
            entity.IsUrgent = request.IsUrgent || evaluation.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.Emergency;
            entity.IsBeforeShift = evaluation.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.BeforeShift;
            entity.IsAfterShift = evaluation.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.AfterShift;
            entity.IsRestDay = evaluation.IsRestDay;
            entity.IsHoliday = evaluation.IsHoliday;
            entity.HasScheduleConflict = evaluation.HasScheduleConflict;
            entity.HasLeaveConflict = false;
            entity.HasTrainingConflict = false;
            entity.HasMinimumRestConflict = false;
            entity.HasWorkHourLimitConflict = evaluation.HasWorkHourLimitConflict;
            entity.IsPolicyCompliant = evaluation.IsPolicyCompliant;
            entity.ValidationResultJson = SerializeValidation(evaluation);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
        }

        private static void ApplyDetailEvaluation(
            TrxOvertimeRequestDetail detail,
            CreateMyOvertimeRequest request,
            MyOvertimePreviewResponse evaluation,
            string parentStatus,
            Guid actorUserId,
            DateTime now)
        {
            detail.OvertimeDate = request.OvertimeDate;
            detail.WorkScheduleId = evaluation.WorkScheduleId;
            detail.ShiftId = evaluation.ShiftId;
            detail.ShiftAssignmentId = evaluation.ShiftAssignmentId;
            detail.OvertimeRateId = evaluation.PreviewOvertimeRateId;
            detail.PlannedStartAt = evaluation.PlannedStartAt;
            detail.PlannedEndAt = evaluation.PlannedEndAt;
            detail.RequestedMinutes = evaluation.RoundedMinutes;
            detail.BreakMinutes = evaluation.EstimatedBreakMinutes;
            detail.DayType = evaluation.DayType;
            detail.OvertimeCategory = evaluation.OvertimeCategory;
            detail.RateCodeSnapshot = evaluation.PreviewOvertimeRateCode;
            detail.RateMultiplierSnapshot = evaluation.PreviewRateMultiplier <= 0 ? 1 : evaluation.PreviewRateMultiplier;
            detail.WorkDescription = request.WorkDescription.Trim();
            detail.Notes = NormalizeText(request.Notes);
            detail.DetailStatus = parentStatus == OvertimeValueConstants.RequestStatus.NeedRevision
                ? OvertimeValueConstants.RequestStatus.NeedRevision
                : OvertimeValueConstants.RequestStatus.Draft;
            detail.IsActive = true;
            detail.IsCancel = false;
            detail.UpdateDateTime = now;
            detail.UpdateBy = actorUserId;
        }

        private static PreviewMyOvertimeRequest ToPreviewRequest(
            CreateMyOvertimeRequest source,
            Guid? excludeRequestId) => new()
        {
            OvertimeDate = source.OvertimeDate,
            PlannedStartAt = source.PlannedStartAt,
            PlannedEndAt = source.PlannedEndAt,
            EstimatedBreakMinutes = source.EstimatedBreakMinutes,
            OvertimeCategory = source.OvertimeCategory,
            RequestReasonId = source.RequestReasonId,
            Reason = source.Reason,
            WorkDescription = source.WorkDescription,
            Notes = source.Notes,
            IsUrgent = source.IsUrgent,
            ExcludeRequestId = excludeRequestId
        };

        private static UpdateMyOvertimeRequest ToUpdateRequest(WfpOvertimeRequest entity)
        {
            var detail = entity.Details.OrderBy(x => x.SequenceNumber).FirstOrDefault();
            return new UpdateMyOvertimeRequest
            {
                OvertimeDate = entity.OvertimeDate,
                PlannedStartAt = entity.PlannedStartAt ?? detail?.PlannedStartAt ?? DateTime.UtcNow,
                PlannedEndAt = entity.PlannedEndAt ?? detail?.PlannedEndAt ?? DateTime.UtcNow,
                EstimatedBreakMinutes = entity.EstimatedBreakMinutes,
                OvertimeCategory = detail?.OvertimeCategory ?? ResolveCategory(entity),
                RequestReasonId = entity.RequestReasonId,
                Reason = entity.Reason,
                WorkDescription = entity.WorkDescription ?? detail?.WorkDescription ?? entity.Reason,
                Notes = detail?.Notes,
                IsUrgent = entity.IsUrgent
            };
        }

        private static string ResolveCategory(WfpOvertimeRequest entity)
        {
            if (entity.IsUrgent) return OvertimeValueConstants.OvertimeCategory.Emergency;
            if (entity.IsHoliday) return OvertimeValueConstants.OvertimeCategory.Holiday;
            if (entity.IsRestDay) return OvertimeValueConstants.OvertimeCategory.RestDay;
            if (entity.IsBeforeShift) return OvertimeValueConstants.OvertimeCategory.BeforeShift;
            return OvertimeValueConstants.OvertimeCategory.AfterShift;
        }

        private static (int StatusCode, string Message)? ValidateEditableEntity(WfpOvertimeRequest? entity)
        {
            if (entity == null)
                return (StatusCodes.Status404NotFound, "Pengajuan lembur tidak ditemukan atau bukan milik user login.");

            if (entity.RequestSource != OvertimeValueConstants.RequestSource.EmployeeSelfService)
                return (StatusCodes.Status409Conflict, "Request yang berasal dari manager planning bersifat read-only pada employee self service.");

            if (entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Draft &&
                entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.NeedRevision)
            {
                return (StatusCodes.Status409Conflict, "Request hanya dapat diubah dari status Draft atau NeedRevision.");
            }

            if (entity.WorkflowInstanceId.HasValue &&
                entity.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.NeedRevision)
            {
                return (StatusCodes.Status409Conflict, "Request yang sudah berjalan di workflow hanya dapat diubah ketika workflow meminta revisi.");
            }

            return null;
        }

        private async Task<string> GenerateRequestNumberAsync(
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var prefix = $"OTR-{date:yyyyMMdd}-";
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                new object[] { "OVERTIME_REQUEST_" + date.ToString("yyyyMMdd") },
                cancellationToken);

            var last = await _dbContext.WfpOvertimeRequests
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.RequestNumber.StartsWith(prefix))
                .OrderByDescending(x => x.RequestNumber)
                .Select(x => x.RequestNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var sequence = last != null &&
                           last.StartsWith(prefix) &&
                           int.TryParse(last[prefix.Length..], out var parsed)
                ? parsed + 1
                : 1;

            return prefix + sequence.ToString("D5");
        }

        private static bool IsCategoryAllowed(MstOvertimePolicy policy, string category) => category switch
        {
            OvertimeValueConstants.OvertimeCategory.BeforeShift => policy.AllowBeforeShift,
            OvertimeValueConstants.OvertimeCategory.AfterShift => policy.AllowAfterShift,
            OvertimeValueConstants.OvertimeCategory.RestDay => policy.AllowRestDay,
            OvertimeValueConstants.OvertimeCategory.Holiday => policy.AllowHoliday,
            _ => true
        };

        private static bool IsDayTypeAllowed(MstOvertimePolicy policy, string dayType) => dayType switch
        {
            OvertimeValueConstants.DayType.RestDay => policy.AllowRestDay,
            OvertimeValueConstants.DayType.Holiday => policy.AllowHoliday,
            OvertimeValueConstants.DayType.SpecialHoliday => policy.AllowHoliday,
            _ => true
        };

        private static int RoundMinutes(int minutes, int interval, string method)
        {
            if (minutes <= 0 || interval <= 1 || method == OvertimeValueConstants.RoundingMethod.None)
                return Math.Max(0, minutes);

            var quotient = minutes / (decimal)interval;
            var rounded = method switch
            {
                OvertimeValueConstants.RoundingMethod.Up => Math.Ceiling(quotient),
                OvertimeValueConstants.RoundingMethod.Nearest => Math.Round(quotient, MidpointRounding.AwayFromZero),
                _ => Math.Floor(quotient)
            };

            return Math.Max(0, (int)rounded * interval);
        }

        private static DateOnly GetWeekStart(DateOnly date)
        {
            var offset = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-offset);
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static string SerializeValidation(MyOvertimePreviewResponse response) =>
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        private static string BuildIssueMessage(
            MyOvertimePreviewResponse response,
            string prefix)
        {
            var blocking = response.Issues
                .Where(x => x.IsBlocking)
                .Take(5)
                .Select(x => x.Message)
                .ToList();

            return blocking.Count == 0
                ? prefix
                : prefix + " " + string.Join(" ", blocking);
        }

        private static void AddIssue(
            MyOvertimePreviewResponse response,
            string code,
            string message,
            bool blocking,
            string? field = null,
            Guid? referenceId = null)
        {
            if (response.Issues.Any(x => x.Code == code && x.Message == message)) return;
            response.Issues.Add(new MyOvertimeValidationIssueResponse
            {
                Code = code,
                Severity = blocking ? "Error" : "Warning",
                Message = message,
                Field = field,
                ReferenceId = referenceId,
                IsBlocking = blocking
            });
        }

        private static string? NormalizeToken(string? value, IReadOnlyCollection<string> allowed)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return allowed.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static OvertimeSelfServiceServiceResult<T> NotFound<T>() =>
            OvertimeSelfServiceServiceResult<T>.Fail(
                StatusCodes.Status404NotFound,
                "Pengajuan lembur tidak ditemukan atau bukan milik user login.");

        private static OvertimeSelfServiceServiceResult<T> Conflict<T>(string message) =>
            OvertimeSelfServiceServiceResult<T>.Fail(
                StatusCodes.Status409Conflict,
                message);
    }
}
