using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-453 — AT-HRD-PK-49 (HRD-DEC-241, HRD-DEC-245, kontrak v30 API 36.4, VR-PK-32 s.d. VR-PK-34, VR-PK-36, state 34.1,
// INT-HR-115): Sari (perawat ICU) mengajukan izin pulang cepat dari Layanan Mandiri; Dodi (Kepala Ruang) memutuskan di layar HR.
public sealed class EarlyLeaveSelfServiceTests : IDisposable
{
    private const string AlasanIzin = "Anak sakit, harus menjemput ke dokter";

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly PenangkapLog _log = new();
    private readonly Guid _icu = Guid.NewGuid();
    private readonly Guid _uSari = Guid.NewGuid(), _uDodi = Guid.NewGuid(), _uTanpaProfil = Guid.NewGuid(), _uNonaktif = Guid.NewGuid();
    private readonly Guid _sari = Guid.NewGuid(), _dodi = Guid.NewGuid(), _raka = Guid.NewGuid(), _nonaktif = Guid.NewGuid();
    private readonly Guid _hSari = Guid.NewGuid(), _hSariTutup = Guid.NewGuid(), _hSariKunci = Guid.NewGuid(), _hDodi = Guid.NewGuid(), _hRaka = Guid.NewGuid();

    private static readonly DateOnly T10 = new(2027, 2, 10), T11 = new(2027, 2, 11), T12 = new(2027, 2, 12), T20 = new(2027, 3, 20);

    // Jam dalam UTC; shift pagi 07.00–14.00 WIB = 00.00–07.00 UTC.
    private static DateTime Jam(DateOnly d, int jamWib) => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(jamWib - 7);

    public EarlyLeaveSelfServiceTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        foreach (var (id, nama, aktif) in new[] { (_sari, "Sari", true), (_dodi, "Dodi", true), (_raka, "Raka", true), (_nonaktif, "Lama", false) })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = "WFP-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = aktif });
        }

        ApplicationUser Akun(Guid id, Guid? profil) => new()
        { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = UserType.Employee, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_uSari, _sari), Akun(_uDodi, _dodi), Akun(_uTanpaProfil, null), Akun(_uNonaktif, _nonaktif));

        HrdAttendanceDaily Harian(Guid id, Guid pegawai, DateOnly tanggal, bool terkunci = false) => new()
        {
            Id = id, UserId = Guid.NewGuid(), WorkforceProfileId = pegawai, UserType = UserType.Employee, OrganizationUnitId = _icu,
            AttendanceDate = tanggal, AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Present, IsPresent = true,
            ScheduledCheckInAt = Jam(tanggal, 7), ScheduledCheckOutAt = Jam(tanggal, 14), IsLocked = terkunci,
            ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed, IsActive = true,
        };
        ctx.HrdAttendanceDailies.AddRange(
            Harian(_hSari, _sari, T10), Harian(_hSariKunci, _sari, T11, terkunci: true), Harian(_hSariTutup, _sari, T20),
            Harian(_hDodi, _dodi, T10), Harian(_hRaka, _raka, T10));

        // Periode Maret ditutup; 20 Maret ada di dalamnya.
        ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod
        {
            PeriodCode = "ATP-2027-03", PeriodName = "Maret 2027", StartDate = new DateOnly(2027, 3, 1), EndDate = new DateOnly(2027, 3, 31),
            OrganizationUnitId = _icu, PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Closed,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private EarlyLeavePermissionService Layanan(ApplicationDbContext? ctx = null) =>
        new(ctx ?? _db.CreateContext(), NullLogger<EarlyLeavePermissionService>.Instance);

    private EarlyLeavePermissionSelfServiceController Ess(Guid userId)
    {
        var ctx = _db.CreateContext();
        var akses = Pelaku.Sebagai(userId);
        var userManager = new UserManager<ApplicationUser>(
            new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!, NullLogger<UserManager<ApplicationUser>>.Instance);
        return new EarlyLeavePermissionSelfServiceController(Layanan(ctx), new HumanResourceContextService(ctx, akses, userManager), new LoggerService(_log, akses))
        {
            ControllerContext = new ControllerContext { HttpContext = akses.HttpContext! }
        };
    }

    private static (int Status, T? Data, string Message) Baca<T>(IActionResult hasil)
    {
        var o = Assert.IsAssignableFrom<ObjectResult>(hasil);
        return o.Value switch
        {
            ApiResponse<T> a => (o.StatusCode ?? 200, a.Data, a.Message),
            ApiResponse<object> f => (o.StatusCode ?? 200, default, f.Message),
            _ => throw new InvalidOperationException("Tipe respons tidak dikenal")
        };
    }

    private async Task<(int Status, EarlyLeaveSelfResponse? Data, string Message)> Ajukan(Guid userId, DateOnly tanggal, int jamWib = 12, string alasan = AlasanIzin) =>
        Baca<EarlyLeaveSelfResponse>(await Ess(userId).Create(
            new CreateEarlyLeaveSelfRequest { AttendanceDate = tanggal, RequestedEarlyLeaveAt = Jam(tanggal, jamWib), Reason = alasan }));

    private async Task<(int Status, EarlyLeaveSelfResponse? Data, string Message)> Batal(Guid userId, Guid id, string alasan = "Anak sudah sembuh") =>
        Baca<EarlyLeaveSelfResponse>(await Ess(userId).Cancel(id, new CancelSelfRequest { Reason = alasan }));

    private HrdEarlyLeavePermission Izin(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdEarlyLeavePermissions.AsNoTracking().Single(x => x.Id == id);
    }

    private Guid? TautanHarian(Guid harian)
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendanceDailies.AsNoTracking().Single(x => x.Id == harian).EarlyLeavePermissionId;
    }

    // AC1: workforceProfileId pegawai lain di body diabaikan; izin tercatat atas profil sesi dan tampil di daftar HR.
    [Fact]
    public async Task Ajukan_tercatat_atas_profil_sesi_dan_tampil_di_daftar_HR()
    {
        var json = $$"""{"workforceProfileId":"{{_raka}}","attendanceDailyId":"{{_hRaka}}","attendanceDate":"2027-02-10","requestedEarlyLeaveAt":"{{Jam(T10, 12):O}}","reason":"{{AlasanIzin}}"}""";
        var body = JsonSerializer.Deserialize<CreateEarlyLeaveSelfRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        var (status, data, pesan) = Baca<EarlyLeaveSelfResponse>(await Ess(_uSari).Create(body));
        Assert.True(status == 200, pesan);
        Assert.Equal(T10, data!.AttendanceDate);
        Assert.Equal("Submitted", data.Status);
        Assert.Equal("Diajukan", data.StatusLabel);
        Assert.Equal(AlasanIzin, data.Reason);

        var izin = Izin(data.Id);
        Assert.Equal(_sari, izin.WorkforceProfileId);
        Assert.Equal(_hSari, izin.AttendanceDailyId);
        Assert.Equal(_uSari, izin.CreateBy);
        Assert.Equal(data.Id, TautanHarian(_hSari));
        Assert.Null(TautanHarian(_hRaka));

        var hr = await Layanan().GetPagedAsync(new EarlyLeavePermissionQueryRequest());
        var baris = Assert.Single(hr.Items);
        Assert.Equal(data.Id, baris.Id);
        Assert.Equal("Sari", baris.EmployeeName);

        // Log SelfCreate: Id, tanggal, jam izin; alasan tidak dicatat.
        var log = Assert.Single(_log.Pesan, x => x.Contains("EarlyLeavePermission.SelfCreate"));
        Assert.Contains(data.Id.ToString(), log);
        Assert.Contains("2027-02-10", log);
        Assert.Contains("jam izin", log);
        Assert.DoesNotContain("Anak sakit", log);
    }

    // AC2: tanpa rekap → 400; periode tertutup atau rekap terkunci → 409; akun tanpa profil (atau profil nonaktif) → 404.
    [Fact]
    public async Task Ajukan_ditolak_bila_rekap_tidak_ada_periode_tertutup_atau_tanpa_profil()
    {
        var tanpaRekap = await Ajukan(_uSari, T12);
        Assert.Equal(400, tanpaRekap.Status);
        Assert.Equal("Belum ada jadwal atau rekap kehadiran Anda pada tanggal ini.", tanpaRekap.Message);

        var tutup = await Ajukan(_uSari, T20);
        Assert.Equal(409, tutup.Status);
        Assert.Equal("Periode kehadiran tanggal ini sudah ditutup.", tutup.Message);

        var kunci = await Ajukan(_uSari, T11);
        Assert.Equal(409, kunci.Status);
        Assert.Equal("Periode kehadiran tanggal ini sudah ditutup.", kunci.Message);

        foreach (var akun in new[] { _uTanpaProfil, _uNonaktif })
        {
            var (s, _, pesan) = await Ajukan(akun, T10);
            Assert.Equal(404, s);
            Assert.Equal("Akun ini belum terhubung ke profil pegawai.", pesan);
            Assert.Equal(404, Baca<PagedResult<EarlyLeaveSelfResponse>>(await Ess(akun).GetMine(new EarlyLeaveSelfQuery())).Status);
        }

        // Aturan CreateAsync yang ada tetap berlaku: alasan wajib, jam tidak sebelum jam masuk, tidak ganda.
        Assert.Equal(400, (await Ajukan(_uSari, T10, alasan: "  ")).Status);
        Assert.Equal(400, (await Ajukan(_uSari, T10, alasan: new string('x', 501))).Status);
        Assert.Equal(400, (await Ajukan(_uSari, T10, jamWib: 6)).Status);
        Assert.Equal(200, (await Ajukan(_uSari, T10)).Status);
        Assert.Equal(409, (await Ajukan(_uSari, T10)).Status);

        using var ctx = _db.CreateContext();
        Assert.Equal(1, ctx.HrdEarlyLeavePermissions.Count());
    }

    // AC3: batal Submitted → Cancelled; batal kedua dan sesudah Approved → 409; milik orang lain → 404; tanpa alasan → 400.
    [Fact]
    public async Task Batal_hanya_milik_sendiri_yang_masih_diajukan()
    {
        var id = (await Ajukan(_uSari, T10)).Data!.Id;

        var tanpaAlasan = await Batal(_uSari, id, " ");
        Assert.Equal(400, tanpaAlasan.Status);
        Assert.Equal("Alasan pembatalan wajib diisi.", tanpaAlasan.Message);
        Assert.Equal("Submitted", Izin(id).Status);

        Assert.Equal(404, (await Batal(_uDodi, id)).Status);   // bukan milik Dodi
        Assert.Equal("Submitted", Izin(id).Status);

        var (status, data, pesan) = await Batal(_uSari, id);
        Assert.True(status == 200, pesan);
        Assert.Equal("Cancelled", data!.Status);
        Assert.Equal("Dibatalkan", data.StatusLabel);
        Assert.NotNull(data.CancelDateTime);
        var izin = Izin(id);
        Assert.Equal("Cancelled", izin.Status);
        Assert.True(izin.IsCancel);
        Assert.Equal(_uSari, izin.CancelBy);
        Assert.NotNull(izin.CancelDateTime);
        Assert.Null(TautanHarian(_hSari));   // rekap harian tidak lagi menunjuk izin yang dibatalkan

        var log = Assert.Single(_log.Pesan, x => x.Contains("EarlyLeavePermission.SelfCancel"));
        Assert.Contains(id.ToString(), log);
        Assert.Contains("status sebelum Submitted", log);
        Assert.DoesNotContain("sembuh", log);

        var kedua = await Batal(_uSari, id);
        Assert.Equal(409, kedua.Status);
        Assert.Equal("Pengajuan sudah diputuskan dan tidak dapat dibatalkan.", kedua.Message);

        // Sesudah dibatalkan, hari yang sama boleh diajukan lagi; yang disetujui tidak bisa dibatalkan.
        var baru = (await Ajukan(_uSari, T10)).Data!.Id;
        var setuju = await Layanan().ApproveAsync(baru, new ApproveEarlyLeavePermissionRequest { ApproverWorkforceProfileId = _dodi }, _uDodi);
        Assert.True(setuju.Success, setuju.Message);
        Assert.Equal(409, (await Batal(_uSari, baru)).Status);
        Assert.Equal("Approved", Izin(baru).Status);
    }

    // Daftar dan detail hanya milik sendiri; saringan status dan batas 100 baris.
    [Fact]
    public async Task Daftar_dan_detail_hanya_milik_sendiri()
    {
        var milikSari = (await Ajukan(_uSari, T10)).Data!.Id;
        var milikDodi = (await Ajukan(_uDodi, T10)).Data!.Id;

        var (_, daftar, _) = Baca<PagedResult<EarlyLeaveSelfResponse>>(await Ess(_uSari).GetMine(new EarlyLeaveSelfQuery { PageSize = 500 }));
        Assert.Equal(milikSari, Assert.Single(daftar!.Items).Id);
        Assert.Equal(100, daftar.PageSize);

        Assert.Equal(200, Baca<EarlyLeaveSelfResponse>(await Ess(_uSari).GetMineById(milikSari)).Status);
        var (s, _, pesan) = Baca<EarlyLeaveSelfResponse>(await Ess(_uSari).GetMineById(milikDodi));
        Assert.Equal(404, s);
        Assert.Equal("Permohonan izin pulang cepat tidak ditemukan.", pesan);

        Assert.Empty(Baca<PagedResult<EarlyLeaveSelfResponse>>(await Ess(_uSari).GetMine(new EarlyLeaveSelfQuery { Status = "Approved" })).Data!.Items);
        Assert.Single(Baca<PagedResult<EarlyLeaveSelfResponse>>(await Ess(_uSari).GetMine(new EarlyLeaveSelfQuery { Status = "Submitted", FromDate = T10, ToDate = T10 })).Data!.Items);

        var meta = Assert.IsType<OkObjectResult>(Ess(_uSari).GetFilterMetadata());
        var opsi = Assert.IsType<ApiResponse<EarlyLeaveSelfFilterMetadataResponse>>(meta.Value).Data!.StatusOptions;
        Assert.Equal(new[] { "Submitted", "Approved", "Rejected", "Cancelled" }, opsi.Select(x => x.Value));
    }

    // AC4: Kepala Ruang tidak dapat menyetujui atau menolak izinnya sendiri (identitas dari akun, bukan body); izin pegawai lain tetap bisa.
    [Fact]
    public async Task Pemohon_tidak_dapat_memutuskan_izinnya_sendiri()
    {
        var milikDodi = (await Ajukan(_uDodi, T10)).Data!.Id;
        var milikSari = (await Ajukan(_uSari, T10)).Data!.Id;

        // Body menyebut penyetuju lain; tetap ditolak karena akun login adalah pemilik pengajuan.
        var setujuSendiri = await Layanan().ApproveAsync(milikDodi, new ApproveEarlyLeavePermissionRequest { ApproverWorkforceProfileId = _raka }, _uDodi);
        Assert.False(setujuSendiri.Success);
        Assert.Equal(403, setujuSendiri.StatusCode);
        Assert.Equal("Anda tidak dapat memutuskan pengajuan Anda sendiri.", setujuSendiri.Message);

        var tolakSendiri = await Layanan().RejectAsync(milikDodi, new RejectEarlyLeavePermissionRequest { ApproverWorkforceProfileId = _raka, RejectionReason = "Uji" }, _uDodi);
        Assert.Equal(403, tolakSendiri.StatusCode);
        Assert.Equal("Submitted", Izin(milikDodi).Status);
        Assert.Null(Izin(milikDodi).ApprovedAt);

        var setujuLain = await Layanan().ApproveAsync(milikSari, new ApproveEarlyLeavePermissionRequest { ApproverWorkforceProfileId = _dodi }, _uDodi);
        Assert.True(setujuLain.Success, setujuLain.Message);
        Assert.Equal("Approved", Izin(milikSari).Status);

        // Penolakan izin pegawai lain juga tetap bisa; akun tanpa profil (mis. superadmin HR) tidak terhalang.
        var milikRaka = (await Layanan().CreateAsync(new CreateEarlyLeavePermissionRequest { AttendanceDailyId = _hRaka, WorkforceProfileId = _raka, RequestedEarlyLeaveAt = Jam(T10, 12), Reason = "Urusan keluarga" }, _uTanpaProfil)).Data!.Id;
        var tolakLain = await Layanan().RejectAsync(milikRaka, new RejectEarlyLeavePermissionRequest { ApproverWorkforceProfileId = _dodi, RejectionReason = "Jadwal penuh" }, _uTanpaProfil);
        Assert.True(tolakLain.Success, tolakLain.Message);
    }

    // Hak akses: lima endpoint dijaga MyEarlyLeavePermission : Read/Create/Cancel di modul Layanan Mandiri; validator bersih.
    [Fact]
    public async Task Hak_akses_MyEarlyLeavePermission_terdaftar_dan_ditegakkan()
    {
        var izin = typeof(EarlyLeavePermissionSelfServiceController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(m => m.GetCustomAttribute<AccessPermissionAttribute>())
            .Where(a => a != null)
            .Select(a => $"{a!.Arguments![0]}:{a.Arguments[1]}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(new[] { "MyEarlyLeavePermission:Cancel", "MyEarlyLeavePermission:Create", "MyEarlyLeavePermission:Read", "MyEarlyLeavePermission:Read", "MyEarlyLeavePermission:Read" }, izin);

        var snapshot = PermissionRegistryDescriptor.BuildFromAssembly(typeof(EarlyLeavePermissionSelfServiceController).Assembly);
        foreach (var aksi in new[] { "Read", "Create", "Cancel" })
        {
            Assert.Contains(snapshot.Actions, x => x.ModuleCode == "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE" && x.ResourceName == "MyEarlyLeavePermission" && x.ActionName == aksi);
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(EarlyLeavePermissionSelfServiceController).Assembly);
        using var provider = services.BuildServiceProvider();
        var hasil = new PermissionRegistryValidator(provider.GetRequiredService<IActionDescriptorCollectionProvider>(), NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);
        var temuan = hasil.UnregisterableEndpoints.Concat(hasil.DuplicateResourceIdentities).Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints).Concat(hasil.NakedBusinessEndpoints)
            .Where(x => x.StartsWith("MyEarlyLeavePermission.", StringComparison.Ordinal) || x.Contains("'MyEarlyLeavePermission'", StringComparison.Ordinal))
            .ToList();
        Assert.True(temuan.Count == 0, string.Join(Environment.NewLine, temuan));

        using var ctx = _db.CreateContext();
        Assert.False(await Pelaku.Izin(ctx).HasAccessAsync(Pelaku.Sebagai(_uSari).HttpContext!.User, "MyEarlyLeavePermission", "Create"));
    }

    private sealed class PenangkapLog : Microsoft.Extensions.Logging.ILogger<LoggerService>
    {
        public List<string> Pesan { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Pesan.Add(formatter(state, exception));
    }
}
