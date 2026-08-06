using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class ResignationWorkflowIntegrationService
    {
        private static readonly HashSet<string> ReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ResignationValueConstants.Workflow.ReferenceType,
                "ResignationRequest",
                "TrxResignationRequest",
                "RESIGNATION"
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly ResignationRequestService _resignationRequestService;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowReferenceLifecycleService _workflowReferenceLifecycleService;

        public ResignationWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            ResignationRequestService resignationRequestService,
            WorkflowService workflowService,
            WorkflowReferenceLifecycleService workflowReferenceLifecycleService)
        {
            _dbContext = dbContext;
            _resignationRequestService = resignationRequestService;
            _workflowService = workflowService;
            _workflowReferenceLifecycleService = workflowReferenceLifecycleService;
        }

        public async Task<ResignationServiceResult<ResignationWorkflowResponse>> GetWorkflowAsync(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var source = await _resignationRequestService.GetByIdAsync(requestId, cancellationToken);
            if (!source.Success || source.Data == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    source.StatusCode,
                    source.Message);
            }

            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Ok(
                    new ResignationWorkflowResponse
                    {
                        ResignationRequestId = source.Data.Id,
                        RequestNumber = source.Data.RequestNumber,
                        RequestStatus = source.Data.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = source.Data.RequestStatus == ResignationValueConstants.Status.Draft,
                        Resignation = source.Data
                    },
                    "Pengajuan resign belum mempunyai workflow.");
            }

            var detail = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            if (!detail.Success || detail.Data == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    detail.StatusCode,
                    detail.Message);
            }

            return BuildResponse(source.Data, detail.Data, "Workflow pengajuan resign berhasil diambil.");
        }

        public async Task<ResignationServiceResult<ResignationWorkflowResponse>> SubmitAsync(
            Guid requestId,
            Guid workforceProfileId,
            Guid actorUserId,
            ResignationSubmitRequest? request,
            CancellationToken cancellationToken = default)
        {
            var prepared = await _resignationRequestService.PrepareSubmitAsync(
                requestId,
                workforceProfileId,
                actorUserId,
                cancellationToken);

            if (!prepared.Success || prepared.Data == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    prepared.StatusCode,
                    prepared.Message);
            }

            var sourceEntity = await _dbContext.TrxResignationRequests
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
                        return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
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
                        return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                            detail.StatusCode,
                            detail.Message);
                    }

                    workflowDetail = detail.Data;
                }
                else
                {
                    return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Workflow sebelumnya sudah berstatus {existingWorkflow.WorkflowStatus} dan tidak dapat digunakan kembali.");
                }
            }
            else
            {
                var codeResult = await ResolveWorkflowCodeAsync(sourceEntity, cancellationToken);
                if (!codeResult.Success)
                {
                    return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                        codeResult.StatusCode,
                        codeResult.Message);
                }

                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = codeResult.Code!,
                        ReferenceType = ResignationValueConstants.Workflow.ReferenceType,
                        ReferenceId = sourceEntity.Id,
                        ExternalReferenceNumber = sourceEntity.RequestNumber,
                        SourceChannel = NormalizeSourceChannel(request?.SourceChannel),
                        RequestCorrelationId = NormalizeText(request?.RequestCorrelationId),
                        IdempotencyKey = $"resignation:{sourceEntity.Id:N}:workflow",
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            resignationRequestId = sourceEntity.Id,
                            sourceEntity.RequestNumber,
                            sourceEntity.WorkforceProfileId,
                            sourceEntity.EmployeeId,
                            sourceEntity.RequestDate,
                            sourceEntity.ProposedLastWorkingDate,
                            sourceEntity.NoticePeriodDays,
                            sourceEntity.RequestReasonId,
                            sourceEntity.ResignationReason,
                            sourceEntity.HandoverPlan
                        }),
                        SelectedApproverUserIds = new List<Guid>()
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
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
                    return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                        submitResult.StatusCode,
                        submitResult.Message);
                }

                workflowDetail = submitResult.Data;
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflowDetail.Id,
                actorUserId,
                allowAutoApply: false,
                cancellationToken: cancellationToken);

            var refreshed = await _resignationRequestService.GetByIdAsync(requestId, cancellationToken);
            if (!refreshed.Success || refreshed.Data == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    refreshed.StatusCode,
                    refreshed.Message);
            }

            return BuildResponse(
                refreshed.Data,
                workflowDetail,
                createdNow
                    ? "Workflow resign berhasil dibuat dan di-submit."
                    : "Pengajuan resign berhasil di-submit ke workflow.");
        }

        public async Task<ResignationServiceResult<ResignationWorkflowResponse>> CancelAsync(
            Guid requestId,
            Guid workforceProfileId,
            Guid actorUserId,
            ResignationCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            var source = await _resignationRequestService.GetByIdAsync(requestId, cancellationToken);
            if (!source.Success || source.Data == null || source.Data.WorkforceProfileId != workforceProfileId)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan atau bukan milik user login.");
            }

            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                var cancelled = await _resignationRequestService.CancelAsync(
                    requestId,
                    workforceProfileId,
                    actorUserId,
                    request.Reason,
                    cancellationToken);

                if (!cancelled.Success || cancelled.Data == null)
                {
                    return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                        cancelled.StatusCode,
                        cancelled.Message);
                }

                return ResignationServiceResult<ResignationWorkflowResponse>.Ok(
                    new ResignationWorkflowResponse
                    {
                        ResignationRequestId = cancelled.Data.Id,
                        RequestNumber = cancelled.Data.RequestNumber,
                        RequestStatus = cancelled.Data.RequestStatus,
                        HasWorkflow = false,
                        IsSynchronized = true,
                        Resignation = cancelled.Data
                    },
                    "Pengajuan resign berhasil dibatalkan sebelum workflow dibuat.");
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
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: false,
                cancellationToken: cancellationToken);

            var refreshed = await _resignationRequestService.GetByIdAsync(requestId, cancellationToken);
            if (!refreshed.Success || refreshed.Data == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    refreshed.StatusCode,
                    refreshed.Message);
            }

            return BuildResponse(
                refreshed.Data,
                workflowResult.Data,
                "Pengajuan resign dan workflow berhasil dibatalkan.");
        }

        public async Task<ResignationServiceResult<ResignationWorkflowResponse>> SynchronizeAsync(
            Guid requestId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var workflow = await FindLatestWorkflowAsync(requestId, cancellationToken);
            if (workflow == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow resign tidak ditemukan.");
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: false,
                cancellationToken: cancellationToken);

            var detail = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            var source = await _resignationRequestService.GetByIdAsync(requestId, cancellationToken);

            if (!detail.Success || detail.Data == null || !source.Success || source.Data == null)
            {
                return ResignationServiceResult<ResignationWorkflowResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    "Sinkronisasi selesai tetapi response akhir tidak dapat dibentuk.");
            }

            return BuildResponse(source.Data, detail.Data, "Workflow resign berhasil disinkronkan.");
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
            TrxResignationRequest source,
            CancellationToken cancellationToken)
        {
            if (!source.WorkflowDefinitionId.HasValue || source.WorkflowDefinitionId == Guid.Empty)
            {
                return WorkflowCodeResult.Ok(ResignationValueConstants.Workflow.Code);
            }

            var definition = await _dbContext.MstWorkflowDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == source.WorkflowDefinitionId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            return definition == null
                ? WorkflowCodeResult.Fail(StatusCodes.Status400BadRequest, "Workflow definition resign tidak ditemukan atau tidak aktif.")
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

        private static ResignationServiceResult<ResignationWorkflowResponse> BuildResponse(
            ResignationDetailResponse source,
            WorkflowInstanceDetailResponse workflow,
            string message)
        {
            return ResignationServiceResult<ResignationWorkflowResponse>.Ok(
                new ResignationWorkflowResponse
                {
                    ResignationRequestId = source.Id,
                    RequestNumber = source.RequestNumber,
                    RequestStatus = source.RequestStatus,
                    HasWorkflow = true,
                    IsSynchronized = IsSynchronized(source.RequestStatus, workflow.WorkflowStatus),
                    Resignation = source,
                    Workflow = workflow
                },
                message);
        }

        private static bool IsSynchronized(string sourceStatus, string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed)
            {
                return sourceStatus == ResignationValueConstants.Status.Approved ||
                       sourceStatus == ResignationValueConstants.Status.HandoffCompleted;
            }

            if (IsRunning(workflowStatus))
            {
                return sourceStatus == ResignationValueConstants.Status.Submitted ||
                       sourceStatus == ResignationValueConstants.Status.UnderReview;
            }

            return sourceStatus == ResignationWorkflowLifecycleService.MapStatus(workflowStatus);
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
