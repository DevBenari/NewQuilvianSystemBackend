using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
using R = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.RawLogProcessingStatus;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-345 — AT-HRD-PK-29, -30 (HRD-DEC-212, INV-A52-06, INT-HR-103; state matrix 30.7).
public sealed class DeviceHoldTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _aktor = Guid.NewGuid(), _periode = Guid.NewGuid();
    private readonly DateOnly _kemarin = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7).AddDays(-1));

    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);

    public DeviceHoldTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "GDB", UnitName = "Gedung B" });
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

    private Guid Pegawai(string kode)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = kode, DisplayName = kode, UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), UserName = "u" + kode, UserCode = ("U" + kode).PadRight(8, 'X')[..8], DisplayName = kode, UserType = UserType.Employee, WorkforceProfileId = id, IsActive = true });
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = id, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary", IsPrimary = true,
            EffectiveStartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = id, AssignmentStatus = "Published", IsActive = true });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            RosterAssignmentId = ra, WorkforceProfileId = id, ShiftId = _pagi, ShiftDate = _kemarin, ScheduledStartAt = Wib(_kemarin, 7), ScheduledEndAt = Wib(_kemarin, 14),
            AssignmentStatus = "Published", IsActive = true,
        });
        ctx.SaveChanges();
        return id;
    }

    private Guid Mesin(string kode, bool aktif = true, string? external = null)
    {
        using var ctx = _db.CreateContext();
        var m = new MstAttendanceDevice { AttendanceDeviceCode = kode, AttendanceDeviceName = "Mesin " + kode, ExternalDeviceId = external, IsActive = aktif };
        ctx.Set<MstAttendanceDevice>().Add(m);
        ctx.SaveChanges();
        return m.Id;
    }

    private sealed record Layanan(AttendanceRawLogService Log, AttendanceProcessingService Proses);

    private Layanan Buat(params IInterceptor[] i)
    {
        var ctx = _db.CreateContext(i);
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var proses = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var penjadwal = new AttendanceSchedulerService(ctx, proses, new AttendancePeriodService(ctx, NullLogger<AttendancePeriodService>.Instance),
            Options.Create(new AttendanceSchedulerOptions { Enabled = true, MaximumCatchUpDays = 7 }), NullLogger<AttendanceSchedulerService>.Instance, setelan);
        return new Layanan(new AttendanceRawLogService(ctx, penjadwal, NullLogger<AttendanceRawLogService>.Instance), proses);
    }

    private static CreateAttendanceRawLogRequest Scan(string? mesin, string kodePegawai, DateTime waktu, string jenis = "CheckIn", Guid? mesinId = null, string? logId = null) => new()
    {
        ExternalDeviceId = mesin, AttendanceDeviceId = mesinId, DeviceUserKey = kodePegawai, EventAt = new DateTimeOffset(waktu), EventType = jenis,
        SourceType = AttendanceValueConstants.RawLogSourceType.Device, ExternalLogId = logId,
    };

    private List<HrdAttendanceRawLog> Log()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendanceRawLogs.AsNoTracking().ToList();
    }

    private async Task<HrdAttendanceDaily> Proses(Guid pegawai)
    {
        var r = await Buat().Proses.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = pegawai, WorkDate = _kemarin, ForceReprocess = true }, _aktor);
        Assert.True(r.Success, r.Message);
        using var baca = _db.CreateContext();
        return baca.HrdAttendanceDailies.AsNoTracking().Single(d => d.WorkforceProfileId == pegawai && d.AttendanceDate == _kemarin);
    }

    private async Task<List<Guid>> KirimLimaPuluhScanGdb01()
    {
        var pegawai = new List<Guid>();
        var batch = new CreateAttendanceRawLogBatchRequest();
        for (var i = 0; i < 25; i++)
        {
            var kode = $"GDB{i:00}";
            pegawai.Add(Pegawai(kode));
            batch.Items.Add(Scan("GDB-01", kode, Wib(_kemarin, 6, 50 + i % 10)));
            batch.Items.Add(Scan("GDB-01", kode, Wib(_kemarin, 14, 2), "CheckOut"));
        }

        var hasil = await Buat().Log.CreateBatchAsync(batch, _aktor, null, null);
        Assert.Equal(50, hasil.SuccessCount);
        Assert.Equal(0, hasil.FailedCount);
        Assert.All(hasil.Items, x => Assert.Equal(R.AwaitingDeviceRegistration, x.ProcessingStatus));
        return pegawai;
    }

    // AT-HRD-PK-29: 50 scan dari GDB-01 yang belum terdaftar tersimpan ditahan, tidak 404, tidak ikut dihitung.
    [Fact]
    public async Task Scan_mesin_tak_terdaftar_ditahan_bukan_404()
    {
        var pegawai = await KirimLimaPuluhScanGdb01();
        var log = Log();
        Assert.Equal(50, log.Count);
        Assert.All(log, x =>
        {
            Assert.Equal(R.AwaitingDeviceRegistration, x.ProcessingStatus);
            Assert.Null(x.AttendanceDeviceId);
            Assert.Equal("GDB-01", x.ExternalDeviceId);
            Assert.NotNull(x.WorkforceProfileId);              // identitas pegawai tetap dicocokkan
            Assert.Contains("belum terdaftar", x.ProcessingMessage);
        });

        // Tidak dipakai menghitung kehadiran (HRD-DEC-212 butir 2).
        var d = await Proses(pegawai[0]);
        Assert.Equal(S.NoScan, d.AttendanceStatus);
        Assert.All(Log().Where(x => x.WorkforceProfileId == pegawai[0]), x => Assert.Equal(R.AwaitingDeviceRegistration, x.ProcessingStatus));

        // Kiriman ulang dari mesin yang sama dikenali ganda (butir 4).
        var ulang = await Buat().Log.CreateAsync(Scan("GDB-01", "GDB00", Wib(_kemarin, 6, 50)), _aktor, null, null);
        Assert.True(ulang.Success);
        Assert.True(ulang.Data!.IsDuplicate);
        Assert.Equal(50, Log().Count);
    }

    // AT-HRD-PK-30: GDB-01 didaftarkan → 50 scan dilepas, hitung ulang dijadwalkan, status pegawai terisi.
    [Fact]
    public async Task Pendaftaran_mesin_melepas_scan_dan_menjadwalkan_hitung_ulang()
    {
        var pegawai = await KirimLimaPuluhScanGdb01();
        Assert.Equal(S.NoScan, (await Proses(pegawai[3])).AttendanceStatus);
        // Rekap sehari sebelumnya yang sudah ada (mis. shift malam) ikut dihitung ulang; rekap terkunci tidak.
        using (var ctx = _db.CreateContext())
        {
            foreach (var (p, kunci) in new[] { (pegawai[5], false), (pegawai[6], true) })
            {
                ctx.HrdAttendanceDailies.Add(new HrdAttendanceDaily
                {
                    UserId = Guid.NewGuid(), WorkforceProfileId = p, UserType = UserType.Employee, OrganizationUnitId = _unit, AttendanceDate = _kemarin.AddDays(-1),
                    AttendanceStatus = S.NoScan, ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed, IsLocked = kunci, IsActive = true,
                });
            }
            ctx.SaveChanges();
        }
        int RekapCount() { using var c = _db.CreateContext(); return c.HrdAttendanceDailies.Count(); }
        var rekapSebelum = RekapCount();

        var mesin = Mesin("GDB-01");
        var r = await Buat().Log.ReleaseHeldLogsForDeviceAsync(mesin, _aktor);
        Assert.True(r.Success, r.Message);
        Assert.Equal(50, r.Data!.HeldLogCount);
        Assert.Equal(50, r.Data.MatchedCount);
        Assert.Equal(0, r.Data.PendingCount + r.Data.DuplicateCount + r.Data.RejectedCount + r.Data.ErrorCount);
        Assert.Equal(2, r.Data.ScheduledReprocessJobCount);   // rekap pegawai[3] (kemarin) dan pegawai[5] (kemarin−1)
        Assert.All(Log(), x =>
        {
            Assert.Equal(R.Matched, x.ProcessingStatus);
            Assert.Equal(mesin, x.AttendanceDeviceId);
        });

        using (var ctx = _db.CreateContext())
        {
            var jobs = ctx.Set<HrdAttendanceSchedulerJob>().AsNoTracking().ToList();
            Assert.Equal(2, jobs.Count);
            Assert.All(jobs, j =>
            {
                Assert.True(j.ForceReprocess);
                Assert.Equal(j.StartDate, j.EndDate);
                Assert.StartsWith("ATT-DEVREL-", j.CorrelationId);
            });
            Assert.Contains(jobs, j => j.WorkforceProfileId == pegawai[3] && j.StartDate == _kemarin);
            Assert.Contains(jobs, j => j.WorkforceProfileId == pegawai[5] && j.StartDate == _kemarin.AddDays(-1));
        }
        Assert.Equal(rekapSebelum, RekapCount());   // pelepasan tidak membuat rekap baru

        // Pemrosesan (setara job) → status terisi tanpa scan ulang.
        var d = await Proses(pegawai[3]);
        Assert.Equal(S.Present, d.AttendanceStatus);
        Assert.Equal(Wib(_kemarin, 6, 53), DateTime.SpecifyKind(d.FirstCheckInAt!.Value, DateTimeKind.Utc));

        // Pelepasan kedua tidak menemukan apa pun.
        var lagi = await Buat().Log.ReleaseHeldLogsForDeviceAsync(mesin, _aktor);
        Assert.Equal(0, lagi.Data!.HeldLogCount);
        Assert.Equal(0, lagi.Data.ScheduledReprocessJobCount);

        // Kiriman ulang scan yang sama sesudah pendaftaran tetap dikenali ganda (sidik dihitung ulang dengan id mesin).
        var ulang = await Buat().Log.CreateAsync(Scan("GDB-01", "GDB03", Wib(_kemarin, 6, 53)), _aktor, null, null);
        Assert.True(ulang.Success, ulang.Message);
        Assert.True(ulang.Data!.IsDuplicate);
        Assert.Equal(50, Log().Count);
    }

    // Scan baru dari mesin yang sudah terdaftar tetap diterima normal (regresi).
    [Fact]
    public async Task Mesin_terdaftar_tetap_normal()
    {
        Pegawai("REG01");
        var mesin = Mesin("IGD-01", external: "EXT-IGD-01");
        var r = await Buat().Log.CreateAsync(Scan("EXT-IGD-01", "REG01", Wib(_kemarin, 7)), _aktor, null, null);
        Assert.True(r.Success, r.Message);
        Assert.Equal(R.Matched, r.Data!.ProcessingStatus);
        Assert.Equal(mesin, r.Data.AttendanceDeviceId);
    }

    // HRD-DEC-212 butir 2 + uji keputusan (c): mesin nonaktif = tak terdaftar; id mesin asing tanpa identitas lain tetap 404.
    [Fact]
    public async Task Mesin_nonaktif_ditahan_dan_dilepas_saat_diaktifkan()
    {
        var p = Pegawai("NON01");
        var mesin = Mesin("GDB-02", aktif: false, external: "EXT-GDB-02");
        var r = await Buat().Log.CreateAsync(Scan(null, "NON01", Wib(_kemarin, 7), mesinId: mesin), _aktor, null, null);
        Assert.True(r.Success, r.Message);
        Assert.Equal(R.AwaitingDeviceRegistration, r.Data!.ProcessingStatus);
        Assert.Equal("EXT-GDB-02", Log().Single().ExternalDeviceId);

        var asing = await Buat().Log.CreateAsync(Scan(null, "NON01", Wib(_kemarin, 8), mesinId: Guid.NewGuid()), _aktor, null, null);
        Assert.False(asing.Success);
        Assert.Equal(404, asing.StatusCode);

        // Pelepasan saat mesin masih nonaktif ditolak; scan tetap tertahan.
        var tolak = await Buat().Log.ReleaseHeldLogsForDeviceAsync(mesin, _aktor);
        Assert.Equal(409, tolak.StatusCode);
        Assert.Equal(R.AwaitingDeviceRegistration, Log().Single().ProcessingStatus);

        using (var ctx = _db.CreateContext())
        {
            ctx.Set<MstAttendanceDevice>().Single(x => x.Id == mesin).IsActive = true;
            ctx.SaveChanges();
        }
        var lepas = await Buat().Log.ReleaseHeldLogsForDeviceAsync(mesin, _aktor);
        Assert.Equal(1, lepas.Data!.MatchedCount);
        Assert.Equal(p, Log().Single().WorkforceProfileId);
        Assert.Equal(mesin, Log().Single().AttendanceDeviceId);
    }

    // Retry tidak meloloskan scan tertahan; sesudah mesin terdaftar, retry melepas lewat jalur pelepasan.
    [Fact]
    public async Task Retry_scan_tertahan_menunggu_mesin()
    {
        Pegawai("RTY01");
        var r = await Buat().Log.CreateAsync(Scan("GDB-03", "RTY01", Wib(_kemarin, 7)), _aktor, null, null);
        var id = r.Data!.Id;

        var coba = await Buat().Log.RetryAsync(id, _aktor);
        Assert.Equal(409, coba.StatusCode);
        Assert.Equal(R.AwaitingDeviceRegistration, Log().Single().ProcessingStatus);

        var mesin = Mesin("GDB-03");
        var lagi = await Buat().Log.RetryAsync(id, _aktor);
        Assert.True(lagi.Success, lagi.Message);
        Assert.Equal(R.AwaitingDeviceRegistration, lagi.Data!.PreviousProcessingStatus);
        Assert.Equal(R.Matched, lagi.Data.ProcessingStatus);
        Assert.Equal(mesin, Log().Single().AttendanceDeviceId);
    }

    // Identitas belum dikenal → Pending; scan yang sudah masuk lewat mesin terdaftar → Duplicate (tidak dihitung dua kali).
    [Fact]
    public async Task Pelepasan_menangani_identitas_kosong_dan_ganda()
    {
        Pegawai("DUP01");
        var svc = Buat().Log;
        await svc.CreateAsync(Scan("GDB-04", "TIDAK-ADA", Wib(_kemarin, 7)), _aktor, null, null);
        await svc.CreateAsync(Scan("GDB-04", "DUP01", Wib(_kemarin, 7, 1), logId: "L-1"), _aktor, null, null);

        var mesin = Mesin("GDB-04");
        // Log L-1 yang sama dikirim ulang lewat mesin terdaftar sebelum pelepasan berjalan.
        var lewatTerdaftar = await Buat().Log.CreateAsync(Scan("GDB-04", "DUP01", Wib(_kemarin, 7, 1), logId: "L-1"), _aktor, null, null);
        Assert.Equal(R.Matched, lewatTerdaftar.Data!.ProcessingStatus);

        var r = await Buat().Log.ReleaseHeldLogsForDeviceAsync(mesin, _aktor);
        Assert.True(r.Success, r.Message);
        Assert.Equal(2, r.Data!.HeldLogCount);
        Assert.Equal(1, r.Data.PendingCount);
        Assert.Equal(1, r.Data.DuplicateCount);
        Assert.Equal(0, r.Data.ScheduledReprocessJobCount);
        var log = Log();
        Assert.Single(log, x => x.ProcessingStatus == R.Duplicate && x.AttendanceDeviceId == null);
        Assert.Single(log, x => x.ProcessingStatus == R.Pending && x.AttendanceDeviceId == mesin);
    }

    // Jalur gagal: galat saat menyimpan pelepasan → seluruh scan tetap tertahan, tanpa job.
    [Fact]
    public async Task Galat_pelepasan_membiarkan_scan_tetap_tertahan()
    {
        await KirimLimaPuluhScanGdb01();
        var mesin = Mesin("GDB-01");
        var r = await Buat(new GagalSimpan()).Log.ReleaseHeldLogsForDeviceAsync(mesin, _aktor);
        Assert.False(r.Success);
        Assert.Equal(500, r.StatusCode);
        Assert.All(Log(), x =>
        {
            Assert.Equal(R.AwaitingDeviceRegistration, x.ProcessingStatus);
            Assert.Null(x.AttendanceDeviceId);
        });
        using var ctx = _db.CreateContext();
        Assert.Empty(ctx.Set<HrdAttendanceSchedulerJob>().AsNoTracking().ToList());
    }

    // Identitas mesin yang dipakai dua mesin aktif → 409, scan tetap tertahan.
    [Fact]
    public async Task Identitas_mesin_ganda_ditolak()
    {
        Pegawai("AMB01");
        await Buat().Log.CreateAsync(Scan("GDB-05", "AMB01", Wib(_kemarin, 7)), _aktor, null, null);
        var a = Mesin("GDB-05");
        Mesin("LAIN-05", external: "GDB-05");
        var r = await Buat().Log.ReleaseHeldLogsForDeviceAsync(a, _aktor);
        Assert.Equal(409, r.StatusCode);
        Assert.Equal(R.AwaitingDeviceRegistration, Log().Single().ProcessingStatus);
    }

    // ===== BE-HRD-346 — API mesin absen (lewat service) =====

    private AttendanceDeviceService Registri(params IInterceptor[] i)
    {
        var ctx = _db.CreateContext(i);
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var proses = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var penjadwal = new AttendanceSchedulerService(ctx, proses, new AttendancePeriodService(ctx, NullLogger<AttendancePeriodService>.Instance),
            Options.Create(new AttendanceSchedulerOptions { Enabled = true, MaximumCatchUpDays = 7 }), NullLogger<AttendanceSchedulerService>.Instance, setelan);
        var log = new AttendanceRawLogService(ctx, penjadwal, NullLogger<AttendanceRawLogService>.Instance);
        return new AttendanceDeviceService(ctx, log, NullLogger<AttendanceDeviceService>.Instance);
    }

    private static CreateAttendanceDeviceRequest Baru(string kode, bool aktif = true, string? serial = null, string? external = null) => new()
    {
        AttendanceDeviceCode = kode, AttendanceDeviceName = "Mesin " + kode, DeviceType = "Fingerprint", SerialNumber = serial, ExternalDeviceId = external, IsActive = aktif,
    };

    private static UpdateAttendanceDeviceRequest Ubah(string kode, string? external = null) => new()
    {
        AttendanceDeviceCode = kode, AttendanceDeviceName = "Mesin " + kode, DeviceType = "Fingerprint", ExternalDeviceId = external,
    };

    // AT-HRD-PK-30 lewat pendaftaran: POST mesin aktif melepas 50 scan sesudah commit; mesin tak dikenal hilang dari daftar.
    [Fact]
    public async Task Mendaftarkan_mesin_melepas_scan_tertahan()
    {
        var pegawai = await KirimLimaPuluhScanGdb01();
        Assert.Equal(S.NoScan, (await Proses(pegawai[0])).AttendanceStatus);

        var tak = Assert.Single(await Registri().GetUnregisteredAsync());
        Assert.Equal("GDB-01", tak.ExternalDeviceId);
        Assert.Equal(50, tak.HeldScanCount);
        Assert.Null(tak.InactiveAttendanceDeviceId);

        var r = await Registri().CreateAsync(Baru("GDB-01"), _aktor);
        Assert.True(r.Success, r.Message);
        Assert.Equal(50, r.Data!.HeldScanRelease!.MatchedCount);
        Assert.Equal(1, r.Data.HeldScanRelease.ScheduledReprocessJobCount);   // rekap pegawai[0] yang sudah ada
        Assert.Equal(0, r.Data.HeldScanCount);
        Assert.Contains("50 scan tertahan dilepas", r.Message);
        Assert.All(Log(), x => Assert.Equal(r.Data.Id, x.AttendanceDeviceId));
        Assert.Empty(await Registri().GetUnregisteredAsync());
        Assert.Equal(S.Present, (await Proses(pegawai[0])).AttendanceStatus);
    }

    // Mesin didaftarkan nonaktif: scan tetap tertahan dan tampil sebagai milik mesin nonaktif; mengaktifkan melepasnya.
    [Fact]
    public async Task Mengaktifkan_mesin_melepas_scan_tertahan()
    {
        await KirimLimaPuluhScanGdb01();
        var r = await Registri().CreateAsync(Baru("GDB-01", aktif: false), _aktor);
        Assert.True(r.Success, r.Message);
        Assert.Null(r.Data!.HeldScanRelease);
        Assert.Equal(50, r.Data.HeldScanCount);
        Assert.Equal(r.Data.Id, Assert.Single(await Registri().GetUnregisteredAsync()).InactiveAttendanceDeviceId);
        Assert.All(Log(), x => Assert.Equal(R.AwaitingDeviceRegistration, x.ProcessingStatus));

        var aktif = await Registri().UpdateStatusAsync(r.Data.Id, true, _aktor);
        Assert.True(aktif.Success, aktif.Message);
        Assert.Equal(50, aktif.Data!.HeldScanRelease!.MatchedCount);
        Assert.All(Log(), x => Assert.Equal(R.Matched, x.ProcessingStatus));

        // Menonaktifkan tidak melepas apa pun; scan baru dari mesin itu kembali ditahan.
        var non = await Registri().UpdateStatusAsync(r.Data.Id, false, _aktor);
        Assert.Null(non.Data!.HeldScanRelease);
        var baru = await Buat().Log.CreateAsync(Scan("GDB-01", "GDB00", Wib(_kemarin, 15)), _aktor, null, null);
        Assert.Equal(R.AwaitingDeviceRegistration, baru.Data!.ProcessingStatus);
    }

    // Mengubah identitas mesin aktif ke identitas yang selama ini ditahan juga melepasnya.
    [Fact]
    public async Task Mengubah_identitas_mesin_melepas_scan_tertahan()
    {
        await KirimLimaPuluhScanGdb01();
        var r = await Registri().CreateAsync(Baru("GDB-LAMA"), _aktor);
        Assert.Equal(0, r.Data!.HeldScanRelease!.HeldLogCount);
        var u = await Registri().UpdateAsync(r.Data.Id, Ubah("GDB-LAMA", external: "GDB-01"), _aktor);
        Assert.True(u.Success, u.Message);
        Assert.Equal(50, u.Data!.HeldScanRelease!.MatchedCount);
    }

    // VR-PK-26: kode, nomor seri, atau ID eksternal yang dipakai mesin aktif lain → 409; kode/serial unik juga terhadap mesin nonaktif.
    [Fact]
    public async Task Identitas_mesin_bentrok_ditolak_409()
    {
        var svc = Registri();
        Assert.True((await svc.CreateAsync(Baru("IGD-01", serial: "SN-1", external: "EXT-IGD"), _aktor)).Success);
        Assert.True((await Registri().CreateAsync(Baru("ICU-09", aktif: false), _aktor)).Success);

        foreach (var bentrok in new[]
        {
            Baru("IGD-01"),                          // kode sama
            Baru("X-1", serial: "SN-1"),             // nomor seri sama
            Baru("EXT-IGD"),                         // kode = ID eksternal mesin aktif lain
            Baru("X-2", external: "IGD-01"),         // ID eksternal = kode mesin aktif lain
            Baru("X-3", external: "SN-1"),           // ID eksternal = nomor seri mesin aktif lain
            Baru("ICU-09", aktif: false),            // kode sama dengan mesin nonaktif (indeks unik)
        })
        {
            var r = await Registri().CreateAsync(bentrok, _aktor);
            Assert.False(r.Success);
            Assert.Equal(409, r.StatusCode);
            Assert.Equal("Identitas mesin sudah dipakai mesin lain", r.Message);
        }

        // Mesin nonaktif ber-ID eksternal sama boleh disimpan, tetapi tidak boleh diaktifkan selama bentrok.
        var cadangan = await Registri().CreateAsync(Baru("X-4", aktif: false, external: "EXT-IGD"), _aktor);
        Assert.True(cadangan.Success, cadangan.Message);
        var aktif = await Registri().UpdateStatusAsync(cadangan.Data!.Id, true, _aktor);
        Assert.Equal(409, aktif.StatusCode);
    }

    [Fact]
    public async Task Validasi_masukan_mesin()
    {
        var lokasiRsLain = Guid.NewGuid();
        var rs = Guid.NewGuid();
        using (var ctx = _db.CreateContext())
        {
            ctx.Set<MstAttendanceLocation>().Add(new MstAttendanceLocation { Id = lokasiRsLain, AttendanceLocationCode = "L1", AttendanceLocationName = "Lobi", HospitalSiteId = Guid.NewGuid(), IsActive = true });
            ctx.MstHospitalSites.Add(new MstHospitalSite { Id = rs, SiteCode = "RS1", SiteName = "RS 1" });
            ctx.SaveChanges();
        }

        var kasus = new (CreateAttendanceDeviceRequest Req, string Pesan)[]
        {
            (Baru(" "), "Kode mesin wajib diisi."),
            (new CreateAttendanceDeviceRequest { AttendanceDeviceCode = "A", AttendanceDeviceName = "A", DeviceType = "Telepati" }, "Jenis mesin tidak valid."),
            (new CreateAttendanceDeviceRequest { AttendanceDeviceCode = "A", AttendanceDeviceName = "A", DeviceType = "Fingerprint", Port = 70000 }, "Port harus 1 sampai 65535."),
            (new CreateAttendanceDeviceRequest { AttendanceDeviceCode = "A", AttendanceDeviceName = "A", DeviceType = "Fingerprint", IpAddress = "999.1.1" }, "Alamat IP tidak valid."),
            (new CreateAttendanceDeviceRequest { AttendanceDeviceCode = "A", AttendanceDeviceName = "A", DeviceType = "Fingerprint", HospitalSiteId = rs, AttendanceLocationId = lokasiRsLain }, "Lokasi absensi tidak berada di rumah sakit yang dipilih."),
        };
        foreach (var (req, pesan) in kasus)
        {
            var r = await Registri().CreateAsync(req, _aktor);
            Assert.Equal(400, r.StatusCode);
            Assert.Equal(pesan, r.Message);
        }
        using var baca = _db.CreateContext();
        Assert.Empty(baca.Set<MstAttendanceDevice>().ToList());
    }

    // Hapus ditolak bila mesin sudah punya log; tanpa log → hapus lunak dan kode dapat dipakai lagi.
    [Fact]
    public async Task Hapus_mesin_ditolak_bila_sudah_punya_log()
    {
        await KirimLimaPuluhScanGdb01();
        var dipakai = await Registri().CreateAsync(Baru("GDB-01"), _aktor);
        var tolak = await Registri().DeleteAsync(dipakai.Data!.Id, _aktor);
        Assert.Equal(409, tolak.StatusCode);
        Assert.Contains("sudah punya log", tolak.Message);

        var kosong = await Registri().CreateAsync(Baru("CADANGAN"), _aktor);
        var hapus = await Registri().DeleteAsync(kosong.Data!.Id, _aktor);
        Assert.True(hapus.Success, hapus.Message);
        Assert.Null(await Registri().GetByIdAsync(kosong.Data.Id));
        using (var ctx = _db.CreateContext())
        {
            var m = ctx.Set<MstAttendanceDevice>().AsNoTracking().Single(x => x.Id == kosong.Data.Id);
            Assert.True(m.IsDelete);
            Assert.Equal(_aktor, m.DeleteBy);
        }
        Assert.True((await Registri().CreateAsync(Baru("CADANGAN"), _aktor)).Success);
        Assert.Equal(404, (await Registri().DeleteAsync(Guid.NewGuid(), _aktor)).StatusCode);
    }

    // Kegagalan pelepasan tidak membatalkan pendaftaran mesin (35.10); scan tetap tertahan dan dapat dipicu ulang.
    [Fact]
    public async Task Pelepasan_gagal_tidak_membatalkan_pendaftaran()
    {
        await KirimLimaPuluhScanGdb01();
        var r = await Registri(new GagalSimpan()).CreateAsync(Baru("GDB-01"), _aktor);
        Assert.True(r.Success, r.Message);
        Assert.Null(r.Data!.HeldScanRelease);
        Assert.NotNull(r.Data.HeldScanReleaseMessage);
        Assert.Equal(50, r.Data.HeldScanCount);
        Assert.All(Log(), x => Assert.Equal(R.AwaitingDeviceRegistration, x.ProcessingStatus));

        // Dipicu ulang dengan menonaktifkan lalu mengaktifkan.
        await Registri().UpdateStatusAsync(r.Data.Id, false, _aktor);
        var ulang = await Registri().UpdateStatusAsync(r.Data.Id, true, _aktor);
        Assert.Equal(50, ulang.Data!.HeldScanRelease!.MatchedCount);
    }

    private sealed class GagalSimpan : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<HrdAttendanceRawLog>().Any(e => e.State == EntityState.Modified))
                throw new InvalidOperationException("Galat uji saat menyimpan pelepasan");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
