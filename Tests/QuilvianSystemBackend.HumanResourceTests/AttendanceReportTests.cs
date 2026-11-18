using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-349 — tujuh laporan M12 (HRD-DEC-206, VR-PK-27, VR-PK-28); AT-HRD-PK-39. "Sekarang" = 21 Feb 2027.
// Hierarki: RS → Bidang Keperawatan → {ICU, HCU}; Farmasi di luar bidang.
public sealed class AttendanceReportTests : IDisposable
{
    private static readonly DateTime Sekarang = new(2027, 2, 21, 3, 0, 0, DateTimeKind.Utc);
    private static DateOnly F(int hari) => new(2027, 2, hari);
    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _rs = Guid.NewGuid(), _bidang = Guid.NewGuid(), _icu = Guid.NewGuid(), _hcu = Guid.NewGuid(), _farmasi = Guid.NewGuid();
    private readonly Guid _uSari = Guid.NewGuid(), _uDodi = Guid.NewGuid(), _uAdmin = Guid.NewGuid(), _uBudi = Guid.NewGuid(), _uSuper = Guid.NewGuid();
    private readonly Dictionary<string, Guid> _wf = new();
    private readonly Guid _pagi = Guid.NewGuid();

    public AttendanceReportTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        MstOrganizationUnit Unit(Guid id, string kode, Guid? induk) => new() { Id = id, LegalEntityId = legal, UnitCode = kode, UnitName = kode, ParentOrganizationUnitId = induk };
        ctx.MstOrganizationUnits.AddRange(Unit(_rs, "RS", null), Unit(_bidang, "BIDKEP", _rs), Unit(_icu, "ICU", _bidang), Unit(_hcu, "HCU", _bidang), Unit(_farmasi, "FARMASI", _rs));
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });

        foreach (var nama in new[] { "Sari", "Dodi", "Admin", "Budi", "Rina", "Tono", "Andi", "Fara" })
        {
            var id = Guid.NewGuid();
            _wf[nama] = id;
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        }

        ApplicationUser Akun(Guid id, Guid? profil, UserType tipe) => new() { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipe, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_uSari, _wf["Sari"], UserType.Employee), Akun(_uDodi, _wf["Dodi"], UserType.Employee), Akun(_uAdmin, _wf["Admin"], UserType.Employee),
            Akun(_uBudi, _wf["Budi"], UserType.Employee), Akun(_uSuper, null, UserType.SuperAdmin));

        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WfpOrganizationAssignment Tempat(string nama, Guid unit, bool jabatan) => new()
        { WorkforceProfileId = _wf[nama], OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), IsManagerialAssignment = jabatan, IsPrimary = true, EffectiveStartDate = mulai, IsActive = true };
        ctx.WfpOrganizationAssignments.AddRange(Tempat("Sari", _icu, true), Tempat("Dodi", _bidang, true), Tempat("Budi", _icu, false));

        // Admin HRD: penanda AttendanceReport : ReadAllUnits lewat Akses Role.
        var dept = Guid.NewGuid();
        var pos = Guid.NewGuid();
        var controller = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "AttendanceReport", DisplayName = "Attendance Report" };
        var action = new SysActionAccess { ControllerAccessId = controller.Id, ActionName = "ReadAllUnits", DisplayName = "Read All", AccessType = "Read" };
        ctx.SysControllerAccesses.Add(controller);
        ctx.SysActionAccesses.Add(action);
        ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = _uAdmin, DepartmentId = dept, PositionId = pos, IsActive = true });
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = dept, PositionId = pos, ControllerAccessId = controller.Id, ActionAccessId = action.Id, IsAllowed = true, IsActive = true });

        HrdAttendanceDaily Harian(string nama, Guid unit, DateOnly d, string status, int telat = 0, bool hadir = false) => new()
        {
            UserId = Guid.NewGuid(), WorkforceProfileId = _wf[nama], UserType = UserType.Employee, OrganizationUnitId = unit, AttendanceDate = d, AttendanceStatus = status,
            ShiftId = _pagi, ScheduledCheckInAt = Wib(d, 7), FirstCheckInAt = hadir ? Wib(d, 7, telat) : null, LastCheckOutAt = hadir ? Wib(d, 14) : null,
            LateMinutes = telat, IsPresent = hadir, ActualWorkMinutes = hadir ? 420 - telat : 0, ProcessingStatus = "Processed", IsActive = true,
        };
        ctx.HrdAttendanceDailies.AddRange(
            Harian("Tono", _icu, F(10), S.Late, telat: 15, hadir: true),
            Harian("Tono", _icu, F(11), S.Present, hadir: true),
            Harian("Rina", _icu, F(12), S.NoScan),
            Harian("Rina", _icu, F(13), S.Incomplete),
            Harian("Andi", _hcu, F(10), S.Late, telat: 20, hadir: true),
            Harian("Fara", _farmasi, F(10), S.Absent));

        // AT-HRD-PK-39: Rina sakit 5 Feb dengan surat dokter dan diagnosis.
        var sakit = new MstLeaveType { LeaveTypeCode = "CS", LeaveTypeName = "Cuti Sakit Dengan Surat Dokter", LeaveCategory = "Sick", IsActive = true };
        var tahunan = new MstLeaveType { LeaveTypeCode = "CT", LeaveTypeName = "Cuti Tahunan", LeaveCategory = "Annual", IsActive = true };
        ctx.Set<MstLeaveType>().AddRange(sakit, tahunan);
        ctx.Set<WfpLeaveRequest>().AddRange(
            new WfpLeaveRequest { RequestNumber = "LV-1", WorkforceProfileId = _wf["Rina"], OrganizationUnitId = _icu, LeaveTypeId = sakit.Id, StartDate = F(5), EndDate = F(5), RequestedDays = 1, Reason = "Demam berdarah dengue, diagnosis A90", LeaveRequestStatus = "Approved" },
            new WfpLeaveRequest { RequestNumber = "LV-2", WorkforceProfileId = _wf["Tono"], OrganizationUnitId = _icu, LeaveTypeId = tahunan.Id, StartDate = F(17), EndDate = F(18), RequestedDays = 2, Reason = "Urusan keluarga", LeaveRequestStatus = "Approved" },
            new WfpLeaveRequest { RequestNumber = "LV-3", WorkforceProfileId = _wf["Andi"], OrganizationUnitId = _hcu, LeaveTypeId = tahunan.Id, StartDate = F(3), EndDate = F(3), RequestedDays = 1, Reason = "x", LeaveRequestStatus = "Rejected" });

        ctx.Set<WfpOvertimeRequest>().AddRange(
            new WfpOvertimeRequest { RequestNumber = "OT-1", WorkforceProfileId = _wf["Tono"], OrganizationUnitId = _icu, OvertimeDate = F(10), PlannedStartAt = Wib(F(10), 14), PlannedEndAt = Wib(F(10), 16), RequestedMinutes = 120, ApprovedMinutes = 120, Reason = "Pasien kritis", OvertimeRequestStatus = "Realized" },
            new WfpOvertimeRequest { RequestNumber = "OT-2", WorkforceProfileId = _wf["Tono"], OrganizationUnitId = _icu, OvertimeDate = F(11), RequestedMinutes = 60, ApprovedMinutes = 0, Reason = "x", OvertimeRequestStatus = "Draft" });

        var rosterIcu = Guid.NewGuid();
        var rosterDraft = Guid.NewGuid();
        ctx.TrxRosterPeriods.AddRange(
            new TrxRosterPeriod { Id = rosterIcu, OrganizationUnitId = _icu, RosterPeriodCode = "R-ICU", RosterPeriodName = "R", RosterStatus = "Published", PeriodStartDate = F(1), PeriodEndDate = F(28), IsActive = true },
            new TrxRosterPeriod { Id = rosterDraft, OrganizationUnitId = _icu, RosterPeriodCode = "R-ICU-DRAFT", RosterPeriodName = "R", RosterStatus = "Draft", PeriodStartDate = F(1), PeriodEndDate = F(28), IsActive = true });
        foreach (var (roster, d) in new[] { (rosterIcu, F(10)), (rosterDraft, F(20)) })
        {
            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = roster, WorkforceProfileId = _wf["Tono"], AssignmentStatus = "Published", IsActive = true });
            ctx.TrxShiftAssignments.Add(new TrxShiftAssignment { RosterAssignmentId = ra, WorkforceProfileId = _wf["Tono"], ShiftId = _pagi, ShiftDate = d, ScheduledStartAt = Wib(d, 7), ScheduledEndAt = Wib(d, 14), AssignmentStatus = "Published", IsActive = true });
        }

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private AttendanceReportService Svc(Guid user)
    {
        var ctx = _db.CreateContext();
        return new AttendanceReportService(ctx, new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(user)));
    }

    private async Task<AttendanceReportResponse> Laporan(Guid user, string kode, Guid? unit = null, string? status = null, int ukuran = 100)
    {
        var r = await Svc(user).GetReportAsync(kode, new AttendanceReportQueryRequest { OrganizationUnitId = unit, FromDate = F(1), ToDate = F(28), Status = status, PageSize = ukuran }, Sekarang);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    // AT-HRD-PK-39 + HRD-DEC-206 butir 5: sakit hanya "Sakit"; tidak ada diagnosis, alasan, atau nama jenis cuti sakit.
    [Fact]
    public async Task Ketidakhadiran_dan_cuti_menulis_sakit_saja()
    {
        var absen = await Laporan(_uSari, "absence");
        Assert.Equal(new[] { "Rina|5|Sakit", "Rina|12|Tidak Ada Scan", "Rina|13|Belum Lengkap", "Tono|17|Cuti", "Tono|18|Cuti" },
            absen.Items.Select(x => $"{x.EmployeeName}|{x.Date!.Value.Day}|{x.Category}"));
        Assert.Equal(new[] { "profileCode", "employeeName", "organizationUnitName", "date", "category" }, absen.Columns.Select(c => c.Key));

        var cuti = await Laporan(_uSari, "leave");
        Assert.Equal(new[] { "Sakit", "Cuti Tahunan" }, cuti.Items.Select(x => x.Category));

        foreach (var json in new[] { JsonSerializer.Serialize(absen), JsonSerializer.Serialize(cuti) })
        {
            Assert.DoesNotContain("Demam", json);
            Assert.DoesNotContain("A90", json);
            Assert.DoesNotContain("Surat Dokter", json);
            Assert.DoesNotContain("keluarga", json);
        }
        Assert.DoesNotContain(typeof(AttendanceReportRow).GetProperties(), p => p.Name.Contains("Reason") || p.Name.Contains("Attachment") || p.Name.Contains("Diagnos"));

        // Saring kategori.
        Assert.Single((await Laporan(_uSari, "absence", status: "Sick")).Items);
    }

    // HRD-DEC-206 butir 2 / VR-PK-27: Kepala Unit hanya unitnya; Kepala Bidang unit di bawahnya; Admin HRD (penanda) semua unit.
    [Fact]
    public async Task Cakupan_unit_mengikuti_pengguna()
    {
        string Unit(AttendanceReportResponse r) => string.Join(",", r.Items.Select(x => x.OrganizationUnitName).Distinct().OrderBy(x => x));
        Assert.Equal("ICU", Unit(await Laporan(_uSari, "lateness")));
        Assert.Equal("HCU,ICU", Unit(await Laporan(_uDodi, "lateness")));
        Assert.Equal("FARMASI,HCU,ICU", Unit(await Laporan(_uAdmin, "attendance")));
        Assert.Equal("FARMASI,HCU,ICU", Unit(await Laporan(_uSuper, "attendance")));
        Assert.Equal("HCU", Unit(await Laporan(_uDodi, "lateness", _hcu)));

        foreach (var (user, unit) in new[] { (_uSari, (Guid?)_hcu), (_uSari, (Guid?)_farmasi), (_uDodi, (Guid?)_farmasi), (_uBudi, (Guid?)null), (_uBudi, (Guid?)_icu) })
        {
            var r = await Svc(user).GetReportAsync("lateness", new AttendanceReportQueryRequest { OrganizationUnitId = unit, FromDate = F(1), ToDate = F(28) }, Sekarang);
            Assert.Equal(403, r.StatusCode);
            Assert.Equal("Anda tidak berwenang melihat data unit ini", r.Message);
            Assert.Null(r.Data);
        }
    }

    // Isi tiap laporan.
    [Fact]
    public async Task Isi_tujuh_laporan()
    {
        var shift = await Laporan(_uSari, "shift");
        Assert.Equal("Tono|10|Pagi|R-ICU", Assert.Single(shift.Items, x => true) is var s ? $"{s.EmployeeName}|{s.Date!.Value.Day}|{s.ShiftName}|{s.RosterPeriodCode}" : "");   // roster Draft tidak tampil

        var hadir = await Laporan(_uSari, "attendance");
        Assert.Equal(4, hadir.TotalData);
        Assert.Equal("Terlambat", hadir.Items.First(x => x.Date == F(10)).StatusLabel);
        Assert.Equal(1, (await Laporan(_uSari, "attendance", status: S.Present)).TotalData);

        var telat = await Laporan(_uSari, "lateness");
        var t = Assert.Single(telat.Items);
        Assert.Equal(15, t.LateMinutes);
        Assert.Equal(Wib(F(10), 7, 15), DateTime.SpecifyKind(t.CheckInAt!.Value, DateTimeKind.Utc));

        var lembur = await Laporan(_uSari, "overtime");
        var o = Assert.Single(lembur.Items);
        Assert.Equal(120, o.ApprovedMinutes);
        Assert.DoesNotContain("Pasien kritis", JsonSerializer.Serialize(lembur));

        var rekap = await Laporan(_uDodi, "unit-summary");
        var icu = rekap.Items.Single(x => x.OrganizationUnitName == "ICU");
        Assert.Equal((2, 2, 1, 0, 1, 1, 2, 1, 120), (icu.EmployeeCount!.Value, icu.PresentDays!.Value, icu.LateCount!.Value, icu.AbsentDays!.Value, icu.NoScanDays!.Value, icu.IncompleteDays!.Value, icu.LeaveDays!.Value, icu.SickDays!.Value, icu.OvertimeMinutes!.Value));
        Assert.Equal(new[] { "HCU", "ICU" }, rekap.Items.Select(x => x.OrganizationUnitName));

        // Paginasi: jumlah total tetap, halaman memotong.
        var hal = await Svc(_uAdmin).GetReportAsync("attendance", new AttendanceReportQueryRequest { FromDate = F(1), ToDate = F(28), PageNumber = 2, PageSize = 4 }, Sekarang);
        Assert.Equal(6, hal.Data!.TotalData);
        Assert.Equal(2, hal.Data.Items.Count);
        Assert.Equal(2, hal.Data.TotalPage);
        var semua = await Svc(_uAdmin).BuildAsync("attendance", new AttendanceReportQueryRequest { FromDate = F(1), ToDate = F(28), PageSize = 4 }, Sekarang, paging: false);
        Assert.Equal(6, semua.Data!.Items.Count);
    }

    // ===== BE-HRD-350 — ekspor .xlsx dan log ekspor =====

    private static List<string[]> BacaBerkas(byte[] isi, out string[] kepala)
    {
        using var wb = new ClosedXML.Excel.XLWorkbook(new MemoryStream(isi));
        var ws = wb.Worksheet(1);
        var kolom = ws.Row(6).LastCellUsed()!.Address.ColumnNumber;
        kepala = Enumerable.Range(1, kolom).Select(c => ws.Cell(6, c).GetString()).ToArray();
        var baris = new List<string[]>();
        for (var r = 7; !ws.Row(r).IsEmpty(); r++) baris.Add(Enumerable.Range(1, kolom).Select(c => ws.Cell(r, c).GetString()).ToArray());
        return baris;
    }

    // AT-HRD-PK-37: ekspor Kepala Unit ICU hanya ICU; jumlah baris berkas = jumlah baris layar; kepala kolom = kolom layar.
    [Fact]
    public async Task Ekspor_sama_dengan_layar_untuk_ketujuh_laporan()
    {
        foreach (var kode in AttendanceReportService.ReportNames.Keys)
        {
            foreach (var user in new[] { _uSari, _uDodi, _uAdmin })
            {
                var q = new AttendanceReportQueryRequest { FromDate = F(1), ToDate = F(28), PageSize = 2 };
                var layar = await Svc(user).GetReportAsync(kode, q, Sekarang);
                var ekspor = await Svc(user).ExportAsync(kode, q, Sekarang);
                Assert.True(ekspor.Success, ekspor.Message);
                var baris = BacaBerkas(ekspor.Content, out var kepala);
                Assert.Equal(layar.Data!.TotalData, baris.Count);
                Assert.Equal(layar.Data.TotalData, ekspor.RowCount);
                Assert.Equal(layar.Data.Columns.Select(c => c.Label), kepala);
            }
        }

        var telat = await Svc(_uSari).ExportAsync("lateness", new AttendanceReportQueryRequest { OrganizationUnitId = _icu, FromDate = F(1), ToDate = F(28) }, Sekarang);
        var isi = BacaBerkas(telat.Content, out var kepalaTelat);
        var unitKe = Array.IndexOf(kepalaTelat, "Unit");
        Assert.All(isi, b => Assert.Equal("ICU", b[unitKe]));
        Assert.Equal("laporan-lateness_2027-02-01_2027-02-28.xlsx", telat.FileName);
        Assert.Equal("ICU", telat.UnitLabel);
        using var wb = new ClosedXML.Excel.XLWorkbook(new MemoryStream(telat.Content));
        Assert.Equal("Laporan Keterlambatan", wb.Worksheet(1).Cell(1, 1).GetString());
        Assert.Equal("Periode masih terbuka — data dapat berubah", wb.Worksheet(1).Cell(4, 1).GetString());
        Assert.Equal("2027-02-10 07:15", isi[0][Array.IndexOf(kepalaTelat, "Masuk")]);   // jam dinding WIB
    }

    // AT-HRD-PK-38 dan AT-HRD-PK-39 pada berkas: unit di luar cakupan 403 tanpa berkas; sakit hanya "Sakit".
    [Fact]
    public async Task Ekspor_lintas_unit_ditolak_dan_berkas_tanpa_data_sakit()
    {
        var tolak = await Svc(_uSari).ExportAsync("lateness", new AttendanceReportQueryRequest { OrganizationUnitId = _hcu, FromDate = F(1), ToDate = F(28) }, Sekarang);
        Assert.Equal(403, tolak.StatusCode);
        Assert.Empty(tolak.Content);

        var absen = await Svc(_uSari).ExportAsync("absence", new AttendanceReportQueryRequest { FromDate = F(1), ToDate = F(28) }, Sekarang);
        var sel = BacaBerkas(absen.Content, out var kepala).SelectMany(x => x).Concat(kepala).ToList();
        Assert.Contains("Sakit", sel);
        Assert.DoesNotContain(sel, x => x.Contains("Demam") || x.Contains("A90") || x.Contains("Surat Dokter") || x.Contains("Lampiran") || x.Contains("Diagnos"));
    }

    private sealed class PenangkapLog : Microsoft.Extensions.Logging.ILogger<QuilvianSystemBackend.Services.Logging.LoggerService>
    {
        public List<string> Pesan { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Pesan.Add(formatter(state, exception));
    }

    // HRD-DEC-206 butir 4: satu log per ekspor berhasil berisi pelaku (dari sesi), laporan, filter, jumlah baris; 403 tanpa log.
    [Fact]
    public async Task Satu_log_per_ekspor()
    {
        var penangkap = new PenangkapLog();
        QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers.AttendanceReportController Controller()
        {
            var akses = Pelaku.Sebagai(_uSari);
            var c = new QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers.AttendanceReportController(
                Svc(_uSari), new QuilvianSystemBackend.Services.Logging.LoggerService(penangkap, akses));
            c.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = akses.HttpContext! };
            return c;
        }

        var ok = await Controller().Export("lateness", new AttendanceReportQueryRequest { OrganizationUnitId = _icu, FromDate = F(1), ToDate = F(28) });
        var berkas = Assert.IsType<Microsoft.AspNetCore.Mvc.FileContentResult>(ok);
        Assert.Equal(AttendanceReportService.XlsxContentType, berkas.ContentType);
        var log = Assert.Single(penangkap.Pesan);
        Assert.Contains("Ekspor Laporan Keterlambatan (lateness)", log);
        Assert.Contains("unit: ICU", log);
        Assert.Contains("rentang 2027-02-01 s.d. 2027-02-28", log);
        Assert.Contains("1 baris", log);
        Assert.Contains(_uSari.ToString(), log);   // pelaku dari sesi

        var tolak = await Controller().Export("lateness", new AttendanceReportQueryRequest { OrganizationUnitId = _hcu, FromDate = F(1), ToDate = F(28) });
        Assert.Equal(403, Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(tolak).StatusCode);
        Assert.Single(penangkap.Pesan);
    }

    // VR-PK-28, kode tak dikenal, PeriodState.
    [Fact]
    public async Task Rentang_kode_dan_status_periode()
    {
        var panjang = await Svc(_uSari).GetReportAsync("attendance", new AttendanceReportQueryRequest { FromDate = new DateOnly(2027, 1, 1), ToDate = new DateOnly(2027, 4, 2) }, Sekarang);   // 92 hari
        Assert.True(panjang.Success, panjang.Message);
        var lebih = await Svc(_uSari).GetReportAsync("attendance", new AttendanceReportQueryRequest { FromDate = new DateOnly(2027, 1, 1), ToDate = new DateOnly(2027, 4, 3) }, Sekarang);   // 93 hari
        Assert.Equal(400, lebih.StatusCode);
        Assert.Equal("Rentang laporan paling lama 92 hari", lebih.Message);
        Assert.Equal(400, (await Svc(_uSari).GetReportAsync("attendance", new AttendanceReportQueryRequest { FromDate = F(10), ToDate = F(9) }, Sekarang)).StatusCode);
        Assert.Equal(404, (await Svc(_uSari).GetReportAsync("payroll", new AttendanceReportQueryRequest(), Sekarang)).StatusCode);

        var bawaan = await Svc(_uSari).GetReportAsync("attendance", new AttendanceReportQueryRequest(), Sekarang);
        Assert.Equal((F(1), F(28)), (bawaan.Data!.FromDate, bawaan.Data.ToDate));
        Assert.Equal("Open", bawaan.Data.PeriodState);

        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod { PeriodCode = "ATP-2027-02", PeriodName = "Feb", StartDate = F(1), EndDate = F(28), PeriodStatus = "Closed" });
            ctx.SaveChanges();
        }
        Assert.Equal("Closed", (await Laporan(_uSari, "attendance")).PeriodState);
        var lintas = await Svc(_uSari).GetReportAsync("attendance", new AttendanceReportQueryRequest { FromDate = F(20), ToDate = new DateOnly(2027, 3, 5) }, Sekarang);
        Assert.Equal("Open", lintas.Data!.PeriodState);
    }
}
