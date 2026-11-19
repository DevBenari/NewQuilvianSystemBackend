using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services
{
    /// <summary>
    /// Jadwal Saya (HRD-DEC-209): jadwal terbit milik pegawai yang login secara rinci, dan roster terbit unitnya
    /// berisi nama + kode shift tanpa alasan ketidakhadiran rekan. Identitas selalu dari sesi; roster yang belum terbit
    /// (Draft/Validation) tidak pernah dibaca. Rentang mengikuti HRD-ASM-80 (VR-PK-29).
    /// </summary>
    public class MyScheduleSelfService
    {
        private const string RangeMessage = "Jadwal yang dapat dilihat: 3 bulan ke belakang sampai bulan depan";
        private const int MaximumRangeDays = 31;
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);

        // Roster dianggap terbit sejak Published, termasuk sesudah ditutup.
        private static readonly string[] PublishedRosterStatuses = { "Published", "Closed" };

        private static readonly string[] AcceptedShiftStatuses =
        {
            AttendanceValueConstants.ShiftAssignmentStatus.Published,
            AttendanceValueConstants.ShiftAssignmentStatus.Confirmed,
            AttendanceValueConstants.ShiftAssignmentStatus.Completed
        };

        private static readonly string[] TakenLeaveStatuses =
        {
            LeaveRequestValueConstants.Status.Approved,
            LeaveRequestValueConstants.Status.Taken,
            LeaveRequestValueConstants.Status.Completed
        };

        private static readonly string[] InactiveOnCallStatuses = { "Cancelled", "Rejected" };

        private readonly ApplicationDbContext _dbContext;
        private readonly HumanResourceContextService _contextService;
        private readonly AttendanceScheduleResolverService _scheduleResolver;

        public MyScheduleSelfService(
            ApplicationDbContext dbContext,
            HumanResourceContextService contextService,
            AttendanceScheduleResolverService scheduleResolver)
        {
            _dbContext = dbContext;
            _contextService = contextService;
            _scheduleResolver = scheduleResolver;
        }

        public async Task<MyScheduleServiceResult<List<MyScheduleDayResponse>>> GetMyScheduleAsync(
            Guid userId,
            MyScheduleQueryRequest request,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var (minDate, maxDate, today) = Window(utcNow);
            var from = request.FromDate ?? new DateOnly(today.Year, today.Month, 1);
            var to = request.ToDate ?? new DateOnly(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            if (to < from) return MyScheduleServiceResult<List<MyScheduleDayResponse>>.Fail(StatusCodes.Status400BadRequest, "Tanggal selesai tidak boleh sebelum tanggal mulai.");
            if (from < minDate || to > maxDate) return MyScheduleServiceResult<List<MyScheduleDayResponse>>.Fail(StatusCodes.Status400BadRequest, RangeMessage);
            if (to.DayNumber - from.DayNumber + 1 > MaximumRangeDays)
            {
                return MyScheduleServiceResult<List<MyScheduleDayResponse>>.Fail(StatusCodes.Status400BadRequest, $"Satu permintaan paling panjang {MaximumRangeDays} hari.");
            }

            var me = await ResolveMeAsync(userId, cancellationToken);
            if (!me.Success) return MyScheduleServiceResult<List<MyScheduleDayResponse>>.Fail(me.StatusCode, me.Message);
            var context = me.Data!;
            var myId = context.WorkforceProfileId!.Value;

            var shifts = await PublishedShiftsQuery(from, to)
                .Where(x => x.Shift.WorkforceProfileId == myId)
                .ToListAsync(cancellationToken);
            var revisions = await LatestRevisionsAsync(shifts.Select(x => x.Shift.Id).ToList(), cancellationToken);
            var unitNames = await UnitNamesAsync(shifts.Select(x => x.UnitId), cancellationToken);
            var released = await ReleasedShiftsAsync(myId, from, to, cancellationToken);
            var onCalls = await OnCallsAsync(myId, from, to, cancellationToken);
            var absences = await AbsencesAsync(new List<Guid> { myId }, from, to, cancellationToken);
            var resolved = await _scheduleResolver.ResolveRangeAsync(myId, from, to, cancellationToken);
            var resolvedByDate = resolved.Success && resolved.Data != null
                ? resolved.Data.Items.ToDictionary(x => x.WorkDate)
                : new Dictionary<DateOnly, AttendanceScheduleResolutionResponse>();

            var days = new List<MyScheduleDayResponse>();
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var day = new MyScheduleDayResponse { Date = date, WorkLocationName = context.WorkLocationName };
                resolvedByDate.TryGetValue(date, out var resolution);
                if (resolution != null)
                {
                    day.IsHoliday = resolution.IsHoliday;
                    day.HolidayNames = resolution.Holidays.Select(x => x.HolidayName).ToList();
                }

                day.Shifts = shifts
                    .Where(x => x.Shift.ShiftDate == date)
                    .OrderBy(x => x.Shift.ScheduledStartAt)
                    .Select(x => MapShift(x, revisions.GetValueOrDefault(x.Shift.Id), unitNames))
                    .ToList();
                day.ReleasedShifts = released.Where(x => x.Date == date).Select(x => x.Response).ToList();
                day.OnCalls = onCalls.Where(x => x.Date == date).Select(x => x.Response).ToList();

                if (day.Shifts.Count > 0)
                {
                    day.ScheduleSource = AttendanceValueConstants.ScheduleSource.PublishedRoster;
                }
                else if (resolution != null &&
                         resolution.ScheduleSource == AttendanceValueConstants.ScheduleSource.FixedWorkSchedule)
                {
                    day.ScheduleSource = resolution.ScheduleSource;
                    if (!resolution.IsRestDay)
                    {
                        day.FixedSchedule = new MyFixedScheduleResponse
                        {
                            WorkScheduleName = resolution.WorkScheduleName,
                            StartAt = resolution.ScheduledStartAt,
                            EndAt = resolution.ScheduledEndAt
                        };
                    }
                }

                var absence = absences.FirstOrDefault(x => x.WorkforceProfileId == myId && x.Date == date);
                if (absence != null)
                {
                    day.DayStatus = absence.Status;
                    day.LeaveTypeName = absence.LeaveTypeName;
                }
                else if (day.Shifts.Count > 0 || day.FixedSchedule != null)
                {
                    // Jadwal tetap pada hari libur: tidak wajib masuk (HRD-DEC-210 butir 2); roster tetap berlaku (butir 3).
                    day.DayStatus = day.FixedSchedule != null && day.IsHoliday ? "Holiday" : "Scheduled";
                }
                else if (day.ScheduleSource == AttendanceValueConstants.ScheduleSource.FixedWorkSchedule)
                {
                    day.DayStatus = day.IsHoliday ? "Holiday" : "RestDay";
                }
                else
                {
                    day.DayStatus = "NoSchedule";
                }

                day.DayStatusLabel = DayStatusLabel(day.DayStatus);
                days.Add(day);
            }

            return MyScheduleServiceResult<List<MyScheduleDayResponse>>.Ok(days, "Jadwal saya berhasil diambil.");
        }

        public async Task<MyScheduleServiceResult<MyUnitRosterResponse>> GetMyUnitRosterAsync(
            Guid userId,
            MyUnitRosterQueryRequest request,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var (minDate, maxDate, today) = Window(utcNow);
            DateOnly monthStart;
            if (string.IsNullOrWhiteSpace(request.Month))
            {
                monthStart = new DateOnly(today.Year, today.Month, 1);
            }
            else if (DateOnly.TryParseExact(request.Month.Trim() + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                monthStart = parsed;
            }
            else
            {
                return MyScheduleServiceResult<MyUnitRosterResponse>.Fail(StatusCodes.Status400BadRequest, "Format bulan harus yyyy-MM.");
            }

            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            if (monthStart < minDate || monthEnd > maxDate) return MyScheduleServiceResult<MyUnitRosterResponse>.Fail(StatusCodes.Status400BadRequest, RangeMessage);

            var me = await ResolveMeAsync(userId, cancellationToken);
            if (!me.Success) return MyScheduleServiceResult<MyUnitRosterResponse>.Fail(me.StatusCode, me.Message);
            var myId = me.Data!.WorkforceProfileId!.Value;

            // Unit sendiri: unit penempatan utama, ditambah unit yang roster terbitnya memuat shift pegawai ini bulan itu
            // (penempatan sementara, HRD-DEC-213). Unit lain tidak pernah dibaca (HRD-DEC-209 butir 5).
            var unitIds = await PublishedShiftsQuery(monthStart, monthEnd)
                .Where(x => x.Shift.WorkforceProfileId == myId && x.UnitId != null)
                .Select(x => x.UnitId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
            if (me.Data.OrganizationUnitId.HasValue && !unitIds.Contains(me.Data.OrganizationUnitId.Value))
            {
                unitIds.Add(me.Data.OrganizationUnitId.Value);
            }

            var response = new MyUnitRosterResponse
            {
                Month = monthStart.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                StartDate = monthStart,
                EndDate = monthEnd
            };
            if (unitIds.Count == 0)
            {
                return MyScheduleServiceResult<MyUnitRosterResponse>.Ok(response, "Pegawai belum punya unit penempatan.");
            }

            var rows = await PublishedShiftsQuery(monthStart, monthEnd)
                .Where(x => x.UnitId != null && unitIds.Contains(x.UnitId.Value))
                .ToListAsync(cancellationToken);
            var memberIds = rows.Select(x => x.Shift.WorkforceProfileId).Distinct().ToList();
            var names = await _dbContext.Set<MstWorkforceProfile>().AsNoTracking()
                .Where(x => memberIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName })
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
            var unitNames = await _dbContext.Set<MstOrganizationUnit>().AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .Select(x => new { x.Id, x.UnitName })
                .ToDictionaryAsync(x => x.Id, x => x.UnitName, cancellationToken);
            var absences = await AbsencesAsync(memberIds, monthStart, monthEnd, cancellationToken);
            var absent = absences.Select(x => (x.WorkforceProfileId, x.Date)).ToHashSet();

            foreach (var unitId in unitIds)
            {
                var unitRows = rows.Where(x => x.UnitId == unitId).ToList();
                response.Units.Add(new MyUnitRosterUnitResponse
                {
                    OrganizationUnitId = unitId,
                    OrganizationUnitName = unitNames.GetValueOrDefault(unitId),
                    RosterPeriodCodes = unitRows.Select(x => x.RosterPeriodCode).Distinct().OrderBy(x => x).ToList(),
                    Members = unitRows
                        .GroupBy(x => x.Shift.WorkforceProfileId)
                        .Select(g => new MyUnitRosterMemberResponse
                        {
                            WorkforceProfileId = g.Key,
                            DisplayName = names.GetValueOrDefault(g.Key) ?? string.Empty,
                            IsMe = g.Key == myId,
                            // Hari ketidakhadiran dihilangkan sehingga tampil sama dengan hari libur biasa: tanpa alasan.
                            Days = g.Where(x => !absent.Contains((g.Key, x.Shift.ShiftDate)))
                                .OrderBy(x => x.Shift.ShiftDate).ThenBy(x => x.Shift.ScheduledStartAt)
                                .Select(x => new MyUnitRosterDayResponse
                                {
                                    Date = x.Shift.ShiftDate,
                                    ShiftCode = x.ShiftCode,
                                    ShiftName = x.ShiftName,
                                    ColorCode = x.ColorCode
                                })
                                .ToList()
                        })
                        .OrderBy(x => x.DisplayName)
                        .ToList()
                });
            }

            return MyScheduleServiceResult<MyUnitRosterResponse>.Ok(response, "Roster unit berhasil diambil.");
        }

        private static (DateOnly Min, DateOnly Max, DateOnly Today) Window(DateTime utcNow)
        {
            var today = DateOnly.FromDateTime(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc).Add(WibOffset));
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            return (monthStart.AddMonths(-3), monthStart.AddMonths(2).AddDays(-1), today);
        }

        private async Task<MyScheduleServiceResult<HumanResourceUserContextDto>> ResolveMeAsync(Guid userId, CancellationToken cancellationToken)
        {
            HumanResourceUserContextDto context;
            try
            {
                context = await _contextService.GetByUserIdAsync(userId, cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return MyScheduleServiceResult<HumanResourceUserContextDto>.Fail(StatusCodes.Status401Unauthorized, ex.Message);
            }

            return context.WorkforceProfileId.HasValue
                ? MyScheduleServiceResult<HumanResourceUserContextDto>.Ok(context, string.Empty)
                : MyScheduleServiceResult<HumanResourceUserContextDto>.Fail(StatusCodes.Status404NotFound, "Akun ini belum terhubung ke profil pegawai, sehingga tidak punya jadwal.");
        }

        private IQueryable<PublishedShiftRow> PublishedShiftsQuery(DateOnly startDate, DateOnly endDate) =>
            from s in _dbContext.Set<TrxShiftAssignment>().AsNoTracking()
            join ra in _dbContext.Set<TrxRosterAssignment>().AsNoTracking() on s.RosterAssignmentId equals ra.Id
            join rp in _dbContext.Set<TrxRosterPeriod>().AsNoTracking() on ra.RosterPeriodId equals rp.Id
            join sh in _dbContext.Set<MstShift>().AsNoTracking() on s.ShiftId equals sh.Id into shj
            from sh in shj.DefaultIfEmpty()
            where s.ShiftDate >= startDate && s.ShiftDate <= endDate &&
                  s.IsActive && !s.IsDelete && !s.IsCancel &&
                  AcceptedShiftStatuses.Contains(s.AssignmentStatus) &&
                  !rp.IsDelete && PublishedRosterStatuses.Contains(rp.RosterStatus)
            select new PublishedShiftRow
            {
                Shift = s,
                UnitId = rp.OrganizationUnitId,
                RosterPeriodCode = rp.RosterPeriodCode,
                ShiftCode = sh != null ? sh.ShiftCode : null,
                ShiftName = sh != null ? sh.ShiftName : null,
                ColorCode = sh != null ? sh.ColorCode : null
            };

        private async Task<Dictionary<Guid, (HrdShiftAssignmentRevision Revision, string? PreviousShiftName)>> LatestRevisionsAsync(
            List<Guid> shiftIds,
            CancellationToken cancellationToken)
        {
            if (shiftIds.Count == 0) return new();
            var revisions = await _dbContext.Set<HrdShiftAssignmentRevision>().AsNoTracking()
                .Where(x => !x.IsDelete && shiftIds.Contains(x.ShiftAssignmentId))
                .ToListAsync(cancellationToken);
            var latest = revisions.GroupBy(x => x.ShiftAssignmentId).Select(g => g.OrderByDescending(x => x.ChangedAt).First()).ToList();
            var shiftNames = await ShiftNamesAsync(latest.Select(x => x.PreviousShiftId), cancellationToken);
            return latest.ToDictionary(
                x => x.ShiftAssignmentId,
                x => (x, x.PreviousShiftId.HasValue ? shiftNames.GetValueOrDefault(x.PreviousShiftId.Value) : null));
        }

        private async Task<List<(DateOnly Date, MyReleasedShiftResponse Response)>> ReleasedShiftsAsync(
            Guid myId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
        {
            var removeShift = SchedulingRequestValueConstants.ShiftRevisionChangeType.RemoveShift;
            var candidates = await _dbContext.Set<HrdShiftAssignmentRevision>().AsNoTracking()
                .Where(x => !x.IsDelete && x.PreviousWorkforceProfileId == myId && x.ShiftDate >= from && x.ShiftDate <= to &&
                            (x.NewWorkforceProfileId != myId || x.ChangeType == removeShift))
                .Join(_dbContext.Set<TrxRosterPeriod>().AsNoTracking().Where(p => PublishedRosterStatuses.Contains(p.RosterStatus)),
                    r => r.RosterPeriodId, p => p.Id, (r, p) => r)
                .ToListAsync(cancellationToken);
            if (candidates.Count == 0) return new();

            // Shift yang sekarang kembali menjadi milik pegawai ini (mis. A → B → A) bukan shift yang dilepas.
            var ids = candidates.Select(x => x.ShiftAssignmentId).Distinct().ToList();
            var stillMine = await _dbContext.Set<TrxShiftAssignment>().AsNoTracking()
                .Where(x => ids.Contains(x.Id) && x.WorkforceProfileId == myId && x.IsActive && !x.IsDelete && !x.IsCancel &&
                            AcceptedShiftStatuses.Contains(x.AssignmentStatus))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            var latest = candidates.Where(x => !stillMine.Contains(x.ShiftAssignmentId))
                .GroupBy(x => x.ShiftAssignmentId)
                .Select(g => g.OrderByDescending(x => x.ChangedAt).First())
                .ToList();
            var names = await ShiftNamesAsync(latest.Select(x => x.PreviousShiftId), cancellationToken);
            return latest.Select(x => (x.ShiftDate, new MyReleasedShiftResponse
            {
                ShiftAssignmentId = x.ShiftAssignmentId,
                ShiftName = x.PreviousShiftId.HasValue ? names.GetValueOrDefault(x.PreviousShiftId.Value) : null,
                StartAt = x.PreviousStartAt,
                EndAt = x.PreviousEndAt,
                ChangeType = x.ChangeType,
                ChangeTypeLabel = SchedulingRequestValueConstants.ShiftRevisionChangeType.Label(x.ChangeType),
                ChangedAt = x.ChangedAt
            })).ToList();
        }

        private async Task<List<(DateOnly Date, MyOnCallResponse Response)>> OnCallsAsync(
            Guid myId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
        {
            var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - WibOffset;
            var toUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - WibOffset;
            var rows = await (
                    from o in _dbContext.Set<TrxOnCallAssignment>().AsNoTracking()
                    join rp in _dbContext.Set<TrxRosterPeriod>().AsNoTracking() on o.RosterPeriodId equals rp.Id
                    join t in _dbContext.Set<MstOnCallType>().AsNoTracking() on o.OnCallTypeId equals t.Id into tj
                    from t in tj.DefaultIfEmpty()
                    where o.WorkforceProfileId == myId && o.IsActive && !o.IsDelete && !o.IsCancel &&
                          !InactiveOnCallStatuses.Contains(o.AssignmentStatus) &&
                          PublishedRosterStatuses.Contains(rp.RosterStatus) &&
                          o.StartAt < toUtc && o.EndAt > fromUtc
                    select new { o.Id, o.StartAt, o.EndAt, o.OnCallRole, o.AssignmentStatus, TypeName = t != null ? t.OnCallTypeName : null })
                .ToListAsync(cancellationToken);
            return rows.Select(x => (
                DateOnly.FromDateTime(DateTime.SpecifyKind(x.StartAt, DateTimeKind.Utc).Add(WibOffset)),
                new MyOnCallResponse
                {
                    OnCallAssignmentId = x.Id,
                    OnCallTypeName = x.TypeName,
                    OnCallRole = x.OnCallRole,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    AssignmentStatus = x.AssignmentStatus
                })).ToList();
        }

        private sealed record Absence(Guid WorkforceProfileId, DateOnly Date, string Status, string? LeaveTypeName);

        /// <summary>Cuti/sakit yang disetujui, dinas luar disetujui, dan tugas belajar aktif, per pegawai per tanggal.</summary>
        private async Task<List<Absence>> AbsencesAsync(List<Guid> workforceIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            var from = startDate;
            var to = endDate;
            var result = new List<Absence>();
            if (workforceIds.Count == 0) return result;

            var leaves = await (
                    from l in _dbContext.Set<WfpLeaveRequest>().AsNoTracking()
                    join t in _dbContext.Set<MstLeaveType>().AsNoTracking() on l.LeaveTypeId equals t.Id into tj
                    from t in tj.DefaultIfEmpty()
                    where workforceIds.Contains(l.WorkforceProfileId) && !l.IsDelete &&
                          TakenLeaveStatuses.Contains(l.LeaveRequestStatus) && l.StartDate <= endDate && l.EndDate >= startDate
                    select new { l.WorkforceProfileId, l.StartDate, l.EndDate, Category = t != null ? t.LeaveCategory : null, Name = t != null ? t.LeaveTypeName : null })
                .ToListAsync(cancellationToken);
            foreach (var l in leaves)
            {
                for (var d = Max(l.StartDate, from); d <= Min(l.EndDate, to); d = d.AddDays(1))
                {
                    result.Add(new Absence(l.WorkforceProfileId, d, l.Category == "Sick" ? "SickLeave" : "Leave", l.Name));
                }
            }

            var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - WibOffset;
            var toUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - WibOffset;
            var duties = await _dbContext.Set<HrdOfficialDutyRequest>().AsNoTracking()
                .Where(x => workforceIds.Contains(x.WorkforceProfileId) && !x.IsDelete && x.Status == "Approved" && x.StartAt < toUtc && x.EndAt > fromUtc)
                .Select(x => new { x.WorkforceProfileId, x.StartAt, x.EndAt })
                .ToListAsync(cancellationToken);
            foreach (var x in duties)
            {
                var start = DateOnly.FromDateTime(DateTime.SpecifyKind(x.StartAt, DateTimeKind.Utc).Add(WibOffset));
                var end = DateOnly.FromDateTime(DateTime.SpecifyKind(x.EndAt, DateTimeKind.Utc).Add(WibOffset).AddTicks(-1));
                for (var d = Max(start, from); d <= Min(end, to); d = d.AddDays(1))
                {
                    result.Add(new Absence(x.WorkforceProfileId, d, "OfficialDuty", null));
                }
            }

            var active = AttendanceValueConstants.StudyAssignmentStatus.Active;
            var studies = await _dbContext.Set<HrdStudyAssignment>().AsNoTracking()
                .Where(x => workforceIds.Contains(x.WorkforceProfileId) && !x.IsDelete && x.Status == active && x.StartDate <= to && x.EndDate >= from)
                .Select(x => new { x.WorkforceProfileId, x.StartDate, x.EndDate })
                .ToListAsync(cancellationToken);
            foreach (var x in studies)
            {
                for (var d = Max(x.StartDate, from); d <= Min(x.EndDate, to); d = d.AddDays(1))
                {
                    result.Add(new Absence(x.WorkforceProfileId, d, "StudyAssignment", null));
                }
            }

            // Satu status per pegawai per tanggal: tugas belajar > dinas luar > cuti.
            var rank = new Dictionary<string, int> { ["StudyAssignment"] = 0, ["OfficialDuty"] = 1, ["SickLeave"] = 2, ["Leave"] = 3 };
            return result.GroupBy(x => (x.WorkforceProfileId, x.Date)).Select(g => g.OrderBy(x => rank[x.Status]).First()).ToList();
        }

        private async Task<Dictionary<Guid, string>> UnitNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        {
            var list = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            if (list.Count == 0) return new();
            return await _dbContext.Set<MstOrganizationUnit>().AsNoTracking()
                .Where(x => list.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.UnitName, cancellationToken);
        }

        private async Task<Dictionary<Guid, string>> ShiftNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        {
            var list = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            if (list.Count == 0) return new();
            return await _dbContext.Set<MstShift>().AsNoTracking()
                .Where(x => list.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.ShiftName, cancellationToken);
        }

        private static MyScheduleShiftResponse MapShift(
            PublishedShiftRow row,
            (HrdShiftAssignmentRevision Revision, string? PreviousShiftName) revision,
            IReadOnlyDictionary<Guid, string> unitNames)
        {
            var response = new MyScheduleShiftResponse
            {
                ShiftAssignmentId = row.Shift.Id,
                ShiftId = row.Shift.ShiftId,
                ShiftCode = row.ShiftCode,
                ShiftName = row.ShiftName,
                ColorCode = row.ColorCode,
                StartAt = row.Shift.ScheduledStartAt,
                EndAt = row.Shift.ScheduledEndAt,
                IsNightShift = row.Shift.IsNightShift,
                OrganizationUnitId = row.UnitId,
                OrganizationUnitName = row.UnitId.HasValue ? unitNames.GetValueOrDefault(row.UnitId.Value) : null,
                RosterPeriodCode = row.RosterPeriodCode
            };
            if (revision.Revision != null)
            {
                response.IsChanged = true;
                response.ChangeType = revision.Revision.ChangeType;
                response.ChangeTypeLabel = SchedulingRequestValueConstants.ShiftRevisionChangeType.Label(revision.Revision.ChangeType);
                response.ChangedAt = revision.Revision.ChangedAt;
                response.PreviousShiftName = revision.PreviousShiftName;
                response.PreviousStartAt = revision.Revision.PreviousStartAt;
                response.PreviousEndAt = revision.Revision.PreviousEndAt;
            }

            return response;
        }

        private static string DayStatusLabel(string status) => status switch
        {
            "Scheduled" => "Dinas",
            "RestDay" => "Libur",
            "Holiday" => "Hari libur",
            "Leave" => "Cuti",
            "SickLeave" => "Sakit",
            "OfficialDuty" => "Dinas Luar",
            "StudyAssignment" => "Tugas Belajar",
            _ => "Tidak ada jadwal terbit"
        };

        private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;
        private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

        private sealed class PublishedShiftRow
        {
            public TrxShiftAssignment Shift { get; set; } = null!;
            public Guid? UnitId { get; set; }
            public string RosterPeriodCode { get; set; } = string.Empty;
            public string? ShiftCode { get; set; }
            public string? ShiftName { get; set; }
            public string? ColorCode { get; set; }
        }
    }

    public class MyScheduleServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static MyScheduleServiceResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static MyScheduleServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
