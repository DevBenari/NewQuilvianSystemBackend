using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Workers;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-298 / AT-HRD-OHS-17 (VR-OHS-17) — dosis TLD diimpor bulanan (INT-HR-91); ambang dinilai KUMULATIF: triwulanan > 5 mSv
// (investigasi K3RS) dan tahunan > 20 mSv (rotasi wajib). Rekaman yang melampaui ditandai tidak patuh, radar memberi rekomendasi
// rotasi, dan worker OHS-W5 mengirim alarm kritis ke pegawai serta pemegang OccupationalMedicalExamination : RecordHazardSurveillance.
public sealed class RadiationDoseThresholdTests : IDisposable
{
    private static DateTime Tgl(int tahun, int bulan, int hari) => new(tahun, bulan, hari, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _dept = Guid.NewGuid(), _pos = Guid.NewGuid(), _deptK3 = Guid.NewGuid(), _posK3 = Guid.NewGuid();
    private readonly Guid _rinaWf = Guid.NewGuid(), _akunRina = Guid.NewGuid(), _petugasK3 = Guid.NewGuid();
    private Guid _rina;
    private int _urut;

    private sealed class HubTidakAda : IHubContext<HrNotificationHub>
    {
        public IHubClients Clients => throw new NotSupportedException("SignalR tidak dipakai di harness");
        public IGroupManager Groups => throw new NotSupportedException("SignalR tidak dipakai di harness");
    }

    private sealed class PabrikKonteks(TestDatabase db) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext();
    }

    public RadiationDoseThresholdTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstDepartments.AddRange(new MstDepartment { Id = _dept, DepartmentCode = "RAD", DepartmentName = "Radiologi" }, new MstDepartment { Id = _deptK3, DepartmentCode = "K3", DepartmentName = "K3RS" });
        ctx.MstPositions.AddRange(new MstPosition { Id = _pos, DepartmentId = _dept, PositionCode = "RGF", PositionName = "Radiografer" }, new MstPosition { Id = _posK3, DepartmentId = _deptK3, PositionCode = "PK3", PositionName = "Petugas K3RS" });
        _rina = Karyawan(ctx, _rinaWf, "Rina Radiografer");
        ctx.Users.AddRange(
            new ApplicationUser { Id = _akunRina, UserName = "rina", UserCode = "RINA0001", DisplayName = "Rina", UserType = UserType.Employee, IsActive = true, WorkforceProfileId = _rinaWf },
            new ApplicationUser { Id = _petugasK3, UserName = "k3", UserCode = "K3RS0001", DisplayName = "Petugas K3RS", UserType = UserType.Employee, IsActive = true, PrimaryDepartmentId = _deptK3, PrimaryPositionId = _posK3 });
        var c = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "OccupationalMedicalExamination", DisplayName = "Pemeriksaan Kesehatan Kerja" };
        var a = new SysActionAccess { ControllerAccessId = c.Id, ActionName = "RecordHazardSurveillance", DisplayName = "Catat Surveilans Bahaya", AccessType = "Update" };
        ctx.SysControllerAccesses.Add(c);
        ctx.SysActionAccesses.Add(a);
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = _deptK3, PositionId = _posK3, ControllerAccessId = c.Id, ActionAccessId = a.Id, IsAllowed = true, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private Guid Karyawan(ApplicationDbContext ctx, Guid wf, string nama)
    {
        var n = ++_urut;
        var e = new MstEmployee
        {
            WorkforceProfileId = wf, EmployeeCode = $"EMP-{n:000}", EmployeeNumber = $"NIP-{n:000}", FullName = nama, IdentityType = "KTP", IdentityNumber = $"3171{n:000000000000}",
            Email = $"k{n}@uji.local", BirthDate = Tgl(1990, 1, 1), JoinDate = Tgl(2020, 1, 1), PrimaryDepartmentId = _dept, PrimaryPositionId = _pos,
            WorkforceTypeId = Guid.NewGuid(), EmployeeCategoryId = Guid.NewGuid(), EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), IsActive = true,
        };
        ctx.MstEmployees.Add(e);
        return e.Id;
    }

    private HazardSurveillanceService Layanan() =>
        new(new OccupationalHealthUnitOfWork(_db.CreateContext()), new NumberSeriesAllocator(new PabrikKonteks(_db)));

    private Task<HealthSurveillanceResponseDto> Catat(int tahun, int bulan, decimal dosis) =>
        Layanan().RecordRadiationDoseAsync(new RecordRadiationDoseRequestDto
        {
            EmployeeId = _rina, PeriodStart = Tgl(tahun, bulan, 1), PeriodEnd = Tgl(tahun, bulan, 1).AddMonths(1).AddDays(-1), DoseMsv = dosis, BadgeNumber = "TLD-07",
        }, _petugasK3);

    private async Task JalankanWorkerSekali()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => _db.CreateContext());
        services.AddScoped<IHrdNotificationService>(sp => new HrdNotificationService(sp.GetRequiredService<ApplicationDbContext>(), new HubTidakAda(), NullLogger<HrdNotificationService>.Instance));
        await using var provider = services.BuildServiceProvider();
        using var cts = new CancellationTokenSource();
        var worker = new RadiationDoseThresholdAlertWorker(provider, NullLogger<RadiationDoseThresholdAlertWorker>.Instance);
        await worker.StartAsync(cts.Token);
        // Siklus pertama selesai sebelum worker menunggu 12 jam; tunggu sampai tidak ada pemberitahuan baru lagi.
        var sebelum = -1;
        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(250);
            var kini = Pemberitahuan().Count;
            if (kini == sebelum && i > 4) break;
            sebelum = kini;
        }
        cts.Cancel();
        await worker.StopAsync(CancellationToken.None);
    }

    private List<Areas.Corporate.HumanResource.HrServiceManagement.Models.HrdNotification> Pemberitahuan()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdNotifications.AsNoTracking().ToList();
    }

    // VR-OHS-17 (triwulanan): dua bacaan bulanan 3 + 3 mSv dalam satu triwulan = 6 mSv > 5 → bacaan kedua tidak patuh.
    // Sebelum perbaikan keduanya "Dalam batas aman" karena hanya satu bacaan > 5 mSv yang dinilai.
    [Fact]
    public async Task Kumulatif_triwulan_melebihi_5_mSv_dari_bacaan_bulanan()
    {
        var jan = await Catat(2026, 1, 3.0m);
        var feb = await Catat(2026, 2, 3.0m);
        Assert.True(jan.IsCompliant);
        Assert.Contains("Dalam batas aman.", jan.AdministrativeOutcome);
        Assert.False(feb.IsCompliant);
        Assert.Contains("kumulatif triwulanan 5 mSv; wajib investigasi K3RS", feb.AdministrativeOutcome);
        Assert.Contains("Kumulatif triwulan 1/2026: 6.0 mSv; tahun 2026: 6.0 mSv.", feb.AdministrativeOutcome);
        Assert.StartsWith("Dosis Tercatat: 3.0 mSv.", feb.AdministrativeOutcome);
    }

    // AT-HRD-OHS-17: kumulatif tahunan melewati 20 mSv → rekaman tidak patuh dengan pesan VR-OHS-17 (wajib dirotasi), radar
    // merekomendasikan rotasi keluar zona radiasi, dan worker OHS-W5 mengirim alarm kritis ke pegawai dan petugas K3RS, sekali saja.
    [Fact]
    public async Task OHS17_kumulatif_tahunan_di_atas_20_mSv_alarm_dan_rekomendasi_rotasi()
    {
        Assert.True((await Catat(2026, 2, 5.0m)).IsCompliant);   // Q1 = 5,0 (tidak > 5)
        Assert.True((await Catat(2026, 5, 5.0m)).IsCompliant);   // Q2
        Assert.True((await Catat(2026, 8, 5.0m)).IsCompliant);   // Q3 → tahun 15,0
        Assert.True((await Catat(2026, 10, 4.0m)).IsCompliant);  // Q4 4,0 → tahun 19,0
        var nov = await Catat(2026, 11, 1.5m);                   // Q4 5,5 dan tahun 20,5

        Assert.False(nov.IsCompliant);
        Assert.Contains("Batas Dosis Radiasi Terlampaui (Bapeten): Staf telah mencapai batas dosis paparan radiasi dan wajib dirotasi sementara", nov.AdministrativeOutcome);
        Assert.Contains("tahun 2026: 20.5 mSv", nov.AdministrativeOutcome);

        var radar = Assert.Single(await Layanan().GetRadiationRadarAsync(2026, null));
        Assert.Equal((20.5m, true, "RotasiWajibKeluarZonaRadiasi"), (radar.CumulativeYearlyDoseMsv, radar.IsOverYearlyThreshold, radar.RotationRecommendation));

        await JalankanWorkerSekali();
        var alarm = Pemberitahuan();
        Assert.Equal(new[] { _akunRina, _petugasK3 }.OrderBy(x => x), alarm.Select(x => x.RecipientUserId).OrderBy(x => x));
        Assert.All(alarm, x =>
        {
            Assert.Equal((HrdNotificationPriorities.Critical, HrdNotificationCategories.OccupationalHealth, nov.Id), (x.Priority, x.Category, x.ReferenceId));
            Assert.Matches(@"1[.,]5 mSv", x.Body);
            Assert.Contains("rotasi tugas", x.Body);
        });

        await JalankanWorkerSekali();
        Assert.Equal(2, Pemberitahuan().Count);
    }

    // Bacaan pada tahun berbeda tidak dijumlahkan; satu bacaan > 5 mSv tetap tidak patuh; catatan lama tanpa kolom dosis dibaca dari teks.
    [Fact]
    public async Task Batas_tahun_bacaan_tunggal_dan_catatan_lama()
    {
        Assert.True((await Catat(2025, 12, 4.0m)).IsCompliant);
        Assert.True((await Catat(2026, 1, 3.0m)).IsCompliant);    // tahun baru, triwulan baru: 3,0
        Assert.False((await Catat(2026, 4, 5.5m)).IsCompliant);   // satu bacaan > 5

        using (var ctx = _db.CreateContext())
        {
            ctx.TrxEmployeeHealthSurveillances.Add(new TrxEmployeeHealthSurveillance
            {
                WorkforceProfileId = _rinaWf, EmployeeId = _rina, SurveillanceNumber = "LAMA-001", SurveillanceProgram = "Pemantauan Dosis Radiasi Pekerja Radiasi (TLD Badge)",
                RiskFactor = "Ionizing Radiation", ScheduledDate = Tgl(2026, 7, 31), SurveillanceStatus = "Completed", MeasuredDoseMsv = null,
                AdministrativeOutcome = "Dosis Tercatat: 4,2 mSv. Badge: TLD-LAMA. Dalam batas aman.", IsCompliant = true, IsActive = true,
            });
            ctx.SaveChanges();
        }
        var agu = await Catat(2026, 8, 1.0m);                       // Q3: 4,2 (lama) + 1,0 = 5,2
        Assert.False(agu.IsCompliant);
        Assert.Contains("Kumulatif triwulan 3/2026: 5.2 mSv", agu.AdministrativeOutcome);
    }
}
