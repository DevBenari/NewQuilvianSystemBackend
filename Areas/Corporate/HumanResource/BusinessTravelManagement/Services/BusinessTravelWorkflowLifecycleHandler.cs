using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    /// <summary>
    /// Penangan siklus referensi Workflow V2 untuk SPPD (<c>INT-HR-109</c>, <c>INT-HR-110</c>; kontrak <c>v28</c>,
    /// BE-HRD-376). Menyalin hasil workflow ke status SPPD dan, saat disetujui, memasang presensi
    /// "Hadir Dinas Luar" serta gembok roster.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Workflow V2 memanggil penangan ini <b>sesudah</b> aksi workflow tersimpan dan mencatat galatnya tanpa
    /// menggagalkan aksi (keputusan pengguna 19 November 2026, pilihan (a)). SPPD yang tertinggal karena itu
    /// diselaraskan ulang oleh <see cref="BusinessTravelWorkflowReconciliationService"/>.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Status SPPD diturunkan ulang dari status workflow saat ini; bila sudah sama tidak ada yang
    /// diubah, sehingga presensi dan gembok hanya dipasang sekali. Status sesudah disetujui (uang muka dan seterusnya)
    /// serta status final (ditolak, dibatalkan) tidak pernah diturunkan. Instance lama yang bukan instance SPPD saat
    /// ini diabaikan.
    /// </para>
    /// </remarks>
    public sealed class BusinessTravelWorkflowLifecycleHandler : IWorkflowReferenceLifecycleHandler
    {
        private readonly ApplicationDbContext _context;
        private readonly ITravelAttendanceAndRosterLockService _lockService;
        private readonly ILogger<BusinessTravelWorkflowLifecycleHandler> _logger;

        public BusinessTravelWorkflowLifecycleHandler(
            ApplicationDbContext context,
            ITravelAttendanceAndRosterLockService lockService,
            ILogger<BusinessTravelWorkflowLifecycleHandler> logger)
        {
            _context = context;
            _lockService = lockService;
            _logger = logger;
        }

        public bool CanHandle(string referenceType) =>
            string.Equals(referenceType?.Trim(), BusinessTravelWorkflowCodes.ReferenceType, StringComparison.Ordinal);

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var travel = await _context.TrxBusinessTravelRequests
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);

            if (travel == null)
            {
                _logger.LogWarning("[TRV-WF] SPPD {ReferenceId} untuk workflow {WorkflowInstanceId} tidak ditemukan.", workflow.ReferenceId, workflow.Id);
                return Result(workflow, string.Empty, string.Empty, false, isHandled: false, "SPPD tidak ditemukan.");
            }

            var previous = travel.TravelStatus;
            if (travel.WorkflowInstanceId != workflow.Id)
            {
                return Result(workflow, previous, previous, false, isHandled: true, "Instance workflow ini bukan instance SPPD saat ini.");
            }

            var target = BusinessTravelWorkflowStatusMap.ToTravelStatus(workflow.WorkflowStatus);
            if (target == null || target == previous || !BusinessTravelWorkflowStatusMap.IsInApprovalPhase(previous))
            {
                if (target == previous && target != null)
                {
                    travel.CurrentApprovalStep = workflow.CurrentStepOrder;
                    if (_context.Entry(travel).State == EntityState.Modified)
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                return Result(workflow, previous, previous, false, isHandled: true, null);
            }

            var now = DateTime.UtcNow;
            travel.TravelStatus = target;
            travel.CurrentApprovalStep = workflow.CurrentStepOrder;
            travel.UpdateBy = actorUserId;
            travel.UpdateDateTime = now;

            Guid? approverUserId = null;
            switch (target)
            {
                case BusinessTravelRequestStatuses.Submitted:
                    travel.SubmittedAt ??= workflow.SubmittedAt ?? now;
                    travel.SubmittedByUserId ??= workflow.RequestedByUserId;
                    break;

                case BusinessTravelRequestStatuses.Approved:
                    approverUserId = await LastActorAsync(workflow.Id, cancellationToken,
                        WorkflowValueConstants.ActionType.Approve, WorkflowValueConstants.ActionType.AutoApprove) ?? actorUserId;
                    travel.ApprovedAt ??= workflow.CompletedAt ?? workflow.FinalizedAt ?? now;
                    travel.ApprovedByUserId ??= approverUserId;
                    // HRD-DEC-226: anggaran disetujui = total estimasi yang diajukan.
                    travel.ApprovedBudgetAmount = travel.EstimatedTotalAmount;
                    break;

                case BusinessTravelRequestStatuses.Rejected:
                    travel.RejectedAt ??= workflow.CompletedAt ?? workflow.FinalizedAt ?? now;
                    travel.RejectedByUserId ??= await LastActorAsync(workflow.Id, cancellationToken,
                        WorkflowValueConstants.ActionType.Reject, WorkflowValueConstants.ActionType.AutoReject) ?? actorUserId;
                    break;

                case BusinessTravelRequestStatuses.Cancelled:
                    travel.CancelledAt ??= workflow.WithdrawnAt ?? workflow.CancelledAt ?? now;
                    travel.CancelledByUserId ??= actorUserId;
                    break;
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (target == BusinessTravelRequestStatuses.Approved)
            {
                // INT-HR-110 / VR-TRV-32: presensi Dinas Luar dan gembok roster hanya saat SPPD disetujui.
                await _lockService.SyncAttendanceAndLockRosterAsync(travel.Id, approverUserId ?? actorUserId);
            }

            _logger.LogInformation("[TRV-WF] SPPD {Number}: {Previous} → {Current} (workflow {WorkflowInstanceId}).",
                travel.TravelRequestNumber, previous, target, workflow.Id);

            return Result(workflow, previous, target, true, isHandled: true, null);
        }

        private async Task<Guid?> LastActorAsync(Guid workflowInstanceId, CancellationToken cancellationToken, params string[] actionTypes) =>
            await _context.Set<TrxApprovalAction>()
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId && !x.IsDelete && actionTypes.Contains(x.ActionType))
                .OrderByDescending(x => x.ActionAt)
                .Select(x => x.ActualActionByUserId)
                .FirstOrDefaultAsync(cancellationToken);

        private static WorkflowReferenceLifecycleSynchronizationResult Result(
            TrxWorkflowInstance workflow, string previous, string current, bool changed, bool isHandled, string? warning) => new()
        {
            IsHandled = isHandled,
            WorkflowInstanceId = workflow.Id,
            ReferenceId = workflow.ReferenceId,
            WorkflowStatus = workflow.WorkflowStatus,
            PreviousReferenceStatus = previous,
            CurrentReferenceStatus = current,
            StatusChanged = changed,
            WarningMessage = warning
        };
    }

    /// <summary>
    /// Pemetaan status Workflow V2 → status SPPD (state-transition-matrix bagian 32.3, kontrak <c>v28</c>).
    /// </summary>
    public static class BusinessTravelWorkflowStatusMap
    {
        /// <summary>
        /// <c>null</c> berarti status workflow tidak memindahkan SPPD. Instance <c>Draft</c> (baru dibuat, belum
        /// dikirim) sengaja tidak memindahkan SPPD: SPPD baru menjadi <c>Submitted</c> saat Ajukan berhasil, sehingga
        /// Ajukan yang gagal tidak meninggalkan SPPD berstatus menunggu.
        /// </summary>
        public static string? ToTravelStatus(string? workflowStatus)
        {
            var s = workflowStatus?.Trim() ?? string.Empty;
            if (Eq(s, WorkflowValueConstants.WorkflowStatus.Submitted) || Eq(s, WorkflowValueConstants.WorkflowStatus.InProgress))
                return BusinessTravelRequestStatuses.Submitted;
            if (Eq(s, WorkflowValueConstants.WorkflowStatus.RevisionRequested) || Eq(s, WorkflowValueConstants.WorkflowStatus.Returned))
                return BusinessTravelRequestStatuses.NeedRevision;
            if (Eq(s, WorkflowValueConstants.WorkflowStatus.Approved) || Eq(s, WorkflowValueConstants.WorkflowStatus.Completed))
                return BusinessTravelRequestStatuses.Approved;
            if (Eq(s, WorkflowValueConstants.WorkflowStatus.Rejected))
                return BusinessTravelRequestStatuses.Rejected;
            if (Eq(s, WorkflowValueConstants.WorkflowStatus.Cancelled) || Eq(s, WorkflowValueConstants.WorkflowStatus.Withdrawn))
                return BusinessTravelRequestStatuses.Cancelled;
            return null;
        }

        /// <summary>Status SPPD yang masih dapat dipindahkan oleh workflow persetujuan.</summary>
        public static bool IsInApprovalPhase(string? travelStatus) =>
            travelStatus == BusinessTravelRequestStatuses.Draft ||
            travelStatus == BusinessTravelRequestStatuses.Submitted ||
            travelStatus == BusinessTravelRequestStatuses.NeedRevision;

        private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
