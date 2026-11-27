using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IWorkflowApprovalActionService
    {
        Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> ApproveAsync(Guid stepInstanceId, ApproveActionRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> RejectAsync(Guid stepInstanceId, RejectActionRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default);
        Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> ReturnForRevisionAsync(Guid stepInstanceId, ReturnForRevisionRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// BE-HRD-444 (kontrak v29 §35.2, §35.7; HRD-DEC-229, HRD-DEC-230; VR-WF-26 s.d. VR-WF-28, VR-WF-30): adapter Kotak Masuk
    /// Terpadu. Penugasan engine terbuka pada tahap <c>stepInstanceId</c> yang boleh dijalankan pengguna dipilih dengan aturan
    /// yang sama dengan antrean dan <c>myAction</c> (<see cref="IApprovalInboxQueryService.FindActionableAssignmentAsync"/>),
    /// lalu keputusan diteruskan ke <see cref="WorkflowService"/>. Engine yang mengubah status, mencatat jejak (termasuk Plh),
    /// dan menjalankan penangan modul (<see cref="IWorkflowReferenceLifecycleHandler"/>). Adapter tidak mengubah status
    /// sendiri, tidak meminta PIN, dan tidak menghitung tanda tangan.
    /// </summary>
    public class WorkflowApprovalActionService : IWorkflowApprovalActionService
    {
        public const string NotApproverMessage = "Anda bukan penyetuju tahap ini.";
        public const string OwnRequestMessage = "Pejabat tidak boleh menyetujui permohonan sendiri.";
        public const string InvalidReasonMessage = "Pilih alasan penolakan yang berlaku untuk permohonan ini.";
        public const string CommentRequiredMessage = "Alasan ini mewajibkan catatan.";
        public const string RevisionNotesRequiredMessage = "Isi catatan revisi untuk pemohon.";
        public const string StepNotFoundMessage = "Tahapan persetujuan tidak ditemukan.";
        public const string NotWaitingMessage = "Permohonan tidak lagi menunggu keputusan pada tahap ini.";
        public const string AlreadyProcessedMessage = "Penugasan Anda pada tahap ini sudah diproses.";

        private readonly ApplicationDbContext _context;
        private readonly WorkflowService _workflowService;
        private readonly IApprovalInboxQueryService _inboxQueryService;
        private readonly IUnifiedNotificationGatewayService _notificationGateway;
        private readonly ILogger<WorkflowApprovalActionService> _logger;

        public WorkflowApprovalActionService(
            ApplicationDbContext context,
            WorkflowService workflowService,
            IApprovalInboxQueryService inboxQueryService,
            IUnifiedNotificationGatewayService notificationGateway,
            ILogger<WorkflowApprovalActionService> logger)
        {
            _context = context;
            _workflowService = workflowService;
            _inboxQueryService = inboxQueryService;
            _notificationGateway = notificationGateway;
            _logger = logger;
        }

        public async Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> ApproveAsync(
            Guid stepInstanceId,
            ApproveActionRequestDto request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var target = await ResolveTargetAsync(stepInstanceId, currentUserId, cancellationToken);
            if (target.Failure != null)
            {
                return target.Failure;
            }

            var assignment = target.Assignment!;
            var result = await _workflowService.ApproveAsync(
                assignment.WorkflowInstanceId,
                assignment.AssignmentId,
                new WorkflowApproveRequest { Comment = NormalizeText(request?.Note) },
                cancellationToken);

            return await CompleteAsync(
                result,
                assignment,
                WorkflowService.ResolveApprovalActionType(assignment.StepType),
                "Permohonan berhasil disetujui.",
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> RejectAsync(
            Guid stepInstanceId,
            RejectActionRequestDto request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var target = await ResolveTargetAsync(stepInstanceId, currentUserId, cancellationToken);
            if (target.Failure != null)
            {
                return target.Failure;
            }

            var assignment = target.Assignment!;
            var comment = NormalizeText(request?.Comment);
            var reasonId = request?.RejectionReasonId ?? Guid.Empty;

            // VR-WF-26 / VR-WF-27: alasan dinilai dari daftar yang sama dengan myAction.rejectionReasons.
            var reasons = await WorkflowRejectionReasonOptions.QueryAsync(
                _context,
                assignment.DefinitionRequestType,
                assignment.WorkflowDefinitionId,
                assignment.WorkflowStepId,
                DateTime.UtcNow.Date,
                cancellationToken);
            var reason = reasonId == Guid.Empty ? null : reasons.FirstOrDefault(x => x.Id == reasonId);

            if (reason == null)
            {
                return Fail(StatusCodes.Status400BadRequest, InvalidReasonMessage);
            }

            if (reason.IsCommentRequired && comment == null)
            {
                return Fail(StatusCodes.Status400BadRequest, CommentRequiredMessage);
            }

            var result = await _workflowService.RejectAsync(
                assignment.WorkflowInstanceId,
                assignment.AssignmentId,
                new WorkflowRejectRequest { RejectionReasonId = reason.Id, Comment = comment },
                cancellationToken);

            return await CompleteAsync(
                result,
                assignment,
                WorkflowValueConstants.ActionType.Reject,
                $"Permohonan ditolak. Alasan: {ComposeReasonText(reason.ReasonName, comment)}",
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> ReturnForRevisionAsync(
            Guid stepInstanceId,
            ReturnForRevisionRequestDto request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var target = await ResolveTargetAsync(stepInstanceId, currentUserId, cancellationToken);
            if (target.Failure != null)
            {
                return target.Failure;
            }

            var notes = NormalizeText(request?.RevisionNotes);
            if (notes == null)
            {
                return Fail(StatusCodes.Status400BadRequest, RevisionNotesRequiredMessage);
            }

            var assignment = target.Assignment!;
            var result = await _workflowService.RequestRevisionAsync(
                assignment.WorkflowInstanceId,
                assignment.AssignmentId,
                new WorkflowRequestRevisionRequest { Comment = notes },
                cancellationToken);

            return await CompleteAsync(
                result,
                assignment,
                WorkflowValueConstants.ActionType.RequestRevision,
                $"Permohonan dikembalikan untuk revisi. Catatan: {notes}",
                cancellationToken);
        }

        /// <summary>
        /// Kontrak v29 §35.2: penugasan terbuka atas nama pengguna, atau milik pejabat yang mendelegasikan kepadanya secara
        /// aktif. Bila tidak ada, jawaban dibedakan: tahap tidak ada (<c>404</c>), permohonan tidak lagi menunggu atau
        /// penugasan pengguna sudah diproses (<c>409</c>), pengguna adalah pemohon atau bukan penyetuju (<c>403</c>).
        /// </summary>
        private async Task<ActionTarget> ResolveTargetAsync(Guid stepInstanceId, Guid currentUserId, CancellationToken cancellationToken)
        {
            if (currentUserId == Guid.Empty)
            {
                return ActionTarget.Fail(Fail(StatusCodes.Status401Unauthorized, "Sesi login tidak valid."));
            }

            var assignment = await _inboxQueryService.FindActionableAssignmentAsync(stepInstanceId, currentUserId, cancellationToken);
            if (assignment != null)
            {
                return ActionTarget.Ok(assignment);
            }

            var step = await _context.TrxWorkflowStepInstances
                .AsNoTracking()
                .Where(s => s.Id == stepInstanceId && !s.IsDelete && s.WorkflowInstance != null && !s.WorkflowInstance.IsDelete)
                .Select(s => new
                {
                    s.IsCurrentStep,
                    s.StepStatus,
                    s.WorkflowInstance!.WorkflowStatus,
                    s.WorkflowInstance.RequestedByUserId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (step == null)
            {
                return ActionTarget.Fail(Fail(StatusCodes.Status404NotFound, StepNotFoundMessage));
            }

            if (step.RequestedByUserId == currentUserId)
            {
                return ActionTarget.Fail(Fail(StatusCodes.Status403Forbidden, OwnRequestMessage));
            }

            var isWaiting = step.IsCurrentStep &&
                            step.StepStatus == WorkflowValueConstants.StepStatus.InProgress &&
                            step.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress;

            var heldAssignmentOnStep = await _context.TrxWorkflowApproverAssignments
                .AsNoTracking()
                .AnyAsync(a =>
                    a.WorkflowStepInstanceId == stepInstanceId &&
                    a.ApproverEmployeeId == null &&
                    a.AssignedApproverUserId == currentUserId &&
                    !a.IsDelete,
                    cancellationToken);

            if (heldAssignmentOnStep)
            {
                return ActionTarget.Fail(Fail(StatusCodes.Status409Conflict, isWaiting ? AlreadyProcessedMessage : NotWaitingMessage));
            }

            return ActionTarget.Fail(Fail(StatusCodes.Status403Forbidden, NotApproverMessage));
        }

        private async Task<WorkflowServiceResult<WorkflowActionV2ResponseDto>> CompleteAsync(
            WorkflowServiceResult<WorkflowInstanceDetailResponse> engineResult,
            WorkflowActionableAssignment assignment,
            string actionType,
            string requesterMessage,
            CancellationToken cancellationToken)
        {
            if (!engineResult.Success || engineResult.Data == null)
            {
                return Fail(engineResult.StatusCode, NormalizeEngineMessage(engineResult));
            }

            var detail = engineResult.Data;
            var stepStatus = await _context.TrxWorkflowStepInstances
                .AsNoTracking()
                .Where(s => s.Id == assignment.StepInstanceId)
                .Select(s => s.StepStatus)
                .FirstOrDefaultAsync(cancellationToken);

            await NotifyAsync(assignment, detail, actionType, requesterMessage, cancellationToken);

            return WorkflowServiceResult<WorkflowActionV2ResponseDto>.Ok(
                new WorkflowActionV2ResponseDto
                {
                    Success = true,
                    ActionType = actionType,
                    Message = engineResult.Message,
                    WorkflowInstanceId = detail.Id,
                    ResultingWorkflowStatus = detail.WorkflowStatus,
                    ResultingStepStatus = stepStatus ?? string.Empty,
                    ActionTimestamp = detail.LastActionAt ?? DateTime.UtcNow
                },
                engineResult.Message);
        }

        // Kontrak v29 §35.7: penolakan otorisasi engine memakai pesan kontrak; pesan lain diteruskan apa adanya.
        private static string NormalizeEngineMessage(WorkflowServiceResult<WorkflowInstanceDetailResponse> engineResult)
        {
            if (engineResult.StatusCode == StatusCodes.Status403Forbidden &&
                !string.Equals(engineResult.Message, OwnRequestMessage, StringComparison.Ordinal))
            {
                return NotApproverMessage;
            }

            return engineResult.Message;
        }

        /// <summary>
        /// WF-B7 / BE-HRD-314: pemohon diberi tahu hasil keputusan, dan penyetuju tahap berikutnya diberi tahu ada tugas baru.
        /// Keputusan sudah tersimpan di engine, sehingga kegagalan pemberitahuan hanya dicatat.
        /// </summary>
        private async Task NotifyAsync(
            WorkflowActionableAssignment assignment,
            WorkflowInstanceDetailResponse detail,
            string actionType,
            string requesterMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                var title = actionType switch
                {
                    WorkflowValueConstants.ActionType.Reject => "Permohonan ditolak",
                    WorkflowValueConstants.ActionType.RequestRevision => "Permohonan dikembalikan untuk revisi",
                    _ => "Permohonan disetujui"
                };

                if (assignment.RequestedByUserId != Guid.Empty)
                {
                    await _notificationGateway.SendInstantNotificationToUserAsync(
                        assignment.RequestedByUserId,
                        title,
                        $"{assignment.RequestNumber}: {requesterMessage}",
                        $"/hrd/workflow/my-requests/{assignment.WorkflowInstanceId}",
                        assignment.WorkflowInstanceId);
                }

                if (actionType == WorkflowValueConstants.ActionType.Reject ||
                    actionType == WorkflowValueConstants.ActionType.RequestRevision ||
                    detail.WorkflowStatus != WorkflowValueConstants.WorkflowStatus.InProgress)
                {
                    return;
                }

                var nextApprovers = await _context.TrxWorkflowApproverAssignments
                    .AsNoTracking()
                    .Where(a =>
                        a.WorkflowInstanceId == assignment.WorkflowInstanceId &&
                        a.WorkflowStepInstanceId != assignment.StepInstanceId &&
                        a.IsCurrentAssignment &&
                        a.ApproverEmployeeId == null &&
                        a.AssignedApproverUserId != Guid.Empty &&
                        a.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Available &&
                        !a.IsDelete &&
                        a.WorkflowStepInstance != null &&
                        a.WorkflowStepInstance.IsCurrentStep)
                    .Select(a => new { a.AssignedApproverUserId, a.DueAt })
                    .ToListAsync(cancellationToken);

                foreach (var next in nextApprovers)
                {
                    await _notificationGateway.SendInstantNotificationToUserAsync(
                        next.AssignedApproverUserId,
                        "Permohonan baru menunggu persetujuan Anda",
                        $"Permohonan {assignment.RequestNumber} ({assignment.ReferenceType}) masuk ke kotak persetujuan Anda." +
                        (next.DueAt.HasValue ? $" Batas SLA: {next.DueAt.Value:dd MMM yyyy HH:mm} UTC." : string.Empty),
                        null,
                        assignment.WorkflowInstanceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Pemberitahuan keputusan workflow {WorkflowInstanceId} gagal dikirim; keputusan tetap tersimpan.",
                    assignment.WorkflowInstanceId);
            }
        }

        private static string ComposeReasonText(string reasonName, string? comment)
        {
            return comment == null ? reasonName : $"{reasonName} — {comment}";
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static WorkflowServiceResult<WorkflowActionV2ResponseDto> Fail(int statusCode, string message)
        {
            return WorkflowServiceResult<WorkflowActionV2ResponseDto>.Fail(statusCode, message);
        }

        private sealed class ActionTarget
        {
            public WorkflowActionableAssignment? Assignment { get; private init; }
            public WorkflowServiceResult<WorkflowActionV2ResponseDto>? Failure { get; private init; }

            public static ActionTarget Ok(WorkflowActionableAssignment assignment) => new() { Assignment = assignment };

            public static ActionTarget Fail(WorkflowServiceResult<WorkflowActionV2ResponseDto> failure) => new() { Failure = failure };
        }
    }
}
