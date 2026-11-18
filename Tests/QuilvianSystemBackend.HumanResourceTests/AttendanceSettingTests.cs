using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-342 — AT-HRD-PK-24, -25, -27 (HRD-DEC-215, HRD-DEC-211, HRD-ASM-83): setelan kehadiran satu-satunya
// sumber toleransi terlambat dan jendela scan.
public sealed class AttendanceSettingTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _rina = Guid.NewGuid(), _akunRina = Guid.NewGuid(), _pagi = Guid.NewGuid(), _setelan = Guid.NewGuid(), _aktor = Guid.NewGuid();
    private static readonly DateOnly Tanggal = new(2027, 3, 2);

    // Jam dinding WIB → UTC.
    private static DateTime Wib(int jam, int menit, int hari = 2) => new DateTime(2027, 3, hari, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);

    public AttendanceSettingTests()
    {
        using var ctx = _db.CreateContext();
        var unit = Guid.NewGuid();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = unit, LegalEntityId = Guid.NewGuid(), UnitCode = "IGD", UnitName = "IGD" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), PaidWorkMinutes = 420, IsActive = true });
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = _rina, ProfileCode = "RINA", DisplayName = "Rina", UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = _akunRina, UserName = "rina", UserCode = "RINA0001", DisplayName = "Rina", UserType = UserType.Employee, WorkforceProfileId = _rina, IsActive = true });
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = _rina, OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary",
            IsPrimary = true, EffectiveStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });

        var periode = Guid.NewGuid();
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = periode, OrganizationUnitId = unit, RosterPeriodCode = "R-IGD", RosterPeriodName = "IGD Maret", RosterStatus = "Published", IsPublished = true,
            VersionNumber = 1, PeriodStartDate = new DateOnly(2027, 3, 1), PeriodEndDate = new DateOnly(2027, 3, 31), IsActive = true,
        });
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = periode, WorkforceProfileId = _rina, AssignmentStatus = "Published", IsActive = true });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            RosterAssignmentId = ra, WorkforceProfileId = _rina, ShiftId = _pagi, ShiftDate = Tanggal, ScheduledStartAt = Wib(7, 0), ScheduledEndAt = Wib(14, 0),
            PlannedWorkMinutes = 420, AssignmentStatus = "Published", IsActive = true,
        });

        // Scan 02.30 (4,5 jam sebelum shift), 07.12, dan pulang 14.05.
        foreach (var (jam, menit, jenis) in new[] { (2, 30, "CheckIn"), (7, 12, "CheckIn"), (14, 5, "CheckOut") })
        {
            ctx.HrdAttendanceRawLogs.Add(new HrdAttendanceRawLog
            {
                UserId = _akunRina, WorkforceProfileId = _rina, EventAt = Wib(jam, menit), EventType = jenis,
                SourceType = AttendanceValueConstants.RawLogSourceType.Device, ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Matched,
            });
        }

        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { Id = _setelan, HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private void Setelan(int toleransi, int sebelum = 240, int sesudah = 480, bool hapus = false)
    {
        using var ctx = _db.CreateContext();
        var s = ctx.HrdAttendanceSettings.Single(x => x.Id == _setelan);
        s.GracePeriodMinutes = toleransi;
        s.ScanWindowBeforeMinutes = sebelum;
        s.ScanWindowAfterMinutes = sesudah;
        s.IsDelete = hapus;
        ctx.SaveChanges();
    }

    private void KebijakanToleransi(int menit)
    {
        using var ctx = _db.CreateContext();
        var grace = new MstGracePeriodPolicy { Id = Guid.NewGuid(), GracePeriodPolicyCode = "G30", GracePeriodPolicyName = "Toleransi 30", LateCheckInGraceMinutes = menit, EarlyCheckInMinutes = 600, LateCheckOutMinutes = 900, IsActive = true };
        ctx.Set<MstGracePeriodPolicy>().Add(grace);
        ctx.Set<MstAttendancePolicy>().Add(new MstAttendancePolicy { Id = Guid.NewGuid(), AttendancePolicyCode = "AP", AttendancePolicyName = "Global", GracePeriodPolicyId = grace.Id, IsDefault = true, IsActive = true, RequireCheckIn = true, RequireCheckOut = true });
        ctx.SaveChanges();
    }

    private async Task<HrdAttendanceDaily> Proses()
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var r = await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = _rina, WorkDate = Tanggal, ForceReprocess = true }, _aktor);
        Assert.True(r.Success, r.Message);
        using var baca = _db.CreateContext();
        return baca.HrdAttendanceDailies.AsNoTracking().Single(d => d.WorkforceProfileId == _rina && d.AttendanceDate == Tanggal);
    }

    // AT-HRD-PK-24: toleransi 10 → 15; scan 07.12 menjadi tepat waktu.
    [Fact]
    public async Task Toleransi_dibaca_dari_setelan()
    {
        var awal = await Proses();
        Assert.Equal(12, awal.LateMinutes);
        Assert.True(awal.IsLate);

        Setelan(15);
        var sesudah = await Proses();
        Assert.Equal(0, sesudah.LateMinutes);
        Assert.False(sesudah.IsLate);
    }

    [Fact]
    public async Task Toleransi_nol_berarti_tanpa_toleransi()
    {
        Setelan(0);
        Assert.Equal(12, (await Proses()).LateMinutes);
    }

    // AT-HRD-PK-25: MstGracePeriodPolicy 30 menit tidak berpengaruh (juga EarlyCheckIn/LateCheckOut-nya).
    [Fact]
    public async Task Kebijakan_toleransi_per_jadwal_tidak_berpengaruh()
    {
        KebijakanToleransi(30);
        var d = await Proses();
        Assert.Equal(12, d.LateMinutes);   // dengan kebijakan 30 menit akan 0
        Assert.Equal(Wib(7, 12), DateTime.SpecifyKind(d.FirstCheckInAt!.Value, DateTimeKind.Utc)); // scan 02.30 tetap di luar jendela 240

        using var ctx = _db.CreateContext();
        var resolver = new AttendanceScheduleResolverService(ctx, new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance));
        var s = (await resolver.ResolveAsync(_rina, Tanggal)).Data!;
        Assert.Equal(10, s.LateCheckInGraceMinutes);
        Assert.Equal(240, s.EarlyCheckInMinutes);
        Assert.Equal(480, s.LateCheckOutMinutes);
        Assert.Equal(Wib(3, 0), DateTime.SpecifyKind(s.EarliestCheckInAt!.Value, DateTimeKind.Utc));
    }

    // HRD-DEC-215 uji (b): mengubah jendela mengubah scan mana yang dipasangkan.
    [Fact]
    public async Task Jendela_scan_dibaca_dari_setelan()
    {
        Assert.Equal(Wib(7, 12), DateTime.SpecifyKind((await Proses()).FirstCheckInAt!.Value, DateTimeKind.Utc));
        Setelan(10, sebelum: 300);
        var d = await Proses();
        Assert.Equal(Wib(2, 30), DateTime.SpecifyKind(d.FirstCheckInAt!.Value, DateTimeKind.Utc));
        Assert.Equal(0, d.LateMinutes);
    }

    // AT-HRD-PK-27 / VR-PK-30: setelan tidak ada → Error, tanpa angka cadangan.
    [Fact]
    public async Task Setelan_tidak_ada_pemrosesan_Error_tanpa_angka_cadangan()
    {
        var awal = await Proses();
        Assert.Equal(AttendanceValueConstants.AttendanceProcessingStatus.Processed, awal.ProcessingStatus);

        Setelan(10, hapus: true);
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var r = await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = _rina, WorkDate = Tanggal, ForceReprocess = true }, _aktor);

        using var baca = _db.CreateContext();
        var d = baca.HrdAttendanceDailies.AsNoTracking().Single(x => x.WorkforceProfileId == _rina && x.AttendanceDate == Tanggal);
        Assert.Equal(AttendanceValueConstants.AttendanceProcessingStatus.Error, d.ProcessingStatus);
        Assert.Equal("Setelan kehadiran tidak terbaca", d.ProcessingMessage);
        Assert.Equal(AttendanceValueConstants.AttendanceStatus.Unprocessed, d.AttendanceStatus);
        Assert.False(d.IsPayrollEligible);
        Assert.Contains("Setelan kehadiran tidak terbaca", r.Data?.Items.Single().Message ?? r.Message);
        Assert.Equal(3, baca.HrdAttendanceRawLogs.Count()); // log mesin tidak berubah jumlahnya
    }

    [Fact]
    public async Task Setelan_di_luar_batas_diperlakukan_tidak_terbaca()
    {
        Setelan(90);
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = _rina, WorkDate = Tanggal }, _aktor);
        using var baca = _db.CreateContext();
        Assert.Equal(AttendanceValueConstants.AttendanceProcessingStatus.Error,
            baca.HrdAttendanceDailies.AsNoTracking().Single(x => x.WorkforceProfileId == _rina).ProcessingStatus);
    }

    // Kontrak Settings v26: GET/PUT memuat jendela; AutoConvertIncompleteToAbsent dihapus; VR-PK-23.
    [Fact]
    public async Task Setelan_GET_PUT_sesuai_kontrak()
    {
        Assert.Null(typeof(AttendanceSettingResponse).GetProperty("AutoConvertIncompleteToAbsent"));
        Assert.Null(typeof(UpdateAttendanceSettingRequest).GetProperty("AutoConvertIncompleteToAbsent"));

        var svc = new AttendanceSettingService(_db.CreateContext(), NullLogger<AttendanceSettingService>.Instance);
        var get = await svc.GetSettingAsync();
        Assert.Equal(240, get.Data!.ScanWindowBeforeMinutes);
        Assert.Equal(480, get.Data.ScanWindowAfterMinutes);

        var tolak = await svc.UpdateSettingAsync("RSMMC", new UpdateAttendanceSettingRequest { GracePeriodMinutes = 15, MaxBackdatedCorrectionDays = 3, ScanWindowBeforeMinutes = 721 }, _aktor);
        Assert.Equal(400, tolak.StatusCode);
        Assert.Equal("Nilai toleransi atau jendela scan di luar batas.", tolak.Message);

        // Layar lama tidak mengirim jendela: nilai tersimpan dipertahankan.
        var lama = await svc.UpdateSettingAsync("RSMMC", new UpdateAttendanceSettingRequest { GracePeriodMinutes = 15, MaxBackdatedCorrectionDays = 3 }, _aktor);
        Assert.True(lama.Success, lama.Message);
        Assert.Equal(240, lama.Data!.ScanWindowBeforeMinutes);
        Assert.Equal(15, lama.Data.GracePeriodMinutes);

        var baru = await new AttendanceSettingService(_db.CreateContext(), NullLogger<AttendanceSettingService>.Instance)
            .UpdateSettingAsync("RSMMC", new UpdateAttendanceSettingRequest { GracePeriodMinutes = 15, MaxBackdatedCorrectionDays = 3, ScanWindowBeforeMinutes = 120, ScanWindowAfterMinutes = 240 }, _aktor);
        Assert.Equal(120, baru.Data!.ScanWindowBeforeMinutes);
        Assert.Equal(240, baru.Data.ScanWindowAfterMinutes);
        Assert.Equal(_aktor, baru.Data.UpdatedBy);
    }
}
