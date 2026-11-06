using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Service guard evaluasi ketersediaan staf unit 24/7 (Safe Staffing Coverage Guard)
    /// Menghubungkan pengajuan cuti ke jadwal dinas Roster Terbit (INT-HR-45, HRD-DEC-111, LEV-W1).
    /// </summary>
    public class SafeStaffingCoverageGuard : ISafeStaffingQueryService
    {
        private const int DefaultMinimumStaffFor247Ward = 3;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SafeStaffingCoverageGuard> _logger;

        public SafeStaffingCoverageGuard(
            ApplicationDbContext dbContext,
            ILogger<SafeStaffingCoverageGuard> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Evaluasi ketersediaan tenaga per unit, tanggal, dan shift tertentu (INT-HR-45).
        /// </summary>
        public async Task<StaffingCoverageEvaluationResult> EvaluateCoverageAsync(
            Guid unitId,
            DateOnly targetDate,
            Guid? shiftId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var unit = await _dbContext.Set<MstOrganizationUnit>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDelete, cancellationToken);
                var unitName = unit?.UnitName ?? "Unit Rawat Inap / IGD";

                string shiftName = "Shift Reguler";
                if (shiftId.HasValue && shiftId.Value != Guid.Empty)
                {
                    var shift = await _dbContext.Set<MstShift>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == shiftId.Value && !s.IsDelete, cancellationToken);
                    if (shift != null)
                    {
                        shiftName = shift.ShiftName;
                    }
                }

                // Hitung jumlah staf bertugas aktif pada shift dan tanggal tersebut di unit terkait
                var scheduledStaffQuery = _dbContext.Set<TrxShiftAssignment>()
                    .AsNoTracking()
                    .Where(a => !a.IsDelete && !a.IsCancel &&
                                a.ShiftDate == targetDate &&
                                (a.OrganizationUnitId == unitId || a.DepartmentId == unitId) &&
                                (a.AssignmentStatus == "Published" || a.AssignmentStatus == "Confirmed" || a.AssignmentStatus == "Scheduled"));

                if (shiftId.HasValue && shiftId.Value != Guid.Empty)
                {
                    scheduledStaffQuery = scheduledStaffQuery.Where(a => a.ShiftId == shiftId.Value);
                }

                var scheduledCount = await scheduledStaffQuery.CountAsync(cancellationToken);

                // Ambil kebutuhan kuota minimal dari TrxDailyStaffingRequirement
                var targetDateTime = targetDate.ToDateTime(TimeOnly.MinValue);
                var requirement = await _dbContext.Set<TrxDailyStaffingRequirement>()
                    .AsNoTracking()
                    .Where(r => !r.IsDelete &&
                                r.StaffingDate.Date == targetDateTime.Date &&
                                (r.OrganizationUnitId == unitId || r.DepartmentId == unitId))
                    .OrderByDescending(r => r.MinimumRequiredHeadcount)
                    .FirstOrDefaultAsync(cancellationToken);

                int minRequired = requirement != null && requirement.MinimumRequiredHeadcount > 0
                    ? (int)Math.Ceiling(requirement.MinimumRequiredHeadcount)
                    : DefaultMinimumStaffFor247Ward;

                int remainingIfApproved = Math.Max(0, scheduledCount - 1);
                bool isWarning = remainingIfApproved < minRequired;

                string advisoryMessage = isWarning
                    ? $"[Safe Staffing Coverage Warning] Tanggal {targetDate:yyyy-MM-dd} {shiftName} di unit '{unitName}' hanya menyisakan {remainingIfApproved} staf (kuota minimal {minRequired} staf). Memerlukan tindakan mitigasi staf pengganti (VR-LEV-21)."
                    : $"Kapasitas ketenagaan aman: Tersisa {remainingIfApproved} staf dari kuota minimal {minRequired} staf.";

                return new StaffingCoverageEvaluationResult(
                    IsWarningTriggered: isWarning,
                    MinimumRequiredStaff: minRequired,
                    ScheduledStaffCount: scheduledCount,
                    RemainingStaffIfApproved: remainingIfApproved,
                    ShiftName: shiftName,
                    UnitName: unitName,
                    AdvisoryMessage: advisoryMessage
                );
            }
            catch (Exception ex)
            {
                // Prinsip fail-open with audit warning bila integrasi roster / scheduler mengalami anomali
                _logger.LogWarning(ex, "Gagal mengevaluasi ketersediaan staf untuk unit {UnitId} tanggal {TargetDate}. Menggunakan fallback fail-open.", unitId, targetDate);

                return new StaffingCoverageEvaluationResult(
                    IsWarningTriggered: false,
                    MinimumRequiredStaff: DefaultMinimumStaffFor247Ward,
                    ScheduledStaffCount: 0,
                    RemainingStaffIfApproved: 0,
                    ShiftName: "Tidak Terdefinisi",
                    UnitName: "Unit Terkait",
                    AdvisoryMessage: "Evaluasi kuota jadwal tidak dapat diverifikasi secara otomatis; mohon verifikasi jadwal manual."
                );
            }
        }

        /// <summary>
        /// Evaluasi agregat seluruh tanggal permohonan cuti untuk pegawai tertentu.
        /// </summary>
        public async Task<StaffingCoverageEvaluationResult> EvaluateLeaveRequestCoverageAsync(
            Guid workforceProfileId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Ambil profil pegawai dan penempatan unitnya
                var workforce = await _dbContext.Set<MstWorkforceProfile>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete, cancellationToken);

                if (workforce == null)
                {
                    return new StaffingCoverageEvaluationResult(false, 0, 0, 0, string.Empty, string.Empty, string.Empty);
                }

                // Temukan unit penempatan aktif
                var unitAssignment = await _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpOrganizationAssignment>()
                    .AsNoTracking()
                    .Where(a => a.WorkforceProfileId == workforceProfileId && a.IsActive && !a.IsDelete)
                    .OrderByDescending(a => a.EffectiveStartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                var unitId = unitAssignment?.OrganizationUnitId ?? Guid.Empty;
                if (unitId == Guid.Empty)
                {
                    return new StaffingCoverageEvaluationResult(false, 0, 0, 0, string.Empty, string.Empty, "Pegawai tidak terikat pada unit kerja klinis 24/7.");
                }

                // Periksa setiap hari dalam rentang cuti
                var warnings = new List<string>();
                bool anyWarning = false;
                int maxMinRequired = 0;
                int minRemaining = int.MaxValue;
                string primaryUnitName = string.Empty;

                for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                {
                    // Temukan shift assignment pegawai pada tanggal tersebut jika ada
                    var employeeShift = await _dbContext.Set<TrxShiftAssignment>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => !a.IsDelete && !a.IsCancel &&
                                                  a.WorkforceProfileId == workforceProfileId &&
                                                  a.ShiftDate == currentDate &&
                                                  (a.AssignmentStatus == "Published" || a.AssignmentStatus == "Confirmed" || a.AssignmentStatus == "Scheduled"),
                                                  cancellationToken);

                    var eval = await EvaluateCoverageAsync(unitId, currentDate, employeeShift?.ShiftId, cancellationToken);
                    if (string.IsNullOrEmpty(primaryUnitName))
                    {
                        primaryUnitName = eval.UnitName;
                    }

                    if (eval.MinimumRequiredStaff > maxMinRequired)
                    {
                        maxMinRequired = eval.MinimumRequiredStaff;
                    }
                    if (eval.RemainingStaffIfApproved < minRemaining)
                    {
                        minRemaining = eval.RemainingStaffIfApproved;
                    }

                    if (eval.IsWarningTriggered)
                    {
                        anyWarning = true;
                        warnings.Add($"{currentDate:yyyy-MM-dd} ({eval.ShiftName}): sisa {eval.RemainingStaffIfApproved}/{eval.MinimumRequiredStaff}");
                    }
                }

                string finalMessage = anyWarning
                    ? $"[Safe Staffing Coverage Warning] Kuota perawat unit '{primaryUnitName}' berisiko di bawah standar pada: {string.Join("; ", warnings)}. Mohon tetapkan staf pengganti pada opsi Approve with Mitigation."
                    : "Kapasitas kuota ketenagaan unit aman sepanjang periode cuti.";

                return new StaffingCoverageEvaluationResult(
                    IsWarningTriggered: anyWarning,
                    MinimumRequiredStaff: maxMinRequired,
                    ScheduledStaffCount: 0,
                    RemainingStaffIfApproved: minRemaining == int.MaxValue ? 0 : minRemaining,
                    ShiftName: "Agregat Periode Cuti",
                    UnitName: primaryUnitName,
                    AdvisoryMessage: finalMessage
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gagal mengevaluasi agregat kuota staf cuti untuk pegawai {WorkforceProfileId}.", workforceProfileId);
                return new StaffingCoverageEvaluationResult(
                    IsWarningTriggered: false,
                    MinimumRequiredStaff: DefaultMinimumStaffFor247Ward,
                    ScheduledStaffCount: 0,
                    RemainingStaffIfApproved: 0,
                    ShiftName: "Agregat",
                    UnitName: "Unit Terkait",
                    AdvisoryMessage: "Evaluasi kuota jadwal tidak dapat diverifikasi secara otomatis; mohon verifikasi jadwal manual."
                );
            }
        }
    }
}
