using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Service rekonsiliasi retrospektif cuti pending saat cut-off payroll dan penutupan periode cuti (IsPayrollClosed)
    /// (BE-HRD-129, HRD-DEC-114, HRD-DEC-115, LEV-W5, VR-LEV-19, VR-LEV-20, INT-HR-48).
    /// </summary>
    public class LeavePayrollReconciliationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveExecutionProcessorService _executionProcessorService;
        private readonly ILogger<LeavePayrollReconciliationService> _logger;

        public LeavePayrollReconciliationService(
            ApplicationDbContext dbContext,
            LeaveExecutionProcessorService executionProcessorService,
            ILogger<LeavePayrollReconciliationService> logger)
        {
            _dbContext = dbContext;
            _executionProcessorService = executionProcessorService;
            _logger = logger;
        }

        /// <summary>
        /// Mengambil daftar cuti pending yang akan direkonsiliasi secara retrospektif ke periode payroll berikutnya (INT-HR-48, LEV-W5).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<List<PendingLeavePayrollDto>>> GetPendingReconciliationsAsync(
            int? periodYear,
            int? periodMonth,
            CancellationToken cancellationToken = default)
        {
            var targetYear = periodYear ?? DateTime.UtcNow.Year;
            var targetMonth = periodMonth ?? DateTime.UtcNow.Month;
            var startOfMonth = new DateOnly(targetYear, targetMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Cuti yang pending approval atau cuti disetujui terlambat pasca-closing (IsPendingPayrollReconciliation = true)
            var query = _dbContext.Set<WfpLeaveRequest>()
                .Include(r => r.WorkforceProfile)
                .Include(r => r.OrganizationUnit)
                .Include(r => r.LeaveType)
                .AsNoTracking()
                .Where(r => !r.IsDelete &&
                            (r.IsPendingPayrollReconciliation ||
                             r.LeaveRequestStatus == LeaveRequestValueConstants.Status.Submitted ||
                             r.LeaveRequestStatus == LeaveRequestValueConstants.Status.WaitingApproval ||
                             r.LeaveRequestStatus == "WaitingSupervisorApproval"));

            query = query.Where(r => r.StartDate <= endOfMonth && r.EndDate >= startOfMonth);

            var rows = await query
                .OrderBy(r => r.StartDate)
                .ToListAsync(cancellationToken);

            var items = rows.Select(r => new PendingLeavePayrollDto
            {
                LeaveRequestId = r.Id,
                RequestNumber = r.RequestNumber,
                WorkforceProfileId = r.WorkforceProfileId,
                EmployeeCode = r.WorkforceProfile?.ProfileCode ?? string.Empty,
                EmployeeName = r.WorkforceProfile?.DisplayName ?? string.Empty,
                UnitName = r.OrganizationUnit?.UnitName ?? string.Empty,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                RequestedDays = r.CalculatedWorkingDays > 0 ? r.CalculatedWorkingDays : r.RequestedDays,
                LeaveTypeName = r.LeaveType?.LeaveTypeName ?? string.Empty,
                Status = r.LeaveRequestStatus,
                Reason = r.Reason,
                IsPendingPayrollReconciliation = r.IsPendingPayrollReconciliation,
                SubmittedAt = r.SubmittedAt ?? r.CreateDateTime
            }).ToList();

            return LeaveBalanceLedgerServiceResult<List<PendingLeavePayrollDto>>.Ok(
                items,
                $"Berhasil mengambil {items.Count} permohonan cuti pending untuk rekonsiliasi retrospektif payroll.");
        }

        /// <summary>
        /// Mengunci periode cuti bulanan pra-payroll (IsPayrollClosed = true) dan serahkan snapshot ke Payroll (HRD-DEC-114, HRD-DEC-115).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<CloseLeavePeriodResultDto>> CloseLeavePeriodAsync(
            CloseLeavePeriodCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return LeaveBalanceLedgerServiceResult<CloseLeavePeriodResultDto>.Fail(StatusCodes.Status400BadRequest, "Command tidak boleh kosong.");
            }

            var startOfMonth = new DateOnly(command.PeriodYear, command.PeriodMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 1. Ambil cuti selesai/disetujui pada periode ini yang belum dikunci
            var completedRequests = await _dbContext.Set<WfpLeaveRequest>()
                .Where(r => !r.IsDelete &&
                            r.StartDate <= endOfMonth &&
                            r.EndDate >= startOfMonth &&
                            (r.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved ||
                             r.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed ||
                             r.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken) &&
                            !r.IsPayrollClosed)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var req in completedRequests)
            {
                req.IsPayrollClosed = true;
                req.IsPendingPayrollReconciliation = false;
                req.UpdateBy = actorUserId;
                req.UpdateDateTime = now;
            }

            // 2. Tandai cuti yang masih pending approval pada periode ini agar ditandai retrospektif bila disetujui nanti (LEV-W5)
            var pendingRequests = await _dbContext.Set<WfpLeaveRequest>()
                .Where(r => !r.IsDelete &&
                            r.StartDate <= endOfMonth &&
                            r.EndDate >= startOfMonth &&
                            (r.LeaveRequestStatus == LeaveRequestValueConstants.Status.Submitted ||
                             r.LeaveRequestStatus == LeaveRequestValueConstants.Status.WaitingApproval ||
                             r.LeaveRequestStatus == "WaitingSupervisorApproval"))
                .ToListAsync(cancellationToken);

            foreach (var pending in pendingRequests)
            {
                pending.IsPendingPayrollReconciliation = true;
                pending.UpdateBy = actorUserId;
                pending.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Periode cuti {Year}-{Month} ditutup oleh user {UserId}. {ClosedCount} cuti dikunci mutlak, {PendingCount} cuti pending disiapkan rekonsiliasi retrospektif.",
                command.PeriodYear, command.PeriodMonth, actorUserId, completedRequests.Count, pendingRequests.Count);

            return LeaveBalanceLedgerServiceResult<CloseLeavePeriodResultDto>.Ok(
                new CloseLeavePeriodResultDto
                {
                    PeriodYear = command.PeriodYear,
                    PeriodMonth = command.PeriodMonth,
                    TotalClosedRequests = completedRequests.Count,
                    TotalPendingReconciledRequests = pendingRequests.Count,
                    ClosedAt = now,
                    Message = $"Periode cuti {command.PeriodYear}-{command.PeriodMonth:D2} berhasil ditutup. {completedRequests.Count} cuti selesai dikunci mutlak (IsPayrollClosed = true), {pendingRequests.Count} cuti pending dialokasikan ke rekonsiliasi retrospektif bulan berikutnya."
                },
                "Periode cuti berhasil ditutup secara resmi.");
        }

        /// <summary>
        /// Percobaan pembalikan cuti — Ditolak HTTP 409 bila status Completed atau PayrollClosed (VR-LEV-19, VR-LEV-20, HRD-DEC-115).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveExecutionActionResponse>> ReverseExecutionAsync(
            Guid executionId,
            ReverseExecutionCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var execution = await _dbContext.Set<TrxLeaveExecution>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == executionId && !x.IsDelete, cancellationToken);

            if (execution == null || execution.LeaveRequest == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave execution tidak ditemukan.");
            }

            // Panggil processor dengan guard ketat LEV-B4
            var result = await _executionProcessorService.ReverseAsync(
                execution.LeaveRequestId,
                new ReverseLeaveExecutionRequest { Reason = command?.Reason ?? "Pembalikan eksekusi cuti" },
                actorUserId,
                cancellationToken);

            return result.Success
                ? LeaveBalanceLedgerServiceResult<LeaveExecutionActionResponse>.Ok(result.Data, result.Message)
                : LeaveBalanceLedgerServiceResult<LeaveExecutionActionResponse>.Fail(result.StatusCode, result.Message);
        }
    }
}
