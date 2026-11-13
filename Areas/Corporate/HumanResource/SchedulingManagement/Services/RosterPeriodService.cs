using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
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
        private readonly HrdUnitScopeService _unitScopeService;
        private readonly ShiftRevisionRecorder _revisionRecorder;

        // VR-PK-06 dan VR-PK-27 (kontrak v26).
        private const string ManageScopeDeniedMessage = "Anda tidak berwenang mengubah roster unit ini.";
        private const string ReadScopeDeniedMessage = "Anda tidak berwenang melihat data unit ini.";
        private const string ReasonRequiredMessage = "Alasan perubahan wajib diisi.";
        private const string WarningReasonRequiredMessage = "Pilih alasan untuk setiap peringatan sebelum menyimpan.";
        private const string ShiftChangedByOthersMessage = "Shift ini baru saja diubah orang lain. Muat ulang lalu coba lagi.";
        private const string PublishedStatus = "Published";

        public RosterPeriodService(
            ApplicationDbContext context,
            IRosterValidationService rosterValidationService,
            IHrdEmploymentReadinessReadService readinessService,
            ILogger<RosterPeriodService> logger,
            HrdUnitScopeService unitScopeService,
            ShiftRevisionRecorder revisionRecorder)
        {
            _revisionRecorder = revisionRecorder ?? throw new ArgumentNullException(nameof(revisionRecorder));
            _unitScopeService = unitScopeService ?? throw new ArgumentNullException(nameof(unitScopeService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _rosterValidationService = rosterValidationService ?? throw new ArgumentNullException(nameof(rosterValidationService));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RosterPeriodSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var periods = await ApplyReadScope(
                    _context.TrxRosterPeriods.AsNoTracking().Where(p => p.IsActive),
                    await _unitScopeService.GetReadScopeAsync(cancellationToken))
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

            query = ApplyReadScope(query, await _unitScopeService.GetReadScopeAsync(cancellationToken));

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

            if (!(await _unitScopeService.GetReadScopeAsync(cancellationToken)).Contains(period.OrganizationUnitId))
            {
                return SchedulingRequestServiceResult<RosterPeriodDetailResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ReadScopeDeniedMessage);
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

            if (!await CanManageUnitAsync(req.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
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

            if (!await CanManageUnitAsync(period.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
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

            // HRD-DEC-195: roster terbit tidak turun ke Draft; shift baru diperiksa penuh dan masuk Riwayat Revisi.
            if (period.RosterStatus == PublishedStatus)
            {
                return await AddShiftsToPublishedAsync(period, workforce, req, currentUserId, cancellationToken);
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

                var startDateTime = AppDateTimeHelper.ToUtc(shiftItem.ShiftDate.ToDateTime(shift.StartTime));
                var endDateTime = AppDateTimeHelper.ToUtc(shift.IsOvernight || shift.EndTime < shift.StartTime
                    ? shiftItem.ShiftDate.AddDays(1).ToDateTime(shift.EndTime)
                    : shiftItem.ShiftDate.ToDateTime(shift.EndTime));

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

            if (!await CanManageUnitAsync(period.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<PopulateEligibleStaffResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
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

            if (!await CanManageUnitAsync(targetPeriod.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
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

            if (!(await _unitScopeService.GetReadScopeAsync(cancellationToken)).Contains(sourcePeriod.OrganizationUnitId))
            {
                return SchedulingRequestServiceResult<CopyPreviousRosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ReadScopeDeniedMessage);
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

            if (!await CanManageUnitAsync(period.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<ApplyShiftPatternResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
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
            var periodUnit = await _context.TrxRosterPeriods
                .AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new { p.OrganizationUnitId })
                .FirstOrDefaultAsync(cancellationToken);

            if (periodUnit != null && !await CanManageUnitAsync(periodUnit.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<RosterValidationResultResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
            }

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

            if (!await CanManageUnitAsync(period.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penerbitan ulang (HRD-DEC-102).");
            }

            // 1. Recheck Otomatis Mesin Validasi B1-B8 & W1-W7 (HRD-DEC-098, HRD-TF-052 a)
            var valResult = await _rosterValidationService.ValidateRosterPeriodAsync(id, currentUserId, cancellationToken);

            if (valResult.BlockingConflicts > 0)
            {
                period.IsValidationPassed = false;
                period.IsRecheckPassed = false;
                await _context.SaveChangesAsync(cancellationToken);

                return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Penerbitan roster ditolak: ditemukan {valResult.BlockingConflicts} konflik keselamatan dinas (B1–B8). Selesaikan seluruh konflik sebelum menerbitkan.");
            }

            // 2. Evaluasi Peringatan W1-W5: Harus Ada Alasan Konfirmasi Resmi
            if (valResult.WarningConflicts > 0)
            {
                var reasonsCount = req.WarningReasons?.Count ?? 0;
                if (reasonsCount < valResult.WarningConflicts)
                {
                    return SchedulingRequestServiceResult<PublishRosterPeriodResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Penerbitan roster memerlukan alasan konfirmasi resmi untuk seluruh {valResult.WarningConflicts} peringatan jadwal (W1–W7). Diterima {reasonsCount} alasan.");
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

            if (!await CanManageUnitAsync(period.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<RosterPeriodResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
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

        /// <summary>
        /// Tambah shift pada roster terbit (HRD-DEC-195, INV-A52-03): alasan wajib, setiap shift lolos pemeriksaan
        /// B1–B8/W1–W7 yang sama dengan terbit, status tetap Published dengan versi yang sama, dan setiap shift
        /// menulis satu baris Riwayat Revisi "Tambah Shift" dalam satu transaksi.
        /// </summary>
        private async Task<SchedulingRequestServiceResult<RosterAssignmentResponse>> AddShiftsToPublishedAsync(
            TrxRosterPeriod period,
            MstWorkforceProfile workforce,
            CreateRosterAssignmentRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            var reasonCategory = req.ReasonCategory?.Trim() ?? string.Empty;
            var reasonNote = req.ReasonNote?.Trim() ?? string.Empty;
            if (reasonCategory.Length == 0 || reasonNote.Length == 0)
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(StatusCodes.Status400BadRequest, ReasonRequiredMessage);
            }

            if (reasonCategory.Length > ShiftRevisionRecorder.ReasonCategoryMaxLength)
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Kategori alasan maksimal {ShiftRevisionRecorder.ReasonCategoryMaxLength} karakter.");
            }

            if (req.Shifts.Count == 0)
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pilih minimal satu shift untuk ditambahkan.");
            }

            var existing = await _context.TrxRosterAssignments
                .FirstOrDefaultAsync(a => a.RosterPeriodId == period.Id && a.WorkforceProfileId == workforce.Id && a.IsActive, cancellationToken);
            var positionId = req.PositionId ?? existing?.PositionId;

            // Bentuk shift baru. Pada roster terbit, shift yang tidak dikenal ditolak, bukan dilewati diam-diam.
            var planned = new List<TrxShiftAssignment>();
            foreach (var item in req.Shifts)
            {
                if (item.ShiftDate < period.PeriodStartDate || item.ShiftDate > period.PeriodEndDate)
                {
                    return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Tanggal shift {item.ShiftDate:yyyy-MM-dd} berada di luar periode roster.");
                }

                var shift = await _context.MstShifts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == item.ShiftId && s.IsActive, cancellationToken);
                if (shift == null)
                {
                    return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        $"Master shift dengan ID '{item.ShiftId}' tidak ditemukan atau tidak aktif.");
                }

                var start = AppDateTimeHelper.ToUtc(item.ShiftDate.ToDateTime(shift.StartTime));
                var end = AppDateTimeHelper.ToUtc(shift.IsOvernight || shift.EndTime < shift.StartTime
                    ? item.ShiftDate.AddDays(1).ToDateTime(shift.EndTime)
                    : item.ShiftDate.ToDateTime(shift.EndTime));
                var paidMinutes = shift.PaidWorkMinutes > 0
                    ? shift.PaidWorkMinutes
                    : (int)(end - start).TotalMinutes - shift.BreakDurationMinutes;
                var isOnCall = item.IsOnCall || shift.IsOnCall;

                planned.Add(new TrxShiftAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforce.Id,
                    ShiftId = shift.Id,
                    ShiftDate = item.ShiftDate,
                    ScheduledStartAt = start,
                    ScheduledEndAt = end,
                    BreakDurationMinutes = shift.BreakDurationMinutes,
                    PlannedWorkMinutes = Math.Max(0, paidMinutes),
                    AssignmentType = isOnCall ? "OnCall" : "Regular",
                    AssignmentStatus = PublishedStatus,
                    AssignmentSource = "Roster",
                    IsNightShift = item.IsNightShift || shift.IsNightShift,
                    IsOnCall = isOnCall,
                    IsDayOff = shift.IsOffShift,
                    IsActive = true,
                    CreateBy = currentUserId,
                });
            }

            // Pemeriksaan satu shift tidak melihat shift lain dalam permintaan yang sama; tabrakan di antaranya ditolak di sini.
            var ordered = planned.Where(s => !s.IsDayOff).OrderBy(s => s.ScheduledStartAt).ToList();
            for (var i = 1; i < ordered.Count; i++)
            {
                if (ordered[i].ScheduledStartAt < ordered[i - 1].ScheduledEndAt)
                {
                    return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Shift yang ditambahkan saling bertabrakan pada {ordered[i].ShiftDate:yyyy-MM-dd}.");
                }
            }

            // Pemeriksaan penuh dengan evaluator yang sama dengan terbit (INV-A52-03); B6 ikut memblokir.
            var items = new List<RosterValidationConflictItemDto>();
            var infoMessages = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var s in planned)
            {
                var check = await _rosterValidationService.ValidateSingleShiftAsync(new RosterSingleShiftValidationRequest
                {
                    RosterPeriodId = period.Id,
                    WorkforceProfileId = workforce.Id,
                    PositionId = positionId,
                    ShiftId = s.ShiftId,
                    ShiftDate = s.ShiftDate,
                    ScheduledStartAt = s.ScheduledStartAt,
                    ScheduledEndAt = s.ScheduledEndAt,
                    IsNightShift = s.IsNightShift,
                    IsDayOff = s.IsDayOff,
                }, cancellationToken);

                if (check == null)
                {
                    return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        $"Periode roster dengan ID '{period.Id}' tidak ditemukan.");
                }

                items.AddRange(check.Items);
                infoMessages.UnionWith(check.InfoMessages);
            }

            var blocking = items.Where(x => x.Severity == "BLOCKING").ToList();
            if (blocking.Count > 0)
            {
                var message = blocking.Any(x => x.RuleCode == "B6")
                    ? "Status kesiapan pegawai tidak dapat dibaca. Coba lagi atau hubungi HRD."
                    : $"Tambah shift ditolak: {string.Join(" ", blocking.Select(x => x.Message).Distinct())}";
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(StatusCodes.Status422UnprocessableEntity, message, items);
            }

            var warnings = items.Where(x => x.Severity == "WARNING").ToList();
            var reasonedCodes = (req.WarningReasons ?? new List<WarningReasonItemRequest>())
                .Where(x => !string.IsNullOrWhiteSpace(x.WarningCode) && !string.IsNullOrWhiteSpace(x.ReasonCategory))
                .Select(x => x.WarningCode.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (warnings.Any(w => !reasonedCodes.Contains(w.RuleCode)))
            {
                return SchedulingRequestServiceResult<RosterAssignmentResponse>.Fail(StatusCodes.Status422UnprocessableEntity, WarningReasonRequiredMessage, warnings);
            }

            var now = DateTime.UtcNow;
            var revisions = new List<HrdShiftAssignmentRevision>();
            TrxRosterAssignment assignment;
            await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                if (existing != null)
                {
                    assignment = existing;
                    assignment.UpdateBy = currentUserId;
                    assignment.UpdateDateTime = now;
                }
                else
                {
                    assignment = new TrxRosterAssignment
                    {
                        Id = Guid.NewGuid(),
                        RosterPeriodId = period.Id,
                        WorkforceProfileId = workforce.Id,
                        PositionId = positionId,
                        AssignmentStatus = PublishedStatus,
                        Notes = req.Notes?.Trim(),
                        IsActive = true,
                        CreateBy = currentUserId,
                        CreateDateTime = now
                    };
                    _context.TrxRosterAssignments.Add(assignment);
                    period.TotalAssignmentCount += 1;
                }

                foreach (var s in planned)
                {
                    s.RosterAssignmentId = assignment.Id;
                    s.CreateDateTime = now;
                    _context.TrxShiftAssignments.Add(s);

                    assignment.PlannedShiftCount += 1;
                    assignment.PlannedWorkMinutes += s.PlannedWorkMinutes;
                    if (s.IsNightShift) assignment.PlannedNightShiftCount += 1;
                    if (s.IsOnCall) assignment.PlannedOnCallCount += 1;

                    revisions.Add(_revisionRecorder.Record(new HrdShiftAssignmentRevision
                    {
                        ShiftAssignmentId = s.Id,
                        RosterPeriodId = period.Id,
                        ShiftDate = s.ShiftDate,
                        ChangeType = SchedulingRequestValueConstants.ShiftRevisionChangeType.AddShift,
                        NewWorkforceProfileId = workforce.Id,
                        NewShiftId = s.ShiftId,
                        NewStartAt = s.ScheduledStartAt,
                        NewEndAt = s.ScheduledEndAt,
                        ReasonCategory = reasonCategory,
                        ReasonNote = reasonNote,
                        ChangedByUserId = currentUserId,
                        ChangedAt = now,
                    }));
                }

                // Status dan versi roster sengaja tidak diubah (memperbaiki perilaku lama yang menurunkan ke Draft).
                period.UpdateBy = currentUserId;
                period.UpdateDateTime = now;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            _logger.LogInformation("Tambah {Count} shift ke roster terbit {PeriodId} untuk pegawai {WorkforceProfileId}; riwayat ditulis.",
                planned.Count, period.Id, workforce.Id);

            var response = new RosterAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = period.Id,
                WorkforceProfileId = workforce.Id,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                PositionId = assignment.PositionId,
                AssignmentStatus = assignment.AssignmentStatus,
                PlannedShiftCount = assignment.PlannedShiftCount,
                Notes = assignment.Notes,
                ShiftAssignments = planned.Select(s => new ShiftAssignmentItemResponse
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
                }).ToList(),
                RevisionIds = revisions.Select(r => r.Id).ToList(),
                Warnings = warnings,
                InfoMessages = infoMessages.ToList()
            };

            return SchedulingRequestServiceResult<RosterAssignmentResponse>.Ok(
                response,
                "Shift berhasil ditambahkan ke roster terbit; roster tetap terbit.",
                StatusCodes.Status201Created);
        }

        /// <summary>
        /// Hapus satu shift dari roster terbit (HRD-DEC-195). Shift ditandai batal (tidak dihapus fisik) dan satu baris
        /// Riwayat Revisi "Hapus Shift" ditulis dalam transaksi yang sama. Status roster tetap Published.
        /// </summary>
        public async Task<SchedulingRequestServiceResult<ShiftRevisionResponse>> RemovePublishedShiftAsync(
            Guid rosterPeriodId,
            Guid shiftAssignmentId,
            RemovePublishedShiftRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan.");
            }

            if (!await CanManageUnitAsync(period.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(StatusCodes.Status403Forbidden, ManageScopeDeniedMessage);
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penambahan, pengubahan, dan penggantian shift (HRD-DEC-102).");
            }

            if (period.RosterStatus != PublishedStatus)
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hapus shift dengan alasan hanya berlaku untuk roster yang sudah terbit.");
            }

            var reasonCategory = req.ReasonCategory?.Trim() ?? string.Empty;
            var reasonNote = req.ReasonNote?.Trim() ?? string.Empty;
            if (reasonCategory.Length == 0 || reasonNote.Length == 0)
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(StatusCodes.Status400BadRequest, ReasonRequiredMessage);
            }

            if (reasonCategory.Length > ShiftRevisionRecorder.ReasonCategoryMaxLength)
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Kategori alasan maksimal {ShiftRevisionRecorder.ReasonCategoryMaxLength} karakter.");
            }

            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.RosterAssignment)
                .FirstOrDefaultAsync(s =>
                    s.Id == shiftAssignmentId &&
                    s.IsActive &&
                    !s.IsCancel &&
                    s.RosterAssignment != null &&
                    s.RosterAssignment.RosterPeriodId == period.Id,
                    cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift tidak ditemukan pada periode roster ini.");
            }

            var workforceProfileId = shiftAssignment.WorkforceProfileId;
            var shiftId = shiftAssignment.ShiftId;
            var now = DateTime.UtcNow;

            HrdShiftAssignmentRevision revision;
            await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                // Pembaruan bersyarat: shift yang sudah diganti petugas atau dihapus orang lain menolak 409.
                var updated = await _context.TrxShiftAssignments
                    .Where(s =>
                        s.Id == shiftAssignment.Id &&
                        s.IsActive &&
                        !s.IsCancel &&
                        s.WorkforceProfileId == workforceProfileId &&
                        s.ShiftId == shiftId)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(s => s.IsActive, false)
                        .SetProperty(s => s.IsCancel, true)
                        .SetProperty(s => s.CancelBy, currentUserId)
                        .SetProperty(s => s.CancelDateTime, now)
                        .SetProperty(s => s.AssignmentStatus, "Cancelled")
                        .SetProperty(s => s.UpdateBy, currentUserId)
                        .SetProperty(s => s.UpdateDateTime, now),
                        cancellationToken);

                if (updated == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return SchedulingRequestServiceResult<ShiftRevisionResponse>.Fail(StatusCodes.Status409Conflict, ShiftChangedByOthersMessage);
                }

                var rosterAssignment = shiftAssignment.RosterAssignment!;
                rosterAssignment.PlannedShiftCount = Math.Max(0, rosterAssignment.PlannedShiftCount - 1);
                rosterAssignment.PlannedWorkMinutes = Math.Max(0, rosterAssignment.PlannedWorkMinutes - shiftAssignment.PlannedWorkMinutes);
                if (shiftAssignment.IsNightShift) rosterAssignment.PlannedNightShiftCount = Math.Max(0, rosterAssignment.PlannedNightShiftCount - 1);
                if (shiftAssignment.IsOnCall) rosterAssignment.PlannedOnCallCount = Math.Max(0, rosterAssignment.PlannedOnCallCount - 1);
                rosterAssignment.UpdateBy = currentUserId;
                rosterAssignment.UpdateDateTime = now;

                revision = _revisionRecorder.Record(new HrdShiftAssignmentRevision
                {
                    ShiftAssignmentId = shiftAssignment.Id,
                    RosterPeriodId = period.Id,
                    ShiftDate = shiftAssignment.ShiftDate,
                    ChangeType = SchedulingRequestValueConstants.ShiftRevisionChangeType.RemoveShift,
                    PreviousWorkforceProfileId = workforceProfileId,
                    PreviousShiftId = shiftId,
                    PreviousStartAt = shiftAssignment.ScheduledStartAt,
                    PreviousEndAt = shiftAssignment.ScheduledEndAt,
                    ReasonCategory = reasonCategory,
                    ReasonNote = reasonNote,
                    ChangedByUserId = currentUserId,
                    ChangedAt = now,
                });

                // Shift yang dimuat tidak ikut disimpan; perubahannya sudah ditulis lewat pembaruan bersyarat.
                _context.Entry(shiftAssignment).State = EntityState.Unchanged;

                period.UpdateBy = currentUserId;
                period.UpdateDateTime = now;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            _logger.LogInformation("Shift {ShiftId} tanggal {ShiftDate} dihapus dari roster terbit {PeriodId}; riwayat {RevisionId}.",
                shiftAssignment.Id, shiftAssignment.ShiftDate, period.Id, revision.Id);

            var response = (await BuildRevisionResponsesAsync(new List<HrdShiftAssignmentRevision> { revision }, cancellationToken)).Single();
            return SchedulingRequestServiceResult<ShiftRevisionResponse>.Ok(response, "Shift berhasil dihapus dari roster terbit; roster tetap terbit.");
        }

        /// <summary>
        /// Riwayat Revisi satu periode roster (hanya baca), terbaru di atas. Mematuhi cakupan baca (VR-PK-27).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<PagedResult<ShiftRevisionResponse>>> GetRevisionsAsync(
            Guid rosterPeriodId,
            DateOnly? fromDate,
            DateOnly? toDate,
            Guid? workforceProfileId,
            string? changeType,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<PagedResult<ShiftRevisionResponse>>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan.");
            }

            if (!(await _unitScopeService.GetReadScopeAsync(cancellationToken)).Contains(period.OrganizationUnitId))
            {
                return SchedulingRequestServiceResult<PagedResult<ShiftRevisionResponse>>.Fail(StatusCodes.Status403Forbidden, ReadScopeDeniedMessage);
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return SchedulingRequestServiceResult<PagedResult<ShiftRevisionResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh sesudah tanggal akhir.");
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.HrdShiftAssignmentRevisions
                .AsNoTracking()
                .Where(r => r.RosterPeriodId == rosterPeriodId && !r.IsDelete);

            if (fromDate.HasValue) query = query.Where(r => r.ShiftDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(r => r.ShiftDate <= toDate.Value);
            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                var staffId = workforceProfileId.Value;
                query = query.Where(r => r.PreviousWorkforceProfileId == staffId || r.NewWorkforceProfileId == staffId);
            }

            if (!string.IsNullOrWhiteSpace(changeType))
            {
                var type = changeType.Trim();
                query = query.Where(r => r.ChangeType == type);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(r => r.ChangedAt)
                .ThenBy(r => r.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return SchedulingRequestServiceResult<PagedResult<ShiftRevisionResponse>>.Ok(
                new PagedResult<ShiftRevisionResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = await BuildRevisionResponsesAsync(rows, cancellationToken)
                },
                "Riwayat revisi roster berhasil diambil.");
        }

        private async Task<List<ShiftRevisionResponse>> BuildRevisionResponsesAsync(
            List<HrdShiftAssignmentRevision> rows,
            CancellationToken cancellationToken)
        {
            var profileIds = rows
                .SelectMany(r => new[] { r.PreviousWorkforceProfileId, r.NewWorkforceProfileId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var shiftIds = rows
                .SelectMany(r => new[] { r.PreviousShiftId, r.NewShiftId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var userIds = rows.Select(r => r.ChangedByUserId).Distinct().ToList();

            var profiles = await _context.MstWorkforceProfiles.AsNoTracking()
                .Where(w => profileIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, w => (string?)w.DisplayName, cancellationToken);
            var shifts = await _context.MstShifts.AsNoTracking()
                .Where(s => shiftIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => (string?)s.ShiftName, cancellationToken);
            var users = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => (string?)u.DisplayName, cancellationToken);

            static string? Name(Dictionary<Guid, string?> map, Guid? id) =>
                id.HasValue && map.TryGetValue(id.Value, out var value) ? value : null;

            return rows.Select(r => new ShiftRevisionResponse
            {
                Id = r.Id,
                ShiftAssignmentId = r.ShiftAssignmentId,
                RosterPeriodId = r.RosterPeriodId,
                ShiftDate = r.ShiftDate,
                ChangeType = r.ChangeType,
                ChangeTypeLabel = SchedulingRequestValueConstants.ShiftRevisionChangeType.Label(r.ChangeType),
                PreviousWorkforceProfileId = r.PreviousWorkforceProfileId,
                PreviousWorkforceName = Name(profiles, r.PreviousWorkforceProfileId),
                NewWorkforceProfileId = r.NewWorkforceProfileId,
                NewWorkforceName = Name(profiles, r.NewWorkforceProfileId),
                PreviousShiftId = r.PreviousShiftId,
                PreviousShiftName = Name(shifts, r.PreviousShiftId),
                NewShiftId = r.NewShiftId,
                NewShiftName = Name(shifts, r.NewShiftId),
                PreviousStartAt = r.PreviousStartAt,
                PreviousEndAt = r.PreviousEndAt,
                NewStartAt = r.NewStartAt,
                NewEndAt = r.NewEndAt,
                ReasonCategory = r.ReasonCategory,
                ReasonNote = r.ReasonNote,
                ChangedByUserId = r.ChangedByUserId,
                ChangedByName = Name(users, r.ChangedByUserId),
                ChangedAt = r.ChangedAt,
            }).ToList();
        }

        private async Task<bool> CanManageUnitAsync(Guid? organizationUnitId, CancellationToken cancellationToken) =>
            (await _unitScopeService.GetManageScopeAsync(cancellationToken)).Contains(organizationUnitId);

        // Periode tanpa unit hanya terbaca oleh cakupan semua unit (gagal tertutup).
        private static IQueryable<TrxRosterPeriod> ApplyReadScope(IQueryable<TrxRosterPeriod> query, HrdUnitScope scope)
        {
            if (scope.IsAllUnits)
            {
                return query;
            }

            var unitIds = scope.UnitIds.ToList();
            return query.Where(p => p.OrganizationUnitId.HasValue && unitIds.Contains(p.OrganizationUnitId.Value));
        }
    }
}
