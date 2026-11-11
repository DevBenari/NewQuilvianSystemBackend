using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IWorkflowApprovalActionService
    {
        Task<WorkflowActionV2ResponseDto> ApproveAsync(Guid stepInstanceId, ApproveActionRequestDto request, Guid currentEmployeeId);
        Task<WorkflowActionV2ResponseDto> RejectAsync(Guid stepInstanceId, RejectActionRequestDto request, Guid currentEmployeeId);
        Task<WorkflowActionV2ResponseDto> ReturnForRevisionAsync(Guid stepInstanceId, ReturnForRevisionRequestDto request, Guid currentEmployeeId);
    }

    public class WorkflowApprovalActionService : IWorkflowApprovalActionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAntiSelfApprovalGuardService _antiSelfApprovalGuard;
        private readonly IIdempotentDownstreamExecutionService _downstreamService;
        private readonly IUnifiedNotificationGatewayService _notificationGateway;

        public WorkflowApprovalActionService(
            ApplicationDbContext context,
            IAntiSelfApprovalGuardService antiSelfApprovalGuard,
            IIdempotentDownstreamExecutionService downstreamService,
            IUnifiedNotificationGatewayService notificationGateway)
        {
            _context = context;
            _antiSelfApprovalGuard = antiSelfApprovalGuard;
            _downstreamService = downstreamService;
            _notificationGateway = notificationGateway;
        }

        public async Task<WorkflowActionV2ResponseDto> ApproveAsync(Guid stepInstanceId, ApproveActionRequestDto request, Guid currentEmployeeId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var step = await _context.TrxWorkflowStepInstances
                .Include(s => s.WorkflowInstance)
                .Include(s => s.ApproverAssignments)
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);

            if (step == null) throw new KeyNotFoundException("Tahapan persetujuan alur kerja tidak ditemukan");
            var instance = step.WorkflowInstance;
            if (instance == null) throw new InvalidOperationException("Instansi workflow tidak valid");

            EnsureStepIsActionable(step, instance);

            // Validasi Anti Self-Approval (HRD-DEC-183 / VR-WF-02)
            _antiSelfApprovalGuard.ValidateAntiSelfApproval(instance.RequestedByEmployeeId ?? Guid.Empty, currentEmployeeId);

            // Validasi hak otorisasi (apakah approver langsung atau Plh aktif)
            var assignment = FindActionableAssignment(step);
            var onBehalfOf = await EnsureActorIsAssignedApproverAsync(assignment, currentEmployeeId);

            // Hitung hash tanda tangan digital
            var sigHash = ComputeSignatureHash(currentEmployeeId, stepInstanceId, "APPROVE", request.PinSignature);

            var now = DateTime.UtcNow;

            // Catat log aksi otorisasi
            var actionLog = new TrxApprovalAction
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepInstanceId = step.Id,
                WorkflowApproverAssignmentId = assignment?.Id,
                ActionType = "Approve",
                ActionEmployeeId = currentEmployeeId,
                OnBehalfOfEmployeeId = onBehalfOf,
                Comment = request.Note,
                ReasonComment = request.Note,
                DigitalSignatureHash = sigHash,
                ActionAt = now,
                ActionSource = "WebPortal"
            };
            _context.TrxApprovalActions.Add(actionLog);

            // Perbarui status assignment dan step
            if (assignment != null)
            {
                assignment.AssignmentStatus = "Approved";
                assignment.CompletedAt = now;
            }

            step.StepStatus = "Approved";
            step.CompletedAt = now;
            step.IsCurrentStep = false;
            step.ApprovedActionCount += 1;

            // Periksa apakah ini step terakhir
            var allSteps = await _context.TrxWorkflowStepInstances
                .Where(s => s.WorkflowInstanceId == instance.Id)
                .OrderBy(s => s.StepOrder)
                .ToListAsync();

            var nextStep = allSteps.FirstOrDefault(s => s.StepOrder > step.StepOrder && s.StepStatus == "NotStarted");

            string finalWorkflowStatus = "Approved";

            if (nextStep != null)
            {
                // Masih ada tahapan berikutnya
                nextStep.StepStatus = "Pending";
                nextStep.IsCurrentStep = true;
                nextStep.StartedAt = now;
                nextStep.DueAt = now.AddHours(48);

                instance.CurrentStepOrder = nextStep.StepOrder;
                instance.CurrentStepCode = nextStep.StepCodeSnapshot;
                instance.WorkflowStatus = "InProgress";
                finalWorkflowStatus = "InProgress";

                // Resolve approver for next step if not assigned
                var existingAssign = await _context.TrxWorkflowApproverAssignments
                    .FirstOrDefaultAsync(a => a.WorkflowStepInstanceId == nextStep.Id);

                if (existingAssign == null)
                {
                    var approverId = await _antiSelfApprovalGuard.ResolveSkipLevelApproverAsync(
                        instance.RequestedByEmployeeId ?? Guid.Empty, instance.DepartmentId ?? Guid.Empty);

                    _context.TrxWorkflowApproverAssignments.Add(new TrxWorkflowApproverAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkflowInstanceId = instance.Id,
                        WorkflowStepInstanceId = nextStep.Id,
                        ApproverEmployeeId = approverId,
                        AssignmentStatus = "Pending",
                        IsCurrentAssignment = true,
                        AssignedAt = now,
                        DueAt = nextStep.DueAt
                    });
                }
            }
            else
            {
                // Tahapan final disetujui
                instance.WorkflowStatus = "Approved";
                instance.CompletedAt = now;
                instance.FinalizedAt = now;
            }

            // Audit Trail
            _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                FromWorkflowStatus = "Pending",
                ToWorkflowStatus = finalWorkflowStatus,
                ChangedAt = now,
                Comment = $"Disetujui pada tahapan {step.StepNameSnapshot}"
            });

            await _context.SaveChangesAsync();

            // WF-B8 / INT-HR-93: eksekusi hilir sesudah keputusan tersimpan, karena handler membaca
            // instance dari database. Dijalankan juga pada tahapan antara, sama seperti mesin utama,
            // supaya transaksi sumber mengikuti tahapan yang sedang berjalan.
            await _downstreamService.ExecuteDownstreamActionsAsync(instance.Id);

            // WF-B7 / BE-HRD-314: pemohon diberi tahu hasil tahapan; approver tahapan berikutnya diberi tahu ada tiket baru
            await NotifyRequesterAsync(instance, nextStep != null
                ? $"Permohonan {instance.RequestNumber} disetujui pada tahapan {step.StepNameSnapshot} dan diteruskan ke pejabat berikutnya."
                : $"Permohonan {instance.RequestNumber} telah disetujui penuh.", "Permohonan disetujui");
            if (nextStep != null)
            {
                await NotifyStepApproverAsync(instance, nextStep.Id);
            }

            return new WorkflowActionV2ResponseDto
            {
                Success = true,
                ActionType = "Approve",
                Message = nextStep != null ? "Tahapan berhasil disetujui, diteruskan ke pejabat berikutnya" : "Permohonan berhasil disetujui penuh (Final)",
                WorkflowInstanceId = instance.Id,
                ResultingWorkflowStatus = instance.WorkflowStatus,
                ResultingStepStatus = step.StepStatus,
                ActionTimestamp = now
            };
        }

        public async Task<WorkflowActionV2ResponseDto> RejectAsync(Guid stepInstanceId, RejectActionRequestDto request, Guid currentEmployeeId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validasi VR-WF-05: Alasan penolakan minimal 15 karakter
            if (string.IsNullOrWhiteSpace(request.RejectReason) || request.RejectReason.Trim().Length < 15)
            {
                throw new InvalidOperationException("Alasan penolakan wajib diisi minimal 15 karakter untuk transparansi audit staf (VR-WF-05)");
            }

            var step = await _context.TrxWorkflowStepInstances
                .Include(s => s.WorkflowInstance)
                .Include(s => s.ApproverAssignments)
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);

            if (step == null) throw new KeyNotFoundException("Tahapan persetujuan alur kerja tidak ditemukan");
            var instance = step.WorkflowInstance;
            if (instance == null) throw new InvalidOperationException("Instansi workflow tidak valid");

            EnsureStepIsActionable(step, instance);

            _antiSelfApprovalGuard.ValidateAntiSelfApproval(instance.RequestedByEmployeeId ?? Guid.Empty, currentEmployeeId);
            await EnsureActorIsAssignedApproverAsync(FindActionableAssignment(step), currentEmployeeId);

            var now = DateTime.UtcNow;
            var sigHash = ComputeSignatureHash(currentEmployeeId, stepInstanceId, "REJECT", request.PinSignature);

            var actionLog = new TrxApprovalAction
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepInstanceId = step.Id,
                ActionType = "Reject",
                ActionEmployeeId = currentEmployeeId,
                Comment = request.RejectReason,
                ReasonComment = request.RejectReason,
                DigitalSignatureHash = sigHash,
                ActionAt = now,
                ActionSource = "WebPortal"
            };
            _context.TrxApprovalActions.Add(actionLog);

            step.StepStatus = "Rejected";
            step.CompletedAt = now;
            step.IsCurrentStep = false;
            step.RejectedActionCount += 1;

            var assignment = FindActionableAssignment(step);
            if (assignment != null)
            {
                assignment.AssignmentStatus = "Rejected";
                assignment.CompletedAt = now;
            }

            instance.WorkflowStatus = "Rejected";
            instance.CompletedAt = now;
            instance.CompletionNote = request.RejectReason;

            _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                FromWorkflowStatus = "Pending",
                ToWorkflowStatus = "Rejected",
                ChangedAt = now,
                Comment = $"Ditolak: {request.RejectReason}"
            });

            await _context.SaveChangesAsync();

            // WF-B8: penolakan juga diteruskan, supaya transaksi sumber (misalnya permohonan cuti)
            // ikut berstatus ditolak dan tidak tertahan sebagai menunggu persetujuan.
            await _downstreamService.ExecuteDownstreamActionsAsync(instance.Id);

            await NotifyRequesterAsync(instance, $"Permohonan {instance.RequestNumber} ditolak. Alasan: {request.RejectReason}", "Permohonan ditolak");

            return new WorkflowActionV2ResponseDto
            {
                Success = true,
                ActionType = "Reject",
                Message = "Permohonan resmi ditolak dengan alasan tercatat di audit log",
                WorkflowInstanceId = instance.Id,
                ResultingWorkflowStatus = "Rejected",
                ResultingStepStatus = "Rejected",
                ActionTimestamp = now
            };
        }

        public async Task<WorkflowActionV2ResponseDto> ReturnForRevisionAsync(Guid stepInstanceId, ReturnForRevisionRequestDto request, Guid currentEmployeeId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validasi HRD-DEC-186: Catatan revisi minimal 10 karakter
            if (string.IsNullOrWhiteSpace(request.RevisionNotes) || request.RevisionNotes.Trim().Length < 10)
            {
                throw new InvalidOperationException("Catatan perbaikan revisi wajib minimal 10 karakter (HRD-DEC-186)");
            }

            var step = await _context.TrxWorkflowStepInstances
                .Include(s => s.WorkflowInstance)
                .Include(s => s.ApproverAssignments)
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);

            if (step == null) throw new KeyNotFoundException("Tahapan persetujuan alur kerja tidak ditemukan");
            var instance = step.WorkflowInstance;
            if (instance == null) throw new InvalidOperationException("Instansi workflow tidak valid");

            EnsureStepIsActionable(step, instance);
            await EnsureActorIsAssignedApproverAsync(FindActionableAssignment(step), currentEmployeeId);

            var now = DateTime.UtcNow;

            var actionLog = new TrxApprovalAction
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepInstanceId = step.Id,
                ActionType = "ReturnForRevision",
                ActionEmployeeId = currentEmployeeId,
                Comment = request.RevisionNotes,
                ReasonComment = request.RevisionNotes,
                ActionAt = now,
                ActionSource = "WebPortal"
            };
            _context.TrxApprovalActions.Add(actionLog);

            step.StepStatus = "ReturnedForRevision";
            instance.WorkflowStatus = "ReturnedForRevision";
            instance.CompletionNote = request.RevisionNotes;

            // Catat komentar perbaikan ke thread diskusi
            _context.TrxWorkflowComments.Add(new TrxWorkflowComment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                CommentByUserId = currentEmployeeId,
                CommentText = $"[PERMINTAAN REVISI DRAF]: {request.RevisionNotes}",
                IsInternalComment = false,
                CommentedAt = now
            });

            _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                FromWorkflowStatus = "Pending",
                ToWorkflowStatus = "ReturnedForRevision",
                ChangedAt = now,
                Comment = request.RevisionNotes
            });

            await _context.SaveChangesAsync();

            await NotifyRequesterAsync(instance, $"Permohonan {instance.RequestNumber} dikembalikan untuk perbaikan draf (batas 7 hari). Catatan: {request.RevisionNotes}", "Permohonan dikembalikan untuk revisi");

            return new WorkflowActionV2ResponseDto
            {
                Success = true,
                ActionType = "ReturnForRevision",
                Message = "Permohonan dikembalikan ke staf pemohon untuk perbaikan draf (Batas waktu 7 hari)",
                WorkflowInstanceId = instance.Id,
                ResultingWorkflowStatus = "ReturnedForRevision",
                ResultingStepStatus = "ReturnedForRevision",
                ActionTimestamp = now
            };
        }

        private static TrxWorkflowApproverAssignment? FindActionableAssignment(TrxWorkflowStepInstance step)
        {
            return step.ApproverAssignments.FirstOrDefault(a => ActionableStepStatuses.Contains(a.AssignmentStatus, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Hanya approver yang ditugaskan pada tahapan atau Plh aktifnya yang boleh memutus. Mengembalikan pejabat asal bila aktor adalah Plh.
        /// </summary>
        /// <remarks>
        /// Penugasan dicocokkan lewat ID karyawan. Penugasan dari mesin workflow utama hanya mengisi akun approver,
        /// sehingga pada keadaan itu yang dicocokkan akun pengguna.
        /// </remarks>
        private async Task<Guid?> EnsureActorIsAssignedApproverAsync(TrxWorkflowApproverAssignment? assignment, Guid currentEmployeeId)
        {
            if (assignment == null)
            {
                throw new UnauthorizedAccessException("Tahapan ini tidak memiliki penugasan approver yang aktif.");
            }

            if (assignment.ApproverEmployeeId.HasValue && assignment.ApproverEmployeeId.Value != Guid.Empty)
            {
                if (assignment.ApproverEmployeeId.Value == currentEmployeeId) return null;

                var isDelegate = await _context.TrxApprovalDelegations
                    .AnyAsync(d => d.DelegateEmployeeId == currentEmployeeId &&
                                   d.PrincipalEmployeeId == assignment.ApproverEmployeeId &&
                                   d.IsActive);
                if (isDelegate) return assignment.ApproverEmployeeId;
            }
            else
            {
                var isAssignedAccount = await _context.Users.AsNoTracking()
                    .AnyAsync(u => u.Id == assignment.AssignedApproverUserId && (u.Id == currentEmployeeId || u.EmployeeId == currentEmployeeId));
                if (isAssignedAccount) return null;
            }

            throw new UnauthorizedAccessException("Anda bukan approver yang ditugaskan pada tahapan ini dan tidak memegang delegasi Plh yang aktif.");
        }

        private static readonly string[] ActionableStepStatuses =
        {
            WorkflowValueConstants.StepStatus.Pending,
            WorkflowValueConstants.StepStatus.Available,
            WorkflowValueConstants.StepStatus.InProgress
        };

        private static readonly string[] TerminalWorkflowStatuses =
        {
            WorkflowValueConstants.WorkflowStatus.Approved,
            WorkflowValueConstants.WorkflowStatus.Rejected,
            WorkflowValueConstants.WorkflowStatus.Cancelled,
            WorkflowValueConstants.WorkflowStatus.Withdrawn,
            WorkflowValueConstants.WorkflowStatus.Completed
        };

        /// <summary>
        /// WF-B8: satu tahapan hanya boleh diputus sekali.
        /// </summary>
        /// <remarks>
        /// Tanpa penjaga ini, tahapan terakhir yang sudah disetujui dapat disetujui lagi — misalnya
        /// karena tombol ditekan dua kali — sehingga jejak aksi, riwayat status, dan eksekusi hilir
        /// tercatat ganda. Permohonan yang sudah ditolak juga dapat "disetujui" sesudahnya.
        /// <c>InProgress</c> diterima karena itulah status tahapan aktif pada instance yang dibuat
        /// mesin workflow utama.
        /// </remarks>
        private static void EnsureStepIsActionable(TrxWorkflowStepInstance step, TrxWorkflowInstance instance)
        {
            var isTerminal = TerminalWorkflowStatuses.Contains(instance.WorkflowStatus, StringComparer.OrdinalIgnoreCase);
            var isActiveStep = step.IsCurrentStep &&
                               ActionableStepStatuses.Contains(step.StepStatus, StringComparer.OrdinalIgnoreCase);

            if (isTerminal || !isActiveStep)
            {
                throw new InvalidOperationException("Tahapan persetujuan belum aktif atau sudah diproses.");
            }
        }

        /// <summary>WF-B7: pemberitahuan instan ke pemohon. Akun pemohon selalu tercatat pada instance (RequestedByUserId).</summary>
        private async Task NotifyRequesterAsync(TrxWorkflowInstance instance, string message, string title)
        {
            if (instance.RequestedByUserId != Guid.Empty)
            {
                await _notificationGateway.SendInstantNotificationToUserAsync(
                    instance.RequestedByUserId, title, message, $"/hrd/workflow/my-requests/{instance.Id}", instance.Id);
            }
            else if (instance.RequestedByEmployeeId.HasValue)
            {
                await _notificationGateway.SendInstantNotificationAsync(instance.RequestedByEmployeeId.Value, title, message);
            }
        }

        /// <summary>WF-B7: pemberitahuan instan ke approver tahapan yang baru aktif.</summary>
        private async Task NotifyStepApproverAsync(TrxWorkflowInstance instance, Guid stepInstanceId)
        {
            var assignment = await _context.TrxWorkflowApproverAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.WorkflowStepInstanceId == stepInstanceId && a.AssignmentStatus == "Pending");
            if (assignment == null) return;

            var title = "Permohonan baru menunggu persetujuan Anda";
            var message = $"Permohonan {instance.RequestNumber} ({instance.ReferenceType}) masuk ke kotak persetujuan Anda. Batas SLA: {assignment.DueAt:dd MMM yyyy HH:mm} UTC.";
            if (assignment.ApproverEmployeeId.HasValue && assignment.ApproverEmployeeId.Value != Guid.Empty)
            {
                await _notificationGateway.SendInstantNotificationAsync(assignment.ApproverEmployeeId.Value, title, message);
            }
            else if (assignment.AssignedApproverUserId != Guid.Empty)
            {
                await _notificationGateway.SendInstantNotificationToUserAsync(assignment.AssignedApproverUserId, title, message, null, instance.Id);
            }
        }

        private static string ComputeSignatureHash(Guid employeeId, Guid stepId, string action, string pin)
        {
            var raw = $"{employeeId}:{stepId}:{action}:{pin}:{DateTime.UtcNow:yyyyMMdd}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }
    }
}
