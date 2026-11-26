using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Dashboard Kehadiran (HRD-DEC-208, FR-PK-030/031): lima kartu untuk satu tanggal dan cakupan baca pengguna,
    /// dihitung saat diminta. Angka kartu dan daftar rinciannya berasal dari perhitungan yang SAMA (ComputeAsync),
    /// sehingga setiap angka sama dengan jumlah baris rinciannya untuk tanggal, unit, dan jam hitung yang sama.
    /// </summary>
    public class AttendanceDashboardService
    {
        public const string ForbiddenUnitMessage = AttendanceReportService.ForbiddenUnitMessage;
        private const string SickLabel = "Sakit";
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);

        private static readonly string[] PublishedRosterStatuses = { "Published", "Closed" };
        private static readonly string[] InactiveRosterStatuses = { "Published", "Closed", "Cancelled" };
        private static readonly string[] InactiveShiftStatuses = { "Cancelled" };
        private static readonly string[] InactiveOnCallStatuses = { "Cancelled", "Rejected", "Completed" };
        private static readonly string[] TakenLeaveStatuses =
        {
            LeaveRequestValueConstants.Status.Approved,
            LeaveRequestValueConstants.Status.Taken,
            LeaveRequestValueConstants.Status.Completed
        };

        /// <summary>Kartu beserta angka-angkanya, berurutan; angka pertama adalah angka utama kartu.</summary>
        private static readonly (string Code, string Name, (string Metric, string Label)[] Metrics)[] CardDefinitions =
        {
            ("attendance-today", "Kehadiran Hari Ini", new[]
            {
                ("scheduled", "Terjadwal"), ("present", "Hadir"), ("late", "Terlambat"), ("notYetPresent", "Belum Hadir"),
                ("noScan", "Tidak Ada Scan"), ("leave", "Cuti"), ("sick", SickLabel), ("permission", "Izin"),
                ("businessTrip", "Dinas Luar"), ("studyAssignment", "Tugas Belajar")
            }),
            ("on-duty", "Pegawai Bertugas", new[] { ("total", "Bertugas"), ("shift", "Shift Berjalan"), ("onCall", "Siaga Utama") }),
            ("lateness", "Keterlambatan", new[] { ("late", "Terlambat") }),
            ("absence", "Ketidakhadiran", new[]
            {
                ("total", "Tidak Hadir"), ("noScan", "Tidak Ada Scan"), ("incomplete", "Belum Lengkap"), ("leave", "Cuti"),
                ("sick", SickLabel), ("permission", "Izin"), ("businessTrip", "Dinas Luar"), ("studyAssignment", "Tugas Belajar"),
                ("absent", "Alpa")
            }),
            ("incomplete-schedule", "Jadwal Belum Lengkap", new[]
            {
                ("total", "Belum Lengkap"), ("unpublishedRoster", "Roster Belum Terbit"),
                ("quotaWarning", "Peringatan Kuota (W6)"), ("qualificationWarning", "Peringatan Kualifikasi (W7)")
            })
        };

        private static readonly IReadOnlyDictionary<string, string> CategoryLabels = new Dictionary<string, string>
        {
            ["present"] = "Hadir",
            ["late"] = "Terlambat",
            ["notYetPresent"] = "Belum Hadir",
            ["noScan"] = "Tidak Ada Scan",
            ["incomplete"] = "Belum Lengkap",
            ["absent"] = "Alpa",
            ["leave"] = "Cuti",
            ["sick"] = SickLabel,
            ["permission"] = "Izin",
            ["businessTrip"] = "Dinas Luar",
            ["studyAssignment"] = "Tugas Belajar",
            ["unprocessed"] = "Belum diproses"
        };

        private static readonly string[] AbsenceCategories = { "noScan", "incomplete", "leave", "sick", "permission", "businessTrip", "studyAssignment", "absent" };

        private readonly ApplicationDbContext _dbContext;
        private readonly HrdUnitScopeService _unitScopeService;

        public AttendanceDashboardService(ApplicationDbContext dbContext, HrdUnitScopeService unitScopeService)
        {
            _dbContext = dbContext;
            _unitScopeService = unitScopeService;
        }

        public async Task<AttendanceDashboardServiceResult<AttendanceDashboardResponse>> GetDashboardAsync(
            AttendanceDashboardQueryRequest request,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var computed = await ComputeAsync(request, utcNow, cancellationToken);
            if (computed.Error != null) return AttendanceDashboardServiceResult<AttendanceDashboardResponse>.Fail(computed.Error.Value.Status, computed.Error.Value.Message);

            var data = computed.Data!;
            var response = new AttendanceDashboardResponse
            {
                Date = data.Date,
                OrganizationUnitId = request.OrganizationUnitId,
                CalculatedAt = data.CalculatedAt,
                OnDutyReferenceAt = data.OnDutyReferenceAt,
                Cards = CardDefinitions.Select(card =>
                {
                    var metrics = card.Metrics
                        .Select(m => new AttendanceDashboardMetricResponse { Metric = m.Metric, Label = m.Label, Count = data.Rows[(card.Code, m.Metric)].Count })
                        .ToList();
                    return new AttendanceDashboardCardResponse { CardCode = card.Code, CardName = card.Name, Count = metrics[0].Count, Metrics = metrics };
                }).ToList()
            };

            return AttendanceDashboardServiceResult<AttendanceDashboardResponse>.Ok(response, "Dashboard kehadiran berhasil dihitung.");
        }

        public async Task<AttendanceDashboardServiceResult<AttendanceDashboardDetailResponse>> GetCardDetailAsync(
            string cardCode,
            AttendanceDashboardDetailQueryRequest request,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var code = (cardCode ?? string.Empty).Trim().ToLowerInvariant();
            var card = CardDefinitions.FirstOrDefault(x => x.Code == code);
            if (card.Code == null) return AttendanceDashboardServiceResult<AttendanceDashboardDetailResponse>.Fail(StatusCodes.Status404NotFound, "Kartu dashboard tidak dikenal.");

            var metric = string.IsNullOrWhiteSpace(request.Metric)
                ? card.Metrics[0]
                : card.Metrics.FirstOrDefault(x => string.Equals(x.Metric, request.Metric.Trim(), StringComparison.OrdinalIgnoreCase));
            if (metric.Metric == null)
            {
                return AttendanceDashboardServiceResult<AttendanceDashboardDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Rincian '{request.Metric}' tidak dikenal untuk kartu {card.Name}. Pilihan: {string.Join(", ", card.Metrics.Select(x => x.Metric))}.");
            }

            var computed = await ComputeAsync(request, utcNow, cancellationToken);
            if (computed.Error != null) return AttendanceDashboardServiceResult<AttendanceDashboardDetailResponse>.Fail(computed.Error.Value.Status, computed.Error.Value.Message);

            var data = computed.Data!;
            var rows = data.Rows[(card.Code, metric.Metric)];
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var response = new AttendanceDashboardDetailResponse
            {
                CardCode = card.Code,
                CardName = card.Name,
                Metric = metric.Metric,
                MetricLabel = metric.Label,
                Date = data.Date,
                OrganizationUnitId = request.OrganizationUnitId,
                CalculatedAt = data.CalculatedAt,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = rows.Count,
                TotalPage = (int)Math.Ceiling(rows.Count / (double)pageSize),
                Items = rows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };

            return AttendanceDashboardServiceResult<AttendanceDashboardDetailResponse>.Ok(response, $"Rincian {card.Name} — {metric.Label} berhasil diambil.");
        }

        private sealed record Computed(DateOnly Date, DateTime CalculatedAt, DateTime OnDutyReferenceAt, Dictionary<(string Card, string Metric), List<DashboardDetailRow>> Rows);

        private sealed record ComputeOutcome(Computed? Data, (int Status, string Message)? Error);

        private sealed class DailyRow
        {
            public Guid Key { get; set; }
            public Guid? WorkforceProfileId { get; set; }
            public string? ProfileCode { get; set; }
            public string? EmployeeName { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public string? OrganizationUnitName { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool IsPresent { get; set; }
            public int LateMinutes { get; set; }
            public string? ShiftName { get; set; }
            public DateTime? ScheduledCheckInAt { get; set; }
            public DateTime? ScheduledCheckOutAt { get; set; }
            public DateTime? FirstCheckInAt { get; set; }
            public DateTime? LastCheckOutAt { get; set; }
            public bool HasEarlyLeavePermission { get; set; }
        }

        private sealed class LeaveRow
        {
            public Guid WorkforceProfileId { get; set; }
            public string? ProfileCode { get; set; }
            public string? EmployeeName { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public string? OrganizationUnitName { get; set; }
            public bool IsSick { get; set; }
        }

        /// <summary>
        /// Satu-satunya perhitungan dashboard. Kartu memakai Count, rincian memakai baris yang sama.
        /// </summary>
        private async Task<ComputeOutcome> ComputeAsync(AttendanceDashboardQueryRequest request, DateTime utcNow, CancellationToken ct)
        {
            var nowUtc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            var nowWib = nowUtc.Add(WibOffset);
            var today = DateOnly.FromDateTime(nowWib);
            var date = request.Date ?? today;

            // VR-PK-27: cakupan baca sama dengan laporan (Kepala Unit = unitnya, Kepala Bidang = unit di bawahnya, ReadAllUnits = semua).
            var scope = await _unitScopeService.GetReadScopeAsync(ct);
            if (request.OrganizationUnitId.HasValue ? !scope.Contains(request.OrganizationUnitId) : !scope.IsAllUnits && scope.UnitIds.Count == 0)
            {
                return new ComputeOutcome(null, (StatusCodes.Status403Forbidden, ForbiddenUnitMessage));
            }

            var unitIds = request.OrganizationUnitId.HasValue
                ? new List<Guid> { request.OrganizationUnitId.Value }
                : scope.IsAllUnits ? null : scope.UnitIds.ToList();

            // Pegawai Bertugas: jam hitung untuk hari ini; untuk tanggal lain, jam dinding yang sama pada tanggal itu.
            var referenceAt = date == today
                ? nowUtc
                : DateTime.SpecifyKind(date.ToDateTime(TimeOnly.FromDateTime(nowWib)), DateTimeKind.Utc).Subtract(WibOffset);

            var rows = new Dictionary<(string, string), List<DashboardDetailRow>>();
            foreach (var card in CardDefinitions)
            {
                foreach (var m in card.Metrics) rows[(card.Code, m.Metric)] = new List<DashboardDetailRow>();
            }

            await AttendanceAndAbsenceAsync(date, unitIds, rows, ct);
            await OnDutyAsync(referenceAt, unitIds, rows, ct);
            await IncompleteScheduleAsync(date, unitIds, rows, ct);

            foreach (var key in rows.Keys.ToList())
            {
                rows[key] = key.Item1 == "lateness"
                    ? rows[key].OrderByDescending(x => x.LateMinutes).ThenBy(x => x.OrganizationUnitName).ThenBy(x => x.EmployeeName).ToList()
                    : rows[key].OrderBy(x => x.OrganizationUnitName).ThenBy(x => x.Date).ThenBy(x => x.EmployeeName).ThenBy(x => x.StartAt).ToList();
            }

            return new ComputeOutcome(new Computed(date, nowUtc, referenceAt, rows), null);
        }

        /// <summary>Kartu Kehadiran Hari Ini, Keterlambatan, dan Ketidakhadiran dari rekap harian dan cuti pada tanggal itu.</summary>
        private async Task AttendanceAndAbsenceAsync(DateOnly date, List<Guid>? unitIds, Dictionary<(string, string), List<DashboardDetailRow>> rows, CancellationToken ct)
        {
            var dailies = await InUnits(
                    _dbContext.Set<HrdAttendanceDaily>().AsNoTracking().Where(x => !x.IsDelete && x.AttendanceDate == date),
                    unitIds, x => x.OrganizationUnitId)
                .Select(x => new DailyRow
                {
                    Key = x.WorkforceProfileId ?? x.UserId,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    Status = x.AttendanceStatus,
                    IsPresent = x.IsPresent,
                    LateMinutes = x.LateMinutes,
                    ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                    ScheduledCheckInAt = x.ScheduledCheckInAt,
                    ScheduledCheckOutAt = x.ScheduledCheckOutAt,
                    FirstCheckInAt = x.FirstCheckInAt,
                    LastCheckOutAt = x.LastCheckOutAt,
                    // Rekap sudah tertaut ke izin pulang cepat sejak diajukan; hanya izin Approved yang dihitung "Izin".
                    HasEarlyLeavePermission = x.EarlyLeavePermissionId != null &&
                        _dbContext.Set<HrdEarlyLeavePermission>().Any(e => e.Id == x.EarlyLeavePermissionId && !e.IsDelete && e.Status == "Approved")
                })
                .ToListAsync(ct);

            var leaves = await InUnits(
                    _dbContext.Set<WfpLeaveRequest>().AsNoTracking()
                        .Where(l => !l.IsDelete && TakenLeaveStatuses.Contains(l.LeaveRequestStatus) && l.StartDate <= date && l.EndDate >= date),
                    unitIds, l => l.OrganizationUnitId)
                .Select(l => new LeaveRow
                {
                    WorkforceProfileId = l.WorkforceProfileId,
                    ProfileCode = l.WorkforceProfile != null ? l.WorkforceProfile.ProfileCode : null,
                    EmployeeName = l.WorkforceProfile != null ? l.WorkforceProfile.DisplayName : null,
                    OrganizationUnitId = l.OrganizationUnitId,
                    OrganizationUnitName = l.OrganizationUnit != null ? l.OrganizationUnit.UnitName : null,
                    IsSick = l.LeaveType != null && l.LeaveType.LeaveCategory == "Sick"
                })
                .ToListAsync(ct);

            // HRD-DEC-208 butir 4: Alpa hanya dari periode yang sudah ditutup. Periode tanpa unit berlaku untuk semua unit.
            var closed = AttendanceValueConstants.AttendancePeriodStatus.Closed;
            var closedPeriods = await _dbContext.Set<HrdAttendancePeriod>().AsNoTracking()
                .Where(x => !x.IsDelete && x.PeriodStatus == closed && x.StartDate <= date && x.EndDate >= date)
                .Select(x => x.OrganizationUnitId)
                .ToListAsync(ct);
            bool IsClosed(Guid? unitId) => closedPeriods.Any(p => p == null || (unitId.HasValue && p == unitId));

            var dailyByKey = dailies.GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.First());
            var leaveByProfile = leaves
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.IsSick).First());

            foreach (var key in dailyByKey.Keys.Union(leaveByProfile.Keys))
            {
                dailyByKey.TryGetValue(key, out var daily);
                leaveByProfile.TryGetValue(key, out var leave);
                var category = Categorize(daily, leave, IsClosed);
                if (category == null) continue;

                var row = new DashboardDetailRow
                {
                    WorkforceProfileId = daily?.WorkforceProfileId ?? leave!.WorkforceProfileId,
                    ProfileCode = daily?.ProfileCode ?? leave?.ProfileCode,
                    EmployeeName = daily?.EmployeeName ?? leave?.EmployeeName,
                    OrganizationUnitId = daily?.OrganizationUnitId ?? leave?.OrganizationUnitId,
                    OrganizationUnitName = daily?.OrganizationUnitName ?? leave?.OrganizationUnitName,
                    Date = date,
                    Category = CategoryLabels[category],
                    ShiftName = daily?.ShiftName,
                    StartAt = daily?.ScheduledCheckInAt,
                    EndAt = daily?.ScheduledCheckOutAt,
                    CheckInAt = daily?.FirstCheckInAt,
                    CheckOutAt = daily?.LastCheckOutAt,
                    LateMinutes = daily != null && daily.LateMinutes > 0 ? daily.LateMinutes : null,
                    // Status mentah hanya dari rekap harian; untuk cuti tanpa rekap cukup kategorinya.
                    Status = daily?.Status
                };

                rows[("attendance-today", "scheduled")].Add(row);
                if (rows.TryGetValue(("attendance-today", category), out var todayList)) todayList.Add(row);

                if (AbsenceCategories.Contains(category))
                {
                    rows[("absence", "total")].Add(row);
                    rows[("absence", category)].Add(row);
                }

                // Keterlambatan: menit terlambat sudah sesudah toleransi (dan dispensasi bila ada) pada rekap harian.
                if (daily != null && daily.LateMinutes > 0 && category != "leave" && category != "sick")
                {
                    rows[("lateness", "late")].Add(row);
                }
            }
        }

        /// <summary>
        /// Satu kategori per pegawai per tanggal. Urutan: tugas belajar, cuti/sakit (bila tidak hadir), dinas luar,
        /// izin, lalu status harian. Hari libur/off tidak dihitung terjadwal.
        /// </summary>
        private static string? Categorize(DailyRow? daily, LeaveRow? leave, Func<Guid?, bool> isClosed)
        {
            if (daily?.Status == S.StudyAssignment) return "studyAssignment";
            if (leave != null && (daily == null || !daily.IsPresent)) return leave.IsSick ? "sick" : "leave";
            if (daily == null) return null;

            switch (daily.Status)
            {
                case S.RestDay:
                case S.Holiday:
                    return null;
                case S.BusinessTrip:
                case S.OnDuty:
                    return "businessTrip";
                case S.Late:
                    return "late";
                case S.NotYetPresent:
                    return "notYetPresent";
                case S.NoScan:
                    return "noScan";
                case S.Incomplete:
                case S.IncompletePunch:
                    return "incomplete";
                case S.Absent:
                    // Alpa dari data lama sebelum periode ditutup ditampilkan sebagai Tidak Ada Scan (HRD-DEC-207).
                    return isClosed(daily.OrganizationUnitId) ? "absent" : "noScan";
                case S.Present:
                case S.EarlyLeave:
                case S.Remote:
                    return daily.HasEarlyLeavePermission ? "permission" : "present";
                default:
                    return "unprocessed";
            }
        }

        /// <summary>Kartu Pegawai Bertugas: shift terbit yang sedang berjalan (termasuk petugas pengganti) dan siaga utama aktif.</summary>
        private async Task OnDutyAsync(DateTime at, List<Guid>? unitIds, Dictionary<(string, string), List<DashboardDetailRow>> rows, CancellationToken ct)
        {
            // Ganti Petugas menulis pengganti langsung pada shift (OriginalEmployeeId = petugas semula), jadi pengganti ikut terbaca.
            var shiftQuery =
                from s in _dbContext.Set<TrxShiftAssignment>().AsNoTracking()
                join ra in _dbContext.Set<TrxRosterAssignment>().AsNoTracking() on s.RosterAssignmentId equals ra.Id
                join rp in _dbContext.Set<TrxRosterPeriod>().AsNoTracking() on ra.RosterPeriodId equals rp.Id
                where s.IsActive && !s.IsDelete && !s.IsCancel && !s.IsDayOff && !s.IsOnCall &&
                      !InactiveShiftStatuses.Contains(s.AssignmentStatus) &&
                      !rp.IsDelete && PublishedRosterStatuses.Contains(rp.RosterStatus) &&
                      s.ScheduledStartAt <= at && s.ScheduledEndAt > at
                select new { s, UnitId = rp.OrganizationUnitId };
            shiftQuery = InUnits(shiftQuery, unitIds, x => x.UnitId);

            var shifts = await (
                    from x in shiftQuery
                    join p in _dbContext.Set<MstWorkforceProfile>().AsNoTracking() on x.s.WorkforceProfileId equals p.Id into pj
                    from p in pj.DefaultIfEmpty()
                    join u in _dbContext.Set<MstOrganizationUnit>().AsNoTracking() on x.UnitId equals u.Id into uj
                    from u in uj.DefaultIfEmpty()
                    join sh in _dbContext.Set<MstShift>().AsNoTracking() on x.s.ShiftId equals sh.Id into shj
                    from sh in shj.DefaultIfEmpty()
                    select new DashboardDetailRow
                    {
                        WorkforceProfileId = x.s.WorkforceProfileId,
                        ProfileCode = p.ProfileCode,
                        EmployeeName = p.DisplayName,
                        OrganizationUnitId = x.UnitId,
                        OrganizationUnitName = u.UnitName,
                        Date = x.s.ShiftDate,
                        Category = x.s.OriginalEmployeeId != null && x.s.OriginalEmployeeId != x.s.WorkforceProfileId ? "Petugas Pengganti" : "Shift",
                        ShiftName = sh.ShiftName,
                        StartAt = x.s.ScheduledStartAt,
                        EndAt = x.s.ScheduledEndAt,
                        Status = x.s.AssignmentStatus
                    })
                .ToListAsync(ct);

            var onCallQuery =
                from o in _dbContext.Set<TrxOnCallAssignment>().AsNoTracking()
                join rp in _dbContext.Set<TrxRosterPeriod>().AsNoTracking() on o.RosterPeriodId equals rp.Id
                where o.IsActive && !o.IsDelete && !o.IsCancel && o.OnCallRole == "Primary" &&
                      !InactiveOnCallStatuses.Contains(o.AssignmentStatus) &&
                      !rp.IsDelete && PublishedRosterStatuses.Contains(rp.RosterStatus) &&
                      o.StartAt <= at && o.EndAt > at
                select new { o, UnitId = o.UnitId ?? rp.OrganizationUnitId };
            onCallQuery = InUnits(onCallQuery, unitIds, x => x.UnitId);

            var onCalls = await (
                    from x in onCallQuery
                    join p in _dbContext.Set<MstWorkforceProfile>().AsNoTracking() on x.o.WorkforceProfileId equals p.Id into pj
                    from p in pj.DefaultIfEmpty()
                    join u in _dbContext.Set<MstOrganizationUnit>().AsNoTracking() on x.UnitId equals u.Id into uj
                    from u in uj.DefaultIfEmpty()
                    join t in _dbContext.Set<MstOnCallType>().AsNoTracking() on x.o.OnCallTypeId equals t.Id into tj
                    from t in tj.DefaultIfEmpty()
                    select new DashboardDetailRow
                    {
                        WorkforceProfileId = x.o.WorkforceProfileId,
                        ProfileCode = p.ProfileCode,
                        EmployeeName = p.DisplayName,
                        OrganizationUnitId = x.UnitId,
                        OrganizationUnitName = u.UnitName,
                        Category = "Siaga Utama",
                        ShiftName = t.OnCallTypeName,
                        StartAt = x.o.StartAt,
                        EndAt = x.o.EndAt,
                        Status = x.o.AssignmentStatus
                    })
                .ToListAsync(ct);
            foreach (var row in onCalls) row.Date = DateOnly.FromDateTime(row.StartAt!.Value.Add(WibOffset));

            rows[("on-duty", "shift")].AddRange(shifts);
            rows[("on-duty", "onCall")].AddRange(onCalls);
            rows[("on-duty", "total")].AddRange(shifts.Concat(onCalls));
        }

        /// <summary>
        /// Kartu Jadwal Belum Lengkap: roster periode berjalan atau berikutnya (berakhir pada/sesudah tanggal, mulai paling
        /// lambat akhir bulan berikutnya) yang belum terbit, serta peringatan W6/W7 pada roster terbit untuk shift mulai tanggal itu.
        /// </summary>
        private async Task IncompleteScheduleAsync(DateOnly date, List<Guid>? unitIds, Dictionary<(string, string), List<DashboardDetailRow>> rows, CancellationToken ct)
        {
            var windowEnd = new DateOnly(date.Year, date.Month, 1).AddMonths(2).AddDays(-1);
            var periods = await InUnits(
                    _dbContext.Set<TrxRosterPeriod>().AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.PeriodEndDate >= date && x.PeriodStartDate <= windowEnd),
                    unitIds, x => x.OrganizationUnitId)
                .Select(x => new
                {
                    x.Id,
                    x.OrganizationUnitId,
                    UnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    x.RosterPeriodCode,
                    x.PeriodStartDate,
                    x.PeriodEndDate,
                    x.RosterStatus,
                    x.ValidationSummaryJson
                })
                .ToListAsync(ct);

            foreach (var p in periods.Where(x => !InactiveRosterStatuses.Contains(x.RosterStatus)))
            {
                var row = new DashboardDetailRow
                {
                    OrganizationUnitId = p.OrganizationUnitId,
                    OrganizationUnitName = p.UnitName,
                    Date = p.PeriodStartDate,
                    StartAt = p.PeriodStartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    EndAt = p.PeriodEndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    Category = "Roster Belum Terbit",
                    RosterPeriodCode = p.RosterPeriodCode,
                    Status = p.RosterStatus
                };
                rows[("incomplete-schedule", "unpublishedRoster")].Add(row);
                rows[("incomplete-schedule", "total")].Add(row);
            }

            var warnings = new List<(DashboardDetailRow Row, Guid AnchorShiftId)>();
            foreach (var p in periods.Where(x => PublishedRosterStatuses.Contains(x.RosterStatus) && !string.IsNullOrWhiteSpace(x.ValidationSummaryJson)))
            {
                RosterValidationResultResponse? summary;
                try
                {
                    summary = JsonSerializer.Deserialize<RosterValidationResultResponse>(p.ValidationSummaryJson!);
                }
                catch (JsonException)
                {
                    // Ringkasan pemeriksaan yang tidak terbaca tidak boleh menggagalkan dashboard; roster itu dilewati.
                    continue;
                }

                foreach (var item in summary?.ConflictItems ?? new List<RosterValidationConflictItemDto>())
                {
                    if ((item.RuleCode != "W6" && item.RuleCode != "W7") || item.Severity != "WARNING" || item.ShiftDate < date || item.ShiftDate > windowEnd) continue;
                    warnings.Add((new DashboardDetailRow
                    {
                        OrganizationUnitId = p.OrganizationUnitId,
                        OrganizationUnitName = p.UnitName,
                        Date = item.ShiftDate,
                        Category = item.RuleCode == "W6" ? "Peringatan Kuota" : "Peringatan Kualifikasi",
                        RosterPeriodCode = p.RosterPeriodCode,
                        RuleCode = item.RuleCode,
                        Message = item.Message,
                        Status = p.RosterStatus
                    }, item.ShiftAssignmentId));
                }
            }

            if (warnings.Count > 0)
            {
                var anchorIds = warnings.Select(x => x.AnchorShiftId).Where(x => x != Guid.Empty).Distinct().ToList();
                var anchors = await _dbContext.Set<TrxShiftAssignment>().AsNoTracking()
                    .Where(x => anchorIds.Contains(x.Id))
                    .Select(x => new { x.Id, ShiftName = x.Shift != null ? x.Shift.ShiftName : null, x.ScheduledStartAt, x.ScheduledEndAt })
                    .ToDictionaryAsync(x => x.Id, ct);
                foreach (var (row, anchorId) in warnings)
                {
                    if (anchors.TryGetValue(anchorId, out var anchor))
                    {
                        row.ShiftName = anchor.ShiftName;
                        row.StartAt = anchor.ScheduledStartAt;
                        row.EndAt = anchor.ScheduledEndAt;
                    }

                    rows[("incomplete-schedule", row.RuleCode == "W6" ? "quotaWarning" : "qualificationWarning")].Add(row);
                    rows[("incomplete-schedule", "total")].Add(row);
                }
            }
        }

        /// <summary>Saring unit di basis data. unitIds null = semua unit; data tanpa unit hanya terlihat oleh cakupan semua unit.</summary>
        private static IQueryable<T> InUnits<T>(IQueryable<T> query, List<Guid>? unitIds, Expression<Func<T, Guid?>> unit)
        {
            if (unitIds == null) return query;

            var ids = unitIds.Select(x => (Guid?)x).ToList();
            var contains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { typeof(Guid?) }, Expression.Constant(ids), unit.Body);
            return query.Where(Expression.Lambda<Func<T, bool>>(contains, unit.Parameters[0]));
        }
    }

    public class AttendanceDashboardServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static AttendanceDashboardServiceResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static AttendanceDashboardServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
