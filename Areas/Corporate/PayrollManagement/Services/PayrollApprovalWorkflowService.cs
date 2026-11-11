using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollApprovalWorkflowService : IPayrollApprovalWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayrollVarianceResolutionService _varianceResolutionService;
        private readonly ILogger<PayrollApprovalWorkflowService> _logger;

        public PayrollApprovalWorkflowService(
            ApplicationDbContext context,
            IPayrollVarianceResolutionService varianceResolutionService,
            ILogger<PayrollApprovalWorkflowService> logger)
        {
            _context = context;
            _varianceResolutionService = varianceResolutionService;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<PayrollApprovalResultDto>> SubmitRunForReviewAsync(
            Guid runId,
            SubmitPayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai Tier 1 Submit batch penggajian ID: {RunId} oleh Payroll Officer {ActorId}",
                runId, actorUserId);

            var run = await _context.PrlPayrollRuns
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.IsLocked)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {run.RunNumber} telah terkunci dan tidak dapat disubmit ulang.");
            }

            if (run.RunStatus is not ("Draft" or "Calculating" or "Calculated" or "NeedCorrection"))
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian berstatus '{run.RunStatus}' tidak dapat diajukan untuk review (harus Draft atau NeedCorrection).");
            }

            // Gerbang Variansi Keras (PAY-B3, VR-PAY-14): Menolak submit jika ada variansi yang belum terjustifikasi
            var varianceCheck = await _varianceResolutionService.CheckBatchCanBeSubmittedAsync(runId, cancellationToken);
            if (!varianceCheck.Success || !varianceCheck.Data!.CanBeSubmitted)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    varianceCheck.Data?.Message ?? "Pengajuan batch diblokir keras karena terdapat alert variansi gaji yang belum dijustifikasi (PAY-B3).");
            }

            var previousStatus = run.RunStatus;
            run.RunStatus = "WaitingHrReview";
            run.SubmittedAt = DateTime.UtcNow;
            run.SubmittedByUserId = actorUserId;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            var approvalEntry = new PrlPayrollApproval
            {
                Id = Guid.NewGuid(),
                PayrollRunId = run.Id,
                StepOrder = 1,
                ApprovalLevel = "PayrollOfficer",
                ApprovalStatus = "Submitted",
                ActionType = "Submit",
                ActionAt = DateTime.UtcNow,
                ActualActionByUserId = actorUserId,
                Comments = command.Comments ?? "Batch diajukan oleh Payroll Officer untuk review HR Manager.",
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.PrlPayrollApprovals.Add(approvalEntry);
            await _context.SaveChangesAsync(cancellationToken);

            var actorUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);

            _logger.LogInformation(
                "Batch {RunNumber} berhasil disubmit ke Tier 2 (WaitingHrReview) oleh Officer {UserName}.",
                run.RunNumber, actorUser?.UserName);

            return PayrollServiceResult<PayrollApprovalResultDto>.Ok(
                new PayrollApprovalResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    PreviousStatus = previousStatus,
                    CurrentStatus = run.RunStatus,
                    ApprovalTierLevel = "Tier 1: Payroll Officer",
                    ActionTaken = "Submit",
                    ActorUserId = actorUserId,
                    ActorUserName = actorUser?.UserName,
                    ActionTimestamp = approvalEntry.ActionAt.Value,
                    IsLocked = run.IsLocked,
                    IsApproved = false,
                    Comments = approvalEntry.Comments,
                    Message = $"Batch penggajian {run.RunNumber} berhasil diajukan ke HR Manager untuk review."
                },
                "Pengajuan batch berhasil.");
        }

        public async Task<PayrollServiceResult<PayrollApprovalResultDto>> ReviewRunAsync(
            Guid runId,
            ReviewPayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai Tier 2 Review batch penggajian ID: {RunId} oleh HR Manager {ActorId}",
                runId, actorUserId);

            var run = await _context.PrlPayrollRuns
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.RunStatus != "WaitingHrReview")
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian saat ini berstatus '{run.RunStatus}', bukan 'WaitingHrReview'.");
            }

            // Penegakan Keras Aturan Anti-Self-Approval (PAY-B4, VR-PAY-08):
            // Approver dilarang mereview batch yang dia inisiasi/submit sendiri
            if (run.SubmittedByUserId.HasValue && run.SubmittedByUserId.Value == actorUserId)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Pelanggaran Segregation of Duties (PAY-B4): Approver dilarang mereview batch penggajian yang disubmit oleh dirinya sendiri (VR-PAY-08). Wajib direview oleh pejabat HR yang berbeda.");
            }

            var previousStatus = run.RunStatus;
            run.RunStatus = "WaitingFinalApproval";
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            var approvalEntry = new PrlPayrollApproval
            {
                Id = Guid.NewGuid(),
                PayrollRunId = run.Id,
                StepOrder = 2,
                ApprovalLevel = "HrManager",
                ApprovalStatus = "Approved",
                ActionType = "Review",
                ActionAt = DateTime.UtcNow,
                ActualActionByUserId = actorUserId,
                Comments = command.Comments ?? "Batch telah diverifikasi oleh HR Manager dan diteruskan ke Direktur untuk Final Approval.",
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.PrlPayrollApprovals.Add(approvalEntry);
            await _context.SaveChangesAsync(cancellationToken);

            var actorUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);

            _logger.LogInformation(
                "Batch {RunNumber} berhasil di-review oleh HR Manager {UserName} dan siap untuk persetujuan Direktur.",
                run.RunNumber, actorUser?.UserName);

            return PayrollServiceResult<PayrollApprovalResultDto>.Ok(
                new PayrollApprovalResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    PreviousStatus = previousStatus,
                    CurrentStatus = run.RunStatus,
                    ApprovalTierLevel = "Tier 2: HR Manager",
                    ActionTaken = "Review",
                    ActorUserId = actorUserId,
                    ActorUserName = actorUser?.UserName,
                    ActionTimestamp = approvalEntry.ActionAt.Value,
                    IsLocked = run.IsLocked,
                    IsApproved = false,
                    Comments = approvalEntry.Comments,
                    Message = $"Batch penggajian {run.RunNumber} disetujui HR Manager dan diteruskan ke Direktur."
                },
                "Review batch berhasil.");
        }

        public async Task<PayrollServiceResult<PayrollApprovalResultDto>> ApproveRunFinalAsync(
            Guid runId,
            ApprovePayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai Tier 3 Final Approval batch penggajian ID: {RunId} oleh Direktur {ActorId}",
                runId, actorUserId);

            var run = await _context.PrlPayrollRuns
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.RunStatus != "WaitingFinalApproval")
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian saat ini berstatus '{run.RunStatus}', bukan 'WaitingFinalApproval'.");
            }

            // Penegakan Keras Aturan Anti-Self-Approval (PAY-B4, VR-PAY-08):
            // Direktur dilarang merangkap sebagai inisiator atau reviewer yang sama
            if ((run.SubmittedByUserId.HasValue && run.SubmittedByUserId.Value == actorUserId))
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Pelanggaran Segregation of Duties (PAY-B4): Direktur dilarang merangkap menyetujui batch yang dia ajukan sendiri (VR-PAY-08).");
            }

            var previousStatus = run.RunStatus;
            run.RunStatus = "Approved";
            run.IsLocked = true; // Pembekuan permanen pasca Tier 3 (PAY-B2)
            run.LockedAt = DateTime.UtcNow;
            run.LockedByUserId = actorUserId;
            run.ApprovedAt = DateTime.UtcNow;
            run.ApprovedByUserId = actorUserId;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            var approvalEntry = new PrlPayrollApproval
            {
                Id = Guid.NewGuid(),
                PayrollRunId = run.Id,
                StepOrder = 3,
                ApprovalLevel = "Director",
                ApprovalStatus = "Approved",
                ActionType = "FinalApproval",
                ActionAt = DateTime.UtcNow,
                ActualActionByUserId = actorUserId,
                Comments = command.Comments ?? "Batch penggajian telah disetujui final oleh Direktur (Locked & Immutable).",
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.PrlPayrollApprovals.Add(approvalEntry);
            await _context.SaveChangesAsync(cancellationToken);

            var actorUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);

            _logger.LogInformation(
                "Batch {RunNumber} berhasil disetujui final oleh Direktur {UserName} dan telah dikunci permanen.",
                run.RunNumber, actorUser?.UserName);

            return PayrollServiceResult<PayrollApprovalResultDto>.Ok(
                new PayrollApprovalResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    PreviousStatus = previousStatus,
                    CurrentStatus = run.RunStatus,
                    ApprovalTierLevel = "Tier 3: Direktur SDM & Keuangan",
                    ActionTaken = "FinalApproval",
                    ActorUserId = actorUserId,
                    ActorUserName = actorUser?.UserName,
                    ActionTimestamp = approvalEntry.ActionAt.Value,
                    IsLocked = true,
                    IsApproved = true,
                    Comments = approvalEntry.Comments,
                    Message = $"Batch penggajian {run.RunNumber} berhasil disetujui final oleh Direksi dan terkunci permanen (Immutable)."
                },
                "Persetujuan final batch berhasil.");
        }

        public async Task<PayrollServiceResult<PayrollApprovalResultDto>> RejectRunAsync(
            Guid runId,
            RejectPayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memproses penolakan batch penggajian ID: {RunId} oleh Actor {ActorId}",
                runId, actorUserId);

            if (command == null || string.IsNullOrWhiteSpace(command.RejectionReason))
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan alasan penolakan batch wajib diisi.");
            }

            var trimmedReason = command.RejectionReason.Trim();
            if (trimmedReason.Length < 15)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan batch wajib minimal 15 karakter resmi untuk audit trail (PAY-B7, VR-PAY-16).");
            }

            var run = await _context.PrlPayrollRuns
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.RunStatus is not ("WaitingHrReview" or "WaitingFinalApproval"))
            {
                return PayrollServiceResult<PayrollApprovalResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian berstatus '{run.RunStatus}' tidak dalam antrean persetujuan.");
            }

            var previousStatus = run.RunStatus;
            var isDirectorTier = run.RunStatus == "WaitingFinalApproval";

            run.RunStatus = "NeedCorrection";
            run.IsLocked = false; // Buka proteksi agar tim payroll dapat memperbaiki data
            run.Notes = $"[Ditolak {DateTime.UtcNow:yyyy-MM-dd HH:mm}]: {trimmedReason}";
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            var approvalEntry = new PrlPayrollApproval
            {
                Id = Guid.NewGuid(),
                PayrollRunId = run.Id,
                StepOrder = isDirectorTier ? 3 : 2,
                ApprovalLevel = isDirectorTier ? "Director" : "HrManager",
                ApprovalStatus = "Rejected",
                ActionType = "Reject",
                ActionAt = DateTime.UtcNow,
                ActualActionByUserId = actorUserId,
                Comments = trimmedReason,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.PrlPayrollApprovals.Add(approvalEntry);
            await _context.SaveChangesAsync(cancellationToken);

            var actorUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);

            _logger.LogWarning(
                "Batch {RunNumber} ditolak oleh {UserName} dengan status NeedCorrection. Alasan: {Reason}",
                run.RunNumber, actorUser?.UserName, trimmedReason);

            return PayrollServiceResult<PayrollApprovalResultDto>.Ok(
                new PayrollApprovalResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    PreviousStatus = previousStatus,
                    CurrentStatus = run.RunStatus,
                    ApprovalTierLevel = isDirectorTier ? "Tier 3: Direktur" : "Tier 2: HR Manager",
                    ActionTaken = "Reject",
                    ActorUserId = actorUserId,
                    ActorUserName = actorUser?.UserName,
                    ActionTimestamp = approvalEntry.ActionAt.Value,
                    IsLocked = false,
                    IsApproved = false,
                    Comments = trimmedReason,
                    Message = $"Batch {run.RunNumber} ditolak dan dikembalikan ke status 'NeedCorrection' untuk perbaikan."
                },
                "Penolakan batch berhasil diproses.");
        }

        public async Task<PayrollServiceResult<List<PayrollApprovalHistoryDto>>> GetApprovalHistoryAsync(
            Guid runId,
            CancellationToken cancellationToken = default)
        {
            var history = await _context.PrlPayrollApprovals
                .Include(a => a.ActualActionByUser)
                .Where(a => a.PayrollRunId == runId && a.IsActive)
                .OrderBy(a => a.StepOrder)
                .ThenBy(a => a.ActionAt)
                .ToListAsync(cancellationToken);

            var dtos = history.Select(a => new PayrollApprovalHistoryDto
            {
                Id = a.Id,
                PayrollRunId = a.PayrollRunId,
                StepOrder = a.StepOrder,
                ApprovalLevel = a.ApprovalLevel,
                ApprovalStatus = a.ApprovalStatus,
                ActionType = a.ActionType,
                ActionAt = a.ActionAt,
                ActionByUserId = a.ActualActionByUserId,
                ActionByUserName = a.ActualActionByUser?.UserName,
                Comments = a.Comments
            }).ToList();

            return PayrollServiceResult<List<PayrollApprovalHistoryDto>>.Ok(dtos, "Riwayat otorisasi approval berhasil diambil.");
        }
    }
}
