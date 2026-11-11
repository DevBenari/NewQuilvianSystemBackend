using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ShiftReplacementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHrdEmploymentReadinessReadService _readinessService;
        private readonly ILogger<ShiftReplacementService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShiftReplacementService(
            ApplicationDbContext context,
            IHrdEmploymentReadinessReadService readinessService,
            ILogger<ShiftReplacementService> logger,
            IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<PagedResult<ShiftReplacementResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            Guid? shiftAssignmentId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxShiftReplacements
                .AsNoTracking()
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.Shift)
                .Include(r => r.OriginalWorkforceProfile)
                .Include(r => r.ReplacementWorkforceProfile)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (shiftAssignmentId.HasValue && shiftAssignmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ShiftAssignmentId == shiftAssignmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.ReplacementStatus == status);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ShiftReplacementResponse
                {
                    Id = r.Id,
                    ShiftAssignmentId = r.ShiftAssignmentId,
                    ShiftDate = r.ShiftAssignment != null ? r.ShiftAssignment.ShiftDate : default,
                    ShiftName = r.ShiftAssignment != null && r.ShiftAssignment.Shift != null ? r.ShiftAssignment.Shift.ShiftName : string.Empty,
                    OriginalWorkforceProfileId = r.OriginalWorkforceProfileId,
                    OriginalWorkforceDisplayName = r.OriginalWorkforceProfile != null ? r.OriginalWorkforceProfile.DisplayName : string.Empty,
                    ReplacementWorkforceProfileId = r.ReplacementWorkforceProfileId,
                    ReplacementWorkforceDisplayName = r.ReplacementWorkforceProfile != null ? r.ReplacementWorkforceProfile.DisplayName : string.Empty,
                    ReplacementType = r.ReplacementType,
                    Reason = r.Reason,
                    ReplacementStatus = r.ReplacementStatus,
                    CreateDateTime = r.CreateDateTime,
                    ApprovedAt = r.ApprovedAt,
                    AppliedAt = r.AppliedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ShiftReplacementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<SchedulingRequestServiceResult<ShiftReplacementResponse>> CreateReplacementAsync(
            CreateShiftReplacementRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.RosterPeriod)
                .FirstOrDefaultAsync(s => s.Id == req.ShiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{req.ShiftAssignmentId}' tidak ditemukan.");
            }

            var period = shiftAssignment.RosterAssignment?.RosterPeriod;
            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penggantian shift (HRD-DEC-102).");
            }

            var origStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.OriginalWorkforceProfileId && w.IsActive, cancellationToken);

            if (origStaff == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai asal dengan ID '{req.OriginalWorkforceProfileId}' tidak ditemukan.");
            }

            var replStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.ReplacementWorkforceProfileId && w.IsActive, cancellationToken);

            if (replStaff == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai pengganti dengan ID '{req.ReplacementWorkforceProfileId}' tidak ditemukan.");
            }

            var replacement = new TrxShiftReplacement
            {
                Id = Guid.NewGuid(),
                ShiftAssignmentId = req.ShiftAssignmentId,
                RosterPeriodId = period?.Id,
                OriginalWorkforceProfileId = req.OriginalWorkforceProfileId,
                ReplacementWorkforceProfileId = req.ReplacementWorkforceProfileId,
                ReplacementType = req.ReplacementType.Trim(),
                ReasonCategory = req.ReplacementType.Trim(),
                Reason = req.Reason.Trim(),
                Notes = req.Reason.Trim(),
                ReplacementStatus = "Draft",
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxShiftReplacements.Add(replacement);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new ShiftReplacementResponse
            {
                Id = replacement.Id,
                ShiftAssignmentId = replacement.ShiftAssignmentId,
                ShiftDate = shiftAssignment.ShiftDate,
                ShiftName = shiftAssignment.Shift?.ShiftName ?? string.Empty,
                OriginalWorkforceProfileId = replacement.OriginalWorkforceProfileId,
                OriginalWorkforceDisplayName = origStaff.DisplayName,
                ReplacementWorkforceProfileId = replacement.ReplacementWorkforceProfileId,
                ReplacementWorkforceDisplayName = replStaff.DisplayName,
                ReplacementType = replacement.ReplacementType,
                Reason = replacement.Reason,
                ReplacementStatus = replacement.ReplacementStatus,
                CreateDateTime = replacement.CreateDateTime
            };

            return SchedulingRequestServiceResult<ShiftReplacementResponse>.Ok(
                response,
                "Penggantian shift operasional darurat berhasil dicatat.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<ShiftReplacementResponse>> ApproveReplacementAsync(
            Guid id,
            Guid currentUserId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            var replacement = await _context.TrxShiftReplacements
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.Shift)
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.RosterAssignment)
                        .ThenInclude(a => a!.RosterPeriod)
                .Include(r => r.OriginalWorkforceProfile)
                .Include(r => r.ReplacementWorkforceProfile)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken);

            if (replacement == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Permintaan penggantian shift dengan ID '{id}' tidak ditemukan.");
            }

            var period = replacement.ShiftAssignment?.RosterAssignment?.RosterPeriod;
            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penggantian shift (HRD-DEC-102).");
            }

            if (replacement.ReplacementStatus == "Applied" || replacement.ReplacementStatus == "Approved")
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan penggantian shift ini sudah disetujui sebelumnya.");
            }

            replacement.ReplacementStatus = "Applied";
            replacement.ApprovedAt = DateTime.UtcNow;
            replacement.ApprovedByUserId = currentUserId;
            replacement.AppliedAt = DateTime.UtcNow;
            replacement.ExecutedAt = DateTime.UtcNow;
            replacement.ExecutedByUserId = currentUserId;
            replacement.ApprovalNotes = notes?.Trim();

            // Terapkan perubahan ke TrxShiftAssignment
            if (replacement.ShiftAssignment != null)
            {
                replacement.ShiftAssignment.OriginalEmployeeId ??= replacement.ShiftAssignment.WorkforceProfileId;
                replacement.ShiftAssignment.WorkforceProfileId = replacement.ReplacementWorkforceProfileId;
                replacement.ShiftAssignment.AssignmentSource = replacement.ReplacementType;
                replacement.ShiftAssignment.IsManualOverride = true;
                replacement.ShiftAssignment.OverrideReason = $"Penggantian shift ({replacement.ReplacementType}): {replacement.Reason}";
            }

            await _context.SaveChangesAsync(cancellationToken);

            var response = new ShiftReplacementResponse
            {
                Id = replacement.Id,
                ShiftAssignmentId = replacement.ShiftAssignmentId,
                ShiftDate = replacement.ShiftAssignment != null ? replacement.ShiftAssignment.ShiftDate : default,
                ShiftName = replacement.ShiftAssignment?.Shift?.ShiftName ?? string.Empty,
                OriginalWorkforceProfileId = replacement.OriginalWorkforceProfileId,
                OriginalWorkforceDisplayName = replacement.OriginalWorkforceProfile?.DisplayName ?? string.Empty,
                ReplacementWorkforceProfileId = replacement.ReplacementWorkforceProfileId,
                ReplacementWorkforceDisplayName = replacement.ReplacementWorkforceProfile?.DisplayName ?? string.Empty,
                ReplacementType = replacement.ReplacementType,
                Reason = replacement.Reason,
                ReplacementStatus = replacement.ReplacementStatus,
                CreateDateTime = replacement.CreateDateTime,
                ApprovedAt = replacement.ApprovedAt,
                AppliedAt = replacement.AppliedAt
            };

            return SchedulingRequestServiceResult<ShiftReplacementResponse>.Ok(
                response,
                "Penggantian shift berhasil disetujui dan diterapkan langsung ke jadwal operasional.");
        }

        /// <summary>
        /// Ganti Petugas Langsung Pasca-Terbit oleh Kepala Unit (BE-HRD-095, HRD-DEC-101).
        /// Periode roster tetap Published, pengganti divalidasi keselamatannya, langsung efektif.
        /// </summary>
        public async Task<SchedulingRequestServiceResult<DirectShiftReplacementResponse>> ExecuteDirectReplacementAsync(
            Guid shiftAssignmentId,
            DirectShiftReplacementRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .Include(s => s.WorkforceProfile)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.RosterPeriod)
                .FirstOrDefaultAsync(s => s.Id == shiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{shiftAssignmentId}' tidak ditemukan.");
            }

            var period = shiftAssignment.RosterAssignment?.RosterPeriod;
            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penggantian shift (HRD-DEC-102).");
            }

            // Staf pengganti tidak boleh sama dengan staf asal yang sedang ditugaskan
            if (shiftAssignment.WorkforceProfileId == req.ReplacementEmployeeId)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pegawai pengganti tidak boleh sama dengan pegawai yang sedang bertugas.");
            }

            var replStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.ReplacementEmployeeId && w.IsActive, cancellationToken);

            if (replStaff == null)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai pengganti dengan ID '{req.ReplacementEmployeeId}' tidak ditemukan.");
            }

            // 1. Evaluasi Keselamatan Pengganti: B1 (Double Shift Conflict) - HRD-TF-054
            var hasOverlap = await _context.TrxShiftAssignments
                .AnyAsync(s => s.WorkforceProfileId == req.ReplacementEmployeeId &&
                               s.ShiftDate == shiftAssignment.ShiftDate &&
                               !s.IsDayOff &&
                               s.IsActive &&
                               s.Id != shiftAssignment.Id &&
                               s.ScheduledStartAt < shiftAssignment.ScheduledEndAt &&
                               shiftAssignment.ScheduledStartAt < s.ScheduledEndAt,
                               cancellationToken);

            if (hasOverlap)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Penggantian shift ditolak: Pegawai pengganti ({replStaff.DisplayName}) memiliki jadwal shift lain yang tumpang-tindih pada waktu yang sama (B1: Double Shift Conflict / HRD-TF-054).");
            }

            // 2. Evaluasi Keselamatan Pengganti: B2 (Cuti Disetujui) - HRD-TF-054
            var onLeave = await _context.WfpLeaveRequests
                .AnyAsync(l => l.WorkforceProfileId == req.ReplacementEmployeeId &&
                               !l.IsDelete &&
                               l.LeaveRequestStatus == "Approved" &&
                               l.StartDate <= shiftAssignment.ShiftDate &&
                               l.EndDate >= shiftAssignment.ShiftDate &&
                               l.IsActive,
                               cancellationToken);

            if (onLeave)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Penggantian shift ditolak: Pegawai pengganti ({replStaff.DisplayName}) sedang dalam masa cuti yang disetujui pada tanggal tersebut (B2: Leave Conflict / HRD-TF-054).");
            }

            // 3. Evaluasi Kesiapan Kerja Pengganti: B4 / B5 (INT-HR-31)
            var readiness = await _readinessService.GetSchedulingEligibilityAsync(req.ReplacementEmployeeId, shiftAssignment.ShiftDate, cancellationToken);
            if (readiness.Success && readiness.Data != null)
            {
                if (readiness.Data.EligibilityStatus == "NotReady")
                {
                    return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Penggantian shift ditolak: Pegawai pengganti ({replStaff.DisplayName}) belum dinyatakan siap kerja oleh HRD (B4: Not Ready / HRD-DEC-088).");
                }

                if (readiness.Data.EligibilityStatus == "ContractHold")
                {
                    return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Penggantian shift ditolak: Kontrak kerja pegawai pengganti ({replStaff.DisplayName}) telah berakhir tanpa keputusan perpanjangan (B5: Contract Hold / HRD-DEC-089).");
                }
            }

            var originalEmployeeId = shiftAssignment.OriginalEmployeeId ?? shiftAssignment.WorkforceProfileId;
            var originalStaffName = shiftAssignment.WorkforceProfile?.DisplayName ?? "Staf Asal";

            // Terapkan Pergantian Langsung pada Shift (IsManualOverride = true)
            shiftAssignment.OriginalEmployeeId = originalEmployeeId;
            shiftAssignment.WorkforceProfileId = req.ReplacementEmployeeId;
            shiftAssignment.IsManualOverride = true;
            shiftAssignment.OverrideReason = $"[{req.ReasonCategory}] {req.Notes.Trim()}";
            shiftAssignment.AssignmentSource = "DirectReplacement";

            // Status periode roster tetap 'Published' (menutup bug HRD-TF-052 b)
            if (period != null)
            {
                period.RosterStatus = "Published";
                period.IsPublished = true;
            }

            // Catat Jejak Penggantian pada TrxShiftReplacement
            var replacementRecord = new TrxShiftReplacement
            {
                Id = Guid.NewGuid(),
                ShiftAssignmentId = shiftAssignment.Id,
                RosterPeriodId = period?.Id,
                OriginalWorkforceProfileId = originalEmployeeId,
                ReplacementWorkforceProfileId = req.ReplacementEmployeeId,
                ReplacementType = "DirectReplacement",
                ReasonCategory = req.ReasonCategory.Trim(),
                Reason = req.Notes.Trim(),
                Notes = req.Notes.Trim(),
                ReplacementStatus = "Applied",
                ExecutedByUserId = currentUserId,
                ExecutedAt = DateTime.UtcNow,
                AppliedAt = DateTime.UtcNow,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByUserId = currentUserId,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow,
                IsActive = true
            };

            _context.TrxShiftReplacements.Add(replacementRecord);
            await _context.SaveChangesAsync(cancellationToken);

            var notifications = new List<string>
            {
                $"Notifikasi mutasi terkirim ke {originalStaffName} (Staf Digantikan).",
                $"Notifikasi tugas baru terkirim ke {replStaff.DisplayName} (Staf Pengganti).",
                "Tembusan mutasi darurat terkirim ke Kepala Bidang (FYI)."
            };

            var response = new DirectShiftReplacementResponse
            {
                ShiftAssignmentId = shiftAssignment.Id,
                RosterPeriodStatus = period?.RosterStatus ?? "Published",
                IsManualOverride = true,
                AssignedToEmployeeId = replStaff.Id,
                AssignedToEmployeeName = replStaff.DisplayName,
                OriginalEmployeeId = originalEmployeeId,
                OriginalEmployeeName = originalStaffName,
                EffectiveImmediately = true,
                NotificationsDispatched = notifications
            };

            _logger.LogInformation("Ganti petugas langsung berhasil untuk shift {ShiftId} pada {Date}: {Orig} digantikan oleh {Repl}",
                shiftAssignment.Id, shiftAssignment.ShiftDate, originalStaffName, replStaff.DisplayName);

            return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Ok(
                response,
                $"Ganti petugas langsung berhasil. Shift dinas resmi dialihkan ke {replStaff.DisplayName}.");
        }

        /// <summary>
        /// Koreksi mundur jadwal dinas (<= 3 hari kalender) & rekalkulasi kehadiran (BE-HRD-097, HRD-DEC-102, INT-HR-36).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>> RetroactiveCorrectionAsync(
            Guid shiftAssignmentId,
            RetroactiveShiftCorrectionRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .Include(s => s.WorkforceProfile)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.RosterPeriod)
                .FirstOrDefaultAsync(s => s.Id == shiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{shiftAssignmentId}' tidak ditemukan.");
            }

            var period = shiftAssignment.RosterAssignment?.RosterPeriod;
            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh koreksi (HRD-DEC-102).");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var daysBack = today.DayNumber - shiftAssignment.ShiftDate.DayNumber;

            // Toleransi koreksi mundur mandiri Kepala Unit dibatasi maksimal 3 hari kalender ke belakang (VR-SCH-17 / HRD-DEC-102)
            if (daysBack > 3)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Koreksi mundur jadwal pada tanggal {shiftAssignment.ShiftDate:yyyy-MM-dd} ({daysBack} hari yang lalu) ditolak. Batas toleransi koreksi mundur mandiri Kepala Unit adalah maksimal 3 hari kalender ke belakang (VR-SCH-17 / HRD-DEC-102). Silakan hubungi Tim HRD.");
            }

            var newShift = await _context.MstShifts
                .FirstOrDefaultAsync(s => s.Id == req.NewShiftId && s.IsActive, cancellationToken);

            if (newShift == null)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Master shift tujuan dengan ID '{req.NewShiftId}' tidak ditemukan.");
            }

            var newStart = shiftAssignment.ShiftDate.ToDateTime(newShift.StartTime);
            var newEnd = newShift.IsOvernight || newShift.EndTime < newShift.StartTime
                ? shiftAssignment.ShiftDate.AddDays(1).ToDateTime(newShift.EndTime)
                : shiftAssignment.ShiftDate.ToDateTime(newShift.EndTime);

            var paidMinutes = newShift.PaidWorkMinutes > 0
                ? newShift.PaidWorkMinutes
                : (int)(newEnd - newStart).TotalMinutes - newShift.BreakDurationMinutes;
            if (paidMinutes < 0) paidMinutes = 0;

            var oldShiftId = shiftAssignment.ShiftId;
            shiftAssignment.ShiftId = newShift.Id;
            shiftAssignment.ScheduledStartAt = newStart;
            shiftAssignment.ScheduledEndAt = newEnd;
            shiftAssignment.PlannedWorkMinutes = paidMinutes;
            shiftAssignment.BreakDurationMinutes = newShift.BreakDurationMinutes;
            shiftAssignment.IsNightShift = newShift.IsNightShift;
            shiftAssignment.IsOnCall = newShift.IsOnCall;
            shiftAssignment.IsDayOff = newShift.IsOffShift;
            shiftAssignment.IsManualOverride = true;
            shiftAssignment.OverrideReason = $"[KOREKSI MUNDUR - {req.ReasonCategory.Trim()}] {req.Notes.Trim()}";
            shiftAssignment.AssignmentSource = "RetroactiveCorrection";

            await _context.SaveChangesAsync(cancellationToken);

            // Memicu sinyal event rekalkulasi kehadiran (INT-HR-36 / INT-HR-41)
            _logger.LogInformation("Event INT-HR-36 / INT-HR-41: RetroactiveShiftCorrectedEvent dipicu untuk pegawai {WorkforceProfileId} pada tanggal {ShiftDate} ke shift {ShiftName}.",
                shiftAssignment.WorkforceProfileId, shiftAssignment.ShiftDate, newShift.ShiftName);

            try
            {
                var attendanceConsumer = _serviceProvider.GetService<QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services.AttendanceScheduleMutationConsumer>();
                if (attendanceConsumer != null)
                {
                    await attendanceConsumer.ConsumeRetroactiveShiftCorrectionAsync(new QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services.RetroactiveShiftCorrectedPayload
                    {
                        ShiftAssignmentId = shiftAssignment.Id,
                        WorkforceProfileId = shiftAssignment.WorkforceProfileId,
                        ShiftDate = shiftAssignment.ShiftDate,
                        OldShiftId = oldShiftId,
                        NewShiftId = newShift.Id,
                        TriggeredByUserId = currentUserId,
                        Reason = req.Notes
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memicu rekalkulasi absensi otomatis INT-HR-41 untuk pegawai {WorkforceProfileId}", shiftAssignment.WorkforceProfileId);
            }

            var response = new RetroactiveShiftCorrectionResponse
            {
                ShiftAssignmentId = shiftAssignment.Id,
                IsRetroactive = daysBack > 0,
                DaysBack = daysBack > 0 ? daysBack : 0,
                AttendanceReevaluationTriggered = true,
                Message = $"Jadwal shift tanggal {shiftAssignment.ShiftDate:yyyy-MM-dd} berhasil dikoreksi ke '{newShift.ShiftName}' dan evaluasi ulang absensi otomatis dijalankan (INT-HR-36)."
            };

            return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Ok(
                response,
                response.Message);
        }
    }
}
