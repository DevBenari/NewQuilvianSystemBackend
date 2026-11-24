using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    // BE-HRD-442 (kontrak v29 state 33.2–33.4, VR-WF-31, VR-WF-11; HRD-DEC-232, HRD-DEC-185): eskalasi SLA menambahkan penugasan
    // atasan sebagai penyetuju SETARA pada tahap yang sama. Penyetuju awal tetap boleh memutuskan; siapa pun yang memutuskan lebih
    // dulu menutup kembarannya; tidak pernah ada persetujuan otomatis.
    public partial class WorkflowService
    {
        public const string EscalationNotAllowedTargetMessage = "Pejabat tujuan eskalasi tidak sah.";

        /// <summary>
        /// Menambahkan penugasan eskalasi untuk <paramref name="targetApproverUserId"/> pada tahap penugasan
        /// <paramref name="assignmentId"/>. Penugasan baru: <c>Available</c>, <c>IsRequired = false</c>,
        /// <c>EscalatedFromAssignmentId</c> = penugasan asal, <c>DueAt</c> baru dari tahap master, aksi <c>Escalate</c>.
        /// Eskalasi otomatis (<paramref name="isSystem"/>) hanya untuk penugasan yang sudah lewat <c>DueAt</c>.
        /// </summary>
        public async Task<WorkflowServiceResult<Guid>> AddEscalationAssignmentAsync(
            Guid assignmentId,
            Guid targetApproverUserId,
            string? reason,
            bool isSystem,
            Guid? actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (assignmentId == Guid.Empty || targetApproverUserId == Guid.Empty)
            {
                return WorkflowServiceResult<Guid>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Penugasan atau pejabat tujuan eskalasi tidak valid.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var workflowInstanceId = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                    .AsNoTracking()
                    .Where(x => x.Id == assignmentId && !x.IsDelete && !x.IsCancel && x.IsActive)
                    .Select(x => (Guid?)x.WorkflowInstanceId)
                    .FirstOrDefaultAsync(cancellationToken);

                var instance = workflowInstanceId.HasValue
                    ? await LoadTrackedInstanceAsync(workflowInstanceId.Value, cancellationToken)
                    : null;

                var origin = instance?.StepInstances
                    .SelectMany(x => x.ApproverAssignments)
                    .FirstOrDefault(x => x.Id == assignmentId);

                if (instance == null || origin == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<Guid>.Fail(
                        StatusCodes.Status404NotFound,
                        "Approver assignment tidak ditemukan.");
                }

                var step = instance.StepInstances.First(x => x.Id == origin.WorkflowStepInstanceId);
                var now = DateTime.UtcNow;

                var invalid = await ValidateEscalationAsync(
                    instance, step, origin, targetApproverUserId, isSystem, now, cancellationToken);
                if (invalid != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<Guid>.Fail(invalid.Value.StatusCode, invalid.Value.Message);
                }

                var target = await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == targetApproverUserId)
                    .Select(x => new { x.Id, x.WorkforceProfileId, Name = x.DisplayName ?? x.UserName ?? x.Email })
                    .FirstAsync(cancellationToken);

                var dueAfterHours = await _dbContext.Set<MstWorkflowStep>()
                    .AsNoTracking()
                    .Where(x => x.Id == step.WorkflowStepId)
                    .Select(x => x.EscalationAfterHours ?? x.ReminderAfterHours)
                    .FirstOrDefaultAsync(cancellationToken);

                var actor = actorUserId ?? Guid.Empty;
                var escalation = new TrxWorkflowApproverAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = step.Id,
                    ApprovalMatrixId = origin.ApprovalMatrixId,
                    AssignedApproverUserId = target.Id,
                    AssignedApproverWorkforceProfileId = target.WorkforceProfileId,
                    ApproverSourceSnapshot = origin.ApproverSourceSnapshot,
                    AssignmentOrder = step.ApproverAssignments.Where(x => !x.IsDelete).Select(x => x.AssignmentOrder).DefaultIfEmpty(0).Max() + 1,
                    AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Available,
                    AssignedAt = now,
                    AvailableAt = now,
                    DueAt = dueAfterHours.HasValue && dueAfterHours.Value > 0 ? now.AddHours(dueAfterHours.Value) : null,
                    IsRequired = false,
                    IsCurrentAssignment = true,
                    IsDelegated = false,
                    EscalatedFromAssignmentId = origin.Id,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor
                };

                // Atasan yang sedang ber-Plh: penugasan langsung dialihkan, sama seperti penugasan engine lain.
                await ApplyDelegationAsync(escalation, instance.WorkflowDefinitionId, step.WorkflowStepId, now, cancellationToken);

                _dbContext.Set<TrxWorkflowApproverAssignment>().Add(escalation);
                step.TotalAssignmentCount += 1;
                step.UpdateDateTime = now;
                step.UpdateBy = actor;
                instance.LastActionAt = now;
                instance.UpdateDateTime = now;
                instance.UpdateBy = actor;

                var dueText = origin.DueAt.HasValue ? origin.DueAt.Value.ToString("yyyy-MM-dd HH:mm") + " UTC" : "-";
                _dbContext.Set<TrxApprovalAction>().Add(new TrxApprovalAction
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = step.Id,
                    WorkflowApproverAssignmentId = origin.Id,
                    AssignedApproverUserId = escalation.AssignedApproverUserId,
                    AssignedApproverWorkforceProfileId = escalation.AssignedApproverWorkforceProfileId,
                    ActualActionByUserId = isSystem ? null : actorUserId,
                    ActionType = WorkflowValueConstants.ActionType.Escalate,
                    ActionAt = now,
                    Comment = NormalizeOptionalText(reason) ?? $"Dieskalasi ke {target.Name}: lewat batas waktu {dueText}.",
                    IsDelegated = false,
                    IsSystemAction = isSystem,
                    ActionSource = isSystem ? WorkflowValueConstants.SourceChannel.Scheduler : WorkflowValueConstants.SourceChannel.Web,
                    PreviousWorkflowStatus = instance.WorkflowStatus,
                    ResultingWorkflowStatus = instance.WorkflowStatus,
                    PreviousStepStatus = step.StepStatus,
                    ResultingStepStatus = step.StepStatus,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return WorkflowServiceResult<Guid>.Ok(escalation.Id, "Penugasan eskalasi berhasil ditambahkan.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // VR-WF-31: hanya penugasan terbuka pada tahap aktif, sekali per penugasan, ke pejabat aktif yang bukan pemohon dan belum
        // menjadi penyetuju pada tahap itu (indeks unik tahap + pengguna); eskalasi otomatis hanya sesudah DueAt lewat.
        private async Task<(int StatusCode, string Message)?> ValidateEscalationAsync(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment origin,
            Guid targetApproverUserId,
            bool isSystem,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(instance.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase) ||
                !step.IsCurrentStep ||
                !string.Equals(step.StepStatus, WorkflowValueConstants.StepStatus.InProgress, StringComparison.OrdinalIgnoreCase) ||
                !origin.IsCurrentAssignment ||
                !IsAvailableAssignmentStatus(origin.AssignmentStatus))
            {
                return (StatusCodes.Status409Conflict, "Penugasan tidak lagi menunggu keputusan sehingga tidak dapat dieskalasi.");
            }

            if (!origin.IsRequired && !origin.EscalatedFromAssignmentId.HasValue)
            {
                return (StatusCodes.Status409Conflict, "Hanya penugasan wajib yang dapat dieskalasi.");
            }

            if (step.ApproverAssignments.Any(x => !x.IsDelete && x.EscalatedFromAssignmentId == origin.Id))
            {
                return (StatusCodes.Status409Conflict, "Penugasan ini sudah pernah dieskalasi.");
            }

            if (isSystem && (!origin.DueAt.HasValue || origin.DueAt.Value > now))
            {
                return (StatusCodes.Status409Conflict, "Penugasan belum melewati batas waktu.");
            }

            var targetActive = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == targetApproverUserId && x.IsActive, cancellationToken);
            if (!targetActive || targetApproverUserId == instance.RequestedByUserId)
            {
                return (StatusCodes.Status400BadRequest, EscalationNotAllowedTargetMessage);
            }

            if (step.ApproverAssignments.Any(x => !x.IsDelete && x.AssignedApproverUserId == targetApproverUserId))
            {
                return (StatusCodes.Status409Conflict, "Pejabat tujuan sudah menjadi penyetuju pada tahap ini.");
            }

            return null;
        }

        /// <summary>Penugasan asal (akar) dari rantai eskalasi.</summary>
        private static TrxWorkflowApproverAssignment EscalationRoot(
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment assignment)
        {
            var current = assignment;
            var visited = new HashSet<Guid> { current.Id };
            while (current.EscalatedFromAssignmentId.HasValue)
            {
                var parent = step.ApproverAssignments.FirstOrDefault(x => x.Id == current.EscalatedFromAssignmentId.Value);
                if (parent == null || !visited.Add(parent.Id))
                {
                    break;
                }

                current = parent;
            }

            return current;
        }

        /// <summary>Akar dan seluruh turunan eskalasinya.</summary>
        private static List<TrxWorkflowApproverAssignment> EscalationGroup(
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment root)
        {
            var group = new List<TrxWorkflowApproverAssignment> { root };
            var seen = new HashSet<Guid> { root.Id };
            for (var i = 0; i < group.Count; i++)
            {
                var parentId = group[i].Id;
                foreach (var child in step.ApproverAssignments.Where(x => x.EscalatedFromAssignmentId == parentId && seen.Add(x.Id)))
                {
                    group.Add(child);
                }
            }

            return group;
        }

        /// <summary>
        /// State 33.3: penugasan wajib dihitung disetujui bila ia atau salah satu turunan eskalasinya berstatus Approved.
        /// </summary>
        private static bool IsRequiredSlotApproved(
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment required)
        {
            return EscalationGroup(step, required).Any(x =>
                x.IsActive &&
                !x.IsDelete &&
                !x.IsCancel &&
                string.Equals(x.AssignmentStatus, WorkflowValueConstants.AssignmentStatus.Approved, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// State 33.2: sesudah satu anggota kelompok eskalasi menyetujui, anggota lain yang masih terbuka ditutup Skipped dengan aksi
        /// sistem Skip, agar tidak ada keputusan ganda pada slot yang sama.
        /// </summary>
        private void SkipEscalationTwins(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment acting,
            HumanResourceUserContextDto actorContext,
            DateTime now)
        {
            var twins = EscalationGroup(step, EscalationRoot(step, acting))
                .Where(x =>
                    x.Id != acting.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    !IsTerminalAssignmentStatus(x.AssignmentStatus))
                .ToList();

            foreach (var twin in twins)
            {
                var previousStepStatus = step.StepStatus;
                twin.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Skipped;
                twin.CompletedAt = now;
                twin.IsCurrentAssignment = false;
                twin.UpdateDateTime = now;
                twin.UpdateBy = actorContext.UserId;

                _dbContext.Set<TrxApprovalAction>().Add(new TrxApprovalAction
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = step.Id,
                    WorkflowApproverAssignmentId = twin.Id,
                    AssignedApproverUserId = twin.AssignedApproverUserId,
                    AssignedApproverWorkforceProfileId = twin.AssignedApproverWorkforceProfileId,
                    ActionType = WorkflowValueConstants.ActionType.Skip,
                    ActionAt = now,
                    Comment = $"Ditutup karena {actorContext.DisplayName} sudah memutuskan.",
                    IsDelegated = false,
                    IsSystemAction = true,
                    ActionSource = WorkflowValueConstants.SourceChannel.System,
                    PreviousWorkflowStatus = instance.WorkflowStatus,
                    ResultingWorkflowStatus = instance.WorkflowStatus,
                    PreviousStepStatus = previousStepStatus,
                    ResultingStepStatus = step.StepStatus,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorContext.UserId
                });
            }
        }
    }
}
