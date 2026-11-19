using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-343 — AT-HRD-PK-19, -20, -21, -26, -28 (HRD-DEC-207, 210, 211; state matrix 30.6).
public sealed class AttendanceStatusTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _aktor = Guid.NewGuid(), _periode = Guid.NewGuid();

    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);
    private static DateOnly TanggalWib(DateTime utc) => DateOnly.FromDateTime(utc.AddHours(7));
    private static DateTime U(DateTime x) => DateTime.SpecifyKind(x, DateTimeKind.Utc);

    public AttendanceStatusTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _unit, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", IsPublished = true, VersionNumber = 1,
            PeriodStartDate = new DateOnly(2020, 1, 1), PeriodEndDate = new DateOnly(2030, 12, 31), IsActive = true,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

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

    private void Shift(Guid pegawai, DateOnly tanggal, DateTime mulai, DateTime selesai, bool malam = false)
    {
        using var ctx = _db.CreateContext();
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = pegawai, AssignmentStatus = "Published", IsActive = true });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            RosterAssignmentId = ra, WorkforceProfileId = pegawai, ShiftId = _pagi, ShiftDate = tanggal, ScheduledStartAt = mulai, ScheduledEndAt = selesai,
            IsNightShift = malam, AssignmentStatus = "Published", IsActive = true,
        });
        ctx.SaveChanges();
    }

    private Guid Scan(Guid pegawai, DateTime waktu, string jenis = "CheckIn")
    {
        using var ctx = _db.CreateContext();
        var log = new HrdAttendanceRawLog
        {
            WorkforceProfileId = pegawai, EventAt = waktu, EventType = jenis, SourceType = AttendanceValueConstants.RawLogSourceType.Device,
            ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Matched,
        };
        ctx.HrdAttendanceRawLogs.Add(log);
        ctx.SaveChanges();
        return log.Id;
    }

    private async Task<HrdAttendanceDaily> Proses(Guid pegawai, DateOnly tanggal, bool ulang = true)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var r = await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = pegawai, WorkDate = tanggal, ForceReprocess = ulang }, _aktor);
        Assert.True(r.Success, r.Message);
        using var baca = _db.CreateContext();
        return baca.HrdAttendanceDailies.AsNoTracking().Include(d => d.Exceptions).Single(d => d.WorkforceProfileId == pegawai && d.AttendanceDate == tanggal);
    }

    private static List<HrdAttendanceException> Aktif(HrdAttendanceDaily d) => d.Exceptions.Where(x => x.IsActive && !x.IsDelete).ToList();

    // AT-HRD-PK-19: shift berjalan tanpa scan → NotYetPresent; sesudah jam selesai → NoScan; tidak pernah Alpa.
    [Fact]
    public async Task Tanpa_scan_Belum_Hadir_lalu_Tidak_Ada_Scan()
    {
        var now = DateTime.UtcNow;
        var budi = Pegawai("Budi");
        var hariIni = TanggalWib(now);
        Shift(budi, hariIni, now.AddHours(-2), now.AddHours(5));
        var berjalan = await Proses(budi, hariIni);
        Assert.Equal(S.NotYetPresent, berjalan.AttendanceStatus);
        Assert.False(berjalan.IsAbsent);
        Assert.Empty(Aktif(berjalan));   // tanpa MissingCheckIn/Absent selama shift berjalan

        var andi = Pegawai("Andi");
        var kemarin = TanggalWib(now.AddDays(-1));
        Shift(andi, kemarin, Wib(kemarin, 7), Wib(kemarin, 14));
        var selesai = await Proses(andi, kemarin);
        Assert.Equal(S.NoScan, selesai.AttendanceStatus);
        Assert.False(selesai.IsAbsent);
        Assert.DoesNotContain(Aktif(selesai), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.Absent);
    }

    // AT-HRD-PK-20: scan mesin yang tiba belakangan mengubah status; sapuan penjadwal menjadwalkan hitung ulangnya.
    [Fact]
    public async Task Scan_tertunda_mengubah_NoScan_dan_dijadwalkan_ulang_otomatis()
    {
        var budi = Pegawai("Budi2");
        var kemarin = TanggalWib(DateTime.UtcNow.AddDays(-1));
        Shift(budi, kemarin, Wib(kemarin, 7), Wib(kemarin, 14));
        Assert.Equal(S.NoScan, (await Proses(budi, kemarin)).AttendanceStatus);

        // Data mesin IGD-01 tertunda: 06.55 dan 14.02.
        Scan(budi, Wib(kemarin, 6, 55));
        Scan(budi, Wib(kemarin, 14, 2), "CheckOut");

        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var proses = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var penjadwal = new AttendanceSchedulerService(ctx, proses, new AttendancePeriodService(ctx, NullLogger<AttendancePeriodService>.Instance),
            Options.Create(new AttendanceSchedulerOptions { Enabled = true, AutoEnqueueDailyProcessing = false, MaximumCatchUpDays = 7 }),
            NullLogger<AttendanceSchedulerService>.Instance, setelan);
        Assert.Equal(1, await penjadwal.EnsureProvisionalStatusRecheckJobsAsync(DateTime.UtcNow));
        Assert.Equal(0, await penjadwal.EnsureProvisionalStatusRecheckJobsAsync(DateTime.UtcNow)); // tanpa duplikat
        using (var baca = _db.CreateContext())
        {
            var job = baca.Set<HrdAttendanceSchedulerJob>().AsNoTracking().Single();
            Assert.True(job.ForceReprocess);
            Assert.Equal(budi, job.WorkforceProfileId);
            Assert.StartsWith("ATT-RECHECK-", job.CorrelationId);
        }

        Assert.Equal(S.Present, (await Proses(budi, kemarin)).AttendanceStatus);
    }

    // AT-HRD-PK-21: shift malam lintas tengah malam masih berjalan → NotYetPresent walau tanggal shift sudah kemarin.
    [Fact]
    public async Task Shift_malam_dihitung_sampai_jam_selesai_hari_berikutnya()
    {
        var now = DateTime.UtcNow;
        var andi = Pegawai("Andi3");
        var tanggalShift = TanggalWib(now).AddDays(-1);
        Shift(andi, tanggalShift, now.AddHours(-20), now.AddHours(2), malam: true);
        var d = await Proses(andi, tanggalShift);
        Assert.Equal(S.NotYetPresent, d.AttendanceStatus);

        // Sapuan: belum lewat jam selesai → tidak dijadwalkan; sesudahnya → dijadwalkan.
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var proses = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var penjadwal = new AttendanceSchedulerService(ctx, proses, new AttendancePeriodService(ctx, NullLogger<AttendancePeriodService>.Instance),
            Options.Create(new AttendanceSchedulerOptions { Enabled = true, MaximumCatchUpDays = 7 }), NullLogger<AttendanceSchedulerService>.Instance, setelan);
        Assert.Equal(0, await penjadwal.EnsureProvisionalStatusRecheckJobsAsync(now));
        Assert.Equal(1, await penjadwal.EnsureProvisionalStatusRecheckJobsAsync(now.AddHours(3)));
    }

    // AT-HRD-PK-26: scan 02.30 untuk shift 07.00 (jendela 240) tidak dipasangkan, jadi pengecualian; log tetap.
    [Fact]
    public async Task Scan_di_luar_jendela_menjadi_pengecualian_dan_log_tetap()
    {
        var rina = Pegawai("Rina");
        var kemarin = TanggalWib(DateTime.UtcNow.AddDays(-1));
        Shift(rina, kemarin, Wib(kemarin, 7), Wib(kemarin, 14));
        var luar = Scan(rina, Wib(kemarin, 2, 30));

        var d = await Proses(rina, kemarin);
        Assert.Equal(S.NoScan, d.AttendanceStatus);
        Assert.Null(d.FirstCheckInAt);
        var ex = Assert.Single(Aktif(d), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.OutsideScanWindow);
        Assert.Equal(Wib(kemarin, 2, 30), U(ex.ActualAt!.Value));
        Assert.False(ex.IsPayrollBlocking);
        Assert.Contains("02:30", ex.Message);

        using var baca = _db.CreateContext();
        var log = baca.HrdAttendanceRawLogs.AsNoTracking().Single(x => x.Id == luar);
        Assert.Equal(AttendanceValueConstants.RawLogProcessingStatus.Matched, log.ProcessingStatus);
        Assert.Null(log.ProcessedAttendanceDailyId);
        Assert.True(log.IsActive);

        // Proses ulang tidak menggandakan pengecualian.
        Assert.Single(Aktif(await Proses(rina, kemarin)), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.OutsideScanWindow);
    }

    [Fact]
    public async Task Scan_milik_shift_malam_hari_sebelumnya_bukan_pengecualian()
    {
        var sari = Pegawai("Sari");
        var kemarin = TanggalWib(DateTime.UtcNow.AddDays(-1));
        var lusa = kemarin.AddDays(-1);
        Shift(sari, lusa, Wib(lusa, 21), Wib(kemarin, 7), malam: true);
        Shift(sari, kemarin, Wib(kemarin, 14), Wib(kemarin, 21));
        Scan(sari, Wib(kemarin, 7, 5), "CheckOut");   // pulang shift malam; di luar jendela Siang (10.00)

        var d = await Proses(sari, kemarin);
        Assert.DoesNotContain(Aktif(d), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.OutsideScanWindow);
    }

    // AT-HRD-PK-28 (tanggal 2026 agar shift Sari sudah lewat): 17 Agt libur nasional — Andi (jadwal tetap) Holiday, Sari (roster) NoScan.
    [Fact]
    public async Task Hari_libur_hanya_untuk_jadwal_tetap()
    {
        var hari = new DateOnly(2026, 8, 17);
        var andi = Pegawai("AndiTetap");
        var sari = Pegawai("SariRoster");
        using (var ctx = _db.CreateContext())
        {
            var kalender = new MstWorkCalendar { WorkCalendarCode = "CAL26", WorkCalendarName = "2026", CalendarYear = 2026, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), TimeZoneId = "Asia/Jakarta", IsDefault = true, IsActive = true };
            ctx.Set<MstWorkCalendar>().Add(kalender);
            ctx.Set<MstHoliday>().Add(new MstHoliday { WorkCalendarId = kalender.Id, HolidayCode = "HUT-RI", HolidayName = "Hari Kemerdekaan RI", StartDate = new DateTime(2026, 8, 17), EndDate = new DateTime(2026, 8, 17), IsNationalHoliday = true, IsActive = true });
            var jadwal = new MstWorkSchedule { ScheduleCode = "KANTOR", ScheduleName = "Jam Kantor", ScheduleType = "Fixed", WorkStartTime = new TimeOnly(8, 0), WorkEndTime = new TimeOnly(16, 0), IsActive = true };
            ctx.Set<MstWorkSchedule>().Add(jadwal);
            ctx.Set<WfpWorkScheduleAssignment>().Add(new WfpWorkScheduleAssignment { WorkforceProfileId = andi, WorkScheduleId = jadwal.Id, EffectiveStartDate = new DateOnly(2026, 1, 1), IsPrimary = true, IsActive = true });
            ctx.SaveChanges();
        }
        Shift(sari, hari, Wib(hari, 7), Wib(hari, 14));

        var dAndi = await Proses(andi, hari);
        var dSari = await Proses(sari, hari);
        Assert.Equal(AttendanceValueConstants.ScheduleSource.FixedWorkSchedule, dAndi.ScheduleSource);
        Assert.Equal(S.Holiday, dAndi.AttendanceStatus);
        Assert.True(dSari.IsHoliday);   // tanda libur tetap tercatat
        Assert.Equal(S.NoScan, dSari.AttendanceStatus);
    }
}
