using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ScheduleChangeWorkflowIntegrationService
    {
        private static readonly HashSet<string> ReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                SchedulingRequestValueConstants.Workflow.ScheduleChangeReferenceType,
                "ScheduleChange",
                "WfpScheduleChangeRequest"
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly ScheduleChangeService _scheduleChangeService;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowReferenceLifecycleService _workflowReferenceLifecycleService;

        public ScheduleChangeWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            ScheduleChangeService scheduleChangeService,
            WorkflowService workflowService,
            WorkflowReferenceLifecycleService workflowReferenceLifecycleService)
        {
            _dbContext = dbContext;
            _scheduleChangeService = scheduleChangeService;
            _workflowService = workflowService;
            _workflowReferenceLifecycleService = workflowReferenceLifecycleService;
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>> GetWorkflowAsync(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var source = await _scheduleChangeService.GetByIdAsync(requestId, cancellationToken);
            if (!source.Success || source.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    source.StatusCode,
                    source.Message);
            }

            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Ok(
                    new ScheduleChangeWorkflowResponse
                    {
                        ScheduleChangeRequestId = source.Data.Id,
                        RequestNumber = source.Data.RequestNumber,
                        RequestStatus = source.Data.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = source.Data.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Draft,
                        IsAutoApplyPending = false,
                        ScheduleChange = source.Data
                    },
                    "Pengajuan perubahan jadwal belum mempunyai workflow.");
            }

            var workflowResult = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            return BuildResponse(source.Data, workflowResult.Data, "Workflow perubahan jadwal berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>> SubmitAsync(
            Guid requestId,
            Guid workforceProfileId,
            Guid actorUserId,
            ScheduleChangeSubmitRequest? request,
            CancellationToken cancellationToken = default)
        {
            var prepared = await _scheduleChangeService.PrepareSubmitAsync(
                requestId,
                workforceProfileId,
                actorUserId,
                cancellationToken);

            if (!prepared.Success || prepared.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    prepared.StatusCode,
                    prepared.Message);
            }

            var sourceEntity = await _dbContext.WfpScheduleChangeRequests
                .FirstAsync(x => x.Id == requestId && !x.IsDelete, cancellationToken);
            var existingWorkflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            WorkflowInstanceDetailResponse workflowDetail;
            var createdNow = false;

            if (existingWorkflow != null)
            {
                if (existingWorkflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Draft ||
                    existingWorkflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested)
                {
                    var submitResult = await _workflowService.SubmitAsync(
                        existingWorkflow.Id,
                        new WorkflowSubmitRequest
                        {
                            Comment = NormalizeNote(request?.Note),
                            IdempotencyKey = NormalizeText(request?.IdempotencyKey)
                        },
                        cancellationToken);

                    if (!submitResult.Success || submitResult.Data == null)
                    {
                        return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                            submitResult.StatusCode,
                            submitResult.Message);
                    }

                    workflowDetail = submitResult.Data;
                }
                else if (IsRunning(existingWorkflow.WorkflowStatus))
                {
                    var detail = await _workflowService.GetByIdAsync(existingWorkflow.Id, cancellationToken);
                    if (!detail.Success || detail.Data == null)
                    {
                        return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                            detail.StatusCode,
                            detail.Message);
                    }

                    workflowDetail = detail.Data;
                }
                else
                {
                    return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Workflow sebelumnya sudah berstatus {existingWorkflow.WorkflowStatus} dan tidak dapat digunakan kembali.");
                }
            }
            else
            {
                var workflowCodeResult = await ResolveWorkflowCodeAsync(sourceEntity, cancellationToken);
                if (!workflowCodeResult.Success)
                {
                    return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                        workflowCodeResult.StatusCode,
                        workflowCodeResult.Message);
                }

                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = workflowCodeResult.Code!,
                        ReferenceType = SchedulingRequestValueConstants.Workflow.ScheduleChangeReferenceType,
                        ReferenceId = sourceEntity.Id,
                        ExternalReferenceNumber = sourceEntity.RequestNumber,
                        SourceChannel = NormalizeSourceChannel(request?.SourceChannel),
                        RequestCorrelationId = NormalizeText(request?.RequestCorrelationId),
                        IdempotencyKey = $"schedule-change:{sourceEntity.Id:N}:workflow",
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            scheduleChangeRequestId = sourceEntity.Id,
                            sourceEntity.RequestNumber,
                            sourceEntity.WorkforceProfileId,
                            sourceEntity.RequestType,
                            sourceEntity.RequestedDate,
                            sourceEntity.EffectiveStartDate,
                            sourceEntity.EffectiveEndDate,
                            sourceEntity.CurrentWorkScheduleId,
                            sourceEntity.RequestedWorkScheduleId,
                            sourceEntity.CurrentShiftId,
                            sourceEntity.RequestedShiftId,
                            sourceEntity.Reason
                        }),
                        SelectedApproverUserIds = new List<Guid>()
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                        createResult.StatusCode,
                        createResult.Message);
                }

                createdNow = true;
                workflowDetail = createResult.Data;
                sourceEntity.WorkflowDefinitionId = workflowDetail.WorkflowDefinitionId;
                sourceEntity.WorkflowInstanceId = workflowDetail.Id;
                sourceEntity.UpdateDateTime = DateTime.UtcNow;
                sourceEntity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var submitResult = await _workflowService.SubmitAsync(
                    workflowDetail.Id,
                    new WorkflowSubmitRequest
                    {
                        Comment = NormalizeNote(request?.Note),
                        IdempotencyKey = NormalizeText(request?.IdempotencyKey)
                    },
                    cancellationToken);

                if (!submitResult.Success || submitResult.Data == null)
                {
                    await SoftDeleteFailedDraftWorkflowAsync(workflowDetail.Id, actorUserId, cancellationToken);
                    return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                        submitResult.StatusCode,
                        submitResult.Message);
                }

                workflowDetail = submitResult.Data;
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflowDetail.Id,
                actorUserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            var refreshed = await _scheduleChangeService.GetByIdAsync(requestId, cancellationToken);
            if (!refreshed.Success || refreshed.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    refreshed.StatusCode,
                    refreshed.Message);
            }

            return BuildResponse(
                refreshed.Data,
                workflowDetail,
                createdNow
                    ? "Workflow perubahan jadwal berhasil dibuat dan di-submit."
                    : "Pengajuan perubahan jadwal berhasil di-submit ke workflow.");
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>> CancelAsync(
            Guid requestId,
            Guid workforceProfileId,
            Guid actorUserId,
            ScheduleChangeCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            var source = await _scheduleChangeService.GetByIdAsync(requestId, cancellationToken);
            if (!source.Success || source.Data == null || source.Data.WorkforceProfileId != workforceProfileId)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan perubahan jadwal tidak ditemukan atau bukan milik user login.");
            }

            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                var cancelled = await _scheduleChangeService.CancelAsync(
                    requestId,
                    workforceProfileId,
                    actorUserId,
                    request.Reason,
                    cancellationToken);

                if (!cancelled.Success || cancelled.Data == null)
                {
                    return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                        cancelled.StatusCode,
                        cancelled.Message);
                }

                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Ok(
                    new ScheduleChangeWorkflowResponse
                    {
                        ScheduleChangeRequestId = cancelled.Data.Id,
                        RequestNumber = cancelled.Data.RequestNumber,
                        RequestStatus = cancelled.Data.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = true,
                        ScheduleChange = cancelled.Data
                    },
                    "Pengajuan perubahan jadwal berhasil dibatalkan sebelum workflow dibuat.");
            }

            var workflowResult = workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress
                ? await _workflowService.WithdrawAsync(
                    workflow.Id,
                    new WorkflowWithdrawRequest
                    {
                        Reason = request.Reason,
                        IdempotencyKey = NormalizeText(request.IdempotencyKey)
                    },
                    cancellationToken)
                : await _workflowService.CancelAsync(
                    workflow.Id,
                    new WorkflowCancelRequest
                    {
                        Reason = request.Reason,
                        IdempotencyKey = NormalizeText(request.IdempotencyKey)
                    },
                    cancellationToken);

            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: false,
                cancellationToken: cancellationToken);

            var refreshed = await _scheduleChangeService.GetByIdAsync(requestId, cancellationToken);
            if (!refreshed.Success || refreshed.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    refreshed.StatusCode,
                    refreshed.Message);
            }

            return BuildResponse(
                refreshed.Data,
                workflowResult.Data,
                "Pengajuan perubahan jadwal dan workflow berhasil dibatalkan.");
        }

        public async Task<SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>> SynchronizeAsync(
            Guid requestId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow perubahan jadwal tidak ditemukan.");
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            var detail = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            var source = await _scheduleChangeService.GetByIdAsync(requestId, cancellationToken);

            if (!detail.Success || detail.Data == null || !source.Success || source.Data == null)
            {
                return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    "Sinkronisasi selesai tetapi response akhir tidak dapat dibentuk.");
            }

            return BuildResponse(source.Data, detail.Data, "Workflow perubahan jadwal berhasil disinkronkan.");
        }

        private async Task<TrxWorkflowInstance?> FindLatestWorkflowAsync(
            Guid requestId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.TrxWorkflowInstances
                .AsNoTracking()
                .Where(x =>
                    x.ReferenceId == requestId &&
                    ReferenceAliases.Contains(x.ReferenceType) &&
                    !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<WorkflowCodeResult> ResolveWorkflowCodeAsync(
            WfpScheduleChangeRequest source,
            CancellationToken cancellationToken)
        {
            if (!source.WorkflowDefinitionId.HasValue || source.WorkflowDefinitionId == Guid.Empty)
            {
                return WorkflowCodeResult.Ok(SchedulingRequestValueConstants.Workflow.ScheduleChangeCode);
            }

            var definition = await _dbContext.MstWorkflowDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == source.WorkflowDefinitionId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            return definition == null
                ? WorkflowCodeResult.Fail(StatusCodes.Status400BadRequest, "Workflow definition perubahan jadwal tidak ditemukan atau tidak aktif.")
                : WorkflowCodeResult.Ok(definition.WorkflowCode);
        }

        private async Task SoftDeleteFailedDraftWorkflowAsync(
            Guid workflowId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.TrxWorkflowInstances
                .FirstOrDefaultAsync(x =>
                    x.Id == workflowId &&
                    x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Draft &&
                    !x.IsDelete,
                    cancellationToken);

            if (workflow == null)
            {
                return;
            }

            workflow.IsDelete = true;
            workflow.DeleteDateTime = DateTime.UtcNow;
            workflow.DeleteBy = actorUserId;
            workflow.UpdateDateTime = DateTime.UtcNow;
            workflow.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse> BuildResponse(
            ScheduleChangeDetailResponse source,
            WorkflowInstanceDetailResponse workflow,
            string message)
        {
            return SchedulingRequestServiceResult<ScheduleChangeWorkflowResponse>.Ok(
                new ScheduleChangeWorkflowResponse
                {
                    ScheduleChangeRequestId = source.Id,
                    RequestNumber = source.RequestNumber,
                    RequestStatus = source.RequestStatus,
                    HasWorkflow = true,
                    IsSynchronized = IsSynchronized(source.RequestStatus, workflow.WorkflowStatus),
                    IsAutoApplyPending = workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Completed &&
                                         source.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Approved,
                    ScheduleChange = source,
                    Workflow = workflow
                },
                message);
        }

        private static bool IsSynchronized(string sourceStatus, string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed)
            {
                return sourceStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Approved ||
                       sourceStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Applied;
            }

            if (IsRunning(workflowStatus))
            {
                return sourceStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted ||
                       sourceStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview;
            }

            return sourceStatus == ScheduleChangeWorkflowLifecycleService.MapStatus(workflowStatus);
        }

        private static bool IsRunning(string status)
        {
            return status == WorkflowValueConstants.WorkflowStatus.Submitted ||
                   status == WorkflowValueConstants.WorkflowStatus.InProgress;
        }

        private static string NormalizeSourceChannel(string? value)
        {
            var normalized = NormalizeText(value);
            var allowed = new[]
            {
                WorkflowValueConstants.SourceChannel.Web,
                WorkflowValueConstants.SourceChannel.Mobile,
                WorkflowValueConstants.SourceChannel.Api,
                WorkflowValueConstants.SourceChannel.System,
                WorkflowValueConstants.SourceChannel.Integration
            };

            return allowed.FirstOrDefault(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase)) ??
                   WorkflowValueConstants.SourceChannel.Web;
        }

        private static string? NormalizeNote(string? value)
        {
            var normalized = NormalizeText(value);
            return normalized != null && normalized.Length > 4000 ? normalized[..4000] : normalized;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class WorkflowCodeResult
        {
            public bool Success { get; private set; }
            public int StatusCode { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public string? Code { get; private set; }

            public static WorkflowCodeResult Ok(string code) => new()
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Workflow code berhasil ditentukan.",
                Code = code
            };

            public static WorkflowCodeResult Fail(int statusCode, string message) => new()
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
