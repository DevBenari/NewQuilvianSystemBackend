using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Service persetujuan cuti atasan dengan kalkulasi kuota pelayanan unit 24/7 (Safe Staffing Coverage Warning)
    /// dan pemotongan saldo buku besar atomik (BE-HRD-124, HRD-DEC-110, HRD-DEC-111, HRD-DEC-114, VR-LEV-21, INT-HR-45, INT-HR-47).
    /// </summary>
    public class LeaveApprovalService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SafeStaffingCoverageGuard _coverageGuard;
        private readonly LeaveBalanceLedgerService _ledgerService;
        private readonly ILogger<LeaveApprovalService> _logger;

        public LeaveApprovalService(
            ApplicationDbContext dbContext,
            SafeStaffingCoverageGuard coverageGuard,
            LeaveBalanceLedgerService ledgerService,
            ILogger<LeaveApprovalService> logger)
        {
            _dbContext = dbContext;
            _coverageGuard = coverageGuard;
            _ledgerService = ledgerService;
            _logger = logger;
        }

        /// <summary>
        /// Kotak masuk persetujuan cuti bawahan dengan indikator kuota pelayanan dan status surat dokter (HRD-DEC-111).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<List<LeaveApprovalInboxItemDto>>> GetApprovalInboxAsync(
            Guid approverUserId,
            Guid? unitId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var targetStatuses = new List<string> { "Submitted", "WaitingApproval", "WaitingSupervisorApproval" };
            if (!string.IsNullOrWhiteSpace(status))
            {
                targetStatuses = new List<string> { status };
            }

            var query = _dbContext.Set<WfpLeaveRequest>()
                .Include(r => r.WorkforceProfile)
                .Include(r => r.LeaveType)
                .Include(r => r.Department)
                .Include(r => r.OrganizationUnit)
                .Include(r => r.Attachments)
                .AsNoTracking()
                .Where(r => !r.IsDelete && targetStatuses.Contains(r.LeaveRequestStatus));

            if (unitId.HasValue && unitId.Value != Guid.Empty)
            {
                query = query.Where(r => r.OrganizationUnitId == unitId.Value || r.DepartmentId == unitId.Value);
            }

            var requests = await query
                .OrderByDescending(r => r.SubmittedAt ?? r.CreateDateTime)
                .ToListAsync(cancellationToken);

            var items = new List<LeaveApprovalInboxItemDto>();
            foreach (var r in requests)
            {
                // Evaluasi atau baca flag staffing coverage warning
                bool hasWarning = r.HasStaffingCoverageWarning;
                string? warningDetails = r.CoverageWarningDetails;

                if (!hasWarning && r.OrganizationUnitId.HasValue)
                {
                    var eval = await _coverageGuard.EvaluateLeaveRequestCoverageAsync(
                        r.WorkforceProfileId, r.StartDate, r.EndDate, cancellationToken);
                    if (eval.IsWarningTriggered)
                    {
                        hasWarning = true;
                        warningDetails = eval.AdvisoryMessage;
                    }
                }

                var activeMedicalAttachment = r.Attachments?
                    .FirstOrDefault(a => a.IsActive && !a.IsDelete && a.IsMedicalCertificate);

                items.Add(new LeaveApprovalInboxItemDto
                {
                    LeaveRequestId = r.Id,
                    RequestNumber = r.RequestNumber,
                    WorkforceProfileId = r.WorkforceProfileId,
                    EmployeeCode = r.WorkforceProfile?.ProfileCode ?? string.Empty,
                    EmployeeName = r.WorkforceProfile?.DisplayName ?? string.Empty,
                    DepartmentName = r.Department?.DepartmentName ?? string.Empty,
                    UnitName = r.OrganizationUnit?.UnitName ?? string.Empty,
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeCode = r.LeaveType?.LeaveTypeCode ?? string.Empty,
                    LeaveTypeName = r.LeaveType?.LeaveTypeName ?? string.Empty,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RequestedDays = r.CalculatedWorkingDays > 0 ? r.CalculatedWorkingDays : r.RequestedDays,
                    IsHalfDay = r.IsHalfDay,
                    HalfDayPeriod = r.HalfDayPeriod,
                    Reason = r.Reason,
                    Status = r.LeaveRequestStatus,
                    SubmittedAt = r.SubmittedAt ?? r.CreateDateTime,
                    HasStaffingCoverageWarning = hasWarning,
                    CoverageWarningDetails = warningDetails,
                    RequiresMedicalCertificate = r.LeaveType?.RequiresMedicalCertificate ?? false,
                    HasMedicalAttachment = activeMedicalAttachment != null,
                    IsDoctorNoteVerified = r.IsDoctorNoteVerified,
                    DoctorNoteFaskesName = r.DoctorNoteFaskesName,
                    DoctorSipNumber = r.DoctorSipNumber
                });
            }

            return LeaveBalanceLedgerServiceResult<List<LeaveApprovalInboxItemDto>>.Ok(
                items,
                $"Berhasil mengambil {items.Count} permohonan cuti pada kotak masuk persetujuan.");
        }

        /// <summary>
        /// Menyetujui permohonan cuti dengan pengecekan mitigasi perawat pengganti bila kuota warning (VR-LEV-21)
        /// dan mendebit buku besar saldo cuti secara atomik (HRD-DEC-110).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>> ApproveAsync(
            Guid requestId,
            Guid approverUserId,
            ApproveLeaveRequestCommand command,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            // Pencegahan self-approval
            if (request.CreateBy == approverUserId)
            {
                return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Persetujuan mandiri dilarang. Atasan langsung wajib menyetujui cuti bawahan (VR-OT-03).");
            }

            // Evaluasi coverage kuota staf
            var eval = await _coverageGuard.EvaluateLeaveRequestCoverageAsync(
                request.WorkforceProfileId, request.StartDate, request.EndDate, cancellationToken);

            bool isWarning = request.HasStaffingCoverageWarning || eval.IsWarningTriggered;

            // Jika terpicu warning, catatan mitigasi perawat pengganti WAJIB diisi (VR-LEV-21, HRD-DEC-111)
            if (isWarning && string.IsNullOrWhiteSpace(command.MitigationNotes))
            {
                return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan mitigasi penugasan staf pengganti wajib diisi saat menyetujui cuti berperingatan kuota pelayanan (VR-LEV-21, HRD-DEC-111).");
            }

            request.HasStaffingCoverageWarning = isWarning;
            if (isWarning)
            {
                request.CoverageWarningDetails = eval.AdvisoryMessage;
                request.CoverageMitigationNotes = command.MitigationNotes;
            }

            var now = DateTime.UtcNow;
            decimal durationToDeduct = request.CalculatedWorkingDays > 0 ? request.CalculatedWorkingDays : request.RequestedDays;

            // Posting debit Taken ke buku besar saldo jika cuti berjenis potong saldo (HRD-DEC-110)
            if (request.LeaveType != null && request.LeaveType.IsBalanceDeducted)
            {
                var debitResult = await _ledgerService.PostDebitTakenAsync(
                    request.WorkforceProfileId,
                    request.LeaveTypeId,
                    durationToDeduct,
                    request.Id,
                    approverUserId,
                    cancellationToken);

                if (!debitResult.Success)
                {
                    return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Fail(
                        debitResult.StatusCode,
                        $"Gagal mendebit saldo cuti: {debitResult.Message}");
                }
            }

            // Deteksi keterlambatan approval pasca closing/cut-off payroll (25th of month) (HRD-DEC-114, LEV-W5)
            bool isPendingPayroll = now.Day >= 25;
            request.IsPendingPayrollReconciliation = isPendingPayroll;

            request.LeaveRequestStatus = "Approved";
            request.SupervisorApprovedAt = now;
            request.ApprovedByUserId = approverUserId;
            request.UpdateBy = approverUserId;
            request.UpdateDateTime = now;

            // Sinkronkan status jadwal shift di Roster (INT-HR-47)
            var shiftsInRange = await _dbContext.Set<TrxShiftAssignment>()
                .Where(s => !s.IsDelete && !s.IsCancel &&
                            s.WorkforceProfileId == request.WorkforceProfileId &&
                            s.ShiftDate >= request.StartDate &&
                            s.ShiftDate <= request.EndDate)
                .ToListAsync(cancellationToken);

            foreach (var shift in shiftsInRange)
            {
                shift.HasLeaveConflict = true;
                shift.AssignmentStatus = "OnLeave";
                shift.UpdateBy = approverUserId;
                shift.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan cuti {RequestId} disetujui oleh atasan {UserId}. Mitigasi: {Mitigation}.",
                request.Id, approverUserId, request.CoverageMitigationNotes ?? "None");

            var currentBalance = await _ledgerService.GetSummaryAsync(
                request.WorkforceProfileId, request.LeaveTypeId, now.Year, cancellationToken);

            return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Ok(
                new LeaveApprovalResultDto
                {
                    LeaveRequestId = request.Id,
                    RequestNumber = request.RequestNumber,
                    Status = request.LeaveRequestStatus,
                    HasStaffingCoverageWarning = isWarning,
                    MitigationNotes = request.CoverageMitigationNotes,
                    BalanceRemaining = currentBalance?.AvailableBalanceDays,
                    IsPendingPayrollReconciliation = isPendingPayroll,
                    ActionAt = now,
                    Message = isWarning
                        ? "Permohonan cuti berhasil disetujui dengan catatan mitigasi staf pengganti (Approve with Mitigation)."
                        : "Permohonan cuti berhasil disetujui."
                },
                "Persetujuan cuti berhasil diproses.");
        }

        /// <summary>
        /// Menolak permohonan cuti bawahan dengan alasan operasional sah.
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>> RejectAsync(
            Guid requestId,
            Guid approverUserId,
            RejectLeaveRequestCommand command,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            request.LeaveRequestStatus = "Rejected";
            request.UpdateBy = approverUserId;
            request.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan cuti {RequestId} ditolak oleh atasan {UserId}. Alasan: {Reason}.",
                request.Id, approverUserId, command.RejectionReason);

            return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Ok(
                new LeaveApprovalResultDto
                {
                    LeaveRequestId = request.Id,
                    RequestNumber = request.RequestNumber,
                    Status = request.LeaveRequestStatus,
                    ActionAt = now,
                    Message = $"Permohonan cuti ditolak: {command.RejectionReason}"
                },
                "Permohonan cuti berhasil ditolak.");
        }

        /// <summary>
        /// Meminta revisi tanggal/durasi permohonan cuti kepada staf pemohon.
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>> ReviseAsync(
            Guid requestId,
            Guid approverUserId,
            ReviseLeaveRequestCommand command,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            request.LeaveRequestStatus = "NeedRevision";
            request.UpdateBy = approverUserId;
            request.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan cuti {RequestId} diminta revisi oleh atasan {UserId}. Catatan: {Notes}.",
                request.Id, approverUserId, command.RevisionNotes);

            return LeaveBalanceLedgerServiceResult<LeaveApprovalResultDto>.Ok(
                new LeaveApprovalResultDto
                {
                    LeaveRequestId = request.Id,
                    RequestNumber = request.RequestNumber,
                    Status = request.LeaveRequestStatus,
                    ActionAt = now,
                    Message = $"Permohonan cuti dikembalikan untuk revisi: {command.RevisionNotes}"
                },
                "Permohonan cuti berhasil dikembalikan untuk revisi.");
        }
    }
}
