using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Mesin validasi keselamatan jadwal dinas rumah sakit (Roster Validation Engine).
    /// Mengevaluasi aturan memblokir B1–B7 dan peringatan kerja W1–W5.
    /// Mengakomodasi HRD-DEC-099, HRD-DEC-100, INT-HR-31, VR-SCH-01 s.d. VR-SCH-07, VR-SCH-80.
    /// </summary>
    public class RosterValidationService : IRosterValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHrdEmploymentReadinessReadService _readinessService;
        private readonly ISchedulingSettingService _settingService;
        private readonly ILogger<RosterValidationService> _logger;

        public RosterValidationService(
            ApplicationDbContext context,
            IHrdEmploymentReadinessReadService readinessService,
            ISchedulingSettingService settingService,
            ILogger<RosterValidationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RosterValidationResultResponse> ValidateRosterPeriodAsync(
            Guid rosterPeriodId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                        .ThenInclude(s => s.Shift)
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return new RosterValidationResultResponse
                {
                    RosterPeriodId = rosterPeriodId,
                    IsValidationPassed = false,
                    TotalConflicts = 1,
                    BlockingConflicts = 1,
                    ConflictMessages = new List<string> { $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan." }
                };
            }

            // Ambil parameter konfigurasi aturan waktu kerja RS
            var siteId = period.HospitalSiteId ?? Guid.Empty;
            var settingResult = await _settingService.GetSettingAsync(siteId, cancellationToken);
            var setting = settingResult.Data ?? new SchedulingSettingResponse();

            var minRestHours = setting.MinRestHoursBetweenShifts;
            var maxWorkDays = setting.MaxConsecutiveWorkDays;
            var maxNightShifts = setting.MaxConsecutiveNightShifts;
            var restHoursBlocked = setting.RestHoursEscalatedToBlock;
            var workDaysBlocked = setting.ConsecutiveDaysEscalatedToBlock;
            var nightShiftsBlocked = setting.NightShiftsEscalatedToBlock;

            var conflictMessages = new List<string>();
            var warningMessages = new List<string>();
            var conflictItems = new List<RosterValidationConflictItemDto>();

            int totalConflicts = 0;
            int blockingConflicts = 0;
            int warningConflicts = 0;

            // Kumpulkan seluruh shift untuk evaluasi B7 (Skill Mix / Pendampingan)
            var allActiveShifts = new List<TrxShiftAssignment>();

            // 1. Evaluasi per Staf / Pegawai
            foreach (var assignment in period.RosterAssignments)
            {
                var staffId = assignment.WorkforceProfileId;
                var staffName = assignment.WorkforceProfile?.DisplayName ?? staffId.ToString();
                var shifts = assignment.ShiftAssignments.OrderBy(s => s.ScheduledStartAt).ToList();

                for (int i = 0; i < shifts.Count; i++)
                {
                    var currentShift = shifts[i];
                    allActiveShifts.Add(currentShift);

                    currentShift.HasDoubleShiftConflict = false;
                    currentShift.HasLeaveConflict = false;
                    currentShift.HasMinimumRestConflict = false;
                    currentShift.HasWorkHourLimitConflict = false;
                    currentShift.HasBlockingConflict = false;

                    var shiftDate = currentShift.ShiftDate;

                    // --- B1: Tabrakan Jam Shift (Double Shift) ---
                    for (int j = 0; j < shifts.Count; j++)
                    {
                        if (i == j) continue;
                        var other = shifts[j];
                        bool overlaps = currentShift.ScheduledStartAt < other.ScheduledEndAt &&
                                        other.ScheduledStartAt < currentShift.ScheduledEndAt;

                        if (overlaps && !currentShift.IsDayOff && !other.IsDayOff)
                        {
                            currentShift.HasDoubleShiftConflict = true;
                            currentShift.HasBlockingConflict = true;
                            blockingConflicts++;
                            totalConflicts++;

                            var msg = $"VR-SCH-01 (B1): Pegawai {staffName} dijadwalkan dua shift yang bertabrakan pada {shiftDate:dd-MM-yyyy} ({currentShift.Shift?.ShiftName ?? "Shift"}) dan {other.ShiftDate:dd-MM-yyyy} ({other.Shift?.ShiftName ?? "Shift"}).";
                            if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                            conflictItems.Add(new RosterValidationConflictItemDto
                            {
                                ShiftAssignmentId = currentShift.Id,
                                WorkforceProfileId = staffId,
                                StaffName = staffName,
                                ShiftDate = shiftDate,
                                RuleCode = "B1",
                                Severity = "BLOCKING",
                                Message = msg
                            });
                            break;
                        }
                    }

                    // --- B2: Cuti Disetujui ---
                    var hasApprovedLeave = await _context.WfpLeaveRequests
                        .AnyAsync(l => l.WorkforceProfileId == staffId &&
                                       !l.IsDelete &&
                                       l.LeaveRequestStatus == "Approved" &&
                                       l.StartDate <= shiftDate &&
                                       l.EndDate >= shiftDate, cancellationToken);

                    if (hasApprovedLeave && !currentShift.IsDayOff)
                    {
                        currentShift.HasLeaveConflict = true;
                        currentShift.HasBlockingConflict = true;
                        blockingConflicts++;
                        totalConflicts++;

                        var msg = $"VR-SCH-02 (B2): Pegawai {staffName} sedang cuti yang telah disetujui pada {shiftDate:dd-MM-yyyy}.";
                        if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                        conflictItems.Add(new RosterValidationConflictItemDto
                        {
                            ShiftAssignmentId = currentShift.Id,
                            WorkforceProfileId = staffId,
                            StaffName = staffName,
                            ShiftDate = shiftDate,
                            RuleCode = "B2",
                            Severity = "BLOCKING",
                            Message = msg
                        });
                    }

                    // --- W4: Cuti Pending ---
                    var hasPendingLeave = await _context.WfpLeaveRequests
                        .AnyAsync(l => l.WorkforceProfileId == staffId &&
                                       !l.IsDelete &&
                                       (l.LeaveRequestStatus == "Submitted" ||
                                        l.LeaveRequestStatus == "WaitingSupervisorApproval" ||
                                        l.LeaveRequestStatus == "WaitingManagerApproval" ||
                                        l.LeaveRequestStatus == "WaitingHrVerification") &&
                                       l.StartDate <= shiftDate &&
                                       l.EndDate >= shiftDate, cancellationToken);

                    if (hasPendingLeave && !currentShift.IsDayOff)
                    {
                        warningConflicts++;
                        totalConflicts++;
                        var msg = $"W4: Pegawai {staffName} memiliki pengajuan cuti yang masih berstatus pending pada {shiftDate:dd-MM-yyyy}.";
                        if (!warningMessages.Contains(msg)) warningMessages.Add(msg);
                        conflictItems.Add(new RosterValidationConflictItemDto
                        {
                            ShiftAssignmentId = currentShift.Id,
                            WorkforceProfileId = staffId,
                            StaffName = staffName,
                            ShiftDate = shiftDate,
                            RuleCode = "W4",
                            Severity = "WARNING",
                            Message = msg
                        });
                    }

                    // --- B4, B5, B6 & W5: Evaluasi Kesiapan Kerja (INT-HR-31) ---
                    try
                    {
                        var readiness = await _readinessService.GetSchedulingEligibilityAsync(staffId, shiftDate, cancellationToken);
                        if (!readiness.Success || readiness.Data == null)
                        {
                            // B6: Status kesiapan gagal dibaca (gagal tertutup)
                            currentShift.HasBlockingConflict = true;
                            blockingConflicts++;
                            totalConflicts++;
                            var msg = $"VR-SCH-06 (B6): Status kesiapan kerja pegawai {staffName} gagal dibaca dari sistem (INT-HR-31).";
                            if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                            conflictItems.Add(new RosterValidationConflictItemDto
                            {
                                ShiftAssignmentId = currentShift.Id,
                                WorkforceProfileId = staffId,
                                StaffName = staffName,
                                ShiftDate = shiftDate,
                                RuleCode = "B6",
                                Severity = "BLOCKING",
                                Message = msg
                            });
                        }
                        else
                        {
                            var r = readiness.Data;
                            if (r.EligibilityStatus == "NotReady")
                            {
                                // B4: Belum siap kerja
                                currentShift.HasBlockingConflict = true;
                                blockingConflicts++;
                                totalConflicts++;
                                var msg = $"VR-SCH-04 (B4): Pegawai {staffName} belum dinyatakan siap kerja oleh HRD (HRD-DEC-088).";
                                if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                                conflictItems.Add(new RosterValidationConflictItemDto
                                {
                                    ShiftAssignmentId = currentShift.Id,
                                    WorkforceProfileId = staffId,
                                    StaffName = staffName,
                                    ShiftDate = shiftDate,
                                    RuleCode = "B4",
                                    Severity = "BLOCKING",
                                    Message = msg
                                });
                            }
                            else if (r.EligibilityStatus == "ContractHold")
                            {
                                // B5: Kontrak berakhir tanpa keputusan
                                currentShift.HasBlockingConflict = true;
                                blockingConflicts++;
                                totalConflicts++;
                                var msg = $"VR-SCH-05 (B5): Tanggal shift {shiftDate:dd-MM-yyyy} berada sesudah tanggal akhir kontrak {staffName} yang belum diputuskan (HRD-DEC-089).";
                                if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                                conflictItems.Add(new RosterValidationConflictItemDto
                                {
                                    ShiftAssignmentId = currentShift.Id,
                                    WorkforceProfileId = staffId,
                                    StaffName = staffName,
                                    ShiftDate = shiftDate,
                                    RuleCode = "B5",
                                    Severity = "BLOCKING",
                                    Message = msg
                                });
                            }

                            // Tandai Didampingi
                            if (r.IsAccompanied)
                            {
                                currentShift.IsAccompanied = true;
                            }

                            // W5: Kredensial kedaluwarsa
                            if (!string.IsNullOrWhiteSpace(r.CredentialWarning))
                            {
                                warningConflicts++;
                                totalConflicts++;
                                var msg = $"W5: Pegawai {staffName} memiliki peringatan kredensial pada {shiftDate:dd-MM-yyyy}: {r.CredentialWarning}.";
                                if (!warningMessages.Contains(msg)) warningMessages.Add(msg);
                                conflictItems.Add(new RosterValidationConflictItemDto
                                {
                                    ShiftAssignmentId = currentShift.Id,
                                    WorkforceProfileId = staffId,
                                    StaffName = staffName,
                                    ShiftDate = shiftDate,
                                    RuleCode = "W5",
                                    Severity = "WARNING",
                                    Message = msg
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Gagal memanggil HrdEmploymentReadinessReadService untuk pegawai {StaffId}", staffId);
                        currentShift.HasBlockingConflict = true;
                        blockingConflicts++;
                        totalConflicts++;
                        var msg = $"VR-SCH-06 (B6): Status kesiapan kerja pegawai {staffName} gagal dibaca dari sistem (INT-HR-31).";
                        if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                    }

                    // --- W1: Jeda Istirahat Minimum Antar Shift ---
                    if (i > 0)
                    {
                        var prevShift = shifts[i - 1];
                        var gapHours = (currentShift.ScheduledStartAt - prevShift.ScheduledEndAt).TotalHours;

                        if (gapHours < minRestHours && !currentShift.IsDayOff && !prevShift.IsDayOff)
                        {
                            currentShift.HasMinimumRestConflict = true;
                            totalConflicts++;

                            if (restHoursBlocked)
                            {
                                currentShift.HasBlockingConflict = true;
                                blockingConflicts++;
                                var msg = $"B (Eskalasi W1): Jeda istirahat pegawai {staffName} ({gapHours:F1} jam) kurang dari standar {minRestHours} jam antara shift {prevShift.ScheduledEndAt:dd-MM-yyyy HH:mm} dan {currentShift.ScheduledStartAt:dd-MM-yyyy HH:mm}.";
                                if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                                conflictItems.Add(new RosterValidationConflictItemDto
                                {
                                    ShiftAssignmentId = currentShift.Id,
                                    WorkforceProfileId = staffId,
                                    StaffName = staffName,
                                    ShiftDate = shiftDate,
                                    RuleCode = "W1",
                                    Severity = "BLOCKING",
                                    Message = msg
                                });
                            }
                            else
                            {
                                warningConflicts++;
                                var msg = $"W1: Jeda istirahat pegawai {staffName} ({gapHours:F1} jam) kurang dari anjuran {minRestHours} jam antara {prevShift.ScheduledEndAt:dd-MM-yyyy HH:mm} dan {currentShift.ScheduledStartAt:dd-MM-yyyy HH:mm}.";
                                if (!warningMessages.Contains(msg)) warningMessages.Add(msg);
                                conflictItems.Add(new RosterValidationConflictItemDto
                                {
                                    ShiftAssignmentId = currentShift.Id,
                                    WorkforceProfileId = staffId,
                                    StaffName = staffName,
                                    ShiftDate = shiftDate,
                                    RuleCode = "W1",
                                    Severity = "WARNING",
                                    Message = msg
                                });
                            }
                        }
                    }

                    currentShift.IsValidationPassed = !currentShift.HasBlockingConflict;
                    currentShift.ValidatedAt = DateTime.UtcNow;
                    currentShift.ValidatedByUserId = currentUserId;
                }

                // --- W2: Hari Kerja Berturut-turut > MaxConsecutiveWorkDays ---
                int consecutiveWorkDays = 0;
                foreach (var shift in shifts)
                {
                    if (!shift.IsDayOff)
                    {
                        consecutiveWorkDays++;
                        if (consecutiveWorkDays > maxWorkDays)
                        {
                            shift.HasWorkHourLimitConflict = true;
                            totalConflicts++;

                            if (workDaysBlocked)
                            {
                                shift.HasBlockingConflict = true;
                                blockingConflicts++;
                                var msg = $"B (Eskalasi W2): Pegawai {staffName} bekerja {consecutiveWorkDays} hari berturut-turut tanpa libur pada {shift.ShiftDate:dd-MM-yyyy} (Batas maksimal: {maxWorkDays} hari).";
                                if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                            }
                            else
                            {
                                warningConflicts++;
                                var msg = $"W2: Pegawai {staffName} bekerja {consecutiveWorkDays} hari berturut-turut tanpa libur pada {shift.ShiftDate:dd-MM-yyyy} (Batas anjuran: {maxWorkDays} hari).";
                                if (!warningMessages.Contains(msg)) warningMessages.Add(msg);
                            }
                        }
                    }
                    else
                    {
                        consecutiveWorkDays = 0;
                    }
                }

                // --- W3: Shift Malam Berturut-turut > MaxConsecutiveNightShifts ---
                int consecutiveNights = 0;
                foreach (var shift in shifts)
                {
                    if (shift.IsNightShift && !shift.IsDayOff)
                    {
                        consecutiveNights++;
                        if (consecutiveNights > maxNightShifts)
                        {
                            totalConflicts++;
                            if (nightShiftsBlocked)
                            {
                                shift.HasBlockingConflict = true;
                                blockingConflicts++;
                                var msg = $"B (Eskalasi W3): Pegawai {staffName} dijadwalkan shift malam {consecutiveNights} kali berturut-turut pada {shift.ShiftDate:dd-MM-yyyy} (Batas maksimal: {maxNightShifts} malam).";
                                if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                            }
                            else
                            {
                                warningConflicts++;
                                var msg = $"W3: Pegawai {staffName} dijadwalkan shift malam {consecutiveNights} kali berturut-turut pada {shift.ShiftDate:dd-MM-yyyy} (Batas anjuran: {maxNightShifts} malam).";
                                if (!warningMessages.Contains(msg)) warningMessages.Add(msg);
                            }
                        }
                    }
                    else
                    {
                        consecutiveNights = 0;
                    }
                }
            }

            // 2. Evaluasi B7: Staf Didampingi Tanpa Perawat Mandiri (HRD-DEC-100)
            var shiftsBySlot = allActiveShifts
                .Where(s => !s.IsDayOff && s.ShiftId.HasValue)
                .GroupBy(s => new { s.ShiftDate, s.ShiftId });

            foreach (var slot in shiftsBySlot)
            {
                var slotShifts = slot.ToList();
                var accompaniedStaff = slotShifts.Where(s => s.IsAccompanied).ToList();
                var independentStaff = slotShifts.Where(s => !s.IsAccompanied).ToList();

                if (accompaniedStaff.Any() && !independentStaff.Any())
                {
                    // Tidak ada satupun perawat mandiri pada slot shift ini!
                    foreach (var acc in accompaniedStaff)
                    {
                        acc.HasBlockingConflict = true;
                        acc.IsValidationPassed = false;
                        blockingConflicts++;
                        totalConflicts++;

                        var accName = period.RosterAssignments
                            .FirstOrDefault(a => a.ShiftAssignments.Any(s => s.Id == acc.Id))
                            ?.WorkforceProfile?.DisplayName ?? "Staf";

                        var msg = $"VR-SCH-07 (B7): Shift {slot.Key.ShiftDate:dd-MM-yyyy} memiliki staf Didampingi ({accName}) tanpa ada perawat Mandiri yang bertugas bersama (HRD-DEC-100). Ditolak demi keselamatan pasien.";
                        if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                        conflictItems.Add(new RosterValidationConflictItemDto
                        {
                            ShiftAssignmentId = acc.Id,
                            WorkforceProfileId = acc.WorkforceProfileId,
                            StaffName = accName,
                            ShiftDate = slot.Key.ShiftDate,
                            RuleCode = "B7",
                            Severity = "BLOCKING",
                            Message = msg
                        });
                    }
                }
            }

            // 3. Simpan Hasil Ringkasan Validasi ke Entitas Periode
            period.TotalConflictCount = totalConflicts;
            period.BlockingConflictCount = blockingConflicts;
            period.WarningCount = warningConflicts;
            period.IsValidationPassed = blockingConflicts == 0;
            period.ValidatedAt = DateTime.UtcNow;
            period.ValidatedByUserId = currentUserId;

            var response = new RosterValidationResultResponse
            {
                RosterPeriodId = rosterPeriodId,
                IsValidationPassed = period.IsValidationPassed,
                TotalConflicts = totalConflicts,
                BlockingConflicts = blockingConflicts,
                WarningConflicts = warningConflicts,
                ConflictMessages = conflictMessages,
                WarningMessages = warningMessages,
                ConflictItems = conflictItems
            };

            period.ValidationSummaryJson = JsonSerializer.Serialize(response);
            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }

        public async Task<RosterValidationConflictItemDto?> ValidateSingleShiftAsync(
            Guid shiftAssignmentId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shift = await _context.TrxShiftAssignments
                .Include(s => s.WorkforceProfile)
                .Include(s => s.Shift)
                .FirstOrDefaultAsync(s => s.Id == shiftAssignmentId && !s.IsDelete, cancellationToken);

            if (shift == null) return null;

            // Evaluasi cepat kesiapan kerja staf pengganti
            var readiness = await _readinessService.GetSchedulingEligibilityAsync(shift.WorkforceProfileId, shift.ShiftDate, cancellationToken);
            if (!readiness.Success || readiness.Data == null)
            {
                return new RosterValidationConflictItemDto
                {
                    ShiftAssignmentId = shift.Id,
                    WorkforceProfileId = shift.WorkforceProfileId,
                    StaffName = shift.WorkforceProfile?.DisplayName ?? string.Empty,
                    ShiftDate = shift.ShiftDate,
                    RuleCode = "B6",
                    Severity = "BLOCKING",
                    Message = "VR-SCH-06 (B6): Status kesiapan kerja pegawai gagal dibaca (INT-HR-31)."
                };
            }

            if (readiness.Data.EligibilityStatus == "NotReady")
            {
                return new RosterValidationConflictItemDto
                {
                    ShiftAssignmentId = shift.Id,
                    WorkforceProfileId = shift.WorkforceProfileId,
                    StaffName = shift.WorkforceProfile?.DisplayName ?? string.Empty,
                    ShiftDate = shift.ShiftDate,
                    RuleCode = "B4",
                    Severity = "BLOCKING",
                    Message = "VR-SCH-04 (B4): Pegawai belum dinyatakan siap kerja oleh HRD (HRD-DEC-088)."
                };
            }

            if (readiness.Data.EligibilityStatus == "ContractHold")
            {
                return new RosterValidationConflictItemDto
                {
                    ShiftAssignmentId = shift.Id,
                    WorkforceProfileId = shift.WorkforceProfileId,
                    StaffName = shift.WorkforceProfile?.DisplayName ?? string.Empty,
                    ShiftDate = shift.ShiftDate,
                    RuleCode = "B5",
                    Severity = "BLOCKING",
                    Message = "VR-SCH-05 (B5): Kontrak pegawai berakhir tanpa keputusan perpanjangan (HRD-DEC-089)."
                };
            }

            return null;
        }
    }
}
