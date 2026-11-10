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

        public WorkflowApprovalActionService(
            ApplicationDbContext context,
            IAntiSelfApprovalGuardService antiSelfApprovalGuard,
            IIdempotentDownstreamExecutionService downstreamService)
        {
            _context = context;
            _antiSelfApprovalGuard = antiSelfApprovalGuard;
            _downstreamService = downstreamService;
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
            var assignment = step.ApproverAssignments.FirstOrDefault(a => a.AssignmentStatus == "Pending");
            Guid? onBehalfOf = null;
            if (assignment != null && assignment.ApproverEmployeeId != currentEmployeeId)
            {
                // Cek delegasi Plh
                var delegation = await _context.TrxApprovalDelegations
                    .Where(d => d.DelegateEmployeeId == currentEmployeeId &&
                                d.PrincipalEmployeeId == assignment.ApproverEmployeeId &&
                                d.IsActive)
                    .FirstOrDefaultAsync();

                if (delegation != null)
                {
                    onBehalfOf = assignment.ApproverEmployeeId;
                }
            }

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

            var assignment = step.ApproverAssignments.FirstOrDefault(a => a.AssignmentStatus == "Pending");
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
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);

            if (step == null) throw new KeyNotFoundException("Tahapan persetujuan alur kerja tidak ditemukan");
            var instance = step.WorkflowInstance;
            if (instance == null) throw new InvalidOperationException("Instansi workflow tidak valid");

            EnsureStepIsActionable(step, instance);

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

        private static string ComputeSignatureHash(Guid employeeId, Guid stepId, string action, string pin)
        {
            var raw = $"{employeeId}:{stepId}:{action}:{pin}:{DateTime.UtcNow:yyyyMMdd}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }
    }
}
