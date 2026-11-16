using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-348 — Jadwal Saya: AT-HRD-PK-42, -43, -44 (HRD-DEC-209, HRD-ASM-80, VR-PK-29). "Sekarang" = 2 Feb 2027 10.00 WIB.
public sealed class MyScheduleTests : IDisposable
{
    private static readonly DateTime Sekarang = new(2027, 2, 2, 3, 0, 0, DateTimeKind.Utc);
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _igd = Guid.NewGuid(), _pagi = Guid.NewGuid(), _siang = Guid.NewGuid(), _malam = Guid.NewGuid();
    private readonly Guid _rosterFeb = Guid.NewGuid(), _rosterMarDraft = Guid.NewGuid(), _rosterIgd = Guid.NewGuid(), _rosterFebDraft = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _p = new();

    private static DateTime Wib(DateOnly d, int jam) => new DateTime(d.Year, d.Month, d.Day, jam, 0, 0, DateTimeKind.Utc).AddHours(-7);
    private static DateOnly F(int hari) => new(2027, 2, hari);

    public MyScheduleTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _igd, LegalEntityId = Guid.NewGuid(), UnitCode = "IGD", UnitName = "IGD" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.MstShifts.Add(new MstShift { Id = _siang, ShiftCode = "S", ShiftName = "Siang", StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(21, 0), IsActive = true });
        ctx.MstShifts.Add(new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsActive = true });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        foreach (var (id, unit, status, start, end) in new[]
        {
            (_rosterFeb, _icu, "Published", F(1), F(28)),
            (_rosterMarDraft, _icu, "Draft", new DateOnly(2027, 3, 1), new DateOnly(2027, 3, 31)),
            (_rosterIgd, _igd, "Published", F(1), F(28)),
            (_rosterFebDraft, _igd, "Draft", F(1), F(28)),
        })
        {
            ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
            {
                Id = id, OrganizationUnitId = unit, RosterPeriodCode = "R-" + id.ToString()[..4], RosterPeriodName = "R", RosterStatus = status, IsPublished = status == "Published",
                VersionNumber = 1, PeriodStartDate = start, PeriodEndDate = end, IsActive = true,
            });
        }
        ctx.SaveChanges();

        Pegawai("Rina", _icu);
        Pegawai("Tono", _icu);
        Pegawai("Budi", _icu);
        Pegawai("Andi", _igd);
    }

    public void Dispose() => _db.Dispose();

    private void Pegawai(string nama, Guid unit)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        var user = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = user, UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = id, IsActive = true });
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = id, OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary", IsPrimary = true,
            EffectiveStartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });
        ctx.SaveChanges();
        _p[nama] = (id, user);
    }

    private Guid Shift(string nama, Guid roster, DateOnly tanggal, Guid shift, int mulai, int jam = 7)
    {
        using var ctx = _db.CreateContext();
        var wf = _p[nama].Wf;
        var ra = ctx.TrxRosterAssignments.FirstOrDefault(x => x.RosterPeriodId == roster && x.WorkforceProfileId == wf);
        if (ra == null)
        {
            ra = new TrxRosterAssignment { Id = Guid.NewGuid(), RosterPeriodId = roster, WorkforceProfileId = wf, AssignmentStatus = "Published", IsActive = true };
            ctx.TrxRosterAssignments.Add(ra);
        }
        var s = new TrxShiftAssignment
        {
            Id = Guid.NewGuid(), RosterAssignmentId = ra.Id, WorkforceProfileId = wf, ShiftId = shift, ShiftDate = tanggal,
            ScheduledStartAt = Wib(tanggal, mulai), ScheduledEndAt = Wib(tanggal, mulai).AddHours(jam), AssignmentStatus = "Published", IsActive = true,
        };
        ctx.TrxShiftAssignments.Add(s);
        ctx.SaveChanges();
        return s.Id;
    }

    private MyScheduleSelfService Svc()
    {
        var ctx = _db.CreateContext();
        var um = new UserManager<ApplicationUser>(new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(), new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), null!, NullLogger<UserManager<ApplicationUser>>.Instance);
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        return new MyScheduleSelfService(ctx, new HumanResourceContextService(ctx, new HttpContextAccessor(), um), new AttendanceScheduleResolverService(ctx, setelan));
    }

    private void CutiBudi(DateOnly tanggal)
    {
        using var ctx = _db.CreateContext();
        var tipe = new MstLeaveType { LeaveTypeCode = "CT", LeaveTypeName = "Cuti Tahunan", LeaveCategory = "Annual", IsActive = true };
        ctx.Set<MstLeaveType>().Add(tipe);
        ctx.Set<WfpLeaveRequest>().Add(new WfpLeaveRequest
        {
            RequestNumber = "LV-1", WorkforceProfileId = _p["Budi"].Wf, LeaveTypeId = tipe.Id, StartDate = tanggal, EndDate = tanggal, Reason = "Urusan keluarga rahasia",
            LeaveRequestStatus = "Approved",
        });
        ctx.SaveChanges();
    }

    // AT-HRD-PK-42 + contoh HRD-DEC-209: roster unit hanya terbit; cuti Budi = "Libur" tanpa alasan; unit lain tidak tampil.
    [Fact]
    public async Task Roster_unit_hanya_terbit_tanpa_alasan_ketidakhadiran()
    {
        Shift("Rina", _rosterFeb, F(15), _pagi, 7);
        Shift("Tono", _rosterFeb, F(15), _pagi, 7);
        Shift("Budi", _rosterFeb, F(15), _pagi, 7);
        Shift("Budi", _rosterFeb, F(16), _siang, 14);
        Shift("Andi", _rosterIgd, F(15), _pagi, 7);                          // unit lain
        Shift("Rina", _rosterMarDraft, new DateOnly(2027, 3, 3), _pagi, 7);  // roster Draft
        CutiBudi(F(15));

        var r = await Svc().GetMyUnitRosterAsync(_p["Rina"].User, new MyUnitRosterQueryRequest { Month = "2027-02" }, Sekarang);
        Assert.True(r.Success, r.Message);
        var unit = Assert.Single(r.Data!.Units);
        Assert.Equal(_icu, unit.OrganizationUnitId);
        Assert.Equal(new[] { "Budi", "Rina", "Tono" }, unit.Members.Select(x => x.DisplayName));
        Assert.True(unit.Members.Single(x => x.DisplayName == "Rina").IsMe);
        Assert.Equal("P", unit.Members.Single(x => x.DisplayName == "Tono").Days.Single(x => x.Date == F(15)).ShiftCode);
        var budi = unit.Members.Single(x => x.DisplayName == "Budi");
        Assert.DoesNotContain(budi.Days, x => x.Date == F(15));               // cuti → tampil Libur
        Assert.Contains(budi.Days, x => x.Date == F(16) && x.ShiftCode == "S");

        var json = JsonSerializer.Serialize(r.Data);
        Assert.DoesNotContain("Cuti", json);
        Assert.DoesNotContain("rahasia", json);
        Assert.DoesNotContain("Leave", json);
        Assert.DoesNotContain("Andi", json);

        var maret = await Svc().GetMyUnitRosterAsync(_p["Rina"].User, new MyUnitRosterQueryRequest { Month = "2027-03" }, Sekarang);
        Assert.True(maret.Success, maret.Message);
        Assert.Empty(Assert.Single(maret.Data!.Units).Members);               // Draft Maret tidak tampil
    }

    // Jadwal sendiri: rinci, tanda Berubah dengan jadwal sebelumnya, shift yang dialihkan, siaga, Draft tidak tampil.
    [Fact]
    public async Task Jadwal_sendiri_rinci_dengan_tanda_berubah()
    {
        var pagi15 = Shift("Rina", _rosterFeb, F(15), _pagi, 7);
        var malam18 = Shift("Rina", _rosterFeb, F(18), _malam, 21, 10);
        var dialihkan = Shift("Tono", _rosterFeb, F(20), _siang, 14);         // semula milik Rina
        Shift("Rina", _rosterFebDraft, F(22), _pagi, 7);                       // roster Draft (unit IGD)
        using (var ctx = _db.CreateContext())
        {
            ctx.Set<HrdShiftAssignmentRevision>().Add(new HrdShiftAssignmentRevision
            {
                ShiftAssignmentId = malam18, RosterPeriodId = _rosterFeb, ShiftDate = F(18), ChangeType = "RetroactiveCorrection",
                PreviousWorkforceProfileId = _p["Rina"].Wf, NewWorkforceProfileId = _p["Rina"].Wf, PreviousShiftId = _siang, NewShiftId = _malam,
                PreviousStartAt = Wib(F(18), 14), PreviousEndAt = Wib(F(18), 21), ReasonCategory = "Salah catat", ReasonNote = "-", ChangedAt = Sekarang,
            });
            ctx.Set<HrdShiftAssignmentRevision>().Add(new HrdShiftAssignmentRevision
            {
                ShiftAssignmentId = dialihkan, RosterPeriodId = _rosterFeb, ShiftDate = F(20), ChangeType = "Replacement",
                PreviousWorkforceProfileId = _p["Rina"].Wf, NewWorkforceProfileId = _p["Tono"].Wf, PreviousShiftId = _siang, NewShiftId = _siang,
                PreviousStartAt = Wib(F(20), 14), PreviousEndAt = Wib(F(20), 21), ReasonCategory = "Sakit", ReasonNote = "-", ChangedAt = Sekarang,
            });
            var tipe = new MstOnCallType { OnCallTypeCode = "OC", OnCallTypeName = "Siaga Dokter", IsActive = true };
            ctx.Set<MstOnCallType>().Add(tipe);
            ctx.Set<TrxOnCallAssignment>().Add(new TrxOnCallAssignment
            {
                RosterPeriodId = _rosterFeb, WorkforceProfileId = _p["Rina"].Wf, OnCallTypeId = tipe.Id, StartAt = Wib(F(21), 20), EndAt = Wib(F(21), 20).AddHours(12), IsActive = true,
            });
            ctx.SaveChanges();
        }

        var r = await Svc().GetMyScheduleAsync(_p["Rina"].User, new MyScheduleQueryRequest { FromDate = F(1), ToDate = F(28) }, Sekarang);
        Assert.True(r.Success, r.Message);
        Assert.Equal(28, r.Data!.Count);
        var d15 = r.Data.Single(x => x.Date == F(15));
        Assert.Equal("Scheduled", d15.DayStatus);
        var s15 = Assert.Single(d15.Shifts);
        Assert.Equal(pagi15, s15.ShiftAssignmentId);
        Assert.Equal("Pagi", s15.ShiftName);
        Assert.Equal("ICU", s15.OrganizationUnitName);
        Assert.Equal(Wib(F(15), 7), DateTime.SpecifyKind(s15.StartAt, DateTimeKind.Utc));
        Assert.False(s15.IsChanged);

        var s18 = Assert.Single(r.Data.Single(x => x.Date == F(18)).Shifts);
        Assert.True(s18.IsChanged);
        Assert.Equal("Siang", s18.PreviousShiftName);
        Assert.Equal("Malam", s18.ShiftName);

        var d20 = r.Data.Single(x => x.Date == F(20));
        Assert.Empty(d20.Shifts);
        Assert.Equal("Siang", Assert.Single(d20.ReleasedShifts).ShiftName);

        Assert.Equal("Siaga Dokter", Assert.Single(r.Data.Single(x => x.Date == F(21)).OnCalls).OnCallTypeName);
        var d22 = r.Data.Single(x => x.Date == F(22));
        Assert.Empty(d22.Shifts);                                               // roster Draft tidak tampil
        Assert.Equal("NoSchedule", d22.DayStatus);

        // Tono hanya melihat jadwalnya sendiri: shift 20 Feb miliknya, tanpa shift Rina.
        var tono = await Svc().GetMyScheduleAsync(_p["Tono"].User, new MyScheduleQueryRequest { FromDate = F(1), ToDate = F(28) }, Sekarang);
        Assert.Equal(dialihkan, Assert.Single(tono.Data!.SelectMany(x => x.Shifts)).ShiftAssignmentId);
    }

    // Cuti sendiri tampil dengan jenisnya; jadwal tetap: hari kerja, akhir pekan libur, hari libur nasional.
    [Fact]
    public async Task Status_hari_cuti_dan_jadwal_tetap()
    {
        Shift("Budi", _rosterFeb, F(15), _pagi, 7);
        CutiBudi(F(15));
        var budi = await Svc().GetMyScheduleAsync(_p["Budi"].User, new MyScheduleQueryRequest { FromDate = F(15), ToDate = F(15) }, Sekarang);
        var b = Assert.Single(budi.Data!);
        Assert.Equal("Leave", b.DayStatus);
        Assert.Equal("Cuti", b.DayStatusLabel);
        Assert.Equal("Cuti Tahunan", b.LeaveTypeName);

        using (var ctx = _db.CreateContext())
        {
            var jadwal = new MstWorkSchedule { ScheduleCode = "KANTOR", ScheduleName = "Senin-Jumat", ScheduleType = "Fixed", WorkStartTime = new TimeOnly(8, 0), WorkEndTime = new TimeOnly(16, 0), IsActive = true };
            ctx.Set<MstWorkSchedule>().Add(jadwal);
            ctx.Set<WfpWorkScheduleAssignment>().Add(new WfpWorkScheduleAssignment { WorkforceProfileId = _p["Andi"].Wf, WorkScheduleId = jadwal.Id, EffectiveStartDate = new DateOnly(2026, 1, 1), IsPrimary = true, IsActive = true });
            var kalender = new MstWorkCalendar { WorkCalendarCode = "CAL27", WorkCalendarName = "2027", CalendarYear = 2027, StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 12, 31), TimeZoneId = "Asia/Jakarta", IsDefault = true, IsActive = true };
            ctx.Set<MstWorkCalendar>().Add(kalender);
            ctx.Set<MstHoliday>().Add(new MstHoliday { WorkCalendarId = kalender.Id, HolidayCode = "IMLEK", HolidayName = "Tahun Baru Imlek", StartDate = new DateTime(2027, 2, 8), EndDate = new DateTime(2027, 2, 8), IsNationalHoliday = true, IsActive = true });
            ctx.SaveChanges();
        }

        var andi = await Svc().GetMyScheduleAsync(_p["Andi"].User, new MyScheduleQueryRequest { FromDate = F(5), ToDate = F(8) }, Sekarang);
        Assert.True(andi.Success, andi.Message);
        var jumat = andi.Data!.Single(x => x.Date == F(5));
        Assert.Equal("FixedWorkSchedule", jumat.ScheduleSource);
        Assert.Equal("Scheduled", jumat.DayStatus);
        Assert.Equal("Senin-Jumat", jumat.FixedSchedule!.WorkScheduleName);
        var imlek = andi.Data!.Single(x => x.Date == F(8));
        Assert.True(imlek.IsHoliday);
        Assert.Equal("Holiday", imlek.DayStatus);
        Assert.Contains("Tahun Baru Imlek", imlek.HolidayNames);
    }

    // AT-HRD-PK-43: identitas dari sesi; akun tanpa profil pegawai tidak mendapat data siapa pun.
    [Fact]
    public async Task Identitas_dari_sesi()
    {
        Shift("Rina", _rosterFeb, F(15), _pagi, 7);
        var tanpaProfil = Guid.NewGuid();
        using (var ctx = _db.CreateContext())
        {
            ctx.Users.Add(new ApplicationUser { Id = tanpaProfil, UserName = "admin2", UserCode = "ADMIN2XX", DisplayName = "Admin", UserType = UserType.Employee, IsActive = true });
            ctx.SaveChanges();
        }
        var r = await Svc().GetMyScheduleAsync(tanpaProfil, new MyScheduleQueryRequest(), Sekarang);
        Assert.Equal(404, r.StatusCode);
        Assert.Equal(401, (await Svc().GetMyScheduleAsync(Guid.Empty, new MyScheduleQueryRequest(), Sekarang)).StatusCode);
        Assert.Equal(401, (await Svc().GetMyUnitRosterAsync(Guid.NewGuid(), new MyUnitRosterQueryRequest(), Sekarang)).StatusCode);
        Assert.DoesNotContain(typeof(MyScheduleQueryRequest).GetProperties(), p => p.Name.Contains("Workforce") || p.Name.Contains("Employee") || p.Name.Contains("User"));
        Assert.DoesNotContain(typeof(MyUnitRosterQueryRequest).GetProperties(), p => p.Name.Contains("Workforce") || p.Name.Contains("Unit"));
    }

    // AT-HRD-PK-44 / VR-PK-29: di luar 3 bulan ke belakang sampai bulan depan → 400.
    [Fact]
    public async Task Rentang_di_luar_batas_ditolak()
    {
        const string pesan = "Jadwal yang dapat dilihat: 3 bulan ke belakang sampai bulan depan";
        var rina = _p["Rina"].User;
        foreach (var (dari, sampai) in new[]
        {
            (new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),   // 6 bulan lalu
            (new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 1)), // sehari sebelum batas
            (new DateOnly(2027, 3, 31), new DateOnly(2027, 4, 1)),   // lewat bulan depan
        })
        {
            var r = await Svc().GetMyScheduleAsync(rina, new MyScheduleQueryRequest { FromDate = dari, ToDate = sampai }, Sekarang);
            Assert.Equal(400, r.StatusCode);
            Assert.Equal(pesan, r.Message);
        }
        Assert.True((await Svc().GetMyScheduleAsync(rina, new MyScheduleQueryRequest { FromDate = new DateOnly(2026, 11, 1), ToDate = new DateOnly(2026, 11, 30) }, Sekarang)).Success);
        Assert.True((await Svc().GetMyScheduleAsync(rina, new MyScheduleQueryRequest { FromDate = new DateOnly(2027, 3, 1), ToDate = new DateOnly(2027, 3, 31) }, Sekarang)).Success);
        Assert.Equal(400, (await Svc().GetMyScheduleAsync(rina, new MyScheduleQueryRequest { FromDate = F(1), ToDate = new DateOnly(2027, 3, 5) }, Sekarang)).StatusCode);   // > 31 hari
        Assert.Equal(400, (await Svc().GetMyScheduleAsync(rina, new MyScheduleQueryRequest { FromDate = F(10), ToDate = F(9) }, Sekarang)).StatusCode);

        Assert.Equal(pesan, (await Svc().GetMyUnitRosterAsync(rina, new MyUnitRosterQueryRequest { Month = "2026-08" }, Sekarang)).Message);
        Assert.Equal(pesan, (await Svc().GetMyUnitRosterAsync(rina, new MyUnitRosterQueryRequest { Month = "2027-04" }, Sekarang)).Message);
        Assert.Equal("Format bulan harus yyyy-MM.", (await Svc().GetMyUnitRosterAsync(rina, new MyUnitRosterQueryRequest { Month = "Februari" }, Sekarang)).Message);
        Assert.True((await Svc().GetMyUnitRosterAsync(rina, new MyUnitRosterQueryRequest { Month = "2026-11" }, Sekarang)).Success);
    }
}
