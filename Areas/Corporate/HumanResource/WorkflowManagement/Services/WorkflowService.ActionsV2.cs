using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public partial class WorkflowService
    {
        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> RejectAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            WorkflowRejectRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau assignment id tidak valid.");
            }

            if (request == null || request.RejectionReasonId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "RejectionReasonId wajib diisi.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            var idempotent = await TryGetIdempotentDetailAsync(
                workflowInstanceId,
                request.IdempotencyKey,
                actorContext,
                cancellationToken);

            if (idempotent != null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    idempotent,
                    "Reject workflow dengan idempotency key yang sama sudah diproses.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(workflowInstanceId, cancellationToken);
                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                var validation = ValidateActionAssignment(instance, assignmentId, actorContext.UserId);
                if (!validation.IsValid)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        validation.StatusCode,
                        validation.Message);
                }

                var assignment = validation.Assignment!;
                var step = validation.Step!;
                var reason = await ResolveRejectionReasonAsync(
                    instance,
                    step,
                    request.RejectionReasonId,
                    cancellationToken);

                if (reason == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Rejection reason tidak ditemukan, tidak aktif, atau tidak sesuai workflow dan step.");
                }

                var reasonValidation = await ValidateReasonRequirementsAsync(
                    instance.Id,
                    reason,
                    request.Comment,
                    request.AttachmentIds,
                    cancellationToken);

                if (reasonValidation != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        reasonValidation);
                }

                var now = DateTime.UtcNow;
                var previousWorkflowStatus = instance.WorkflowStatus;
                var previousStepStatus = step.StepStatus;
                var resultingStepStatus = WorkflowValueConstants.StepStatus.Rejected;
                var actionMessage = "Workflow berhasil ditolak.";

                if (string.Equals(
                    reason.RejectAction,
                    WorkflowValueConstants.RejectAction.ReturnToRequester,
                    StringComparison.OrdinalIgnoreCase))
                {
                    resultingStepStatus = WorkflowValueConstants.StepStatus.RevisionRequested;
                    SetRevisionRequestedState(instance, step, assignment, actorContext.UserId, now);
                    actionMessage = "Workflow dikembalikan kepada pemohon untuk direvisi.";
                }
                else if (string.Equals(
                    reason.RejectAction,
                    WorkflowValueConstants.RejectAction.ReturnToPreviousStep,
                    StringComparison.OrdinalIgnoreCase))
                {
                    var targetOrder = instance.StepInstances
                        .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel && x.StepOrder < step.StepOrder)
                        .Select(x => (int?)x.StepOrder)
                        .Max();

                    if (!targetOrder.HasValue)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Workflow tidak memiliki step sebelumnya untuk tujuan return.");
                    }

                    resultingStepStatus = WorkflowValueConstants.StepStatus.Returned;
                    MarkActingAssignmentAndStep(
                        assignment,
                        step,
                        WorkflowValueConstants.AssignmentStatus.Returned,
                        WorkflowValueConstants.StepStatus.Returned,
                        actorContext.UserId,
                        now);
                    ResetAndActivateFromStepOrder(instance, targetOrder.Value, actorContext.UserId, now);
                    actionMessage = "Workflow dikembalikan ke step sebelumnya.";
                }
                else if (string.Equals(
                    reason.RejectAction,
                    WorkflowValueConstants.RejectAction.ReturnToSpecificStep,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(reason.ReturnToStepCode))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "ReturnToStepCode belum dikonfigurasi pada rejection reason.");
                    }

                    var targetStep = instance.StepInstances.FirstOrDefault(x =>
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        string.Equals(x.StepCodeSnapshot, reason.ReturnToStepCode, StringComparison.OrdinalIgnoreCase));

                    if (targetStep == null || targetStep.StepOrder >= step.StepOrder)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Step tujuan return tidak ditemukan atau bukan step sebelumnya.");
                    }

                    resultingStepStatus = WorkflowValueConstants.StepStatus.Returned;
                    MarkActingAssignmentAndStep(
                        assignment,
                        step,
                        WorkflowValueConstants.AssignmentStatus.Returned,
                        WorkflowValueConstants.StepStatus.Returned,
                        actorContext.UserId,
                        now);
                    ResetAndActivateFromStepOrder(instance, targetStep.StepOrder, actorContext.UserId, now);
                    actionMessage = $"Workflow dikembalikan ke step {targetStep.StepCodeSnapshot}.";
                }
                else if (string.Equals(
                    reason.RejectAction,
                    WorkflowValueConstants.RejectAction.CancelRequest,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MarkActingAssignmentAndStep(
                        assignment,
                        step,
                        WorkflowValueConstants.AssignmentStatus.Rejected,
                        WorkflowValueConstants.StepStatus.Rejected,
                        actorContext.UserId,
                        now);
                    SetWorkflowTerminalState(
                        instance,
                        WorkflowValueConstants.WorkflowStatus.Cancelled,
                        request.Comment ?? reason.ReasonName,
                        actorContext.UserId,
                        now);
                    actionMessage = "Workflow dibatalkan berdasarkan rejection reason.";
                }
                else
                {
                    MarkActingAssignmentAndStep(
                        assignment,
                        step,
                        WorkflowValueConstants.AssignmentStatus.Rejected,
                        WorkflowValueConstants.StepStatus.Rejected,
                        actorContext.UserId,
                        now);
                    SetWorkflowTerminalState(
                        instance,
                        WorkflowValueConstants.WorkflowStatus.Rejected,
                        request.Comment ?? reason.ReasonName,
                        actorContext.UserId,
                        now);
                }

                instance.LastActionAt = now;
                instance.UpdateDateTime = now;
                instance.UpdateBy = actorContext.UserId;

                var action = BuildAssignmentAction(
                    instance,
                    step,
                    assignment,
                    actorContext,
                    WorkflowValueConstants.ActionType.Reject,
                    request.Comment,
                    request.IdempotencyKey,
                    previousWorkflowStatus,
                    previousStepStatus,
                    resultingStepStatus,
                    now);

                action.ActionReasonId = reason.Id;
                action.ActionReasonType = "RejectionReason";
                action.ActionReasonCodeSnapshot = reason.ReasonCode;
                action.ActionReasonNameSnapshot = reason.ReasonName;
                action.ActionContextJson = JsonSerializer.Serialize(new
                {
                    rejectAction = reason.RejectAction,
                    returnToStepCode = reason.ReturnToStepCode,
                    attachmentIds = request.AttachmentIds
                });

                _dbContext.Set<TrxApprovalAction>().Add(action);

                if (!string.IsNullOrWhiteSpace(request.Comment))
                {
                    AddComment(
                        instance,
                        step,
                        actorContext,
                        WorkflowValueConstants.CommentType.Rejection,
                        request.Comment,
                        now);
                }

                await AttachFilesToActionAsync(
                    instance.Id,
                    step.Id,
                    action.Id,
                    request.AttachmentIds,
                    actorContext.UserId,
                    now,
                    cancellationToken);

                AddStatusHistory(
                    instance,
                    step,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    previousStepStatus,
                    resultingStepStatus,
                    WorkflowValueConstants.ActionType.Reject,
                    request.Comment ?? reason.ReasonName,
                    false,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(instance.Id, actorContext, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    actionMessage);
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Reject workflow gagal karena terjadi konflik data atau request sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Reject workflow gagal: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> RequestRevisionAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            WorkflowRequestRevisionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau assignment id tidak valid.");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Comment))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Komentar revisi wajib diisi.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            var idempotent = await TryGetIdempotentDetailAsync(
                workflowInstanceId,
                request.IdempotencyKey,
                actorContext,
                cancellationToken);

            if (idempotent != null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    idempotent,
                    "Request revision dengan idempotency key yang sama sudah diproses.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(workflowInstanceId, cancellationToken);
                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                var validation = ValidateActionAssignment(instance, assignmentId, actorContext.UserId);
                if (!validation.IsValid)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        validation.StatusCode,
                        validation.Message);
                }

                var assignment = validation.Assignment!;
                var step = validation.Step!;
                MstRejectionReason? reason = null;

                if (request.RejectionReasonId.HasValue && request.RejectionReasonId.Value != Guid.Empty)
                {
                    reason = await ResolveRejectionReasonAsync(
                        instance,
                        step,
                        request.RejectionReasonId.Value,
                        cancellationToken);

                    if (reason == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Rejection reason tidak ditemukan, tidak aktif, atau tidak sesuai workflow dan step.");
                    }

                    var reasonValidation = await ValidateReasonRequirementsAsync(
                        instance.Id,
                        reason,
                        request.Comment,
                        request.AttachmentIds,
                        cancellationToken);

                    if (reasonValidation != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            reasonValidation);
                    }
                }

                var now = DateTime.UtcNow;
                var previousWorkflowStatus = instance.WorkflowStatus;
                var previousStepStatus = step.StepStatus;

                SetRevisionRequestedState(instance, step, assignment, actorContext.UserId, now);

                var action = BuildAssignmentAction(
                    instance,
                    step,
                    assignment,
                    actorContext,
                    WorkflowValueConstants.ActionType.RequestRevision,
                    request.Comment,
                    request.IdempotencyKey,
                    previousWorkflowStatus,
                    previousStepStatus,
                    WorkflowValueConstants.StepStatus.RevisionRequested,
                    now);

                if (reason != null)
                {
                    action.ActionReasonId = reason.Id;
                    action.ActionReasonType = "RejectionReason";
                    action.ActionReasonCodeSnapshot = reason.ReasonCode;
                    action.ActionReasonNameSnapshot = reason.ReasonName;
                }

                action.ActionContextJson = JsonSerializer.Serialize(new
                {
                    revisionRequested = true,
                    attachmentIds = request.AttachmentIds
                });

                _dbContext.Set<TrxApprovalAction>().Add(action);

                AddComment(
                    instance,
                    step,
                    actorContext,
                    WorkflowValueConstants.CommentType.Revision,
                    request.Comment,
                    now);

                await AttachFilesToActionAsync(
                    instance.Id,
                    step.Id,
                    action.Id,
                    request.AttachmentIds,
                    actorContext.UserId,
                    now,
                    cancellationToken);

                AddStatusHistory(
                    instance,
                    step,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    previousStepStatus,
                    WorkflowValueConstants.StepStatus.RevisionRequested,
                    WorkflowValueConstants.ActionType.RequestRevision,
                    request.Comment,
                    false,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(instance.Id, actorContext, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    "Workflow berhasil dikembalikan kepada pemohon untuk direvisi.");
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Request revision gagal karena terjadi konflik data atau request sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Request revision gagal: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> ReturnAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            WorkflowReturnRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau assignment id tidak valid.");
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.ReturnToStepCode) ||
                string.IsNullOrWhiteSpace(request.Comment))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ReturnToStepCode dan komentar wajib diisi.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            var idempotent = await TryGetIdempotentDetailAsync(
                workflowInstanceId,
                request.IdempotencyKey,
                actorContext,
                cancellationToken);

            if (idempotent != null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    idempotent,
                    "Return workflow dengan idempotency key yang sama sudah diproses.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(workflowInstanceId, cancellationToken);
                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                var validation = ValidateActionAssignment(instance, assignmentId, actorContext.UserId);
                if (!validation.IsValid)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        validation.StatusCode,
                        validation.Message);
                }

                var assignment = validation.Assignment!;
                var step = validation.Step!;
                var targetStep = instance.StepInstances.FirstOrDefault(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    string.Equals(x.StepCodeSnapshot, request.ReturnToStepCode.Trim(), StringComparison.OrdinalIgnoreCase));

                if (targetStep == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Step tujuan return tidak ditemukan.");
                }

                if (targetStep.StepOrder >= step.StepOrder)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Workflow hanya dapat dikembalikan ke step sebelumnya.");
                }

                MstRejectionReason? reason = null;
                if (request.RejectionReasonId.HasValue && request.RejectionReasonId.Value != Guid.Empty)
                {
                    reason = await ResolveRejectionReasonAsync(
                        instance,
                        step,
                        request.RejectionReasonId.Value,
                        cancellationToken);

                    if (reason == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Rejection reason tidak ditemukan, tidak aktif, atau tidak sesuai workflow dan step.");
                    }

                    var reasonValidation = await ValidateReasonRequirementsAsync(
                        instance.Id,
                        reason,
                        request.Comment,
                        request.AttachmentIds,
                        cancellationToken);

                    if (reasonValidation != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            reasonValidation);
                    }
                }

                var now = DateTime.UtcNow;
                var previousWorkflowStatus = instance.WorkflowStatus;
                var previousStepStatus = step.StepStatus;

                MarkActingAssignmentAndStep(
                    assignment,
                    step,
                    WorkflowValueConstants.AssignmentStatus.Returned,
                    WorkflowValueConstants.StepStatus.Returned,
                    actorContext.UserId,
                    now);

                ResetAndActivateFromStepOrder(instance, targetStep.StepOrder, actorContext.UserId, now);
                instance.WorkflowStatus = WorkflowValueConstants.WorkflowStatus.InProgress;
                instance.LastActionAt = now;
                instance.UpdateDateTime = now;
                instance.UpdateBy = actorContext.UserId;

                var action = BuildAssignmentAction(
                    instance,
                    step,
                    assignment,
                    actorContext,
                    WorkflowValueConstants.ActionType.Return,
                    request.Comment,
                    request.IdempotencyKey,
                    previousWorkflowStatus,
                    previousStepStatus,
                    WorkflowValueConstants.StepStatus.Returned,
                    now);

                if (reason != null)
                {
                    action.ActionReasonId = reason.Id;
                    action.ActionReasonType = "RejectionReason";
                    action.ActionReasonCodeSnapshot = reason.ReasonCode;
                    action.ActionReasonNameSnapshot = reason.ReasonName;
                }

                action.ActionContextJson = JsonSerializer.Serialize(new
                {
                    returnToStepCode = targetStep.StepCodeSnapshot,
                    returnToStepOrder = targetStep.StepOrder,
                    attachmentIds = request.AttachmentIds
                });

                _dbContext.Set<TrxApprovalAction>().Add(action);

                AddComment(
                    instance,
                    step,
                    actorContext,
                    WorkflowValueConstants.CommentType.Approver,
                    request.Comment,
                    now);

                await AttachFilesToActionAsync(
                    instance.Id,
                    step.Id,
                    action.Id,
                    request.AttachmentIds,
                    actorContext.UserId,
                    now,
                    cancellationToken);

                AddStatusHistory(
                    instance,
                    step,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    previousStepStatus,
                    WorkflowValueConstants.StepStatus.Returned,
                    WorkflowValueConstants.ActionType.Return,
                    request.Comment,
                    false,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(instance.Id, actorContext, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    $"Workflow berhasil dikembalikan ke step {targetStep.StepCodeSnapshot}.");
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Return workflow gagal karena terjadi konflik data atau request sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Return workflow gagal: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> CancelAsync(
            Guid workflowInstanceId,
            WorkflowCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id dan alasan pembatalan wajib diisi.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            var idempotent = await TryGetIdempotentDetailAsync(
                workflowInstanceId,
                request.IdempotencyKey,
                actorContext,
                cancellationToken);

            if (idempotent != null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    idempotent,
                    "Cancel workflow dengan idempotency key yang sama sudah diproses.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(workflowInstanceId, cancellationToken);
                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                if (instance.RequestedByUserId != actorContext.UserId)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Hanya pemohon yang dapat membatalkan workflow.");
                }

                if (instance.WorkflowDefinition?.AllowRequesterCancel != true)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Workflow definition tidak mengizinkan pembatalan oleh pemohon.");
                }

                if (!CanRequesterCancel(instance.WorkflowStatus))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Workflow tidak berada pada status yang dapat dibatalkan. Gunakan withdraw apabila workflow sedang diproses.");
                }

                var now = DateTime.UtcNow;
                var previousWorkflowStatus = instance.WorkflowStatus;

                SetWorkflowTerminalState(
                    instance,
                    WorkflowValueConstants.WorkflowStatus.Cancelled,
                    request.Reason,
                    actorContext.UserId,
                    now);

                var action = BuildInstanceAction(
                    instance,
                    actorContext,
                    WorkflowValueConstants.ActionType.Cancel,
                    request.Reason,
                    request.IdempotencyKey,
                    previousWorkflowStatus,
                    now);

                _dbContext.Set<TrxApprovalAction>().Add(action);

                AddComment(
                    instance,
                    null,
                    actorContext,
                    WorkflowValueConstants.CommentType.Requester,
                    request.Reason,
                    now);

                AddStatusHistory(
                    instance,
                    null,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    null,
                    null,
                    WorkflowValueConstants.ActionType.Cancel,
                    request.Reason,
                    false,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(instance.Id, actorContext, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    "Workflow berhasil dibatalkan.");
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Cancel workflow gagal karena terjadi konflik data atau request sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Cancel workflow gagal: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> WithdrawAsync(
            Guid workflowInstanceId,
            WorkflowWithdrawRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id dan alasan penarikan wajib diisi.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            var idempotent = await TryGetIdempotentDetailAsync(
                workflowInstanceId,
                request.IdempotencyKey,
                actorContext,
                cancellationToken);

            if (idempotent != null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    idempotent,
                    "Withdraw workflow dengan idempotency key yang sama sudah diproses.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(workflowInstanceId, cancellationToken);
                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                if (instance.RequestedByUserId != actorContext.UserId)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Hanya pemohon yang dapat menarik workflow.");
                }

                if (instance.WorkflowDefinition?.AllowRequesterWithdraw != true)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Workflow definition tidak mengizinkan withdraw oleh pemohon.");
                }

                if (!string.Equals(
                    instance.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Workflow hanya dapat ditarik ketika sedang diproses.");
                }

                var now = DateTime.UtcNow;
                var previousWorkflowStatus = instance.WorkflowStatus;

                SetWorkflowTerminalState(
                    instance,
                    WorkflowValueConstants.WorkflowStatus.Withdrawn,
                    request.Reason,
                    actorContext.UserId,
                    now);

                var action = BuildInstanceAction(
                    instance,
                    actorContext,
                    WorkflowValueConstants.ActionType.Withdraw,
                    request.Reason,
                    request.IdempotencyKey,
                    previousWorkflowStatus,
                    now);

                _dbContext.Set<TrxApprovalAction>().Add(action);

                AddComment(
                    instance,
                    null,
                    actorContext,
                    WorkflowValueConstants.CommentType.Requester,
                    request.Reason,
                    now);

                AddStatusHistory(
                    instance,
                    null,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    null,
                    null,
                    WorkflowValueConstants.ActionType.Withdraw,
                    request.Reason,
                    false,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(instance.Id, actorContext, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    "Workflow berhasil ditarik oleh pemohon.");
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Withdraw workflow gagal karena terjadi konflik data atau request sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Withdraw workflow gagal: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> VerifyAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            WorkflowVerifyRequest? request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidatePositiveActionStepTypeAsync(
                workflowInstanceId,
                assignmentId,
                WorkflowValueConstants.StepType.Verification,
                cancellationToken);

            if (validation != null)
            {
                return validation;
            }

            return await ApproveAsync(
                workflowInstanceId,
                assignmentId,
                new WorkflowApproveRequest
                {
                    Comment = request?.Comment,
                    IdempotencyKey = request?.IdempotencyKey
                },
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> AcknowledgeAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            WorkflowAcknowledgeRequest? request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidatePositiveActionStepTypeAsync(
                workflowInstanceId,
                assignmentId,
                WorkflowValueConstants.StepType.Acknowledgement,
                cancellationToken);

            if (validation != null)
            {
                return validation;
            }

            return await ApproveAsync(
                workflowInstanceId,
                assignmentId,
                new WorkflowApproveRequest
                {
                    Comment = request?.Comment,
                    IdempotencyKey = request?.IdempotencyKey
                },
                cancellationToken);
        }

        private async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>?> ValidatePositiveActionStepTypeAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            string expectedStepType,
            CancellationToken cancellationToken)
        {
            if (workflowInstanceId == Guid.Empty || assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau assignment id tidak valid.");
            }

            var stepType = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == assignmentId &&
                    x.WorkflowInstanceId == workflowInstanceId &&
                    !x.IsDelete &&
                    x.WorkflowStepInstance != null)
                .Select(x => x.WorkflowStepInstance!.StepTypeSnapshot)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(stepType))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approver assignment tidak ditemukan.");
            }

            if (!string.Equals(stepType, expectedStepType, StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Endpoint ini hanya dapat digunakan untuk step type {expectedStepType}.");
            }

            return null;
        }

        private ActionAssignmentValidation ValidateActionAssignment(
            TrxWorkflowInstance instance,
            Guid assignmentId,
            Guid actorUserId)
        {
            if (!string.Equals(
                instance.WorkflowStatus,
                WorkflowValueConstants.WorkflowStatus.InProgress,
                StringComparison.OrdinalIgnoreCase))
            {
                return ActionAssignmentValidation.Fail(
                    StatusCodes.Status409Conflict,
                    "Workflow tidak berada pada status yang dapat diproses.");
            }

            var assignment = instance.StepInstances
                .SelectMany(x => x.ApproverAssignments)
                .FirstOrDefault(x =>
                    x.Id == assignmentId &&
                    x.WorkflowInstanceId == instance.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel);

            if (assignment == null)
            {
                return ActionAssignmentValidation.Fail(
                    StatusCodes.Status404NotFound,
                    "Approver assignment tidak ditemukan.");
            }

            var step = instance.StepInstances.FirstOrDefault(x => x.Id == assignment.WorkflowStepInstanceId);
            if (step == null ||
                !step.IsCurrentStep ||
                !string.Equals(step.StepStatus, WorkflowValueConstants.StepStatus.InProgress, StringComparison.OrdinalIgnoreCase))
            {
                return ActionAssignmentValidation.Fail(
                    StatusCodes.Status409Conflict,
                    "Step assignment belum aktif atau sudah selesai.");
            }

            if (!assignment.IsCurrentAssignment || !IsAvailableAssignmentStatus(assignment.AssignmentStatus))
            {
                return ActionAssignmentValidation.Fail(
                    StatusCodes.Status409Conflict,
                    "Assignment belum tersedia atau sudah diproses.");
            }

            if (assignment.AssignedApproverUserId != actorUserId)
            {
                return ActionAssignmentValidation.Fail(
                    StatusCodes.Status403Forbidden,
                    "User login bukan approver yang ditugaskan.");
            }

            return ActionAssignmentValidation.Ok(assignment, step);
        }

        private async Task<MstRejectionReason?> ResolveRejectionReasonAsync(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance step,
            Guid reasonId,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var requestType = instance.WorkflowDefinition?.RequestType;

            return await _dbContext.Set<MstRejectionReason>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == reasonId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today) &&
                    (!x.WorkflowDefinitionId.HasValue || x.WorkflowDefinitionId == instance.WorkflowDefinitionId) &&
                    (!x.WorkflowStepId.HasValue || x.WorkflowStepId == step.WorkflowStepId) &&
                    (requestType == null || x.RequestType.ToLower() == requestType.ToLower()),
                    cancellationToken);
        }

        private async Task<string?> ValidateReasonRequirementsAsync(
            Guid workflowInstanceId,
            MstRejectionReason reason,
            string? comment,
            IReadOnlyCollection<Guid>? attachmentIds,
            CancellationToken cancellationToken)
        {
            if (reason.IsCommentRequired && string.IsNullOrWhiteSpace(comment))
            {
                return "Komentar wajib diisi untuk rejection reason yang dipilih.";
            }

            var normalizedIds = attachmentIds?
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (reason.IsAttachmentRequired && normalizedIds.Count == 0)
            {
                return "Lampiran wajib tersedia untuk rejection reason yang dipilih.";
            }

            if (normalizedIds.Count > 0)
            {
                var attachmentCount = await _dbContext.Set<TrxWorkflowAttachment>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        normalizedIds.Contains(x.Id) &&
                        x.WorkflowInstanceId == workflowInstanceId &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel,
                        cancellationToken);

                if (attachmentCount != normalizedIds.Count)
                {
                    return "Satu atau lebih attachment tidak ditemukan atau bukan milik workflow ini.";
                }
            }

            return null;
        }

        private async Task AttachFilesToActionAsync(
            Guid workflowInstanceId,
            Guid workflowStepInstanceId,
            Guid approvalActionId,
            IReadOnlyCollection<Guid>? attachmentIds,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var normalizedIds = attachmentIds?
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (normalizedIds.Count == 0)
            {
                return;
            }

            var attachments = await _dbContext.Set<TrxWorkflowAttachment>()
                .Where(x =>
                    normalizedIds.Contains(x.Id) &&
                    x.WorkflowInstanceId == workflowInstanceId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .ToListAsync(cancellationToken);

            foreach (var attachment in attachments)
            {
                attachment.WorkflowStepInstanceId ??= workflowStepInstanceId;
                attachment.ApprovalActionId = approvalActionId;
                attachment.UpdateDateTime = now;
                attachment.UpdateBy = actorUserId;
            }
        }

        private async Task<WorkflowInstanceDetailResponse?> TryGetIdempotentDetailAsync(
            Guid workflowInstanceId,
            string? idempotencyKey,
            HumanResourceUserContextDto actorContext,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return null;
            }

            var exists = await _dbContext.Set<TrxApprovalAction>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.WorkflowInstanceId == workflowInstanceId &&
                    x.IdempotencyKey == idempotencyKey.Trim(),
                    cancellationToken);

            return exists
                ? await LoadDetailAsync(workflowInstanceId, actorContext, cancellationToken)
                : null;
        }

        private TrxApprovalAction BuildAssignmentAction(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment assignment,
            HumanResourceUserContextDto actorContext,
            string actionType,
            string? comment,
            string? idempotencyKey,
            string previousWorkflowStatus,
            string previousStepStatus,
            string resultingStepStatus,
            DateTime now)
        {
            return new TrxApprovalAction
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepInstanceId = step.Id,
                WorkflowApproverAssignmentId = assignment.Id,
                ApprovalDelegationId = assignment.ApprovalDelegationId,
                AssignedApproverUserId = assignment.AssignedApproverUserId,
                AssignedApproverWorkforceProfileId = assignment.AssignedApproverWorkforceProfileId,
                ActualActionByUserId = actorContext.UserId,
                ActualActionByWorkforceProfileId = actorContext.WorkforceProfileId,
                DelegatedFromUserId = assignment.OriginalApproverUserId,
                DelegatedFromWorkforceProfileId = assignment.OriginalApproverWorkforceProfileId,
                ActionType = actionType,
                ActionAt = now,
                Comment = NormalizeOptionalText(comment),
                IsDelegated = assignment.IsDelegated,
                IsSystemAction = false,
                ActionSource = ResolveActionSource(instance.SourceChannel),
                IdempotencyKey = NormalizeOptionalText(idempotencyKey),
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent(),
                PreviousWorkflowStatus = previousWorkflowStatus,
                ResultingWorkflowStatus = instance.WorkflowStatus,
                PreviousStepStatus = previousStepStatus,
                ResultingStepStatus = resultingStepStatus,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorContext.UserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private TrxApprovalAction BuildInstanceAction(
            TrxWorkflowInstance instance,
            HumanResourceUserContextDto actorContext,
            string actionType,
            string? comment,
            string? idempotencyKey,
            string previousWorkflowStatus,
            DateTime now)
        {
            return new TrxApprovalAction
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                ActualActionByUserId = actorContext.UserId,
                ActualActionByWorkforceProfileId = actorContext.WorkforceProfileId,
                ActionType = actionType,
                ActionAt = now,
                Comment = NormalizeOptionalText(comment),
                IsDelegated = false,
                IsSystemAction = false,
                ActionSource = ResolveActionSource(instance.SourceChannel),
                IdempotencyKey = NormalizeOptionalText(idempotencyKey),
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent(),
                PreviousWorkflowStatus = previousWorkflowStatus,
                ResultingWorkflowStatus = instance.WorkflowStatus,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorContext.UserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private static void MarkActingAssignmentAndStep(
            TrxWorkflowApproverAssignment assignment,
            TrxWorkflowStepInstance step,
            string assignmentStatus,
            string stepStatus,
            Guid actorUserId,
            DateTime now)
        {
            assignment.AssignmentStatus = assignmentStatus;
            assignment.StartedAt ??= now;
            assignment.CompletedAt = now;
            assignment.IsCurrentAssignment = false;
            assignment.UpdateDateTime = now;
            assignment.UpdateBy = actorUserId;

            step.StepStatus = stepStatus;
            step.CompletedAt = now;
            step.IsCurrentStep = false;
            step.UpdateDateTime = now;
            step.UpdateBy = actorUserId;

            if (string.Equals(assignmentStatus, WorkflowValueConstants.AssignmentStatus.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                step.RejectedActionCount++;
            }

            foreach (var other in step.ApproverAssignments.Where(x =>
                         x.Id != assignment.Id &&
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel &&
                         !IsTerminalAssignmentStatus(x.AssignmentStatus)))
            {
                other.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Cancelled;
                other.CompletedAt = now;
                other.IsCurrentAssignment = false;
                other.UpdateDateTime = now;
                other.UpdateBy = actorUserId;
            }
        }

        private static void SetRevisionRequestedState(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance actingStep,
            TrxWorkflowApproverAssignment actingAssignment,
            Guid actorUserId,
            DateTime now)
        {
            var currentOrder = actingStep.StepOrder;

            foreach (var step in instance.StepInstances.Where(x =>
                         x.StepOrder == currentOrder &&
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel))
            {
                step.StepStatus = WorkflowValueConstants.StepStatus.RevisionRequested;
                step.CompletedAt = now;
                step.IsCurrentStep = false;
                step.UpdateDateTime = now;
                step.UpdateBy = actorUserId;

                foreach (var assignment in step.ApproverAssignments.Where(x =>
                             x.IsActive &&
                             !x.IsDelete &&
                             !x.IsCancel &&
                             !IsTerminalAssignmentStatus(x.AssignmentStatus)))
                {
                    assignment.AssignmentStatus = assignment.Id == actingAssignment.Id
                        ? WorkflowValueConstants.AssignmentStatus.RevisionRequested
                        : WorkflowValueConstants.AssignmentStatus.Cancelled;
                    assignment.CompletedAt = now;
                    assignment.IsCurrentAssignment = false;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                }
            }

            instance.WorkflowStatus = WorkflowValueConstants.WorkflowStatus.RevisionRequested;
            instance.LastActionAt = now;
            instance.CompletedAt = null;
            instance.UpdateDateTime = now;
            instance.UpdateBy = actorUserId;
        }

        private static int? PrepareRevisionResubmit(
            TrxWorkflowInstance instance,
            Guid actorUserId,
            DateTime now)
        {
            var targetOrder = instance.CurrentStepOrder > 0
                ? instance.CurrentStepOrder
                : instance.StepInstances
                    .Where(x =>
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        string.Equals(x.StepStatus, WorkflowValueConstants.StepStatus.RevisionRequested, StringComparison.OrdinalIgnoreCase))
                    .Select(x => (int?)x.StepOrder)
                    .Min();

            if (!targetOrder.HasValue)
            {
                return null;
            }

            foreach (var step in instance.StepInstances.Where(x =>
                         x.StepOrder == targetOrder.Value &&
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel))
            {
                ResetStepForReuse(step, actorUserId, now);
            }

            return targetOrder.Value;
        }

        private static void ResetAndActivateFromStepOrder(
            TrxWorkflowInstance instance,
            int targetStepOrder,
            Guid actorUserId,
            DateTime now)
        {
            foreach (var step in instance.StepInstances.Where(x =>
                         x.StepOrder >= targetStepOrder &&
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel))
            {
                ResetStepForReuse(step, actorUserId, now);
            }

            instance.CompletedAt = null;
            instance.CancelledAt = null;
            instance.WithdrawnAt = null;
            instance.CancellationReason = null;
            ActivateStepGroup(instance, targetStepOrder, now);
        }

        private static void ResetStepForReuse(
            TrxWorkflowStepInstance step,
            Guid actorUserId,
            DateTime now)
        {
            step.StepStatus = WorkflowValueConstants.StepStatus.Pending;
            step.AvailableAt = null;
            step.StartedAt = null;
            step.CompletedAt = null;
            step.SkippedAt = null;
            step.IsCurrentStep = false;
            step.ApprovedActionCount = 0;
            step.RejectedActionCount = 0;
            step.UpdateDateTime = now;
            step.UpdateBy = actorUserId;

            foreach (var assignment in step.ApproverAssignments.Where(x =>
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel))
            {
                assignment.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Pending;
                assignment.AvailableAt = null;
                assignment.StartedAt = null;
                assignment.CompletedAt = null;
                assignment.IsCurrentAssignment = false;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorUserId;
            }
        }

        private static void SetWorkflowTerminalState(
            TrxWorkflowInstance instance,
            string workflowStatus,
            string? reason,
            Guid actorUserId,
            DateTime now)
        {
            foreach (var step in instance.StepInstances.Where(x =>
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel &&
                         !IsTerminalStepStatus(x.StepStatus)))
            {
                step.StepStatus = WorkflowValueConstants.StepStatus.Cancelled;
                step.CompletedAt = now;
                step.IsCurrentStep = false;
                step.UpdateDateTime = now;
                step.UpdateBy = actorUserId;

                foreach (var assignment in step.ApproverAssignments.Where(x =>
                             x.IsActive &&
                             !x.IsDelete &&
                             !x.IsCancel &&
                             !IsTerminalAssignmentStatus(x.AssignmentStatus)))
                {
                    assignment.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Cancelled;
                    assignment.CompletedAt = now;
                    assignment.IsCurrentAssignment = false;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                }
            }

            instance.WorkflowStatus = workflowStatus;
            instance.CurrentStepOrder = 0;
            instance.CurrentStepCode = null;
            instance.LastActionAt = now;
            instance.UpdateDateTime = now;
            instance.UpdateBy = actorUserId;

            if (string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                instance.CancelledAt = now;
                instance.CancellationReason = NormalizeOptionalText(reason);
            }
            else if (string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Withdrawn, StringComparison.OrdinalIgnoreCase))
            {
                instance.WithdrawnAt = now;
                instance.CancellationReason = NormalizeOptionalText(reason);
            }
            else
            {
                instance.CompletedAt = now;
                instance.CompletionNote = NormalizeOptionalText(reason);
            }
        }

        private static bool CanRequesterCancel(string? workflowStatus)
        {
            return string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Draft, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Submitted, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.RevisionRequested, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Returned, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTerminalStepStatus(string? status)
        {
            return string.Equals(status, WorkflowValueConstants.StepStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.StepStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.StepStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.StepStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.StepStatus.Skipped, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTerminalAssignmentStatus(string? status)
        {
            return string.Equals(status, WorkflowValueConstants.AssignmentStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.AssignmentStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.AssignmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.AssignmentStatus.Skipped, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, WorkflowValueConstants.AssignmentStatus.Completed, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ActionAssignmentValidation
        {
            public bool IsValid { get; private set; }
            public int StatusCode { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public TrxWorkflowApproverAssignment? Assignment { get; private set; }
            public TrxWorkflowStepInstance? Step { get; private set; }

            public static ActionAssignmentValidation Ok(
                TrxWorkflowApproverAssignment assignment,
                TrxWorkflowStepInstance step)
            {
                return new ActionAssignmentValidation
                {
                    IsValid = true,
                    StatusCode = StatusCodes.Status200OK,
                    Assignment = assignment,
                    Step = step
                };
            }

            public static ActionAssignmentValidation Fail(int statusCode, string message)
            {
                return new ActionAssignmentValidation
                {
                    IsValid = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
        }
    }
}
