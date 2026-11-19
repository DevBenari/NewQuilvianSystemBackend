using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-347 — API master hari libur (HRD-DEC-210, VR-PK-25); AT-HRD-PK-28 dengan hari libur dari API.
public sealed class HolidayTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _aktor = Guid.NewGuid(), _periode = Guid.NewGuid(), _kalender = Guid.NewGuid(), _jadwal = Guid.NewGuid();
    private static readonly DateOnly Hari = new(2026, 8, 17);

    private static DateTime Wib(DateOnly d, int jam) => new DateTime(d.Year, d.Month, d.Day, jam, 0, 0, DateTimeKind.Utc).AddHours(-7);

    public HolidayTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _unit, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", IsPublished = true, VersionNumber = 1,
            PeriodStartDate = new DateOnly(2026, 8, 1), PeriodEndDate = new DateOnly(2026, 8, 31), IsActive = true,
        });
        ctx.Set<MstWorkCalendar>().Add(new MstWorkCalendar { Id = _kalender, WorkCalendarCode = "CAL26", WorkCalendarName = "2026", CalendarYear = 2026, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), TimeZoneId = "Asia/Jakarta", IsDefault = true, IsActive = true });
        ctx.Set<MstWorkSchedule>().Add(new MstWorkSchedule { Id = _jadwal, ScheduleCode = "KANTOR", ScheduleName = "Jam Kantor", ScheduleType = "Fixed", WorkStartTime = new TimeOnly(8, 0), WorkEndTime = new TimeOnly(16, 0), IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private HolidayService Svc() => new(_db.CreateContext(), NullLogger<HolidayService>.Instance);

    private CreateHolidayRequest Req(string kode, DateOnly mulai, DateOnly selesai, Guid? kalender = null, string jenis = "National") => new()
    {
        WorkCalendarId = kalender ?? _kalender, HolidayCode = kode, HolidayName = "Libur " + kode, StartDate = mulai, EndDate = selesai, HolidayType = jenis, IsNationalHoliday = jenis == "National",
    };

    private UpdateHolidayRequest UReq(string kode, DateOnly mulai, DateOnly selesai) => new()
    {
        WorkCalendarId = _kalender, HolidayCode = kode, HolidayName = "Libur " + kode, StartDate = mulai, EndDate = selesai, HolidayType = "National", IsNationalHoliday = true,
    };

    private Guid Pegawai(string nama)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = id, IsActive = true });
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = id, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary", IsPrimary = true,
            EffectiveStartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });
        ctx.SaveChanges();
        return id;
    }

    private async Task<HrdAttendanceDaily> Proses(Guid pegawai, DateOnly tanggal)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var r = await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = pegawai, WorkDate = tanggal, ForceReprocess = true }, _aktor);
        Assert.True(r.Success, r.Message);
        using var baca = _db.CreateContext();
        return baca.HrdAttendanceDailies.AsNoTracking().Single(d => d.WorkforceProfileId == pegawai && d.AttendanceDate == tanggal);
    }

    private string SidikRoster()
    {
        using var ctx = _db.CreateContext();
        var p = ctx.TrxRosterPeriods.AsNoTracking().Single();
        var s = ctx.TrxShiftAssignments.AsNoTracking().OrderBy(x => x.Id).ToList()
            .Select(x => $"{x.Id}|{x.WorkforceProfileId}|{x.ShiftId}|{x.ShiftDate}|{x.ScheduledStartAt:O}|{x.ScheduledEndAt:O}|{x.AssignmentStatus}|{x.UpdateDateTime:O}");
        return $"{p.RosterStatus}|{p.VersionNumber}|{p.IsPublished}|{p.UpdateDateTime:O}||" + string.Join(";", s);
    }

    // AT-HRD-PK-28 + HRD-DEC-210: hari libur dari API → jadwal tetap Holiday, roster tetap diharapkan hadir; roster terbit tidak berubah.
    [Fact]
    public async Task Hari_libur_dari_api_berlaku_dan_roster_tidak_berubah()
    {
        var andi = Pegawai("AndiTetap");
        var sari = Pegawai("SariRoster");
        using (var ctx = _db.CreateContext())
        {
            ctx.Set<WfpWorkScheduleAssignment>().Add(new WfpWorkScheduleAssignment { WorkforceProfileId = andi, WorkScheduleId = _jadwal, EffectiveStartDate = new DateOnly(2026, 1, 1), IsPrimary = true, IsActive = true });
            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = sari, AssignmentStatus = "Published", IsActive = true });
            ctx.TrxShiftAssignments.Add(new TrxShiftAssignment { RosterAssignmentId = ra, WorkforceProfileId = sari, ShiftId = _pagi, ShiftDate = Hari, ScheduledStartAt = Wib(Hari, 7), ScheduledEndAt = Wib(Hari, 14), AssignmentStatus = "Published", IsActive = true });
            ctx.SaveChanges();
        }

        Assert.Equal(S.NoScan, (await Proses(andi, Hari)).AttendanceStatus);   // sebelum hari libur dicatat
        var sidik = SidikRoster();

        var r = await Svc().CreateAsync(Req("HUT-RI", Hari, Hari), _aktor);
        Assert.True(r.Success, r.Message);
        Assert.Equal(Hari, r.Data!.StartDate);
        Assert.Equal("Libur nasional", r.Data.HolidayTypeLabel);
        Assert.Equal("CAL26", r.Data.WorkCalendarCode);
        Assert.Equal(sidik, SidikRoster());

        var dAndi = await Proses(andi, Hari);
        var dSari = await Proses(sari, Hari);
        Assert.Equal(S.Holiday, dAndi.AttendanceStatus);
        Assert.True(dSari.IsHoliday);
        Assert.Equal(S.NoScan, dSari.AttendanceStatus);
        Assert.Equal(sidik, SidikRoster());

        // Dinonaktifkan → tidak lagi dibaca pemrosesan.
        var non = await Svc().UpdateStatusAsync(r.Data.Id, false, _aktor);
        Assert.False(non.Data!.IsActive);
        Assert.Equal(S.NoScan, (await Proses(andi, Hari)).AttendanceStatus);
        Assert.Equal(sidik, SidikRoster());
    }

    // VR-PK-25: EndDate < StartDate atau di luar rentang kalender → 400; kalender nonaktif/asing → 400.
    [Fact]
    public async Task Tanggal_di_luar_kalender_ditolak()
    {
        var nonaktif = Guid.NewGuid();
        using (var ctx = _db.CreateContext())
        {
            ctx.Set<MstWorkCalendar>().Add(new MstWorkCalendar { Id = nonaktif, WorkCalendarCode = "CAL25", WorkCalendarName = "2025", CalendarYear = 2025, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31), TimeZoneId = "Asia/Jakarta", IsActive = false });
            ctx.SaveChanges();
        }

        foreach (var (mulai, selesai) in new[]
        {
            (new DateOnly(2026, 8, 18), new DateOnly(2026, 8, 17)),   // selesai sebelum mulai
            (new DateOnly(2025, 12, 31), new DateOnly(2026, 1, 1)),   // mulai sebelum kalender
            (new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 1)),   // selesai sesudah kalender
            (new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 1)),     // seluruhnya di luar
        })
        {
            var r = await Svc().CreateAsync(Req("X", mulai, selesai), _aktor);
            Assert.Equal(400, r.StatusCode);
            Assert.Equal("Tanggal libur harus berada di dalam kalender kerja yang dipilih", r.Message);
        }

        // Batas tepat kalender sah.
        Assert.True((await Svc().CreateAsync(Req("TAHUN-BARU", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1)), _aktor)).Success);
        Assert.True((await Svc().CreateAsync(Req("AKHIR", new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 31)), _aktor)).Success);

        Assert.Equal("Kalender kerja tidak ditemukan atau tidak aktif.", (await Svc().CreateAsync(Req("Y", new DateOnly(2025, 5, 1), new DateOnly(2025, 5, 1), nonaktif), _aktor)).Message);
        Assert.Equal(400, (await Svc().CreateAsync(Req("Z", Hari, Hari, Guid.NewGuid()), _aktor)).StatusCode);
        Assert.Equal("Jenis hari libur tidak valid.", (await Svc().CreateAsync(Req("W", Hari, Hari, jenis: "Ulang tahun"), _aktor)).Message);
        Assert.Equal("Kode hari libur wajib diisi.", (await Svc().CreateAsync(Req(" ", Hari, Hari), _aktor)).Message);

        // Ubah ke luar kalender juga ditolak dan data tidak berubah.
        var ok = await Svc().CreateAsync(Req("HUT-RI", Hari, Hari), _aktor);
        var ubah = await Svc().UpdateAsync(ok.Data!.Id, UReq("HUT-RI", Hari, new DateOnly(2027, 1, 2)), _aktor);
        Assert.Equal(400, ubah.StatusCode);
        Assert.Equal(Hari, (await Svc().GetByIdAsync(ok.Data.Id))!.EndDate);
    }

    // Kode unik per kalender (indeks unik): 409; ubah, hapus lunak, kode dapat dipakai lagi; daftar per tahun.
    [Fact]
    public async Task Kode_unik_ubah_hapus_dan_daftar()
    {
        var a = await Svc().CreateAsync(Req("CUTI-BERSAMA", new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 26), jenis: "CollectiveLeave"), _aktor);
        Assert.True(a.Success, a.Message);
        var dup = await Svc().CreateAsync(Req("CUTI-BERSAMA", Hari, Hari), _aktor);
        Assert.Equal(409, dup.StatusCode);

        var u = await Svc().UpdateAsync(a.Data!.Id, UReq("CUTI-BERSAMA", new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 25)), _aktor);
        Assert.True(u.Success, u.Message);
        Assert.Equal(new DateOnly(2026, 12, 25), u.Data!.EndDate);
        Assert.Equal(_aktor, u.Data.UpdateBy);

        await Svc().CreateAsync(Req("HUT-RI", Hari, Hari), _aktor);
        var daftar = await Svc().GetPagedAsync(new HolidayQueryRequest { WorkCalendarId = _kalender, Year = 2026 });
        Assert.Equal(2, daftar.TotalData);
        Assert.Equal("HUT-RI", daftar.Items[0].HolidayCode);   // urut tanggal
        Assert.Equal(0, (await Svc().GetPagedAsync(new HolidayQueryRequest { Year = 2027 })).TotalData);

        var hapus = await Svc().DeleteAsync(a.Data.Id, _aktor);
        Assert.True(hapus.Success);
        Assert.Null(await Svc().GetByIdAsync(a.Data.Id));
        using (var ctx = _db.CreateContext())
        {
            var h = ctx.Set<MstHoliday>().AsNoTracking().Single(x => x.Id == a.Data.Id);
            Assert.True(h.IsDelete);
            Assert.Equal(_aktor, h.DeleteBy);
        }
        Assert.True((await Svc().CreateAsync(Req("CUTI-BERSAMA", new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 24)), _aktor)).Success);
        Assert.Equal(404, (await Svc().UpdateStatusAsync(Guid.NewGuid(), true, _aktor)).StatusCode);
        Assert.Equal(404, (await Svc().DeleteAsync(Guid.NewGuid(), _aktor)).StatusCode);

        var meta = await Svc().GetFilterMetadataAsync();
        Assert.Equal(_kalender, Assert.Single(meta.WorkCalendarOptions).Id);
        Assert.Contains(meta.HolidayTypeOptions, x => x.Value == "CollectiveLeave" && x.Label == "Cuti bersama");
    }
}
