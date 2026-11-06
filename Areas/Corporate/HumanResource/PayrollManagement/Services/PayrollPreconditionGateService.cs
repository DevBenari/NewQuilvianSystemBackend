using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan inspeksi gerbang prasyarat (Precondition Gate) penguncian data waktu kerja Sub-07, 08, 09 sebelum inisiasi batch gaji (BE-HRD-148, VR-PAY-02, HRD-DEC-122).
    /// </summary>
    public class PayrollPreconditionGateService : IPayrollPreconditionGateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollPreconditionGateService> _logger;

        public PayrollPreconditionGateService(
            ApplicationDbContext context,
            ILogger<PayrollPreconditionGateService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PreconditionGateCheckResponse> CheckPreconditionsAsync(
            int periodYear,
            int periodMonth,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = new DateOnly(periodYear, periodMonth, 20);
            var prevMonth = cutoffDate.AddMonths(-1);
            var startDate = new DateOnly(prevMonth.Year, prevMonth.Month, 21);

            var issues = new List<string>();

            // 1. Verifikasi Sub-modul 07: Presensi & Kehadiran (Attendance)
            var unprocessedAttendanceCount = await _context.Set<HrdAttendanceDaily>()
                .CountAsync(x => !x.IsDelete &&
                                 x.AttendanceDate >= startDate &&
                                 x.AttendanceDate <= cutoffDate &&
                                 (x.ProcessingStatus == "Pending" || x.AttendanceStatus == "Unprocessed"),
                            cancellationToken);

            bool attendanceLocked = unprocessedAttendanceCount == 0;
            string attendanceStatus = attendanceLocked ? "Locked" : "PendingResolution";
            string attendanceMessage = attendanceLocked
                ? "Data presensi harian seluruh staf telah final dan terproses."
                : $"Terdapat {unprocessedAttendanceCount} data presensi harian yang belum selesai diproses.";

            if (!attendanceLocked)
            {
                issues.Add($"[Sub-07 Presensi] {attendanceMessage}");
            }

            // 2. Verifikasi Sub-modul 08: Cuti & Izin Pegawai (Leave Management)
            var pendingLeaveCount = await _context.Set<WfpLeaveRequest>()
                .CountAsync(x => !x.IsDelete &&
                                 x.StartDate <= cutoffDate &&
                                 (x.RequestStatus == "WaitingApproval" || x.RequestStatus == "WaitingUnitApproval"),
                            cancellationToken);

            bool leaveLocked = pendingLeaveCount == 0;
            string leaveStatus = leaveLocked ? "Locked" : "PendingApproval";
            string leaveMessage = leaveLocked
                ? "Tidak ada permohonan cuti staf berstatus pending untuk periode ini."
                : $"Terdapat {pendingLeaveCount} pengajuan cuti yang masih menunggu persetujuan atasan/unit.";

            if (!leaveLocked)
            {
                issues.Add($"[Sub-08 Cuti] {leaveMessage}");
            }

            // 3. Verifikasi Sub-modul 09: Administrasi Lembur (Overtime Management)
            var periodCode = $"OTP-{periodYear:D4}-{periodMonth:D2}";
            var otPeriod = await _context.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.PeriodCode == periodCode && !x.IsDelete, cancellationToken);

            bool overtimeLocked = false;
            bool overtimeHandoffExecuted = false;
            string overtimeStatus = "NotCreated";
            string overtimeMessage = string.Empty;

            if (otPeriod == null)
            {
                overtimeMessage = $"Periode lembur {periodCode} belum dibentuk di Sub-modul 09.";
                issues.Add($"[Sub-09 Lembur] {overtimeMessage}");
            }
            else
            {
                overtimeLocked = otPeriod.PeriodStatus == "Locked" || otPeriod.PeriodStatus == "Closed";
                overtimeHandoffExecuted = otPeriod.IsHandoffExecuted;
                overtimeStatus = otPeriod.PeriodStatus;

                if (!overtimeLocked)
                {
                    overtimeMessage = $"Periode lembur {periodCode} belum berstatus Locked (Status saat ini: {otPeriod.PeriodStatus}).";
                    issues.Add($"[Sub-09 Lembur] {overtimeMessage}");
                }
                else if (!overtimeHandoffExecuted)
                {
                    overtimeMessage = $"Paket serah terima lembur {periodCode} belum dieksekusi (IsHandoffExecuted = false).";
                    issues.Add($"[Sub-09 Lembur] {overtimeMessage}");
                }
                else
                {
                    overtimeMessage = $"Periode lembur {periodCode} telah dikunci dan serah terima ke payroll telah selesai.";
                }
            }

            bool canInitialize = attendanceLocked && leaveLocked && overtimeLocked && overtimeHandoffExecuted;

            var summary = canInitialize
                ? "Seluruh prasyarat penguncian data waktu kerja (Presensi, Cuti, Lembur) terpenuhi. Batch penggajian diizinkan untuk diinisiasi."
                : $"Inisiasi batch penggajian DITOLAK: Terdapat {issues.Count} kendala penguncian waktu kerja yang belum diselesaikan.";

            _logger.LogInformation(
                "Pemeriksaan Precondition Gate Payroll {Year}-{Month}: CanInitialize={CanInit}, IssuesCount={Count}",
                periodYear, periodMonth, canInitialize, issues.Count);

            return new PreconditionGateCheckResponse
            {
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                CutoffDate = cutoffDate,
                CanInitialize = canInitialize,
                AttendanceLocked = attendanceLocked,
                AttendanceStatus = attendanceStatus,
                AttendanceMessage = attendanceMessage,
                LeaveLocked = leaveLocked,
                LeaveStatus = leaveStatus,
                LeaveMessage = leaveMessage,
                OvertimeLocked = overtimeLocked,
                OvertimeHandoffExecuted = overtimeHandoffExecuted,
                OvertimeStatus = overtimeStatus,
                OvertimeMessage = overtimeMessage,
                UnresolvedIssues = issues,
                SummaryMessage = summary
            };
        }
    }
}
