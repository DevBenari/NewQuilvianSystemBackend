using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class RosterPeriodService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RosterPeriodService> _logger;

        public RosterPeriodService(
            ApplicationDbContext context,
            ILogger<RosterPeriodService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RosterPeriodSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var periods = await _context.TrxRosterPeriods
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);

            return new RosterPeriodSummaryResponse
            {
                TotalPeriods = periods.Count,
                DraftPeriods = periods.Count(p => p.RosterStatus == "Draft" || p.RosterStatus == "Validation"),
                PublishedPeriods = periods.Count(p => p.RosterStatus == "Published"),
                LockedPeriods = periods.Count(p => p.IsLocked),
                ClosedPeriods = periods.Count(p => p.RosterStatus == "Closed")
            };
        }

        public async Task<PagedResult<RosterPeriodResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? status = null,
            Guid? departmentId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxRosterPeriods
                .AsNoTracking()
                .Include(p => p.Department)
                .Include(p => p.OrganizationUnit)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.RosterPeriodCode.ToLower().Contains(term) ||
                    p.RosterPeriodName.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.RosterStatus == status);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(p => p.DepartmentId == departmentId.Value);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(p => p.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new RosterPeriodResponse
                {
                    Id = p.Id,
                    RosterPeriodCode = p.RosterPeriodCode,
                    RosterPeriodName = p.RosterPeriodName,
                    PeriodStartDate = p.PeriodStartDate,
                    PeriodEndDate = p.PeriodEndDate,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.DepartmentName : null,
                    OrganizationUnitId = p.OrganizationUnitId,
                    OrganizationUnitName = p.OrganizationUnit != null ? p.OrganizationUnit.UnitName : null,
                    RosterStatus = p.RosterStatus,
                    VersionNumber = p.VersionNumber,
                    TotalAssignmentCount = p.TotalAssignmentCount,
                    TotalConflictCount = p.TotalConflictCount,
                    BlockingConflictCount = p.BlockingConflictCount,
                    IsValidationPassed = p.IsValidationPassed,
                    IsPublished = p.IsPublished,
                    IsLocked = p.IsLocked,
                    CreateDateTime = p.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RosterPeriodResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<SchedulingRequestServiceResult<RosterPeriodDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .AsNoTracking()
                .Include(p => p.Department)
                .Include(p => p.OrganizationUnit)
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.WorkforceProfile)
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.Position)
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                        .ThenInclude(s => s.Shift)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<RosterPeriodDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{id}' tidak ditemukan.");
            }

            var detail = new RosterPeriodDetailResponse
            {
                Id = period.Id,
                RosterPeriodCode = period.RosterPeriodCode,
                RosterPeriodName = period.RosterPeriodName,
                PeriodStartDate = period.PeriodStartDate,
                PeriodEndDate = period.PeriodEndDate,
                DepartmentId = period.DepartmentId,
                DepartmentName = period.Department?.DepartmentName,
                OrganizationUnitId = period.OrganizationUnitId,
                OrganizationUnitName = period.OrganizationUnit?.UnitName,
                RosterStatus = period.RosterStatus,
                VersionNumber = period.VersionNumber,
                TotalAssignmentCount = period.TotalAssignmentCount,
                TotalConflictCount = period.TotalConflictCount,
                BlockingConflictCount = period.BlockingConflictCount,
                IsValidationPassed = period.IsValidationPassed,
                IsPublished = period.IsPublished,
                IsLocked = period.IsLocked,
                CreateDateTime = period.CreateDateTime,
                Description = period.Description,
                ValidationSummaryJson = period.ValidationSummaryJson,
                ValidatedAt = period.ValidatedAt,
                SubmittedAt = period.SubmittedAt,
                PublishedAt = period.PublishedAt,
                ClosedAt = period.LockedAt,
                Assignments = period.RosterAssignments.Select(a => new RosterAssignmentResponse
                {
                    Id = a.Id,
                    RosterPeriodId = a.RosterPeriodId,
                    WorkforceProfileId = a.WorkforceProfileId,
                    WorkforceProfileCode = a.WorkforceProfile?.ProfileCode ?? string.Empty,
                    WorkforceDisplayName = a.WorkforceProfile?.DisplayName ?? string.Empty,
                    PositionId = a.PositionId,
                    PositionName = a.Position?.PositionName,
                    AssignmentStatus = a.AssignmentStatus,
                    PlannedShiftCount = a.PlannedShiftCount,
                    HasConflict = a.HasConflict,
                    ConflictCount = a.ConflictCount,
                    IsValidationPassed = a.IsValidationPassed,
                    ValidationResultJson = a.ValidationResultJson,
                    Notes = a.Notes,
                    ShiftAssignments = a.ShiftAssignments
                        .OrderBy(s => s.ShiftDate)
                        .ThenBy(s => s.ScheduledStartAt)
                        .Select(s => new ShiftAssignmentItemResponse
                        {
                            Id = s.Id,
                            ShiftDate = s.ShiftDate,
                            ShiftId = s.ShiftId,
                            ShiftName = s.Shift?.ShiftName,
                            ScheduledStartAt = s.ScheduledStartAt,
                            ScheduledEndAt = s.ScheduledEndAt,
                            AssignmentType = s.AssignmentType,
                            AssignmentStatus = s.AssignmentStatus,
                            IsNightShift = s.IsNightShift,
                            IsOnCall = s.IsOnCall,
                            HasDoubleShiftConflict = s.HasDoubleShiftConflict,
                            HasMinimumRestConflict = s.HasMinimumRestConflict,
                            HasBlockingConflict = s.HasBlockingConflict
                        }).ToList()
                }).ToList()
            };

            return SchedulingRequestServiceResult<RosterPeriodDetailResponse>.Ok(
                detail,
                "Detail periode roster berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<RosterPeriodResponse>> CreateAsync(
            CreateRosterPeriodRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (req.PeriodEndDate < req.PeriodStartDate)
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir periode tidak boleh lebih awal dari tanggal mulai.");
            }

            var nextSeq = await _context.TrxRosterPeriods.CountAsync(cancellationToken) + 1;
            var code = $"ROST-{DateTime.UtcNow:yyyyMM}-{nextSeq:D4}";

            var period = new TrxRosterPeriod
            {
                Id = Guid.NewGuid(),
                RosterPeriodCode = code,
                RosterPeriodName = req.RosterPeriodName.Trim(),
                PeriodStartDate = req.PeriodStartDate,
                PeriodEndDate = req.PeriodEndDate,
                DepartmentId = req.DepartmentId,
                OrganizationUnitId = req.OrganizationUnitId,
                Description = req.Description?.Trim(),
                RosterStatus = "Draft",
                VersionNumber = 1,
                IsValidationPassed = false,
                IsPublished = false,
                IsLocked = false,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxRosterPeriods.Add(period);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new RosterPeriodResponse
            {
                Id = period.Id,
                RosterPeriodCode = period.RosterPeriodCode,
                RosterPeriodName = period.RosterPeriodName,
                PeriodStartDate = period.PeriodStartDate,
                PeriodEndDate = period.PeriodEndDate,
                DepartmentId = period.DepartmentId,
                OrganizationUnitId = period.OrganizationUnitId,
                RosterStatus = period.RosterStatus,
                VersionNumber = period.VersionNumber,
                TotalAssignmentCount = 0,
                TotalConflictCount = 0,
                BlockingConflictCount = 0,
                IsValidationPassed = false,
                IsPublished = false,
                IsLocked = false,
                CreateDateTime = period.CreateDateTime
            };

            return SchedulingRequestServiceResult<RosterPeriodResponse>.Ok(
                response,
                "Periode roster berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<RosterAssignmentResponse>> AddAssignmentAsync(
            Guid rosterPeriodId,
            CreateRosterAssignmentRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan.");
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Periode roster telah ditutup atau dikunci dan tidak dapat diubah.");
            }

            var workforce = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.WorkforceProfileId && w.IsActive, cancellationToken);

            if (workforce == null)
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Profil pegawai dengan ID '{req.WorkforceProfileId}' tidak ditemukan.");
            }

            var assignment = new TrxRosterAssignment
            {
                Id = Guid.NewGuid(),
                RosterPeriodId = rosterPeriodId,
                WorkforceProfileId = req.WorkforceProfileId,
                PositionId = req.PositionId,
                AssignmentStatus = "Draft",
                Notes = req.Notes?.Trim(),
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            int totalMinutes = 0;
            int nightCount = 0;
            int onCallCount = 0;

            foreach (var shiftItem in req.Shifts)
            {
                var shift = await _context.MstShifts
                    .FirstOrDefaultAsync(s => s.Id == shiftItem.ShiftId && s.IsActive, cancellationToken);

                if (shift == null) continue;

                var startDateTime = shiftItem.ShiftDate.ToDateTime(shift.StartTime);
                var endDateTime = shift.IsOvernight || shift.EndTime < shift.StartTime
                    ? shiftItem.ShiftDate.AddDays(1).ToDateTime(shift.EndTime)
                    : shiftItem.ShiftDate.ToDateTime(shift.EndTime);

                var isNight = shiftItem.IsNightShift || shift.IsNightShift;
                var isOnCall = shiftItem.IsOnCall || shift.IsOnCall;
                var paidMinutes = shift.PaidWorkMinutes > 0
                    ? shift.PaidWorkMinutes
                    : (int)(endDateTime - startDateTime).TotalMinutes - shift.BreakDurationMinutes;

                if (paidMinutes < 0) paidMinutes = 0;

                totalMinutes += paidMinutes;
                if (isNight) nightCount++;
                if (isOnCall) onCallCount++;

                var shiftAssignment = new TrxShiftAssignment
                {
                    Id = Guid.NewGuid(),
                    RosterAssignmentId = assignment.Id,
                    WorkforceProfileId = req.WorkforceProfileId,
                    ShiftId = shift.Id,
                    ShiftDate = shiftItem.ShiftDate,
                    ScheduledStartAt = startDateTime,
                    ScheduledEndAt = endDateTime,
                    BreakDurationMinutes = shift.BreakDurationMinutes,
                    PlannedWorkMinutes = paidMinutes,
                    AssignmentType = isOnCall ? "OnCall" : "Regular",
                    AssignmentStatus = "Draft",
                    AssignmentSource = "Roster",
                    IsNightShift = isNight,
                    IsOnCall = isOnCall,
                    IsDayOff = shift.IsOffShift,
                    IsActive = true,
                    CreateBy = currentUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                assignment.ShiftAssignments.Add(shiftAssignment);
            }

            assignment.PlannedShiftCount = assignment.ShiftAssignments.Count;
            assignment.PlannedWorkMinutes = totalMinutes;
            assignment.PlannedNightShiftCount = nightCount;
            assignment.PlannedOnCallCount = onCallCount;

            _context.TrxRosterAssignments.Add(assignment);

            period.TotalAssignmentCount += 1;
            period.RosterStatus = "Draft";
            period.IsValidationPassed = false;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new RosterAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                PositionId = assignment.PositionId,
                AssignmentStatus = assignment.AssignmentStatus,
                PlannedShiftCount = assignment.PlannedShiftCount,
                HasConflict = false,
                ConflictCount = 0,
                IsValidationPassed = false,
                Notes = assignment.Notes,
                ShiftAssignments = assignment.ShiftAssignments.Select(s => new ShiftAssignmentItemResponse
                {
                    Id = s.Id,
                    ShiftDate = s.ShiftDate,
                    ShiftId = s.ShiftId,
                    ScheduledStartAt = s.ScheduledStartAt,
                    ScheduledEndAt = s.ScheduledEndAt,
                    AssignmentType = s.AssignmentType,
                    AssignmentStatus = s.AssignmentStatus,
                    IsNightShift = s.IsNightShift,
                    IsOnCall = s.IsOnCall
                }).ToList()
            };

            return SchedulingRequestServiceResult<RosterAssignmentResponse>.Ok(
                response,
                "Penugasan roster staf berhasil ditambahkan.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<RosterValidationResultResponse>> ValidateRosterPeriodAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                        .ThenInclude(s => s.Shift)
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<RosterValidationResultResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{id}' tidak ditemukan.");
            }

            var conflictMessages = new List<string>();
            int totalConflicts = 0;
            int blockingConflicts = 0;

            var minimumRestHours = 11.0; // Standar jeda istirahat wajib RS per regulasi Kemenkes / ketenagakerjaan

            foreach (var assignment in period.RosterAssignments)
            {
                var staffName = assignment.WorkforceProfile?.DisplayName ?? assignment.WorkforceProfileId.ToString();
                var shifts = assignment.ShiftAssignments.OrderBy(s => s.ScheduledStartAt).ToList();
                int staffConflictCount = 0;

                for (int i = 0; i < shifts.Count; i++)
                {
                    var currentShift = shifts[i];
                    currentShift.HasDoubleShiftConflict = false;
                    currentShift.HasMinimumRestConflict = false;
                    currentShift.HasBlockingConflict = false;

                    // 1. Cek Jadwal Ganda / Tumpang-tindih (Double Shift Conflict)
                    for (int j = 0; j < shifts.Count; j++)
                    {
                        if (i == j) continue;
                        var otherShift = shifts[j];

                        // Tumpang tindih interval waktu
                        bool overlaps = currentShift.ScheduledStartAt < otherShift.ScheduledEndAt &&
                                        otherShift.ScheduledStartAt < currentShift.ScheduledEndAt;

                        if (overlaps && !currentShift.IsDayOff && !otherShift.IsDayOff)
                        {
                            currentShift.HasDoubleShiftConflict = true;
                            currentShift.HasBlockingConflict = true;
                            staffConflictCount++;
                            totalConflicts++;
                            blockingConflicts++;

                            var msg = $"[Jadwal Ganda] Pegawai {staffName} memiliki jadwal bertabrakan pada {currentShift.ShiftDate:yyyy-MM-dd} ({currentShift.Shift?.ShiftName ?? "Shift"}) dan {otherShift.ShiftDate:yyyy-MM-dd} ({otherShift.Shift?.ShiftName ?? "Shift"}).";
                            if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                            break;
                        }
                    }

                    // 2. Cek Jeda Istirahat Wajib Minimum (Minimum Rest Conflict)
                    if (i > 0)
                    {
                        var prevShift = shifts[i - 1];
                        var restGapHours = (currentShift.ScheduledStartAt - prevShift.ScheduledEndAt).TotalHours;

                        if (restGapHours < minimumRestHours && !currentShift.IsDayOff && !prevShift.IsDayOff)
                        {
                            currentShift.HasMinimumRestConflict = true;
                            currentShift.HasBlockingConflict = true;
                            staffConflictCount++;
                            totalConflicts++;
                            blockingConflicts++;

                            var msg = $"[Jeda Istirahat Kurang] Pegawai {staffName} hanya memiliki jeda istirahat {restGapHours:F1} jam (standar minimum: {minimumRestHours} jam) antara shift {prevShift.ScheduledEndAt:yyyy-MM-dd HH:mm} dan {currentShift.ScheduledStartAt:yyyy-MM-dd HH:mm}.";
                            if (!conflictMessages.Contains(msg)) conflictMessages.Add(msg);
                        }
                    }

                    currentShift.IsValidationPassed = !currentShift.HasBlockingConflict;
                    currentShift.ValidatedAt = DateTime.UtcNow;
                    currentShift.ValidatedByUserId = currentUserId;
                }

                assignment.HasConflict = staffConflictCount > 0;
                assignment.ConflictCount = staffConflictCount;
                assignment.IsValidationPassed = staffConflictCount == 0;
                assignment.ValidatedAt = DateTime.UtcNow;
            }

            period.TotalConflictCount = totalConflicts;
            period.BlockingConflictCount = blockingConflicts;
            period.IsValidationPassed = blockingConflicts == 0;
            period.ValidatedAt = DateTime.UtcNow;
            period.ValidatedByUserId = currentUserId;
            period.RosterStatus = blockingConflicts == 0 ? "Validated" : "Draft";
            period.ValidationSummaryJson = JsonSerializer.Serialize(conflictMessages);

            await _context.SaveChangesAsync(cancellationToken);

            var result = new RosterValidationResultResponse
            {
                RosterPeriodId = period.Id,
                IsValidationPassed = period.IsValidationPassed,
                TotalConflicts = totalConflicts,
                BlockingConflicts = blockingConflicts,
                ConflictMessages = conflictMessages
            };

            return SchedulingRequestServiceResult<RosterValidationResultResponse>.Ok(
                result,
                period.IsValidationPassed
                    ? "Validasi roster selesai: seluruh jadwal lulus uji konflik dan jeda istirahat."
                    : $"Validasi roster selesai dengan {blockingConflicts} konflik pemblokiran.");
        }

        public async Task<SchedulingRequestServiceResult<RosterPeriodResponse>> PublishRosterPeriodAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{id}' tidak ditemukan.");
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Periode roster telah ditutup dan tidak dapat dipublikasikan ulang.");
            }

            if (period.BlockingConflictCount > 0 && !period.IsValidationPassed)
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Periode roster belum dapat dipublikasikan karena masih memiliki {period.BlockingConflictCount} konflik pemblokiran.");
            }

            period.RosterStatus = "Published";
            period.IsPublished = true;
            period.PublishedAt = DateTime.UtcNow;
            period.PublishedByUserId = currentUserId;

            foreach (var assignment in period.RosterAssignments)
            {
                assignment.AssignmentStatus = "Published";
                foreach (var shift in assignment.ShiftAssignments)
                {
                    shift.AssignmentStatus = "Published";
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var response = new RosterPeriodResponse
            {
                Id = period.Id,
                RosterPeriodCode = period.RosterPeriodCode,
                RosterPeriodName = period.RosterPeriodName,
                PeriodStartDate = period.PeriodStartDate,
                PeriodEndDate = period.PeriodEndDate,
                DepartmentId = period.DepartmentId,
                OrganizationUnitId = period.OrganizationUnitId,
                RosterStatus = period.RosterStatus,
                VersionNumber = period.VersionNumber,
                TotalAssignmentCount = period.TotalAssignmentCount,
                TotalConflictCount = period.TotalConflictCount,
                BlockingConflictCount = period.BlockingConflictCount,
                IsValidationPassed = period.IsValidationPassed,
                IsPublished = period.IsPublished,
                IsLocked = period.IsLocked,
                CreateDateTime = period.CreateDateTime
            };

            return SchedulingRequestServiceResult<RosterPeriodResponse>.Ok(
                response,
                "Periode roster berhasil dipublikasikan secara resmi ke seluruh staf.");
        }

        public async Task<SchedulingRequestServiceResult<RosterPeriodResponse>> CloseRosterPeriodAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{id}' tidak ditemukan.");
            }

            period.RosterStatus = "Closed";
            period.IsLocked = true;
            period.LockedAt = DateTime.UtcNow;
            period.LockedByUserId = currentUserId;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new RosterPeriodResponse
            {
                Id = period.Id,
                RosterPeriodCode = period.RosterPeriodCode,
                RosterPeriodName = period.RosterPeriodName,
                PeriodStartDate = period.PeriodStartDate,
                PeriodEndDate = period.PeriodEndDate,
                DepartmentId = period.DepartmentId,
                OrganizationUnitId = period.OrganizationUnitId,
                RosterStatus = period.RosterStatus,
                VersionNumber = period.VersionNumber,
                TotalAssignmentCount = period.TotalAssignmentCount,
                TotalConflictCount = period.TotalConflictCount,
                BlockingConflictCount = period.BlockingConflictCount,
                IsValidationPassed = period.IsValidationPassed,
                IsPublished = period.IsPublished,
                IsLocked = period.IsLocked,
                CreateDateTime = period.CreateDateTime
            };

            return SchedulingRequestServiceResult<RosterPeriodResponse>.Ok(
                response,
                "Periode roster berhasil ditutup (final closed) dan dikunci dari perubahan.");
        }
    }
}
