using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class RosterPeriodService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRosterValidationService _rosterValidationService;
        private readonly IHrdEmploymentReadinessReadService _readinessService;
        private readonly ILogger<RosterPeriodService> _logger;

        public RosterPeriodService(
            ApplicationDbContext context,
            IRosterValidationService rosterValidationService,
            IHrdEmploymentReadinessReadService readinessService,
            ILogger<RosterPeriodService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _rosterValidationService = rosterValidationService ?? throw new ArgumentNullException(nameof(rosterValidationService));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                ClosedAt = period.ClosedAt ?? period.LockedAt,
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
                IsRecheckPassed = false,
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
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penambahan, pengubahan, dan penggantian shift (HRD-DEC-102).");
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
            period.IsRecheckPassed = false;

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

        /// <summary>
        /// Pengisian otomatis staf penempatan aktif unit kerja & status pendampingan klinis (BE-HRD-091).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<PopulateEligibleStaffResponse>> PopulateEligibleStaffAsync(
            Guid rosterPeriodId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<PopulateEligibleStaffResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan.");
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<PopulateEligibleStaffResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penambahan, pengubahan, dan penggantian shift (HRD-DEC-102).");
            }

            // 1. Ambil daftar penempatan staf aktif pada unit/departemen periode ini
            var orgAssignments = await _context.WfpOrganizationAssignments
                .AsNoTracking()
                .Include(a => a.WorkforceProfile)
                .Where(a => a.IsActive &&
                    (!period.OrganizationUnitId.HasValue || a.OrganizationUnitId == period.OrganizationUnitId) &&
                    (!period.DepartmentId.HasValue || a.DepartmentId == period.DepartmentId) &&
                    a.EffectiveStartDate <= period.PeriodEndDate.ToDateTime(TimeOnly.MaxValue) &&
                    (a.EffectiveEndDate == null || a.EffectiveEndDate >= period.PeriodStartDate.ToDateTime(TimeOnly.MinValue)))
                .ToListAsync(cancellationToken);

            var candidateProfiles = orgAssignments
                .Where(a => a.WorkforceProfile != null && a.WorkforceProfile.IsActive)
                .GroupBy(a => a.WorkforceProfileId)
                .Select(g => new
                {
                    Profile = g.First().WorkforceProfile!,
                    PositionId = g.First().PositionId
                })
                .ToList();

            // Fallback jika tidak ada WfpOrganizationAssignment: gunakan PrimaryDepartmentId pada MstWorkforceProfile
            if (!candidateProfiles.Any() && period.DepartmentId.HasValue)
            {
                var directProfiles = await _context.MstWorkforceProfiles
                    .AsNoTracking()
                    .Where(w => w.IsActive && w.PrimaryDepartmentId == period.DepartmentId.Value)
                    .ToListAsync(cancellationToken);

                candidateProfiles = directProfiles.Select(p => new
                {
                    Profile = p,
                    PositionId = p.PrimaryPositionId ?? Guid.Empty
                }).ToList();
            }

            var populatedStaffNames = new List<string>();
            int accompaniedCount = 0;

            foreach (var cand in candidateProfiles)
            {
                // Evaluasi status kesiapan kerja staf (BE-HRD-090, INT-HR-31)
                var eligibility = await _readinessService.GetSchedulingEligibilityAsync(cand.Profile.Id, period.PeriodStartDate, cancellationToken);
                if (eligibility.Success && eligibility.Data != null)
                {
                    // Pegawai Pending Onboarding / NotReady disaring keluar otomatis (Acceptance criteria 2 BE-HRD-091)
                    if (eligibility.Data.EligibilityStatus == "NotReady")
                    {
                        _logger.LogInformation("Staf {Name} ({Id}) dilewati karena belum siap kerja HRD.", cand.Profile.DisplayName, cand.Profile.Id);
                        continue;
                    }
                }

                // Cek status pendampingan klinis (BE-HRD-091, HRD-DEC-087, HRD-DEC-100)
                var preceptorship = await _context.Set<HrdClinicalPreceptorship>()
                    .AsNoTracking()
                    .Include(p => p.Preceptors)
                    .Where(p => p.WorkforceProfileId == cand.Profile.Id &&
                                (!period.OrganizationUnitId.HasValue || p.OrganizationUnitId == period.OrganizationUnitId.Value) &&
                                p.PreceptorshipStatus != PreceptorshipStatus.Independent &&
                                p.PreceptorshipStatus != PreceptorshipStatus.Cancelled &&
                                !p.IsDelete)
                    .FirstOrDefaultAsync(cancellationToken);

                bool isAccompanied = preceptorship != null;
                Guid? preceptorId = null;

                if (isAccompanied)
                {
                    accompaniedCount++;
                    preceptorId = preceptorship!.Preceptors
                        .Where(pr => pr.AssignedUntil == null || pr.AssignedUntil >= period.PeriodStartDate)
                        .OrderByDescending(pr => pr.AssignedFrom)
                        .FirstOrDefault()?.PreceptorWorkforceProfileId;
                }

                // Periksa apakah staf sudah ada di daftar penugasan roster periode ini
                var existingAssignment = period.RosterAssignments.FirstOrDefault(a => a.WorkforceProfileId == cand.Profile.Id && a.IsActive);
                if (existingAssignment == null)
                {
                    var newAssignment = new TrxRosterAssignment
                    {
                        Id = Guid.NewGuid(),
                        RosterPeriodId = period.Id,
                        WorkforceProfileId = cand.Profile.Id,
                        PositionId = cand.PositionId != Guid.Empty ? cand.PositionId : null,
                        AssignmentStatus = "Draft",
                        IsActive = true,
                        CreateBy = currentUserId,
                        CreateDateTime = DateTime.UtcNow
                    };

                    _context.TrxRosterAssignments.Add(newAssignment);
                    period.RosterAssignments.Add(newAssignment);
                }

                populatedStaffNames.Add(cand.Profile.DisplayName);
            }

            period.TotalAssignmentCount = period.RosterAssignments.Count;
            period.RosterStatus = "Draft";
            period.IsValidationPassed = false;
            period.IsRecheckPassed = false;

            await _context.SaveChangesAsync(cancellationToken);

            var result = new PopulateEligibleStaffResponse
            {
                RosterPeriodId = period.Id,
                TotalStaffCount = populatedStaffNames.Count,
                AccompaniedStaffCount = accompaniedCount,
                StaffNames = populatedStaffNames,
                Message = $"Berhasil mengisi {populatedStaffNames.Count} staf eligible unit ({accompaniedCount} staf berstatus Didampingi)."
            };

            return SchedulingRequestServiceResult<PopulateEligibleStaffResponse>.Ok(
                result,
                result.Message);
        }

        /// <summary>
        /// Salin matriks shift dari periode roster sebelumnya dengan pergeseran tanggal otomatis (BE-HRD-092).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>> CopyPreviousPeriodAsync(
            Guid targetRosterPeriodId,
            CopyPreviousRosterPeriodRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var targetPeriod = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Id == targetRosterPeriodId && p.IsActive, cancellationToken);

            if (targetPeriod == null)
            {
                return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster target dengan ID '{targetRosterPeriodId}' tidak ditemukan.");
            }

            if (targetPeriod.IsLocked || targetPeriod.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode target telah ditutup dan dikunci mutlak dari seluruh penambahan, pengubahan, dan penggantian shift (HRD-DEC-102).");
            }

            var sourcePeriod = await _context.TrxRosterPeriods
                .AsNoTracking()
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Id == req.SourceRosterPeriodId && p.IsActive, cancellationToken);

            if (sourcePeriod == null)
            {
                return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode sumber dengan ID '{req.SourceRosterPeriodId}' tidak ditemukan.");
            }

            if (!sourcePeriod.RosterAssignments.Any())
            {
                return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Periode sumber tidak memiliki penugasan staf untuk disalin.");
            }

            int dayOffset = targetPeriod.PeriodStartDate.DayNumber - sourcePeriod.PeriodStartDate.DayNumber;
            int copiedCount = 0;

            foreach (var srcAssign in sourcePeriod.RosterAssignments)
            {
                var targetAssign = targetPeriod.RosterAssignments.FirstOrDefault(a => a.WorkforceProfileId == srcAssign.WorkforceProfileId);
                if (targetAssign == null)
                {
                    targetAssign = new TrxRosterAssignment
                    {
                        Id = Guid.NewGuid(),
                        RosterPeriodId = targetPeriod.Id,
                        WorkforceProfileId = srcAssign.WorkforceProfileId,
                        PositionId = srcAssign.PositionId,
                        AssignmentStatus = "Draft",
                        IsActive = true,
                        CreateBy = currentUserId,
                        CreateDateTime = DateTime.UtcNow
                    };
                    _context.TrxRosterAssignments.Add(targetAssign);
                    targetPeriod.RosterAssignments.Add(targetAssign);
                }

                foreach (var srcShift in srcAssign.ShiftAssignments)
                {
                    var newShiftDate = srcShift.ShiftDate.AddDays(dayOffset);
                    if (newShiftDate < targetPeriod.PeriodStartDate || newShiftDate > targetPeriod.PeriodEndDate)
                    {
                        continue;
                    }

                    var newStart = srcShift.ScheduledStartAt.AddDays(dayOffset);
                    var newEnd = srcShift.ScheduledEndAt.AddDays(dayOffset);

                    // Periksa apakah slot shift pada staf dan tanggal tersebut sudah ada
                    var existingShift = targetAssign.ShiftAssignments.FirstOrDefault(s => s.ShiftDate == newShiftDate && s.ShiftId == srcShift.ShiftId);
                    if (existingShift == null)
                    {
                        var newShift = new TrxShiftAssignment
                        {
                            Id = Guid.NewGuid(),
                            RosterAssignmentId = targetAssign.Id,
                            WorkforceProfileId = srcAssign.WorkforceProfileId,
                            ShiftId = srcShift.ShiftId,
                            ShiftDate = newShiftDate,
                            ScheduledStartAt = newStart,
                            ScheduledEndAt = newEnd,
                            BreakDurationMinutes = srcShift.BreakDurationMinutes,
                            PlannedWorkMinutes = srcShift.PlannedWorkMinutes,
                            AssignmentType = srcShift.AssignmentType,
                            AssignmentStatus = "Draft",
                            AssignmentSource = "Roster",
                            IsNightShift = srcShift.IsNightShift,
                            IsOnCall = srcShift.IsOnCall,
                            IsDayOff = srcShift.IsDayOff,
                            IsAccompanied = srcShift.IsAccompanied,
                            PreceptorEmployeeId = srcShift.PreceptorEmployeeId,
                            IsActive = true,
                            CreateBy = currentUserId,
                            CreateDateTime = DateTime.UtcNow
                        };

                        _context.TrxShiftAssignments.Add(newShift);
                        targetAssign.ShiftAssignments.Add(newShift);
                        targetAssign.PlannedShiftCount++;
                        copiedCount++;
                    }
                }
            }

            targetPeriod.TotalAssignmentCount = targetPeriod.RosterAssignments.Count;
            targetPeriod.RosterStatus = "Draft";
            targetPeriod.IsValidationPassed = false;
            targetPeriod.IsRecheckPassed = false;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new CopyPreviousRosterPeriodResponse
            {
                Success = true,
                CopiedAssignmentsCount = copiedCount,
                Message = $"Berhasil menyalin {copiedCount} jadwal shift dari periode {sourcePeriod.RosterPeriodCode}."
            };

            return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Ok(
                response,
                response.Message);
        }

        /// <summary>
        /// Terapkan template pola shift berulang ke staf target (BE-HRD-092).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<ApplyShiftPatternResponse>> ApplyShiftPatternAsync(
            Guid rosterPeriodId,
            ApplyShiftPatternRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan.");
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penambahan, pengubahan, dan penggantian shift (HRD-DEC-102).");
            }

            var pattern = await _context.MstShiftPatterns
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == req.ShiftPatternId && p.IsActive, cancellationToken);

            if (pattern == null)
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pola shift dengan ID '{req.ShiftPatternId}' tidak ditemukan.");
            }

            var startDate = req.StartDate ?? period.PeriodStartDate;
            var endDate = req.EndDate ?? period.PeriodEndDate;

            if (startDate < period.PeriodStartDate || endDate > period.PeriodEndDate || endDate < startDate)
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rentang tanggal penerapan pola di luar batas periode roster.");
            }

            var allShifts = await _context.MstShifts
                .AsNoTracking()
                .Where(s => s.IsActive)
                .ToListAsync(cancellationToken);

            var patternItems = ParsePatternItems(pattern.PatternDefinitionJson, allShifts);
            if (!patternItems.Any())
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Definisi pola shift kosong atau tidak valid.");
            }

            var targetStaffIds = req.TargetWorkforceProfileIds != null && req.TargetWorkforceProfileIds.Any()
                ? req.TargetWorkforceProfileIds
                : period.RosterAssignments.Select(a => a.WorkforceProfileId).ToList();

            if (!targetStaffIds.Any())
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tidak ada staf target untuk diterapkan pola shift.");
            }

            int cycleLength = pattern.CycleLengthDays > 0 ? pattern.CycleLengthDays : patternItems.Count;
            int totalAssigned = 0;

            foreach (var staffId in targetStaffIds)
            {
                var assign = period.RosterAssignments.FirstOrDefault(a => a.WorkforceProfileId == staffId);
                if (assign == null)
                {
                    assign = new TrxRosterAssignment
                    {
                        Id = Guid.NewGuid(),
                        RosterPeriodId = period.Id,
                        WorkforceProfileId = staffId,
                        AssignmentStatus = "Draft",
                        IsActive = true,
                        CreateBy = currentUserId,
                        CreateDateTime = DateTime.UtcNow
                    };
                    _context.TrxRosterAssignments.Add(assign);
                    period.RosterAssignments.Add(assign);
                }

                for (var curDate = startDate; curDate <= endDate; curDate = curDate.AddDays(1))
                {
                    int dayInCycle = ((curDate.DayNumber - startDate.DayNumber) % cycleLength) + 1;
                    var item = patternItems.FirstOrDefault(p => p.Day == dayInCycle);
                    if (item == null || item.Shift == null) continue;

                    var shift = item.Shift;
                    var startDateTime = curDate.ToDateTime(shift.StartTime);
                    var endDateTime = shift.IsOvernight || shift.EndTime < shift.StartTime
                        ? curDate.AddDays(1).ToDateTime(shift.EndTime)
                        : curDate.ToDateTime(shift.EndTime);

                    var paidMinutes = shift.PaidWorkMinutes > 0
                        ? shift.PaidWorkMinutes
                        : (int)(endDateTime - startDateTime).TotalMinutes - shift.BreakDurationMinutes;
                    if (paidMinutes < 0) paidMinutes = 0;

                    var existingShift = assign.ShiftAssignments.FirstOrDefault(s => s.ShiftDate == curDate);
                    if (existingShift != null)
                    {
                        existingShift.ShiftId = shift.Id;
                        existingShift.ScheduledStartAt = startDateTime;
                        existingShift.ScheduledEndAt = endDateTime;
                        existingShift.BreakDurationMinutes = shift.BreakDurationMinutes;
                        existingShift.PlannedWorkMinutes = paidMinutes;
                        existingShift.IsNightShift = shift.IsNightShift;
                        existingShift.IsOnCall = shift.IsOnCall;
                        existingShift.IsDayOff = shift.IsOffShift;
                        existingShift.AssignmentStatus = "Draft";
                    }
                    else
                    {
                        var shiftAssignment = new TrxShiftAssignment
                        {
                            Id = Guid.NewGuid(),
                            RosterAssignmentId = assign.Id,
                            WorkforceProfileId = staffId,
                            ShiftId = shift.Id,
                            ShiftDate = curDate,
                            ScheduledStartAt = startDateTime,
                            ScheduledEndAt = endDateTime,
                            BreakDurationMinutes = shift.BreakDurationMinutes,
                            PlannedWorkMinutes = paidMinutes,
                            AssignmentType = shift.IsOnCall ? "OnCall" : "Regular",
                            AssignmentStatus = "Draft",
                            AssignmentSource = "Roster",
                            IsNightShift = shift.IsNightShift,
                            IsOnCall = shift.IsOnCall,
                            IsDayOff = shift.IsOffShift,
                            IsActive = true,
                            CreateBy = currentUserId,
                            CreateDateTime = DateTime.UtcNow
                        };

                        _context.TrxShiftAssignments.Add(shiftAssignment);
                        assign.ShiftAssignments.Add(shiftAssignment);
                        assign.PlannedShiftCount++;
                    }

                    totalAssigned++;
                }
            }

            period.TotalAssignmentCount = period.RosterAssignments.Count;
            period.RosterStatus = "Draft";
            period.IsValidationPassed = false;
            period.IsRecheckPassed = false;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new ApplyShiftPatternResponse
            {
                Success = true,
                TotalAssignedShifts = totalAssigned,
                AffectedStaffCount = targetStaffIds.Count,
                Message = $"Berhasil menerapkan pola shift '{pattern.ShiftPatternName}' pada {targetStaffIds.Count} staf ({totalAssigned} shift ditugaskan)."
            };

            return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Ok(
                response,
                response.Message);
        }

        public async Task<SchedulingRequestServiceResult<RosterValidationResultResponse>> ValidateRosterPeriodAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var result = await _rosterValidationService.ValidateRosterPeriodAsync(id, currentUserId, cancellationToken);

            return SchedulingRequestServiceResult<RosterValidationResultResponse>.Ok(
                result,
                result.IsValidationPassed
                    ? "Validasi roster selesai: seluruh jadwal lulus uji konflik dan jeda istirahat."
                    : $"Validasi roster selesai dengan {result.BlockingConflicts} konflik pemblokiran.");
        }

        /// <summary>
        /// Penerbitan roster mandiri oleh Kepala Unit dengan validasi ulang otomatis (BE-HRD-093).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<PublishRosterPeriodResponse>> PublishRosterPeriodAsync(
            Guid id,
            PublishRosterPeriodRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{id}' tidak ditemukan.");
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penerbitan ulang (HRD-DEC-102).");
            }

            // 1. Recheck Otomatis Mesin Validasi B1-B7 & W1-W5 (HRD-DEC-098, HRD-TF-052 a)
            var valResult = await _rosterValidationService.ValidateRosterPeriodAsync(id, currentUserId, cancellationToken);

            if (valResult.BlockingConflicts > 0)
            {
                period.IsValidationPassed = false;
                period.IsRecheckPassed = false;
                await _context.SaveChangesAsync(cancellationToken);

                return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Penerbitan roster ditolak: ditemukan {valResult.BlockingConflicts} konflik keselamatan dinas (B1–B7). Selesaikan seluruh konflik sebelum menerbitkan.");
            }

            // 2. Evaluasi Peringatan W1-W5: Harus Ada Alasan Konfirmasi Resmi
            if (valResult.WarningConflicts > 0)
            {
                var reasonsCount = req.WarningReasons?.Count ?? 0;
                if (reasonsCount < valResult.WarningConflicts)
                {
                    return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Penerbitan roster memerlukan alasan konfirmasi resmi untuk seluruh {valResult.WarningConflicts} peringatan waktu kerja (W1–W5). Diterima {reasonsCount} alasan.");
                }
            }

            // 3. Mutasi Status Menjadi Published
            period.RosterStatus = "Published";
            period.IsPublished = true;
            period.IsValidationPassed = true;
            period.IsRecheckPassed = true;
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

            // 4. Catat Jejak Publikasi Resmi (TrxRosterPublication)
            var pubCount = await _context.TrxRosterPublications.CountAsync(cancellationToken) + 1;
            var publication = new TrxRosterPublication
            {
                Id = Guid.NewGuid(),
                RosterPeriodId = period.Id,
                PublicationNumber = $"PUB-{period.RosterPeriodCode}-{pubCount:D4}",
                VersionNumber = period.VersionNumber,
                PublicationStatus = "Published",
                PublicationChannel = "Application",
                PublishedAt = DateTime.UtcNow,
                PublishedByUserId = currentUserId,
                AudienceDefinitionJson = JsonSerializer.Serialize(new
                {
                    period.DepartmentId,
                    period.OrganizationUnitId,
                    TotalAssignments = period.TotalAssignmentCount,
                    WarningReasons = req.WarningReasons
                }),
                PublicationSnapshotJson = period.ValidationSummaryJson,
                Notes = "Diterbitkan mandiri oleh Kepala Unit pasca lolos recheck validasi keselamatan.",
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxRosterPublications.Add(publication);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new PublishRosterPeriodResponse
            {
                RosterPeriodId = period.Id,
                Status = "Published",
                PublishedAt = period.PublishedAt.Value,
                PublishedBy = currentUserId.ToString(),
                Message = "Periode roster berhasil dipublikasikan secara resmi ke seluruh staf dan Kepala Bidang."
            };

            return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Ok(
                response,
                response.Message);
        }

        /// <summary>
        /// Penutupan periode roster oleh HRD Payroll (BE-HRD-098).
        /// </summary>
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

            // Penutupan periode yang masih berstatus Draft ditolak (menutup bug HRD-TF-052 d)
            if (period.RosterStatus == "Draft")
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Penutupan periode yang masih berstatus 'Draft' ditolak. Periode harus berstatus 'Published' terlebih dahulu (HRD-TF-052 d).");
            }

            period.RosterStatus = "Closed";
            period.IsLocked = true;
            period.ClosedAt = DateTime.UtcNow;
            period.ClosedByUserId = currentUserId;
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

        private List<ParsedPatternItem> ParsePatternItems(string json, List<MstShift> allShifts)
        {
            var list = new List<ParsedPatternItem>();
            if (string.IsNullOrWhiteSpace(json)) return list;

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return list;

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    int day = 1;
                    if (el.TryGetProperty("day", out var dayProp) && dayProp.TryGetInt32(out var dVal))
                    {
                        day = dVal;
                    }

                    MstShift? matchedShift = null;

                    if (el.TryGetProperty("shiftId", out var idProp) && Guid.TryParse(idProp.GetString(), out var sId))
                    {
                        matchedShift = allShifts.FirstOrDefault(s => s.Id == sId);
                    }
                    else if (el.TryGetProperty("shiftCode", out var codeProp))
                    {
                        var code = codeProp.GetString();
                        if (!string.IsNullOrWhiteSpace(code))
                        {
                            matchedShift = allShifts.FirstOrDefault(s => string.Equals(s.ShiftCode, code, StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    if (matchedShift != null)
                    {
                        list.Add(new ParsedPatternItem { Day = day, Shift = matchedShift });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gagal mem-parse PatternDefinitionJson.");
            }

            return list;
        }

        private class ParsedPatternItem
        {
            public int Day { get; set; }
            public MstShift? Shift { get; set; }
        }
    }
}
