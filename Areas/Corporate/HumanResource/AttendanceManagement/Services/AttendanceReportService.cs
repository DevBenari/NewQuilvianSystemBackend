using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Tujuh laporan kehadiran M12 (HRD-DEC-206): shift, attendance, leave, lateness, overtime, absence, unit-summary.
    /// Cakupan data mengikuti cakupan baca pengguna (HrdUnitScopeService; penanda AttendanceReport : ReadAllUnits),
    /// rentang paling lama 92 hari (VR-PK-28), dan data sakit hanya ditulis "Sakit". Layar dan ekspor (BE-HRD-350)
    /// memakai BuildAsync yang sama sehingga hasilnya identik untuk filter yang sama.
    /// </summary>
    public class AttendanceReportService
    {
        public const string ForbiddenUnitMessage = "Anda tidak berwenang melihat data unit ini";
        public const string RangeMessage = "Rentang laporan paling lama 92 hari";
        private const int MaximumRangeDays = 92;
        private const string SickLabel = "Sakit";
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);

        private static readonly string[] PublishedRosterStatuses = { "Published", "Closed" };
        private static readonly string[] TakenLeaveStatuses =
        {
            LeaveRequestValueConstants.Status.Approved,
            LeaveRequestValueConstants.Status.Taken,
            LeaveRequestValueConstants.Status.Completed
        };
        private static readonly string[] ApprovedOvertimeStatuses =
        {
            "ApprovedForWork", "InProgress", "WaitingRealization", "WaitingVerification", "Realized", "PostedToPayroll"
        };
        private static readonly string[] AbsenceDailyStatuses =
        {
            AttendanceValueConstants.AttendanceStatus.Absent,
            AttendanceValueConstants.AttendanceStatus.NoScan,
            AttendanceValueConstants.AttendanceStatus.IncompletePunch,
            AttendanceValueConstants.AttendanceStatus.Incomplete
        };

        public static readonly IReadOnlyDictionary<string, string> ReportNames = new Dictionary<string, string>
        {
            ["shift"] = "Laporan Shift",
            ["attendance"] = "Laporan Kehadiran",
            ["leave"] = "Laporan Cuti",
            ["lateness"] = "Laporan Keterlambatan",
            ["overtime"] = "Laporan Lembur",
            ["absence"] = "Laporan Ketidakhadiran",
            ["unit-summary"] = "Rekap Per-Unit"
        };

        private static readonly (string Key, string Label)[] Who = { ("profileCode", "Kode Pegawai"), ("employeeName", "Nama"), ("organizationUnitName", "Unit") };

        private static readonly IReadOnlyDictionary<string, (string Key, string Label)[]> ColumnsByReport = new Dictionary<string, (string, string)[]>
        {
            ["shift"] = Who.Concat(new[] { ("date", "Tanggal"), ("shiftName", "Shift"), ("startAt", "Mulai"), ("endAt", "Selesai"), ("rosterPeriodCode", "Roster"), ("statusLabel", "Status") }).ToArray(),
            ["attendance"] = Who.Concat(new[] { ("date", "Tanggal"), ("shiftName", "Shift"), ("checkInAt", "Masuk"), ("checkOutAt", "Pulang"), ("statusLabel", "Status"), ("lateMinutes", "Terlambat (menit)"), ("earlyLeaveMinutes", "Pulang Cepat (menit)"), ("workMinutes", "Kerja (menit)") }).ToArray(),
            ["leave"] = Who.Concat(new[] { ("date", "Mulai"), ("endDate", "Selesai"), ("days", "Hari"), ("category", "Jenis"), ("statusLabel", "Status") }).ToArray(),
            ["lateness"] = Who.Concat(new[] { ("date", "Tanggal"), ("shiftName", "Shift"), ("startAt", "Jadwal Masuk"), ("checkInAt", "Masuk"), ("lateMinutes", "Terlambat (menit)") }).ToArray(),
            ["overtime"] = Who.Concat(new[] { ("date", "Tanggal"), ("startAt", "Rencana Mulai"), ("endAt", "Rencana Selesai"), ("requestedMinutes", "Diajukan (menit)"), ("approvedMinutes", "Disetujui (menit)"), ("statusLabel", "Status") }).ToArray(),
            ["absence"] = Who.Concat(new[] { ("date", "Tanggal"), ("category", "Keterangan") }).ToArray(),
            ["unit-summary"] = new[] { ("organizationUnitName", "Unit"), ("employeeCount", "Jumlah Pegawai"), ("presentDays", "Hadir"), ("lateCount", "Terlambat"), ("absentDays", "Alpa"), ("noScanDays", "Tidak Ada Scan"), ("incompleteDays", "Belum Lengkap"), ("leaveDays", "Cuti"), ("sickDays", "Sakit"), ("overtimeMinutes", "Lembur (menit)") }
        };

        private static readonly IReadOnlyDictionary<string, string> DailyStatusLabels = new Dictionary<string, string>
        {
            [AttendanceValueConstants.AttendanceStatus.Unprocessed] = "Belum diproses",
            [AttendanceValueConstants.AttendanceStatus.Present] = "Hadir",
            [AttendanceValueConstants.AttendanceStatus.Absent] = "Alpa",
            [AttendanceValueConstants.AttendanceStatus.Late] = "Terlambat",
            [AttendanceValueConstants.AttendanceStatus.EarlyLeave] = "Pulang cepat",
            [AttendanceValueConstants.AttendanceStatus.Incomplete] = "Belum Lengkap",
            [AttendanceValueConstants.AttendanceStatus.IncompletePunch] = "Belum Lengkap",
            [AttendanceValueConstants.AttendanceStatus.OnDuty] = "Bertugas",
            [AttendanceValueConstants.AttendanceStatus.Holiday] = "Hari libur",
            [AttendanceValueConstants.AttendanceStatus.RestDay] = "Libur",
            [AttendanceValueConstants.AttendanceStatus.Leave] = "Cuti",
            [AttendanceValueConstants.AttendanceStatus.BusinessTrip] = "Dinas luar",
            [AttendanceValueConstants.AttendanceStatus.Remote] = "Jarak jauh",
            [AttendanceValueConstants.AttendanceStatus.NotYetPresent] = "Belum Hadir",
            [AttendanceValueConstants.AttendanceStatus.NoScan] = "Tidak Ada Scan",
            [AttendanceValueConstants.AttendanceStatus.StudyAssignment] = "Tugas Belajar"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly HrdUnitScopeService _unitScopeService;

        public AttendanceReportService(ApplicationDbContext dbContext, HrdUnitScopeService unitScopeService)
        {
            _dbContext = dbContext;
            _unitScopeService = unitScopeService;
        }

        /// <summary>Laporan berpaginasi untuk layar. Ekspor memakai BuildAsync dengan paging = false.</summary>
        public Task<AttendanceReportServiceResult> GetReportAsync(
            string reportCode,
            AttendanceReportQueryRequest request,
            DateTime utcNow,
            CancellationToken cancellationToken = default) =>
            BuildAsync(reportCode, request, utcNow, paging: true, cancellationToken);

        public async Task<AttendanceReportServiceResult> BuildAsync(
            string reportCode,
            AttendanceReportQueryRequest request,
            DateTime utcNow,
            bool paging,
            CancellationToken cancellationToken = default)
        {
            var code = (reportCode ?? string.Empty).Trim().ToLowerInvariant();
            if (!ReportNames.ContainsKey(code)) return AttendanceReportServiceResult.Fail(StatusCodes.Status404NotFound, "Laporan tidak dikenal.");

            var today = DateOnly.FromDateTime(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc).Add(WibOffset));
            var from = request.FromDate ?? new DateOnly(today.Year, today.Month, 1);
            var to = request.ToDate ?? new DateOnly(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            if (to < from) return AttendanceReportServiceResult.Fail(StatusCodes.Status400BadRequest, "Tanggal selesai tidak boleh sebelum tanggal mulai.");
            if (to.DayNumber - from.DayNumber + 1 > MaximumRangeDays) return AttendanceReportServiceResult.Fail(StatusCodes.Status400BadRequest, RangeMessage);

            // VR-PK-27: cakupan baca sama untuk layar dan ekspor.
            var scope = await _unitScopeService.GetReadScopeAsync(cancellationToken);
            if (request.OrganizationUnitId.HasValue ? !scope.Contains(request.OrganizationUnitId) : !scope.IsAllUnits && scope.UnitIds.Count == 0)
            {
                return AttendanceReportServiceResult.Fail(StatusCodes.Status403Forbidden, ForbiddenUnitMessage);
            }

            var filter = new Filter(from, to, request.OrganizationUnitId, scope.IsAllUnits ? null : scope.UnitIds.ToList(), string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim());
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var page = paging ? new Page(pageNumber, pageSize) : null;

            var (rows, total) = code switch
            {
                "shift" => await ShiftAsync(filter, page, cancellationToken),
                "attendance" => await DailyAsync(filter, page, latenessOnly: false, cancellationToken),
                "lateness" => await DailyAsync(filter, page, latenessOnly: true, cancellationToken),
                "leave" => await LeaveAsync(filter, page, cancellationToken),
                "overtime" => await OvertimeAsync(filter, page, cancellationToken),
                "absence" => await AbsenceAsync(filter, page, cancellationToken),
                _ => await UnitSummaryAsync(filter, page, cancellationToken)
            };

            return AttendanceReportServiceResult.Ok(new AttendanceReportResponse
            {
                ReportCode = code,
                ReportName = ReportNames[code],
                FromDate = from,
                ToDate = to,
                OrganizationUnitId = request.OrganizationUnitId,
                PeriodState = await PeriodStateAsync(from, to, request.OrganizationUnitId, cancellationToken),
                Columns = ColumnsByReport[code].Select(x => new AttendanceReportColumnResponse { Key = x.Key, Label = x.Label }).ToList(),
                PageNumber = paging ? pageNumber : 1,
                PageSize = paging ? pageSize : rows.Count,
                TotalData = total,
                TotalPage = paging ? (int)Math.Ceiling(total / (double)pageSize) : 1,
                Items = rows
            });
        }

        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Ekspor .xlsx (HRD-DEC-206 butir 1, 3): dibentuk dari BuildAsync yang SAMA dengan layar, tanpa paging, sehingga
        /// cakupan, filter, kolom, dan jumlah baris identik dengan tabel layar. Pencatatan log ekspor dilakukan pemanggil.
        /// </summary>
        public async Task<AttendanceReportExportResult> ExportAsync(
            string reportCode,
            AttendanceReportQueryRequest request,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var built = await BuildAsync(reportCode, request, utcNow, paging: false, cancellationToken);
            if (!built.Success) return AttendanceReportExportResult.Fail(built.StatusCode, built.Message);

            var report = built.Data!;
            var unitLabel = report.OrganizationUnitId.HasValue
                ? await _dbContext.Set<MstOrganizationUnit>().AsNoTracking().Where(x => x.Id == report.OrganizationUnitId).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken) ?? report.OrganizationUnitId.ToString()!
                : "Semua unit dalam cakupan";

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Laporan");
            sheet.Cell(1, 1).Value = report.ReportName;
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(2, 1).Value = $"Unit: {unitLabel}";
            sheet.Cell(3, 1).Value = $"Rentang: {report.FromDate:yyyy-MM-dd} s.d. {report.ToDate:yyyy-MM-dd}" + (string.IsNullOrWhiteSpace(request.Status) ? string.Empty : $" — Status: {request.Status.Trim()}");
            sheet.Cell(4, 1).Value = report.PeriodState == "Closed"
                ? "Periode sudah ditutup — data final"
                : "Periode masih terbuka — data dapat berubah";

            const int headerRow = 6;
            var props = report.Columns
                .Select(c => typeof(AttendanceReportRow).GetProperty(char.ToUpperInvariant(c.Key[0]) + c.Key[1..])!)
                .ToList();
            for (var i = 0; i < report.Columns.Count; i++)
            {
                var cell = sheet.Cell(headerRow, i + 1);
                cell.Value = report.Columns[i].Label;
                cell.Style.Font.Bold = true;
            }

            for (var r = 0; r < report.Items.Count; r++)
            {
                for (var c = 0; c < props.Count; c++)
                {
                    var cell = sheet.Cell(headerRow + 1 + r, c + 1);
                    switch (props[c].GetValue(report.Items[r]))
                    {
                        case null: break;
                        case DateOnly d: cell.Value = d.ToString("yyyy-MM-dd"); break;
                        // Jam disimpan UTC; berkas memakai jam dinding WIB seperti layar rumah sakit.
                        case DateTime t: cell.Value = DateTime.SpecifyKind(t, DateTimeKind.Utc).Add(WibOffset).ToString("yyyy-MM-dd HH:mm"); break;
                        case int n: cell.Value = n; break;
                        case decimal m: cell.Value = m; break;
                        case var v: cell.Value = v.ToString(); break;
                    }
                }
            }

            sheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return AttendanceReportExportResult.Ok(
                stream.ToArray(),
                $"laporan-{report.ReportCode}_{report.FromDate:yyyy-MM-dd}_{report.ToDate:yyyy-MM-dd}.xlsx",
                report.Items.Count,
                report,
                unitLabel);
        }

        private sealed record Filter(DateOnly From, DateOnly To, Guid? UnitId, List<Guid>? ScopeUnitIds, string? Status);
        private sealed record Page(int Number, int Size);

        private IQueryable<T> ApplyUnit<T>(IQueryable<T> query, Filter f, System.Linq.Expressions.Expression<Func<T, Guid?>> unit)
        {
            var param = unit.Parameters[0];
            if (f.UnitId.HasValue)
            {
                var eq = System.Linq.Expressions.Expression.Equal(unit.Body, System.Linq.Expressions.Expression.Constant(f.UnitId, typeof(Guid?)));
                query = query.Where(System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(eq, param));
            }

            if (f.ScopeUnitIds != null)
            {
                var ids = f.ScopeUnitIds.Select(x => (Guid?)x).ToList();
                var contains = System.Linq.Expressions.Expression.Call(
                    typeof(Enumerable), nameof(Enumerable.Contains), new[] { typeof(Guid?) },
                    System.Linq.Expressions.Expression.Constant(ids), unit.Body);
                query = query.Where(System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(contains, param));
            }

            return query;
        }

        private static async Task<(List<AttendanceReportRow>, int)> PageAsync(IQueryable<AttendanceReportRow> query, Page? page, CancellationToken cancellationToken)
        {
            var total = await query.CountAsync(cancellationToken);
            if (page != null) query = query.Skip((page.Number - 1) * page.Size).Take(page.Size);
            return (await query.ToListAsync(cancellationToken), total);
        }

        private static (List<AttendanceReportRow>, int) PageInMemory(List<AttendanceReportRow> rows, Page? page) =>
            (page == null ? rows : rows.Skip((page.Number - 1) * page.Size).Take(page.Size).ToList(), rows.Count);

        private async Task<(List<AttendanceReportRow>, int)> ShiftAsync(Filter f, Page? page, CancellationToken ct)
        {
            var baseQuery =
                from s in _dbContext.Set<TrxShiftAssignment>().AsNoTracking()
                join ra in _dbContext.Set<TrxRosterAssignment>().AsNoTracking() on s.RosterAssignmentId equals ra.Id
                join rp in _dbContext.Set<TrxRosterPeriod>().AsNoTracking() on ra.RosterPeriodId equals rp.Id
                where s.ShiftDate >= f.From && s.ShiftDate <= f.To && s.IsActive && !s.IsDelete && !s.IsCancel &&
                      !rp.IsDelete && PublishedRosterStatuses.Contains(rp.RosterStatus)
                select new { s, rp };
            baseQuery = ApplyUnit(baseQuery, f, x => x.rp.OrganizationUnitId);
            if (f.Status != null) baseQuery = baseQuery.Where(x => x.s.AssignmentStatus == f.Status);

            var query =
                from x in baseQuery
                join p in _dbContext.Set<MstWorkforceProfile>().AsNoTracking() on x.s.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                join u in _dbContext.Set<MstOrganizationUnit>().AsNoTracking() on x.rp.OrganizationUnitId equals u.Id into uj
                from u in uj.DefaultIfEmpty()
                join sh in _dbContext.Set<MstShift>().AsNoTracking() on x.s.ShiftId equals sh.Id into shj
                from sh in shj.DefaultIfEmpty()
                orderby x.s.ShiftDate, u.UnitName, p.DisplayName, x.s.ScheduledStartAt
                select new AttendanceReportRow
                {
                    WorkforceProfileId = x.s.WorkforceProfileId,
                    ProfileCode = p.ProfileCode,
                    EmployeeName = p.DisplayName,
                    OrganizationUnitId = x.rp.OrganizationUnitId,
                    OrganizationUnitName = u.UnitName,
                    Date = x.s.ShiftDate,
                    ShiftName = sh.ShiftName,
                    StartAt = x.s.ScheduledStartAt,
                    EndAt = x.s.ScheduledEndAt,
                    RosterPeriodCode = x.rp.RosterPeriodCode,
                    Status = x.s.AssignmentStatus,
                    StatusLabel = x.s.AssignmentStatus
                };
            return await PageAsync(query, page, ct);
        }

        private IQueryable<HrdAttendanceDaily> Dailies(Filter f) =>
            ApplyUnit(
                _dbContext.Set<HrdAttendanceDaily>().AsNoTracking().Where(x => !x.IsDelete && x.AttendanceDate >= f.From && x.AttendanceDate <= f.To),
                f,
                x => x.OrganizationUnitId);

        private async Task<(List<AttendanceReportRow>, int)> DailyAsync(Filter f, Page? page, bool latenessOnly, CancellationToken ct)
        {
            var dailies = Dailies(f);
            if (latenessOnly) dailies = dailies.Where(x => x.LateMinutes > 0);
            if (f.Status != null) dailies = dailies.Where(x => x.AttendanceStatus == f.Status);

            var query = dailies
                .OrderBy(x => x.AttendanceDate)
                .ThenBy(x => x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : string.Empty)
                .ThenBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                .Select(x => new AttendanceReportRow
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    Date = x.AttendanceDate,
                    ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                    StartAt = x.ScheduledCheckInAt,
                    CheckInAt = x.FirstCheckInAt,
                    CheckOutAt = x.LastCheckOutAt,
                    Status = x.AttendanceStatus,
                    LateMinutes = x.LateMinutes,
                    EarlyLeaveMinutes = x.EarlyLeaveMinutes,
                    WorkMinutes = x.ActualWorkMinutes
                });
            var (rows, total) = await PageAsync(query, page, ct);
            foreach (var row in rows) row.StatusLabel = DailyLabel(row.Status);
            return (rows, total);
        }

        private IQueryable<LeaveRow> Leaves(Filter f)
        {
            var query =
                from l in _dbContext.Set<WfpLeaveRequest>().AsNoTracking()
                where !l.IsDelete && TakenLeaveStatuses.Contains(l.LeaveRequestStatus) && l.StartDate <= f.To && l.EndDate >= f.From
                select new LeaveRow
                {
                    WorkforceProfileId = l.WorkforceProfileId,
                    ProfileCode = l.WorkforceProfile != null ? l.WorkforceProfile.ProfileCode : null,
                    EmployeeName = l.WorkforceProfile != null ? l.WorkforceProfile.DisplayName : null,
                    OrganizationUnitId = l.OrganizationUnitId,
                    OrganizationUnitName = l.OrganizationUnit != null ? l.OrganizationUnit.UnitName : null,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Days = l.RequestedDays,
                    IsSick = l.LeaveType != null && l.LeaveType.LeaveCategory == "Sick",
                    LeaveTypeName = l.LeaveType != null ? l.LeaveType.LeaveTypeName : null,
                    Status = l.LeaveRequestStatus
                };
            return ApplyUnit(query, f, x => x.OrganizationUnitId);
        }

        private sealed class LeaveRow
        {
            public Guid WorkforceProfileId { get; set; }
            public string? ProfileCode { get; set; }
            public string? EmployeeName { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public string? OrganizationUnitName { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public decimal Days { get; set; }
            public bool IsSick { get; set; }
            public string? LeaveTypeName { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        private async Task<(List<AttendanceReportRow>, int)> LeaveAsync(Filter f, Page? page, CancellationToken ct)
        {
            var leaves = Leaves(f);
            if (f.Status != null) leaves = leaves.Where(x => x.Status == f.Status);
            var query = leaves
                .OrderBy(x => x.StartDate).ThenBy(x => x.OrganizationUnitName).ThenBy(x => x.EmployeeName)
                .Select(x => new AttendanceReportRow
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.ProfileCode,
                    EmployeeName = x.EmployeeName,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnitName,
                    Date = x.StartDate,
                    EndDate = x.EndDate,
                    Days = x.Days,
                    // HRD-DEC-206 butir 5: cuti sakit hanya "Sakit", tanpa jenis rinci.
                    Category = x.IsSick ? SickLabel : x.LeaveTypeName,
                    Status = x.Status,
                    StatusLabel = x.Status
                });
            return await PageAsync(query, page, ct);
        }

        private async Task<(List<AttendanceReportRow>, int)> OvertimeAsync(Filter f, Page? page, CancellationToken ct)
        {
            var baseQuery = _dbContext.Set<WfpOvertimeRequest>().AsNoTracking()
                .Where(x => !x.IsDelete && x.OvertimeDate >= f.From && x.OvertimeDate <= f.To && ApprovedOvertimeStatuses.Contains(x.OvertimeRequestStatus));
            baseQuery = ApplyUnit(baseQuery, f, x => x.OrganizationUnitId);
            if (f.Status != null) baseQuery = baseQuery.Where(x => x.OvertimeRequestStatus == f.Status);

            var query =
                from o in baseQuery
                join p in _dbContext.Set<MstWorkforceProfile>().AsNoTracking() on o.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                join u in _dbContext.Set<MstOrganizationUnit>().AsNoTracking() on o.OrganizationUnitId equals u.Id into uj
                from u in uj.DefaultIfEmpty()
                orderby o.OvertimeDate, u.UnitName, p.DisplayName
                select new AttendanceReportRow
                {
                    WorkforceProfileId = o.WorkforceProfileId,
                    ProfileCode = p.ProfileCode,
                    EmployeeName = p.DisplayName,
                    OrganizationUnitId = o.OrganizationUnitId,
                    OrganizationUnitName = u.UnitName,
                    Date = o.OvertimeDate,
                    StartAt = o.PlannedStartAt,
                    EndAt = o.PlannedEndAt,
                    RequestedMinutes = o.RequestedMinutes,
                    ApprovedMinutes = o.ApprovedMinutes,
                    Status = o.OvertimeRequestStatus,
                    StatusLabel = o.OvertimeRequestStatus
                };
            return await PageAsync(query, page, ct);
        }

        /// <summary>Hari tidak hadir: Alpa, Tidak Ada Scan, Belum Lengkap (rekap harian) ditambah hari cuti/sakit (per tanggal).</summary>
        private async Task<List<AttendanceReportRow>> AbsenceRowsAsync(Filter f, CancellationToken ct)
        {
            var daily = await Dailies(f)
                .Where(x => AbsenceDailyStatuses.Contains(x.AttendanceStatus))
                .Select(x => new AttendanceReportRow
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    Date = x.AttendanceDate,
                    Status = x.AttendanceStatus
                })
                .ToListAsync(ct);
            foreach (var row in daily)
            {
                row.Status = row.Status == AttendanceValueConstants.AttendanceStatus.Incomplete ? AttendanceValueConstants.AttendanceStatus.IncompletePunch : row.Status;
                row.Category = DailyLabel(row.Status);
            }

            var rows = daily;
            var leaves = await Leaves(f).ToListAsync(ct);
            var taken = daily.Select(x => (x.WorkforceProfileId, x.Date)).ToHashSet();
            foreach (var l in leaves)
            {
                for (var d = l.StartDate < f.From ? f.From : l.StartDate; d <= (l.EndDate > f.To ? f.To : l.EndDate); d = d.AddDays(1))
                {
                    if (!taken.Add((l.WorkforceProfileId, d))) continue;
                    rows.Add(new AttendanceReportRow
                    {
                        WorkforceProfileId = l.WorkforceProfileId,
                        ProfileCode = l.ProfileCode,
                        EmployeeName = l.EmployeeName,
                        OrganizationUnitId = l.OrganizationUnitId,
                        OrganizationUnitName = l.OrganizationUnitName,
                        Date = d,
                        Status = l.IsSick ? "Sick" : "Leave",
                        Category = l.IsSick ? SickLabel : "Cuti"
                    });
                }
            }

            return rows;
        }

        private async Task<(List<AttendanceReportRow>, int)> AbsenceAsync(Filter f, Page? page, CancellationToken ct)
        {
            var rows = await AbsenceRowsAsync(f, ct);
            if (f.Status != null) rows = rows.Where(x => x.Status == f.Status).ToList();
            rows = rows.OrderBy(x => x.Date).ThenBy(x => x.OrganizationUnitName).ThenBy(x => x.EmployeeName).ToList();
            return PageInMemory(rows, page);
        }

        private async Task<(List<AttendanceReportRow>, int)> UnitSummaryAsync(Filter f, Page? page, CancellationToken ct)
        {
            var daily = await Dailies(f)
                .GroupBy(x => x.OrganizationUnitId)
                .Select(g => new
                {
                    UnitId = g.Key,
                    Employees = g.Select(x => x.WorkforceProfileId).Distinct().Count(),
                    Present = g.Count(x => x.IsPresent),
                    Late = g.Count(x => x.LateMinutes > 0),
                    Absent = g.Count(x => x.AttendanceStatus == AttendanceValueConstants.AttendanceStatus.Absent),
                    NoScan = g.Count(x => x.AttendanceStatus == AttendanceValueConstants.AttendanceStatus.NoScan),
                    Incomplete = g.Count(x => x.AttendanceStatus == AttendanceValueConstants.AttendanceStatus.IncompletePunch || x.AttendanceStatus == AttendanceValueConstants.AttendanceStatus.Incomplete)
                })
                .ToListAsync(ct);

            var leaveDays = new Dictionary<(Guid?, bool), int>();
            foreach (var l in await Leaves(f).ToListAsync(ct))
            {
                var start = l.StartDate < f.From ? f.From : l.StartDate;
                var end = l.EndDate > f.To ? f.To : l.EndDate;
                var key = (l.OrganizationUnitId, l.IsSick);
                leaveDays[key] = leaveDays.GetValueOrDefault(key) + (end.DayNumber - start.DayNumber + 1);
            }

            var overtime = await ApplyUnit(
                    _dbContext.Set<WfpOvertimeRequest>().AsNoTracking().Where(x => !x.IsDelete && x.OvertimeDate >= f.From && x.OvertimeDate <= f.To && ApprovedOvertimeStatuses.Contains(x.OvertimeRequestStatus)),
                    f, x => x.OrganizationUnitId)
                .GroupBy(x => x.OrganizationUnitId)
                .Select(g => new { UnitId = g.Key, Minutes = g.Sum(x => x.ApprovedMinutes) })
                .ToListAsync(ct);

            var unitIds = daily.Select(x => x.UnitId).Concat(leaveDays.Keys.Select(k => k.Item1)).Concat(overtime.Select(x => x.UnitId)).Distinct().ToList();
            var ids = unitIds.Where(x => x.HasValue).Select(x => x!.Value).ToList();
            var names = await _dbContext.Set<MstOrganizationUnit>().AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.UnitName, ct);

            var rows = unitIds.Select(id =>
            {
                var d = daily.FirstOrDefault(x => x.UnitId == id);
                return new AttendanceReportRow
                {
                    OrganizationUnitId = id,
                    OrganizationUnitName = id.HasValue ? names.GetValueOrDefault(id.Value) : "(tanpa unit)",
                    EmployeeCount = d?.Employees ?? 0,
                    PresentDays = d?.Present ?? 0,
                    LateCount = d?.Late ?? 0,
                    AbsentDays = d?.Absent ?? 0,
                    NoScanDays = d?.NoScan ?? 0,
                    IncompleteDays = d?.Incomplete ?? 0,
                    LeaveDays = leaveDays.GetValueOrDefault((id, false)),
                    SickDays = leaveDays.GetValueOrDefault((id, true)),
                    OvertimeMinutes = overtime.FirstOrDefault(x => x.UnitId == id)?.Minutes ?? 0
                };
            }).OrderBy(x => x.OrganizationUnitName).ToList();
            return PageInMemory(rows, page);
        }

        /// <summary>
        /// Closed bila setiap tanggal rentang tercakup periode kehadiran Closed yang berlaku untuk filter unit
        /// (periode tanpa unit berlaku untuk semua unit). Tanpa filter unit hanya periode tanpa unit yang dihitung.
        /// </summary>
        private async Task<string> PeriodStateAsync(DateOnly from, DateOnly to, Guid? unitId, CancellationToken ct)
        {
            var closed = AttendanceValueConstants.AttendancePeriodStatus.Closed;
            var periods = await _dbContext.Set<HrdAttendancePeriod>().AsNoTracking()
                .Where(x => !x.IsDelete && x.PeriodStatus == closed && x.StartDate <= to && x.EndDate >= from &&
                            (x.OrganizationUnitId == null || (unitId.HasValue && x.OrganizationUnitId == unitId)))
                .Select(x => new { x.StartDate, x.EndDate })
                .ToListAsync(ct);
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                if (!periods.Any(p => p.StartDate <= d && p.EndDate >= d)) return "Open";
            }

            return "Closed";
        }

        private static string? DailyLabel(string? status) =>
            status == null ? null : DailyStatusLabels.GetValueOrDefault(status) ?? status;
    }

    public class AttendanceReportExportResult
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public byte[] Content { get; private set; } = Array.Empty<byte>();
        public string FileName { get; private set; } = string.Empty;
        public int RowCount { get; private set; }
        public AttendanceReportResponse? Report { get; private set; }
        public string UnitLabel { get; private set; } = string.Empty;

        public static AttendanceReportExportResult Ok(byte[] content, string fileName, int rowCount, AttendanceReportResponse report, string unitLabel) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Content = content, FileName = fileName, RowCount = rowCount, Report = report, UnitLabel = unitLabel };

        public static AttendanceReportExportResult Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }

    public class AttendanceReportServiceResult
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public AttendanceReportResponse? Data { get; private set; }

        public static AttendanceReportServiceResult Ok(AttendanceReportResponse data) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = $"{data.ReportName} berhasil diambil.", Data = data };

        public static AttendanceReportServiceResult Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
