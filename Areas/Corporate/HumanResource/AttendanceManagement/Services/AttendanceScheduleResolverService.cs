using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceScheduleResolverService
    {
        private const int MaximumRangeDays = 31;
        private const string DefaultTimeZoneId = "Asia/Jakarta";

        private static readonly string[] AcceptedShiftAssignmentStatuses =
        {
            AttendanceValueConstants.ShiftAssignmentStatus.Published,
            AttendanceValueConstants.ShiftAssignmentStatus.Confirmed,
            AttendanceValueConstants.ShiftAssignmentStatus.Completed
        };

        private readonly ApplicationDbContext _dbContext;

        public AttendanceScheduleResolverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AttendanceScheduleResolverMetadataResponse GetMetadata()
        {
            return new AttendanceScheduleResolverMetadataResponse
            {
                MaximumRangeDays = MaximumRangeDays,
                ScheduleSourceOptions = new List<AttendanceScheduleStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.ScheduleSource.PublishedRoster, Label = "Roster dipublikasikan" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.ConfirmedRoster, Label = "Roster dikonfirmasi" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.CompletedRoster, Label = "Roster selesai" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.FixedWorkSchedule, Label = "Jadwal kerja tetap" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.ManualOverride, Label = "Override manual" },
                    new() { Value = AttendanceValueConstants.ScheduleSource.Unresolved, Label = "Belum terselesaikan" }
                },
                ShiftAssignmentStatusOptions = AcceptedShiftAssignmentStatuses
                    .Select(x => new AttendanceScheduleStringOptionResponse
                    {
                        Value = x,
                        Label = x switch
                        {
                            AttendanceValueConstants.ShiftAssignmentStatus.Published => "Dipublikasikan",
                            AttendanceValueConstants.ShiftAssignmentStatus.Confirmed => "Dikonfirmasi",
                            AttendanceValueConstants.ShiftAssignmentStatus.Completed => "Selesai",
                            _ => x
                        }
                    })
                    .ToList(),
                AssignmentTypeOptions = new List<AttendanceScheduleStringOptionResponse>
                {
                    new() { Value = "Regular", Label = "Reguler" },
                    new() { Value = "Overtime", Label = "Lembur" },
                    new() { Value = "OnCall", Label = "On-call" },
                    new() { Value = "Training", Label = "Pelatihan" },
                    new() { Value = "Remote", Label = "Kerja jarak jauh" },
                    new() { Value = "BusinessTrip", Label = "Perjalanan dinas" },
                    new() { Value = "DayOff", Label = "Hari libur" }
                }
            };
        }

        public async Task<AttendanceScheduleResolverServiceResult<AttendanceScheduleResolutionResponse>> ResolveAsync(
            Guid workforceProfileId,
            DateOnly workDate,
            CancellationToken cancellationToken = default)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return AttendanceScheduleResolverServiceResult<AttendanceScheduleResolutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile wajib dipilih.");
            }

            var workforce = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete && x.IsActive)
                .Select(x => new WorkforceSnapshot
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (workforce == null)
            {
                return AttendanceScheduleResolverServiceResult<AttendanceScheduleResolutionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan atau tidak aktif.");
            }

            var result = await ResolveCoreAsync(workforce, workDate, cancellationToken);
            return AttendanceScheduleResolverServiceResult<AttendanceScheduleResolutionResponse>.Ok(
                result,
                result.IsResolved
                    ? "Jadwal attendance berhasil diselesaikan."
                    : "Jadwal attendance belum dapat diselesaikan.");
        }

        public async Task<AttendanceScheduleResolverServiceResult<AttendanceScheduleRangeResponse>> ResolveRangeAsync(
            Guid workforceProfileId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return AttendanceScheduleResolverServiceResult<AttendanceScheduleRangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile wajib dipilih.");
            }

            if (endDate < startDate)
            {
                return AttendanceScheduleResolverServiceResult<AttendanceScheduleRangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal selesai tidak boleh lebih kecil daripada tanggal mulai.");
            }

            var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
            if (totalDays > MaximumRangeDays)
            {
                return AttendanceScheduleResolverServiceResult<AttendanceScheduleRangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Rentang tanggal maksimal {MaximumRangeDays} hari.");
            }

            var workforce = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete && x.IsActive)
                .Select(x => new WorkforceSnapshot
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (workforce == null)
            {
                return AttendanceScheduleResolverServiceResult<AttendanceScheduleRangeResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan atau tidak aktif.");
            }

            var items = new List<AttendanceScheduleResolutionResponse>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                items.Add(await ResolveCoreAsync(workforce, date, cancellationToken));
            }

            var response = new AttendanceScheduleRangeResponse
            {
                WorkforceProfileId = workforce.Id,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                StartDate = startDate,
                EndDate = endDate,
                TotalDate = items.Count,
                ResolvedDate = items.Count(x => x.IsResolved),
                UnresolvedDate = items.Count(x => !x.IsResolved),
                RestDayCount = items.Count(x => x.IsRestDay),
                HolidayCount = items.Count(x => x.IsHoliday),
                BlockingConflictCount = items.Count(x => x.HasBlockingConflict),
                Items = items
            };

            return AttendanceScheduleResolverServiceResult<AttendanceScheduleRangeResponse>.Ok(
                response,
                "Rentang jadwal attendance berhasil diselesaikan.");
        }

        private async Task<AttendanceScheduleResolutionResponse> ResolveCoreAsync(
            WorkforceSnapshot workforce,
            DateOnly workDate,
            CancellationToken cancellationToken)
        {
            var response = new AttendanceScheduleResolutionResponse
            {
                WorkforceProfileId = workforce.Id,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                WorkDate = workDate,
                ScheduleSource = AttendanceValueConstants.ScheduleSource.Unresolved
            };

            var rosterAssignments = await _dbContext.Set<TrxShiftAssignment>()
                .AsNoTracking()
                .Include(x => x.WorkSchedule)
                .Include(x => x.Shift)
                    .ThenInclude(x => x.WorkSchedule)
                .Where(x =>
                    x.WorkforceProfileId == workforce.Id &&
                    x.ShiftDate == workDate &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    AcceptedShiftAssignmentStatuses.Contains(x.AssignmentStatus))
                .ToListAsync(cancellationToken);

            var primaryRoster = ResolvePrimaryRoster(rosterAssignments, response);
            if (primaryRoster != null)
            {
                MapRosterResolution(response, primaryRoster, rosterAssignments);
            }
            else
            {
                await ResolveFixedScheduleAsync(response, workforce.Id, workDate, cancellationToken);
            }

            await ResolveCalendarAndHolidayAsync(response, workDate, cancellationToken);
            await ResolveAttendancePolicyAsync(response, cancellationToken);
            CalculateAttendanceWindows(response);
            BuildResolutionSnapshot(response);

            return response;
        }

        private static TrxShiftAssignment? ResolvePrimaryRoster(
            List<TrxShiftAssignment> assignments,
            AttendanceScheduleResolutionResponse response)
        {
            if (assignments.Count == 0)
            {
                return null;
            }

            var regular = assignments
                .Where(x => IsAssignmentType(x, "Regular") && !x.IsDayOff)
                .OrderBy(x => GetStatusPriority(x.AssignmentStatus))
                .ThenByDescending(x => x.IsManualOverride)
                .ThenBy(x => x.ScheduledStartAt)
                .ToList();

            var dayOff = assignments
                .Where(x => x.IsDayOff || IsAssignmentType(x, "DayOff"))
                .OrderBy(x => GetStatusPriority(x.AssignmentStatus))
                .ThenByDescending(x => x.IsManualOverride)
                .ThenBy(x => x.ScheduledStartAt)
                .ToList();

            if (regular.Count > 0 && dayOff.Count > 0)
            {
                AddConflict(response, "REGULAR_AND_DAY_OFF", "Terdapat jadwal reguler dan day-off pada tanggal yang sama.", true);
            }

            for (var i = 0; i < regular.Count; i++)
            {
                if (regular[i].HasBlockingConflict)
                {
                    AddConflict(response, "ROSTER_BLOCKING_CONFLICT", "Shift assignment utama memiliki blocking conflict.", true);
                }

                for (var j = i + 1; j < regular.Count; j++)
                {
                    if (IsOverlapping(regular[i], regular[j]))
                    {
                        AddConflict(response, "OVERLAPPING_REGULAR_SHIFT", "Terdapat regular shift yang saling bertabrakan.", true);
                    }
                }
            }

            if (regular.Count > 1 && !response.ConflictCodes.Contains("OVERLAPPING_REGULAR_SHIFT"))
            {
                response.Warnings.Add("Terdapat lebih dari satu regular shift yang tidak bertabrakan. Shift pertama digunakan sebagai jadwal utama.");
            }

            var selected = regular.FirstOrDefault() ?? dayOff.FirstOrDefault();
            if (selected != null && selected.HasBlockingConflict)
            {
                AddConflict(response, "PRIMARY_ASSIGNMENT_BLOCKED", "Primary shift assignment ditandai memiliki blocking conflict.", true);
            }

            return selected;
        }

        private static void MapRosterResolution(
            AttendanceScheduleResolutionResponse response,
            TrxShiftAssignment primary,
            List<TrxShiftAssignment> allAssignments)
        {
            response.IsResolved = true;
            response.PrimaryShiftAssignmentId = primary.Id;
            response.WorkScheduleId = primary.WorkScheduleId ?? primary.Shift?.WorkScheduleId;
            response.WorkScheduleCode = primary.WorkSchedule?.ScheduleCode ?? primary.Shift?.WorkSchedule?.ScheduleCode;
            response.WorkScheduleName = primary.WorkSchedule?.ScheduleName ?? primary.Shift?.WorkSchedule?.ScheduleName;
            response.WorkScheduleType = primary.WorkSchedule?.ScheduleType ?? primary.Shift?.WorkSchedule?.ScheduleType;
            response.ShiftId = primary.ShiftId;
            response.ShiftCode = primary.Shift?.ShiftCode;
            response.ShiftName = primary.Shift?.ShiftName;
            response.HospitalSiteId = primary.HospitalSiteId;
            response.OrganizationUnitId = primary.OrganizationUnitId;
            response.DepartmentId = primary.DepartmentId;
            response.WorkLocationId = primary.WorkLocationId;
            response.PrimaryAssignmentType = primary.AssignmentType;
            response.PrimaryAssignmentStatus = primary.AssignmentStatus;
            response.PrimaryAssignmentSource = primary.AssignmentSource;
            response.ScheduleSource = primary.IsManualOverride
                ? AttendanceValueConstants.ScheduleSource.ManualOverride
                : MapScheduleSource(primary.AssignmentStatus);
            response.IsRestDay = primary.IsDayOff || IsAssignmentType(primary, "DayOff");
            response.IsOvernight = primary.IsNightShift || primary.ScheduledEndAt.Date > primary.ScheduledStartAt.Date;
            response.BreakDurationMinutes = Math.Max(0, primary.BreakDurationMinutes);
            response.PlannedWorkMinutes = primary.PlannedWorkMinutes > 0
                ? primary.PlannedWorkMinutes
                : CalculateMinutes(primary.ScheduledStartAt, primary.ScheduledEndAt, primary.BreakDurationMinutes);

            if (!response.IsRestDay)
            {
                response.ScheduledStartAt = NormalizeUtc(primary.ScheduledStartAt);
                response.ScheduledEndAt = NormalizeUtc(primary.ScheduledEndAt);
            }

            response.AdditionalAssignments = allAssignments
                .Where(x => x.Id != primary.Id)
                .OrderBy(x => x.ScheduledStartAt)
                .Select(MapAdditionalAssignment)
                .ToList();
        }

        private async Task ResolveFixedScheduleAsync(
            AttendanceScheduleResolutionResponse response,
            Guid workforceProfileId,
            DateOnly workDate,
            CancellationToken cancellationToken)
        {
            var assignments = await _dbContext.Set<WfpWorkScheduleAssignment>()
                .AsNoTracking()
                .Include(x => x.WorkSchedule)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate <= workDate &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= workDate))
                .OrderByDescending(x => x.IsTemporary)
                .ThenByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            if (assignments.Count == 0)
            {
                response.Warnings.Add("Tidak ditemukan published roster maupun work schedule assignment aktif.");
                return;
            }

            var fixedAssignments = assignments
                .Where(x => !x.IsRotating)
                .ToList();

            if (fixedAssignments.Count == 0)
            {
                response.Warnings.Add("Work schedule assignment bersifat rotating, tetapi shift assignment harian belum dipublikasikan.");
                AddConflict(response, "ROTATING_SCHEDULE_WITHOUT_ROSTER", "Jadwal rotating harus memiliki shift assignment harian.", true);
                return;
            }

            if (fixedAssignments.Count > 1)
            {
                var activePrimaryCount = fixedAssignments.Count(x => x.IsPrimary);
                if (activePrimaryCount > 1)
                {
                    AddConflict(response, "MULTIPLE_PRIMARY_WORK_SCHEDULE", "Terdapat lebih dari satu primary work schedule assignment aktif.", true);
                }
                else
                {
                    response.Warnings.Add("Terdapat lebih dari satu fixed work schedule assignment aktif. Assignment dengan prioritas tertinggi digunakan.");
                }
            }

            var selected = fixedAssignments.First();
            if (selected.WorkSchedule == null || selected.WorkSchedule.IsDelete || !selected.WorkSchedule.IsActive)
            {
                response.Warnings.Add("Work schedule pada assignment tidak ditemukan atau tidak aktif.");
                AddConflict(response, "INVALID_WORK_SCHEDULE", "Work schedule tidak valid.", true);
                return;
            }

            response.IsResolved = true;
            response.ScheduleSource = AttendanceValueConstants.ScheduleSource.FixedWorkSchedule;
            response.WorkScheduleAssignmentId = selected.Id;
            response.WorkScheduleId = selected.WorkScheduleId;
            response.WorkScheduleCode = selected.WorkSchedule.ScheduleCode;
            response.WorkScheduleName = selected.WorkSchedule.ScheduleName;
            response.WorkScheduleType = selected.WorkSchedule.ScheduleType;
            response.HospitalSiteId = selected.HospitalSiteId;
            response.OrganizationUnitId = selected.OrganizationUnitId;
            response.DepartmentId = selected.DepartmentId;
            response.WorkLocationId = selected.WorkLocationId;
            response.PrimaryAssignmentType = selected.AssignmentType;
            response.PrimaryAssignmentStatus = "Effective";
            response.PrimaryAssignmentSource = "WorkScheduleAssignment";
            response.IsRestDay = string.Equals(selected.WorkSchedule.ScheduleType, "Off", StringComparison.OrdinalIgnoreCase);
            response.IsOvernight = selected.WorkSchedule.IsOvernight || selected.WorkSchedule.WorkEndTime <= selected.WorkSchedule.WorkStartTime;

            if (response.IsRestDay)
            {
                response.PlannedWorkMinutes = 0;
                return;
            }

            var timeZone = await ResolveTimeZoneForSiteAsync(selected.HospitalSiteId, workDate, cancellationToken);
            response.TimeZoneId = timeZone.Calendar?.TimeZoneId ?? DefaultTimeZoneId;

            var startLocal = workDate.ToDateTime(selected.WorkSchedule.WorkStartTime, DateTimeKind.Unspecified);
            var endDate = response.IsOvernight ? workDate.AddDays(1) : workDate;
            var endLocal = endDate.ToDateTime(selected.WorkSchedule.WorkEndTime, DateTimeKind.Unspecified);

            response.ScheduledStartAt = ConvertLocalToUtc(startLocal, timeZone.TimeZone, response.Warnings);
            response.ScheduledEndAt = ConvertLocalToUtc(endLocal, timeZone.TimeZone, response.Warnings);
            response.PlannedWorkMinutes = CalculateMinutes(
                response.ScheduledStartAt.Value,
                response.ScheduledEndAt.Value,
                0);
        }

        private async Task ResolveCalendarAndHolidayAsync(
            AttendanceScheduleResolutionResponse response,
            DateOnly workDate,
            CancellationToken cancellationToken)
        {
            var resolved = await ResolveTimeZoneForSiteAsync(response.HospitalSiteId, workDate, cancellationToken);
            var calendar = resolved.Calendar;
            response.TimeZoneId = calendar?.TimeZoneId ?? response.TimeZoneId ?? DefaultTimeZoneId;

            if (calendar == null)
            {
                response.Warnings.Add("Work calendar tidak ditemukan. Time zone default Asia/Jakarta digunakan.");
                return;
            }

            response.WorkCalendarId = calendar.Id;
            response.WorkCalendarCode = calendar.WorkCalendarCode;
            response.WorkCalendarName = calendar.WorkCalendarName;

            var holidays = await _dbContext.Set<MstHoliday>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkCalendarId == calendar.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .ToListAsync(cancellationToken);

            response.Holidays = holidays
                .Where(x => IsHolidayOnDate(x, workDate))
                .Select(x => new AttendanceScheduleHolidayResponse
                {
                    Id = x.Id,
                    HolidayCode = x.HolidayCode,
                    HolidayName = x.HolidayName,
                    HolidayType = x.HolidayType,
                    IsNationalHoliday = x.IsNationalHoliday,
                    IsPaidHoliday = x.IsPaidHoliday
                })
                .ToList();

            response.IsHoliday = response.Holidays.Count > 0;
        }

        private async Task ResolveAttendancePolicyAsync(
            AttendanceScheduleResolutionResponse response,
            CancellationToken cancellationToken)
        {
            var policies = await _dbContext.Set<MstAttendancePolicy>()
                .AsNoTracking()
                .Include(x => x.GracePeriodPolicy)
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    (x.WorkScheduleId == response.WorkScheduleId || x.WorkScheduleId == null))
                .ToListAsync(cancellationToken);

            var exactPolicies = policies
                .Where(x => response.WorkScheduleId.HasValue && x.WorkScheduleId == response.WorkScheduleId)
                .ToList();

            var globalPolicies = policies
                .Where(x => !x.WorkScheduleId.HasValue)
                .ToList();

            if (exactPolicies.Count(x => x.IsDefault) > 1)
            {
                response.Warnings.Add("Terdapat lebih dari satu default attendance policy untuk work schedule yang sama.");
            }

            if (globalPolicies.Count(x => x.IsDefault) > 1)
            {
                response.Warnings.Add("Terdapat lebih dari satu global default attendance policy.");
            }

            var policy = exactPolicies.FirstOrDefault(x => x.IsDefault)
                ?? exactPolicies.OrderByDescending(x => x.CreateDateTime).FirstOrDefault()
                ?? globalPolicies.FirstOrDefault(x => x.IsDefault)
                ?? globalPolicies.OrderByDescending(x => x.CreateDateTime).FirstOrDefault();

            if (policy == null)
            {
                response.Warnings.Add("Attendance policy aktif tidak ditemukan. Nilai policy default pada response digunakan.");
                return;
            }

            response.AttendancePolicyId = policy.Id;
            response.AttendancePolicyCode = policy.AttendancePolicyCode;
            response.AttendancePolicyName = policy.AttendancePolicyName;
            response.RequireCheckIn = policy.RequireCheckIn;
            response.RequireCheckOut = policy.RequireCheckOut;
            response.AllowMultipleCheckInOut = policy.AllowMultipleCheckInOut;
            response.IsOvertimeEnabled = policy.IsOvertimeEnabled;
            response.OvertimeThresholdMinutes = policy.OvertimeThresholdMinutes;
            response.IsAttendanceLocationRequired = policy.IsAttendanceLocationRequired;
            response.AllowManualCorrection = policy.AllowManualCorrection;
            response.CorrectionRequestLimitDays = policy.CorrectionRequestLimitDays;

            if (policy.GracePeriodPolicy == null ||
                policy.GracePeriodPolicy.IsDelete ||
                !policy.GracePeriodPolicy.IsActive)
            {
                if (policy.GracePeriodPolicyId.HasValue)
                {
                    response.Warnings.Add("Grace period policy pada attendance policy tidak aktif atau tidak ditemukan.");
                }

                return;
            }

            response.GracePeriodPolicyId = policy.GracePeriodPolicy.Id;
            response.GracePeriodPolicyCode = policy.GracePeriodPolicy.GracePeriodPolicyCode;
            response.GracePeriodPolicyName = policy.GracePeriodPolicy.GracePeriodPolicyName;
            response.EarlyCheckInMinutes = Math.Max(0, policy.GracePeriodPolicy.EarlyCheckInMinutes);
            response.LateCheckInGraceMinutes = Math.Max(0, policy.GracePeriodPolicy.LateCheckInGraceMinutes);
            response.EarlyCheckOutGraceMinutes = Math.Max(0, policy.GracePeriodPolicy.EarlyCheckOutGraceMinutes);
            response.LateCheckOutMinutes = Math.Max(0, policy.GracePeriodPolicy.LateCheckOutMinutes);
        }

        private async Task<TimeZoneResolution> ResolveTimeZoneForSiteAsync(
            Guid? hospitalSiteId,
            DateOnly workDate,
            CancellationToken cancellationToken)
        {
            var date = workDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            var nextDate = date.AddDays(1);

            var calendars = await _dbContext.Set<MstWorkCalendar>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.StartDate < nextDate &&
                    x.EndDate >= date &&
                    (x.HospitalSiteId == hospitalSiteId || x.HospitalSiteId == null))
                .ToListAsync(cancellationToken);

            var calendar = calendars
                .OrderByDescending(x => hospitalSiteId.HasValue && x.HospitalSiteId == hospitalSiteId)
                .ThenByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CalendarYear == workDate.Year)
                .ThenByDescending(x => x.StartDate)
                .FirstOrDefault();

            var timeZoneId = calendar?.TimeZoneId ?? DefaultTimeZoneId;
            return new TimeZoneResolution
            {
                Calendar = calendar,
                TimeZone = ResolveTimeZone(timeZoneId)
            };
        }

        private static void CalculateAttendanceWindows(AttendanceScheduleResolutionResponse response)
        {
            if (!response.ScheduledStartAt.HasValue || !response.ScheduledEndAt.HasValue)
            {
                return;
            }

            response.EarliestCheckInAt = response.ScheduledStartAt.Value.AddMinutes(-response.EarlyCheckInMinutes);
            response.LatestGraceCheckInAt = response.ScheduledStartAt.Value.AddMinutes(response.LateCheckInGraceMinutes);
            response.EarliestGraceCheckOutAt = response.ScheduledEndAt.Value.AddMinutes(-response.EarlyCheckOutGraceMinutes);
            response.LatestCheckOutAt = response.ScheduledEndAt.Value.AddMinutes(response.LateCheckOutMinutes);
        }

        private static void BuildResolutionSnapshot(AttendanceScheduleResolutionResponse response)
        {
            var snapshot = new
            {
                response.WorkforceProfileId,
                response.WorkDate,
                response.IsResolved,
                response.ScheduleSource,
                response.PrimaryShiftAssignmentId,
                response.WorkScheduleAssignmentId,
                response.WorkScheduleId,
                response.ShiftId,
                response.HospitalSiteId,
                response.OrganizationUnitId,
                response.DepartmentId,
                response.WorkLocationId,
                response.ScheduledStartAt,
                response.ScheduledEndAt,
                response.IsOvernight,
                response.IsRestDay,
                response.IsHoliday,
                HolidayIds = response.Holidays.Select(x => x.Id).ToList(),
                response.WorkCalendarId,
                response.TimeZoneId,
                response.AttendancePolicyId,
                response.GracePeriodPolicyId,
                response.EarliestCheckInAt,
                response.LatestGraceCheckInAt,
                response.EarliestGraceCheckOutAt,
                response.LatestCheckOutAt,
                response.HasBlockingConflict,
                response.ConflictCodes,
                response.Warnings,
                AdditionalAssignmentIds = response.AdditionalAssignments
                    .Select(x => x.ShiftAssignmentId)
                    .ToList()
            };

            response.ResolutionSnapshotJson = JsonSerializer.Serialize(snapshot);
        }

        private static AttendanceScheduleAssignmentResponse MapAdditionalAssignment(TrxShiftAssignment assignment)
        {
            return new AttendanceScheduleAssignmentResponse
            {
                ShiftAssignmentId = assignment.Id,
                WorkScheduleId = assignment.WorkScheduleId ?? assignment.Shift?.WorkScheduleId,
                WorkScheduleCode = assignment.WorkSchedule?.ScheduleCode ?? assignment.Shift?.WorkSchedule?.ScheduleCode,
                WorkScheduleName = assignment.WorkSchedule?.ScheduleName ?? assignment.Shift?.WorkSchedule?.ScheduleName,
                ShiftId = assignment.ShiftId,
                ShiftCode = assignment.Shift?.ShiftCode,
                ShiftName = assignment.Shift?.ShiftName,
                AssignmentType = assignment.AssignmentType,
                AssignmentStatus = assignment.AssignmentStatus,
                AssignmentSource = assignment.AssignmentSource,
                ScheduledStartAt = NormalizeUtc(assignment.ScheduledStartAt),
                ScheduledEndAt = NormalizeUtc(assignment.ScheduledEndAt),
                BreakDurationMinutes = assignment.BreakDurationMinutes,
                PlannedWorkMinutes = assignment.PlannedWorkMinutes,
                IsNightShift = assignment.IsNightShift,
                IsOnCall = assignment.IsOnCall,
                IsDayOff = assignment.IsDayOff,
                IsManualOverride = assignment.IsManualOverride,
                HasBlockingConflict = assignment.HasBlockingConflict
            };
        }

        private static bool IsAssignmentType(TrxShiftAssignment assignment, string value) =>
            string.Equals(assignment.AssignmentType, value, StringComparison.OrdinalIgnoreCase);

        private static bool IsOverlapping(TrxShiftAssignment first, TrxShiftAssignment second) =>
            first.ScheduledStartAt < second.ScheduledEndAt && second.ScheduledStartAt < first.ScheduledEndAt;

        private static int GetStatusPriority(string status) => status switch
        {
            AttendanceValueConstants.ShiftAssignmentStatus.Published => 1,
            AttendanceValueConstants.ShiftAssignmentStatus.Confirmed => 2,
            AttendanceValueConstants.ShiftAssignmentStatus.Completed => 3,
            _ => 99
        };

        private static string MapScheduleSource(string status) => status switch
        {
            AttendanceValueConstants.ShiftAssignmentStatus.Published => AttendanceValueConstants.ScheduleSource.PublishedRoster,
            AttendanceValueConstants.ShiftAssignmentStatus.Confirmed => AttendanceValueConstants.ScheduleSource.ConfirmedRoster,
            AttendanceValueConstants.ShiftAssignmentStatus.Completed => AttendanceValueConstants.ScheduleSource.CompletedRoster,
            _ => AttendanceValueConstants.ScheduleSource.Unresolved
        };

        private static int CalculateMinutes(DateTime startAt, DateTime endAt, int breakMinutes)
        {
            if (endAt <= startAt)
            {
                return 0;
            }

            return Math.Max(0, (int)Math.Round((endAt - startAt).TotalMinutes) - Math.Max(0, breakMinutes));
        }

        private static void AddConflict(
            AttendanceScheduleResolutionResponse response,
            string code,
            string warning,
            bool blocking)
        {
            if (!response.ConflictCodes.Contains(code))
            {
                response.ConflictCodes.Add(code);
            }

            if (!response.Warnings.Contains(warning))
            {
                response.Warnings.Add(warning);
            }

            if (blocking)
            {
                response.HasBlockingConflict = true;
            }
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static DateTime ConvertLocalToUtc(
            DateTime localDateTime,
            TimeZoneInfo timeZone,
            List<string> warnings)
        {
            if (timeZone.IsInvalidTime(localDateTime))
            {
                warnings.Add("Waktu jadwal berada pada rentang waktu lokal yang tidak valid. Waktu digeser satu jam.");
                localDateTime = localDateTime.AddHours(1);
            }

            if (timeZone.IsAmbiguousTime(localDateTime))
            {
                warnings.Add("Waktu jadwal bersifat ambigu pada time zone terkait. Offset standar digunakan.");
            }

            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
                timeZone);
        }

        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            var requested = string.IsNullOrWhiteSpace(timeZoneId)
                ? DefaultTimeZoneId
                : timeZoneId.Trim();

            var candidates = new List<string> { requested };
            if (string.Equals(requested, "Asia/Jakarta", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("SE Asia Standard Time");
            }
            else if (string.Equals(requested, "SE Asia Standard Time", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("Asia/Jakarta");
            }

            foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(candidate);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Try the next compatible identifier.
                }
                catch (InvalidTimeZoneException)
                {
                    // Try the next compatible identifier.
                }
            }

            return TimeZoneInfo.Utc;
        }

        private static bool IsHolidayOnDate(MstHoliday holiday, DateOnly workDate)
        {
            var date = workDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified).Date;
            var start = holiday.StartDate.Date;
            var end = holiday.EndDate.Date;

            if (!holiday.IsRecurringAnnually)
            {
                return date >= start && date <= end;
            }

            var value = workDate.Month * 100 + workDate.Day;
            var startValue = start.Month * 100 + start.Day;
            var endValue = end.Month * 100 + end.Day;

            return startValue <= endValue
                ? value >= startValue && value <= endValue
                : value >= startValue || value <= endValue;
        }

        private sealed class WorkforceSnapshot
        {
            public Guid Id { get; set; }
            public string ProfileCode { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
        }

        private sealed class TimeZoneResolution
        {
            public MstWorkCalendar? Calendar { get; set; }
            public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
        }
    }
}
