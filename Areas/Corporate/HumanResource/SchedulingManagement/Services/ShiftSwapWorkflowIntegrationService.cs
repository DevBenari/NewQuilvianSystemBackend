using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    public class ShiftSwapWorkflowIntegrationService
    {
        private static readonly HashSet<string> ReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                SchedulingRequestValueConstants.Workflow.ShiftSwapReferenceType,
                "ShiftSwap",
                "WfpShiftSwapRequest"
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly ShiftSwapService _shiftSwapService;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowReferenceLifecycleService _workflowReferenceLifecycleService;

        public ShiftSwapWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            ShiftSwapService shiftSwapService,
            WorkflowService workflowService,
            WorkflowReferenceLifecycleService workflowReferenceLifecycleService)
        {
            _dbContext = dbContext;
            _shiftSwapService = shiftSwapService;
            _workflowService = workflowService;
            _workflowReferenceLifecycleService = workflowReferenceLifecycleService;
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>> GetWorkflowAsync(
            Guid requestId,
            Guid? viewerWorkforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var source = await _shiftSwapService.GetByIdAsync(
                requestId,
                viewerWorkforceProfileId,
                cancellationToken);

            if (!source.Success || source.Data == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    source.StatusCode,
                    source.Message);
            }

            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Ok(
                    new ShiftSwapWorkflowResponse
                    {
                        ShiftSwapRequestId = source.Data.Id,
                        RequestNumber = source.Data.RequestNumber,
                        RequestStatus = source.Data.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = source.Data.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Draft ||
                                         source.Data.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingTarget ||
                                         source.Data.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted ||
                                         source.Data.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.TargetRejected,
                        ShiftSwap = source.Data
                    },
                    "Pengajuan tukar shift belum mempunyai workflow manager.");
            }

            var detail = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            if (!detail.Success || detail.Data == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    detail.StatusCode,
                    detail.Message);
            }

            return BuildResponse(source.Data, detail.Data, "Workflow tukar shift berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>> SubmitForManagerApprovalAsync(
            Guid requestId,
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            ShiftSwapWorkflowSubmitRequest? request,
            CancellationToken cancellationToken = default)
        {
            var sourceEntity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == requestId && !x.IsDelete, cancellationToken);

            if (sourceEntity == null ||
                sourceEntity.RequesterWorkforceProfileId != requesterWorkforceProfileId ||
                (sourceEntity.CreateBy != Guid.Empty && sourceEntity.CreateBy != actorUserId))
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan milik user login.");
            }

            if (sourceEntity.IsAcceptedByTarget != true ||
                (sourceEntity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted &&
                 sourceEntity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.NeedRevision &&
                 sourceEntity.RequestStatus != SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval))
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan baru dapat diteruskan ke manager setelah target employee menerima tukar shift.");
            }

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
                        return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
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
                        return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                            detail.StatusCode,
                            detail.Message);
                    }

                    workflowDetail = detail.Data;
                }
                else
                {
                    return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Workflow sebelumnya sudah berstatus {existingWorkflow.WorkflowStatus} dan tidak dapat digunakan kembali.");
                }
            }
            else
            {
                var codeResult = await ResolveWorkflowCodeAsync(sourceEntity, cancellationToken);
                if (!codeResult.Success)
                {
                    return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                        codeResult.StatusCode,
                        codeResult.Message);
                }

                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = codeResult.Code!,
                        ReferenceType = SchedulingRequestValueConstants.Workflow.ShiftSwapReferenceType,
                        ReferenceId = sourceEntity.Id,
                        ExternalReferenceNumber = sourceEntity.RequestNumber,
                        SourceChannel = NormalizeSourceChannel(request?.SourceChannel),
                        RequestCorrelationId = NormalizeText(request?.RequestCorrelationId),
                        IdempotencyKey = $"shift-swap:{sourceEntity.Id:N}:workflow",
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            shiftSwapRequestId = sourceEntity.Id,
                            sourceEntity.RequestNumber,
                            sourceEntity.RequesterWorkforceProfileId,
                            sourceEntity.TargetWorkforceProfileId,
                            sourceEntity.RequesterShiftAssignmentId,
                            sourceEntity.TargetShiftAssignmentId,
                            sourceEntity.RequesterShiftDate,
                            sourceEntity.TargetShiftDate,
                            targetAcceptedAt = sourceEntity.TargetRespondedAt,
                            sourceEntity.Reason
                        }),
                        SelectedApproverUserIds = new List<Guid>()
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                        createResult.StatusCode,
                        createResult.Message);
                }

                createdNow = true;
                workflowDetail = createResult.Data;
                sourceEntity.WorkflowDefinitionId = workflowDetail.WorkflowDefinitionId;
                sourceEntity.WorkflowInstanceId = workflowDetail.Id;
                sourceEntity.RequestStatus = SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval;
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
                    return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
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

            var refreshed = await _shiftSwapService.GetByIdAsync(
                requestId,
                requesterWorkforceProfileId,
                cancellationToken);

            if (!refreshed.Success || refreshed.Data == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    refreshed.StatusCode,
                    refreshed.Message);
            }

            return BuildResponse(
                refreshed.Data,
                workflowDetail,
                createdNow
                    ? "Workflow approval manager untuk tukar shift berhasil dibuat dan di-submit."
                    : "Tukar shift berhasil di-submit ke workflow manager.");
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>> CancelAsync(
            Guid requestId,
            Guid requesterWorkforceProfileId,
            Guid actorUserId,
            ShiftSwapCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            var source = await _shiftSwapService.GetByIdAsync(
                requestId,
                requesterWorkforceProfileId,
                cancellationToken);

            if (!source.Success || source.Data == null || !source.Data.IsRequester)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau bukan milik user login.");
            }

            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                var cancelled = await _shiftSwapService.CancelAsync(
                    requestId,
                    requesterWorkforceProfileId,
                    actorUserId,
                    request.Reason,
                    cancellationToken);

                if (!cancelled.Success || cancelled.Data == null)
                {
                    return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                        cancelled.StatusCode,
                        cancelled.Message);
                }

                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Ok(
                    new ShiftSwapWorkflowResponse
                    {
                        ShiftSwapRequestId = cancelled.Data.Id,
                        RequestNumber = cancelled.Data.RequestNumber,
                        RequestStatus = cancelled.Data.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = true,
                        ShiftSwap = cancelled.Data
                    },
                    "Pengajuan tukar shift berhasil dibatalkan.");
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
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: false,
                cancellationToken: cancellationToken);

            var refreshed = await _shiftSwapService.GetByIdAsync(
                requestId,
                requesterWorkforceProfileId,
                cancellationToken);

            if (!refreshed.Success || refreshed.Data == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    refreshed.StatusCode,
                    refreshed.Message);
            }

            return BuildResponse(
                refreshed.Data,
                workflowResult.Data,
                "Pengajuan tukar shift dan workflow manager berhasil dibatalkan.");
        }

        public async Task<SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>> SynchronizeAsync(
            Guid requestId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow tukar shift tidak ditemukan.");
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            var detail = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            var source = await _shiftSwapService.GetByIdAsync(requestId, null, cancellationToken);

            if (!detail.Success || detail.Data == null || !source.Success || source.Data == null)
            {
                return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    "Sinkronisasi selesai tetapi response akhir tidak dapat dibentuk.");
            }

            return BuildResponse(source.Data, detail.Data, "Workflow tukar shift berhasil disinkronkan.");
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
            WfpShiftSwapRequest source,
            CancellationToken cancellationToken)
        {
            if (!source.WorkflowDefinitionId.HasValue || source.WorkflowDefinitionId == Guid.Empty)
            {
                return WorkflowCodeResult.Ok(SchedulingRequestValueConstants.Workflow.ShiftSwapCode);
            }

            var definition = await _dbContext.MstWorkflowDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == source.WorkflowDefinitionId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            return definition == null
                ? WorkflowCodeResult.Fail(StatusCodes.Status400BadRequest, "Workflow definition tukar shift tidak ditemukan atau tidak aktif.")
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

        private static SchedulingRequestServiceResult<ShiftSwapWorkflowResponse> BuildResponse(
            ShiftSwapDetailResponse source,
            WorkflowInstanceDetailResponse workflow,
            string message)
        {
            return SchedulingRequestServiceResult<ShiftSwapWorkflowResponse>.Ok(
                new ShiftSwapWorkflowResponse
                {
                    ShiftSwapRequestId = source.Id,
                    RequestNumber = source.RequestNumber,
                    RequestStatus = source.RequestStatus,
                    HasWorkflow = true,
                    IsSynchronized = IsSynchronized(source.RequestStatus, workflow.WorkflowStatus),
                    IsAutoApplyPending = workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Completed &&
                                         source.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Approved,
                    ShiftSwap = source,
                    Workflow = workflow
                },
                message);
        }

        private static bool IsSynchronized(string sourceStatus, string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed)
            {
                return sourceStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Approved ||
                       sourceStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Applied;
            }

            if (IsRunning(workflowStatus))
            {
                return sourceStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval;
            }

            return sourceStatus == ShiftSwapWorkflowLifecycleService.MapStatus(workflowStatus);
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
