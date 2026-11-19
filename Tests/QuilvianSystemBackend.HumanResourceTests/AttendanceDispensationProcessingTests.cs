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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

public static class Penjadwal
{
    public static AttendanceSchedulerService Buat(QuilvianSystemBackend.Repositories.ApplicationDbContext ctx, AttendanceSettingService setelan) =>
        new(ctx, new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan),
            new AttendancePeriodService(ctx, NullLogger<AttendancePeriodService>.Instance),
            Options.Create(new AttendanceSchedulerOptions { Enabled = true, AutoEnqueueDailyProcessing = false, MaximumCatchUpDays = 7 }),
            NullLogger<AttendanceSchedulerService>.Instance, setelan);
}

// BE-HRD-354 — dispensasi pada pemrosesan harian (HRD-DEC-200, 35.9.4 butir 7, INV-A52-07, INT-HR-102); AT-HRD-PK-32.
// Tanggal kerja 3 Feb 2026 (lampau, sehingga shift sudah selesai); shift Pagi 07.00–14.00; toleransi 10 menit.
public sealed class AttendanceDispensationProcessingTests : IDisposable
{
    private static readonly DateOnly Hari = new(2026, 2, 3);
    private static DateTime Wib(int jam, int menit = 0) => new DateTime(Hari.Year, Hari.Month, Hari.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);
    private static readonly DateTime Sekarang = Wib(16);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _periode = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _p = new();

    public AttendanceDispensationProcessingTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, MaxBackdatedCorrectionDays = 3, IsActive = true });
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _unit, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", IsPublished = true, VersionNumber = 1,
            PeriodStartDate = new DateOnly(2026, 1, 1), PeriodEndDate = new DateOnly(2026, 12, 31), IsActive = true,
        });
        ctx.SaveChanges();
        Pegawai("Kepala", jabatan: true);
    }

    public void Dispose() => _db.Dispose();

    private Guid Pegawai(string nama, bool jabatan = false)
    {
        using var ctx = _db.CreateContext();
        var wf = Guid.NewGuid();
        var user = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = wf, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = user, UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = wf, IsActive = true });
        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = wf, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary", IsPrimary = true,
            IsManagerialAssignment = jabatan, EffectiveStartDate = mulai, IsActive = true,
        });
        if (!jabatan)
        {
            ctx.WfpManagerAssignments.Add(new WfpManagerAssignment
            { WorkforceProfileId = wf, ManagerWorkforceProfileId = _p["Kepala"].Wf, ManagerType = "Direct", EffectiveStartDate = mulai, IsPrimaryManager = true, CanApproveRequests = true, IsActive = true });

            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = wf, AssignmentStatus = "Published", IsActive = true });
            ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
            { RosterAssignmentId = ra, WorkforceProfileId = wf, ShiftId = _pagi, ShiftDate = Hari, ScheduledStartAt = Wib(7), ScheduledEndAt = Wib(14), AssignmentStatus = "Published", IsActive = true });
        }

        ctx.SaveChanges();
        _p[nama] = (wf, user);
        return wf;
    }

    private void Scan(string nama, DateTime waktu, string jenis)
    {
        using var ctx = _db.CreateContext();
        ctx.HrdAttendanceRawLogs.Add(new HrdAttendanceRawLog
        {
            WorkforceProfileId = _p[nama].Wf, EventAt = waktu, EventType = jenis, SourceType = AttendanceValueConstants.RawLogSourceType.Device,
            ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Matched,
        });
        ctx.SaveChanges();
    }

    private async Task<HrdAttendanceDaily> Proses(string nama)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var r = await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = _p[nama].Wf, WorkDate = Hari, ForceReprocess = true }, _p["Kepala"].User);
        Assert.True(r.Success, r.Message);
        using var baca = _db.CreateContext();
        return baca.HrdAttendanceDailies.AsNoTracking().Include(d => d.Exceptions).Single(d => d.WorkforceProfileId == _p[nama].Wf && d.AttendanceDate == Hari);
    }

    private AttendanceDispensationService Dispensasi(string sebagai)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        return new AttendanceDispensationService(ctx, new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(_p[sebagai].User)), setelan,
            new PemberitahuanTiruan(), Penjadwal.Buat(ctx, setelan), NullLogger<AttendanceDispensationService>.Instance);
    }

    private async Task<DispensationResponse> Ajukan(string nama, int jamMulai, int menitMulai, int jamSelesai, int menitSelesai, string jenis = "LateArrival")
    {
        var r = await Dispensasi(nama).CreateMineAsync(_p[nama].User, new CreateMyDispensationRequest
        { AttendanceDate = Hari, DispensationType = jenis, StartAt = Wib(jamMulai, menitMulai), EndAt = Wib(jamSelesai, menitSelesai), Reason = "Mengantar rujukan pasien" }, Sekarang);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private async Task Setujui(DispensationResponse d)
    {
        var r = await Dispensasi("Kepala").ApproveAsync(_p["Kepala"].User, d.Id, new DispensationDecisionRequest { RowVersion = d.RowVersion });
        Assert.True(r.Success, r.Message);
    }

    private static List<HrdAttendanceException> Aktif(HrdAttendanceDaily d) => d.Exceptions.Where(x => x.IsActive && !x.IsDelete).ToList();

    // AT-HRD-PK-32 / contoh 35.9.4: scan 07.40, dispensasi 07.00–07.40 disetujui → terlambat 0; log mesin tetap 07.40.
    [Fact]
    public async Task AT_HRD_PK_32_dispensasi_disetujui_menghapus_terlambat_tanpa_mengubah_log()
    {
        Pegawai("Sari");
        Scan("Sari", Wib(7, 40), "CheckIn");
        Scan("Sari", Wib(14), "CheckOut");

        var sebelum = await Proses("Sari");
        Assert.Equal((S.Late, 40), (sebelum.AttendanceStatus, sebelum.LateMinutes));
        Assert.Contains(Aktif(sebelum), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.Late);

        var d = await Ajukan("Sari", 7, 0, 7, 40);
        Assert.Equal(40, (await Proses("Sari")).LateMinutes);   // masih Submitted → tidak berpengaruh

        await Setujui(d);
        using (var ctx = _db.CreateContext())
        {
            // INT-HR-102: hitung ulang dijadwalkan untuk pegawai dan tanggal itu.
            var job = ctx.Set<HrdAttendanceSchedulerJob>().AsNoTracking().Single();
            Assert.Equal(($"ATT-DISP-{d.Id:N}", true, _p["Sari"].Wf, Hari, Hari), (job.CorrelationId, job.ForceReprocess, job.WorkforceProfileId!.Value, job.StartDate, job.EndDate));
        }

        var sesudah = await Proses("Sari");
        Assert.Equal((S.Present, 0, false), (sesudah.AttendanceStatus, sesudah.LateMinutes, sesudah.IsLate));
        Assert.DoesNotContain(Aktif(sesudah), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.Late);
        Assert.Contains("Dispensasi terlambat 40 menit", sesudah.ProcessingMessage);
        Assert.Equal(Wib(7, 40), DateTime.SpecifyKind(sesudah.FirstCheckInAt!.Value, DateTimeKind.Utc));

        using var baca = _db.CreateContext();
        var log = baca.HrdAttendanceRawLogs.AsNoTracking().Where(x => x.WorkforceProfileId == _p["Sari"].Wf).OrderBy(x => x.EventAt).ToList();
        Assert.Equal(2, log.Count);
        Assert.Equal(Wib(7, 40), DateTime.SpecifyKind(log[0].EventAt, DateTimeKind.Utc));
    }

    // HRD-DEC-200 uji (a)(b): hanya rentang dispensasi yang dibebaskan; keterlambatan di luar rentang tetap dihitung.
    [Fact]
    public async Task Keterlambatan_di_luar_rentang_tetap_dihitung()
    {
        Pegawai("Tono");
        Scan("Tono", Wib(7, 40), "CheckIn");
        Scan("Tono", Wib(14), "CheckOut");
        await Setujui(await Ajukan("Tono", 7, 0, 7, 30));

        var d = await Proses("Tono");
        Assert.Equal((S.Late, 10), (d.AttendanceStatus, d.LateMinutes));
        var late = Assert.Single(Aktif(d), x => x.ExceptionType == AttendanceValueConstants.AttendanceExceptionType.Late);
        Assert.Equal("Terlambat 10 menit dari jadwal check-in. 30 menit lainnya dibebaskan dispensasi.", late.Message);
        Assert.Contains("Dispensasi terlambat 30 menit", d.ProcessingMessage);
    }

    // Dispensasi pulang lebih awal; jenis yang tidak cocok dan dispensasi yang ditolak tidak berpengaruh.
    [Fact]
    public async Task Pulang_lebih_awal_dan_jenis_yang_tidak_cocok()
    {
        Pegawai("Rina");
        Scan("Rina", Wib(6, 55), "CheckIn");
        Scan("Rina", Wib(13), "CheckOut");
        Assert.Equal((S.EarlyLeave, 60), ((await Proses("Rina")).AttendanceStatus, (await Proses("Rina")).EarlyLeaveMinutes));

        // Dispensasi datang terlambat — walau rentangnya 13.00–13.30 — tidak menghapus pulang cepat; dispensasi pulang cepat yang ditolak juga tidak.
        using (var ctx = _db.CreateContext())
        {
            // Disisip langsung agar terbukti filter jenis pada pemrosesan, bukan hanya larangan bertumpuk sejenis di API.
            ctx.HrdAttendanceDispensations.Add(new HrdAttendanceDispensation
            {
                WorkforceProfileId = _p["Rina"].Wf, AttendanceDate = Hari, DispensationType = "LateArrival", StartAt = Wib(13), EndAt = Wib(13, 30),
                Reason = "x", Status = "Approved", RequestedByUserId = _p["Rina"].User, ApproverWorkforceProfileId = _p["Kepala"].Wf,
            });
            ctx.SaveChanges();
        }
        Assert.Equal(60, (await Proses("Rina")).EarlyLeaveMinutes);
        var ditolak = await Ajukan("Rina", 13, 0, 14, 0, "EarlyDeparture");
        var tolak = await Dispensasi("Kepala").RejectAsync(_p["Kepala"].User, ditolak.Id, new RejectDispensationRequest { RowVersion = ditolak.RowVersion, RejectionReason = "Tidak ada surat tugas" });
        Assert.True(tolak.Success, tolak.Message);
        Assert.Equal(60, (await Proses("Rina")).EarlyLeaveMinutes);

        await Setujui(await Ajukan("Rina", 13, 0, 14, 0, "EarlyDeparture"));
        var d = await Proses("Rina");
        Assert.Equal((S.Present, 0), (d.AttendanceStatus, d.EarlyLeaveMinutes));
        Assert.Contains("Dispensasi pulang cepat 60 menit", d.ProcessingMessage);
    }

    // Rentang bertumpuk tidak dihitung dua kali (data lama/disisip langsung; API menolak tumpang tindih).
    [Fact]
    public async Task Rentang_bertumpuk_digabung()
    {
        Pegawai("Andi");
        Scan("Andi", Wib(7, 40), "CheckIn");
        Scan("Andi", Wib(14), "CheckOut");
        using (var ctx = _db.CreateContext())
        {
            HrdAttendanceDispensation D(int m1, int m2) => new()
            {
                WorkforceProfileId = _p["Andi"].Wf, AttendanceDate = Hari, DispensationType = "LateArrival", StartAt = Wib(7, m1), EndAt = Wib(7, m2),
                Reason = "x", Status = "Approved", RequestedByUserId = _p["Andi"].User, ApproverWorkforceProfileId = _p["Kepala"].Wf,
            };
            ctx.HrdAttendanceDispensations.AddRange(D(0, 20), D(10, 30), D(50, 55));   // gabungan 07.00–07.30; 07.50 di luar keterlambatan
            ctx.SaveChanges();
        }

        Assert.Equal(10, (await Proses("Andi")).LateMinutes);
    }

    // Tanpa rekap atau rekap terkunci → tidak ada job; pemrosesan harian membaca dispensasi saat tanggal itu diproses.
    [Fact]
    public async Task Hitung_ulang_hanya_untuk_rekap_yang_ada_dan_tidak_terkunci()
    {
        Pegawai("Budi");
        await Setujui(await Ajukan("Budi", 7, 0, 7, 40));
        using (var ctx = _db.CreateContext()) Assert.Empty(ctx.Set<HrdAttendanceSchedulerJob>());

        Pegawai("Dewi");
        Scan("Dewi", Wib(7, 40), "CheckIn");
        Scan("Dewi", Wib(14), "CheckOut");
        await Proses("Dewi");
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendanceDailies.Single(x => x.WorkforceProfileId == _p["Dewi"].Wf).IsLocked = true;
            ctx.SaveChanges();
        }
        await Setujui(await Ajukan("Dewi", 7, 0, 7, 40));
        using (var ctx = _db.CreateContext()) Assert.Empty(ctx.Set<HrdAttendanceSchedulerJob>());

        // Rekap Budi baru dibuat sesudah dispensasi disetujui: langsung tanpa terlambat.
        Scan("Budi", Wib(7, 40), "CheckIn");
        Scan("Budi", Wib(14), "CheckOut");
        Assert.Equal(0, (await Proses("Budi")).LateMinutes);
    }
}
