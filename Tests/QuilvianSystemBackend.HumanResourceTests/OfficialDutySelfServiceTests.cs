using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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
using QuilvianSystemBackend.Shared.HumanResource.Helpers;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-467 — AT-HRD-PK-50 (HRD-DEC-252, HRD-DEC-256, kontrak v31 API 37.5, VR-PK-33, VR-PK-34, VR-PK-36, state 35.1):
// Sari (perawat ICU) mengajukan surat tugas dinas luar (kawal ambulans rujukan) dari Layanan Mandiri beserta lampiran surat tugas;
// Dodi (Kepala Ruang) memutuskan di layar HR.
public sealed class OfficialDutySelfServiceTests : IDisposable
{
    private const string Tujuan = "RSUP Dr. Sardjito";
    private const string Uraian = "Mendampingi pasien rujukan ICU";

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly PenangkapLog _log = new();
    private readonly string _akar = Path.Combine(Path.GetTempPath(), "uji-be-hrd-467-" + Guid.NewGuid().ToString("N"));
    private readonly Guid _uSari = Guid.NewGuid(), _uDodi = Guid.NewGuid(), _uTanpaProfil = Guid.NewGuid(), _uNonaktif = Guid.NewGuid();
    private readonly Guid _sari = Guid.NewGuid(), _dodi = Guid.NewGuid(), _raka = Guid.NewGuid(), _nonaktif = Guid.NewGuid();

    // Jam dalam UTC; 08.00–16.00 WIB = 01.00–09.00 UTC.
    private static DateTime Jam(int bulan, int tanggal, int jamWib) => new DateTime(2027, bulan, tanggal, 0, 0, 0, DateTimeKind.Utc).AddHours(jamWib - 7);

    public OfficialDutySelfServiceTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = Guid.NewGuid(), LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        foreach (var (id, nama, aktif) in new[] { (_sari, "Sari", true), (_dodi, "Dodi", true), (_raka, "Raka", true), (_nonaktif, "Lama", false) })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = "WFP-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = aktif });
        }

        ApplicationUser Akun(Guid id, Guid? profil) => new()
        { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = UserType.Employee, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_uSari, _sari), Akun(_uDodi, _dodi), Akun(_uTanpaProfil, null), Akun(_uNonaktif, _nonaktif));

        // Periode Maret ditutup.
        ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod
        {
            PeriodCode = "ATP-2027-03", PeriodName = "Maret 2027", StartDate = new DateOnly(2027, 3, 1), EndDate = new DateOnly(2027, 3, 31),
            PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Closed,
        });
        ctx.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        if (Directory.Exists(_akar)) Directory.Delete(_akar, recursive: true);
    }

    private OfficialDutyRequestService Layanan(ApplicationDbContext? ctx = null) =>
        new(ctx ?? _db.CreateContext(), NullLogger<OfficialDutyRequestService>.Instance);

    private HrAttachmentStorage Penyimpanan() =>
        new(new LingkunganWebUji { ContentRootPath = _akar }, new ConfigurationBuilder().AddInMemoryCollection().Build());

    private OfficialDutySelfServiceController Ess(Guid userId)
    {
        var ctx = _db.CreateContext();
        var akses = Pelaku.Sebagai(userId);
        var userManager = new UserManager<ApplicationUser>(
            new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!, NullLogger<UserManager<ApplicationUser>>.Instance);
        return new OfficialDutySelfServiceController(Layanan(ctx), new HumanResourceContextService(ctx, akses, userManager), Penyimpanan(), new LoggerService(_log, akses))
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

    private static CreateOfficialDutySelfRequest Permintaan(int bulan = 2, string kategori = AttendanceValueConstants.OfficialDutyCategory.AmbulanceReferral, string? lampiran = null) => new()
    {
        DutyCategory = kategori,
        AssignmentOrderNumber = "ST/ICU/2027/014",
        StartAt = Jam(bulan, 10, 8),
        EndAt = Jam(bulan, 10, 16),
        Destination = Tujuan,
        Purpose = Uraian,
        AttachmentUrl = lampiran
    };

    private async Task<(int Status, OfficialDutySelfResponse? Data, string Message)> Ajukan(Guid userId, CreateOfficialDutySelfRequest? permintaan = null) =>
        Baca<OfficialDutySelfResponse>(await Ess(userId).Create(permintaan ?? Permintaan()));

    private async Task<(int Status, OfficialDutySelfResponse? Data, string Message)> Batal(Guid userId, Guid id, string alasan = "Rujukan dibatalkan rumah sakit tujuan") =>
        Baca<OfficialDutySelfResponse>(await Ess(userId).Cancel(id, new CancelSelfRequest { Reason = alasan }));

    private async Task<(int Status, HrStoredAttachment? Data, string Message)> Unggah(Guid userId, string nama = "surat-tugas.pdf") =>
        Baca<HrStoredAttachment>(await Ess(userId).UploadAttachment(BerkasUji(nama)));

    private static IFormFile BerkasUji(string nama)
    {
        var isi = Encoding.UTF8.GetBytes("%PDF-1.4 surat tugas uji");
        return new FormFile(new MemoryStream(isi), 0, isi.Length, "file", nama);
    }

    private HrdOfficialDutyRequest Surat(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.Set<HrdOfficialDutyRequest>().AsNoTracking().Single(x => x.Id == id);
    }

    // AC1: unggah 201; ajukan 200 atas profil sesi (workforceProfileId di body diabaikan); tampil di daftar HR.
    [Fact]
    public async Task Unggah_dan_ajukan_tercatat_atas_profil_sesi_dan_tampil_di_daftar_HR()
    {
        var (sUnggah, lampiran, pesanUnggah) = await Unggah(_uSari);
        Assert.True(sUnggah == 201, pesanUnggah);
        Assert.StartsWith("attendance-official-duty/", lampiran!.Key);
        Assert.Equal(_uSari, HrAttachmentStorage.OwnerOf(lampiran.Key));

        var json = $$"""{"workforceProfileId":"{{_raka}}","dutyCategory":"AmbulanceReferral","assignmentOrderNumber":"ST/ICU/2027/014","startAt":"{{Jam(2, 10, 8):O}}","endAt":"{{Jam(2, 10, 16):O}}","destination":"{{Tujuan}}","purpose":"{{Uraian}}","attachmentUrl":"{{lampiran.Key}}"}""";
        var body = JsonSerializer.Deserialize<CreateOfficialDutySelfRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        var (status, data, pesan) = Baca<OfficialDutySelfResponse>(await Ess(_uSari).Create(body));
        Assert.True(status == 200, pesan);
        Assert.Equal("Submitted", data!.Status);
        Assert.Equal("Diajukan", data.StatusLabel);
        Assert.Equal("AmbulanceReferral", data.DutyCategory);
        Assert.Equal(lampiran.Key, data.AttachmentUrl);
        Assert.Null(data.CancelDateTime);

        var surat = Surat(data.Id);
        Assert.Equal(_sari, surat.WorkforceProfileId);
        Assert.Equal(_uSari, surat.CreateBy);

        var hr = await Layanan().GetPagedAsync(new OfficialDutyQueryRequest());
        var baris = Assert.Single(hr.Items);
        Assert.Equal(data.Id, baris.Id);
        Assert.Equal("Sari", baris.EmployeeName);

        // Log SelfCreate: Id dan rentang; tujuan dan uraian tugas tidak dicatat.
        var log = Assert.Single(_log.Pesan, x => x.Contains("OfficialDuty.SelfCreate"));
        Assert.Contains(data.Id.ToString(), log);
        Assert.Contains("2027-02-10", log);
        Assert.DoesNotContain("Sardjito", log);
        Assert.DoesNotContain("rujukan ICU", log);
    }

    // AC1/AC3 (validasi): kategori di luar metadata, lampiran bukan unggahan sendiri, rentang terbalik → 400; periode tertutup → 409;
    // akun tanpa profil atau profil nonaktif → 404 di semua endpoint.
    [Fact]
    public async Task Ajukan_ditolak_bila_isian_tidak_sah_periode_tertutup_atau_tanpa_profil()
    {
        var kategori = await Ajukan(_uSari, Permintaan(kategori: "Piknik"));
        Assert.Equal(400, kategori.Status);
        Assert.Equal("Kategori dinas luar tidak dikenal. Pilih salah satu kategori yang tersedia.", kategori.Message);

        var lampiranDodi = (await Unggah(_uDodi)).Data!.Key;
        var bukanMilik = await Ajukan(_uSari, Permintaan(lampiran: lampiranDodi));
        Assert.Equal(400, bukanMilik.Status);
        Assert.Equal("Lampiran surat tugas harus berkas yang Anda unggah sendiri.", bukanMilik.Message);
        Assert.Equal(400, (await Ajukan(_uSari, Permintaan(lampiran: "https://contoh.test/surat.pdf"))).Status);
        // Unggahan Sari sendiri tetapi dari kategori lampiran lain (mis. kuitansi perjalanan) tetap ditolak.
        var lampiranSari = (await Unggah(_uSari)).Data!.Key;
        var kategoriLain = await Ajukan(_uSari, Permintaan(lampiran: lampiranSari.Replace("attendance-official-duty/", "travel-receipt/")));
        Assert.Equal(400, kategoriLain.Status);
        Assert.Equal("Lampiran surat tugas harus berkas yang Anda unggah sendiri.", kategoriLain.Message);

        var terbalik = Permintaan();
        terbalik.EndAt = terbalik.StartAt;
        Assert.Equal(400, (await Ajukan(_uSari, terbalik)).Status);
        var tanpaNomor = Permintaan();
        tanpaNomor.AssignmentOrderNumber = " ";
        Assert.Equal(400, (await Ajukan(_uSari, tanpaNomor)).Status);
        var nomorPanjang = Permintaan();
        nomorPanjang.AssignmentOrderNumber = new string('9', 101);
        Assert.Equal(400, (await Ajukan(_uSari, nomorPanjang)).Status);

        Assert.Equal(409, (await Ajukan(_uSari, Permintaan(bulan: 3))).Status);

        foreach (var akun in new[] { _uTanpaProfil, _uNonaktif })
        {
            var (s, _, pesan) = await Ajukan(akun);
            Assert.Equal(404, s);
            Assert.Equal("Akun ini belum terhubung ke profil pegawai.", pesan);
            Assert.Equal(404, Baca<PagedResult<OfficialDutySelfResponse>>(await Ess(akun).GetMine(new OfficialDutySelfQuery())).Status);
            Assert.Equal(404, (await Unggah(akun)).Status);
            Assert.Equal(404, Baca<object>(await Ess(akun).DownloadAttachment(lampiranDodi)).Status);
        }

        // Lampiran yang sah diterima; tanpa lampiran juga diterima.
        var milikSari = (await Unggah(_uSari)).Data!.Key;
        Assert.Equal(200, (await Ajukan(_uSari, Permintaan(lampiran: milikSari))).Status);
        Assert.Equal(200, (await Ajukan(_uSari)).Status);

        using var ctx = _db.CreateContext();
        Assert.Equal(2, ctx.Set<HrdOfficialDutyRequest>().Count());
    }

    // AC2: unduh lampiran pengajuan sendiri 200; lampiran pengajuan pegawai lain, unggahan yang belum dilampirkan, atau kunci asal → 404.
    [Fact]
    public async Task Unduh_hanya_lampiran_pengajuan_sendiri()
    {
        var kunciSari = (await Unggah(_uSari)).Data!.Key;
        await Ajukan(_uSari, Permintaan(lampiran: kunciSari));
        var kunciDodi = (await Unggah(_uDodi)).Data!.Key;
        await Ajukan(_uDodi, Permintaan(lampiran: kunciDodi));
        var belumDilampirkan = (await Unggah(_uSari)).Data!.Key;

        var berkas = Assert.IsType<PhysicalFileResult>(await Ess(_uSari).DownloadAttachment(kunciSari));
        Assert.Equal("application/pdf", berkas.ContentType);
        Assert.True(File.Exists(berkas.FileName));

        foreach (var kunci in new[] { kunciDodi, belumDilampirkan, "attendance-official-duty/../../rahasia.pdf", "", null })
        {
            var (s, _, pesan) = Baca<object>(await Ess(_uSari).DownloadAttachment(kunci));
            Assert.Equal(404, s);
            Assert.Equal("Lampiran tidak ditemukan.", pesan);
        }

        // Dodi dapat mengunduh lampirannya sendiri, tetapi bukan milik Sari.
        Assert.IsType<PhysicalFileResult>(await Ess(_uDodi).DownloadAttachment(kunciDodi));
        Assert.Equal(404, Baca<object>(await Ess(_uDodi).DownloadAttachment(kunciSari)).Status);
    }

    // AC3: batal Submitted → Cancelled (VR-PK-33); batal kedua dan sesudah Approved → 409; milik orang lain → 404; tanpa alasan → 400.
    [Fact]
    public async Task Batal_hanya_milik_sendiri_yang_masih_diajukan()
    {
        var id = (await Ajukan(_uSari)).Data!.Id;

        var tanpaAlasan = await Batal(_uSari, id, " ");
        Assert.Equal(400, tanpaAlasan.Status);
        Assert.Equal("Alasan pembatalan wajib diisi.", tanpaAlasan.Message);
        Assert.Equal(400, (await Batal(_uSari, id, new string('x', 501))).Status);
        Assert.Equal("Submitted", Surat(id).Status);

        Assert.Equal(404, (await Batal(_uDodi, id)).Status);   // bukan milik Dodi
        Assert.Equal(404, (await Batal(_uTanpaProfil, id)).Status);
        Assert.Equal("Submitted", Surat(id).Status);

        var (status, data, pesan) = await Batal(_uSari, id);
        Assert.True(status == 200, pesan);
        Assert.Equal("Cancelled", data!.Status);
        Assert.Equal("Dibatalkan", data.StatusLabel);
        Assert.NotNull(data.CancelDateTime);
        var surat = Surat(id);
        Assert.Equal("Cancelled", surat.Status);
        Assert.True(surat.IsCancel);
        Assert.Equal(_uSari, surat.CancelBy);
        Assert.NotNull(surat.CancelDateTime);

        var log = Assert.Single(_log.Pesan, x => x.Contains("OfficialDuty.SelfCancel"));
        Assert.Contains(id.ToString(), log);
        Assert.Contains("2027-02-10", log);
        Assert.Contains("status sebelum Submitted", log);
        Assert.DoesNotContain("dibatalkan rumah sakit", log);

        var kedua = await Batal(_uSari, id);
        Assert.Equal(409, kedua.Status);
        Assert.Equal("Pengajuan sudah diputuskan dan tidak dapat dibatalkan.", kedua.Message);

        // Yang sudah disetujui tidak dapat dibatalkan dan status kehadirannya tetap.
        var baru = (await Ajukan(_uSari)).Data!.Id;
        var setuju = await Layanan().ApproveAsync(baru, new ApproveOfficialDutyRequest(), _uDodi);
        Assert.True(setuju.Success, setuju.Message);
        Assert.Equal(409, (await Batal(_uSari, baru)).Status);
        Assert.Equal("Approved", Surat(baru).Status);
        Assert.False(Surat(baru).IsCancel);
    }

    // Daftar dan detail hanya milik sendiri; saringan status/tanggal; batas 100 baris; metadata kategori dan status.
    [Fact]
    public async Task Daftar_detail_dan_metadata_hanya_milik_sendiri()
    {
        var milikSari = (await Ajukan(_uSari)).Data!.Id;
        var milikDodi = (await Ajukan(_uDodi)).Data!.Id;

        var (_, daftar, _) = Baca<PagedResult<OfficialDutySelfResponse>>(await Ess(_uSari).GetMine(new OfficialDutySelfQuery { PageSize = 500 }));
        Assert.Equal(milikSari, Assert.Single(daftar!.Items).Id);
        Assert.Equal(100, daftar.PageSize);

        Assert.Equal(200, Baca<OfficialDutySelfResponse>(await Ess(_uSari).GetMineById(milikSari)).Status);
        var (s, _, pesan) = Baca<OfficialDutySelfResponse>(await Ess(_uSari).GetMineById(milikDodi));
        Assert.Equal(404, s);
        Assert.Equal("Surat penugasan dinas luar tidak ditemukan.", pesan);

        Assert.Empty(Baca<PagedResult<OfficialDutySelfResponse>>(await Ess(_uSari).GetMine(new OfficialDutySelfQuery { Status = "Approved" })).Data!.Items);
        Assert.Empty(Baca<PagedResult<OfficialDutySelfResponse>>(await Ess(_uSari).GetMine(new OfficialDutySelfQuery { FromDate = new DateOnly(2027, 2, 11) })).Data!.Items);
        Assert.Single(Baca<PagedResult<OfficialDutySelfResponse>>(await Ess(_uSari).GetMine(new OfficialDutySelfQuery { Status = "Submitted", FromDate = new DateOnly(2027, 2, 10), ToDate = new DateOnly(2027, 2, 10) })).Data!.Items);

        var meta = Assert.IsType<OkObjectResult>(Ess(_uSari).GetFilterMetadata());
        var isi = Assert.IsType<ApiResponse<OfficialDutySelfFilterMetadataResponse>>(meta.Value).Data!;
        Assert.Equal(new[] { "Submitted", "Approved", "Rejected", "Cancelled" }, isi.StatusOptions.Select(x => x.Value));
        Assert.Equal(AttendanceValueConstants.OfficialDutyCategory.All, isi.DutyCategoryOptions.Select(x => x.Value));
        Assert.All(isi.DutyCategoryOptions, x => Assert.False(string.IsNullOrWhiteSpace(x.Label)));
    }

    // AC4: Kepala Ruang tidak dapat menyetujui atau menolak surat tugasnya sendiri (identitas dari akun, bukan body); milik pegawai lain tetap bisa.
    [Fact]
    public async Task Pemohon_tidak_dapat_memutuskan_surat_tugasnya_sendiri()
    {
        var milikDodi = (await Ajukan(_uDodi)).Data!.Id;
        var milikSari = (await Ajukan(_uSari)).Data!.Id;

        var setujuSendiri = await Layanan().ApproveAsync(milikDodi, new ApproveOfficialDutyRequest { ApproverWorkforceProfileId = _raka }, _uDodi);
        Assert.False(setujuSendiri.Success);
        Assert.Equal(403, setujuSendiri.StatusCode);
        Assert.Equal("Anda tidak dapat memutuskan pengajuan Anda sendiri.", setujuSendiri.Message);

        var tolakSendiri = await Layanan().RejectAsync(milikDodi, new RejectOfficialDutyRequest { ApproverWorkforceProfileId = _raka, RejectionReason = "Uji" }, _uDodi);
        Assert.Equal(403, tolakSendiri.StatusCode);
        Assert.Equal("Submitted", Surat(milikDodi).Status);
        Assert.Null(Surat(milikDodi).ApprovedAt);
        using (var ctx = _db.CreateContext())
        {
            Assert.False(ctx.HrdAttendanceDailies.Any(x => x.OfficialDutyRequestId == milikDodi));
        }

        var setujuLain = await Layanan().ApproveAsync(milikSari, new ApproveOfficialDutyRequest { ApproverWorkforceProfileId = _dodi }, _uDodi);
        Assert.True(setujuLain.Success, setujuLain.Message);
        Assert.Equal("Approved", Surat(milikSari).Status);

        // Penolakan milik pegawai lain tetap bisa; akun tanpa profil (mis. superadmin HR) tidak terhalang.
        var milikRaka = (await Layanan().CreateAsync(new CreateOfficialDutyRequest
        {
            WorkforceProfileId = _raka, DutyCategory = "LabCourier", AssignmentOrderNumber = "ST/LAB/2027/002",
            StartAt = Jam(2, 12, 8), EndAt = Jam(2, 12, 12), Destination = "Lab rujukan", Purpose = "Antar spesimen"
        }, _uTanpaProfil)).Data!.Id;
        var tolakLain = await Layanan().RejectAsync(milikRaka, new RejectOfficialDutyRequest { RejectionReason = "Kurir lain bertugas" }, _uTanpaProfil);
        Assert.True(tolakLain.Success, tolakLain.Message);
    }

    // Hak akses: tujuh endpoint dijaga MyOfficialDuty : Read/Create/Cancel di modul Layanan Mandiri; validator bersih.
    [Fact]
    public async Task Hak_akses_MyOfficialDuty_terdaftar_dan_ditegakkan()
    {
        var metode = typeof(OfficialDutySelfServiceController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<AccessPermissionAttribute>() != null)
            .ToDictionary(m => m.Name, m => { var a = m.GetCustomAttribute<AccessPermissionAttribute>()!; return $"{a.Arguments![0]}:{a.Arguments[1]}"; });
        Assert.Equal(new Dictionary<string, string>
        {
            ["GetFilterMetadata"] = "MyOfficialDuty:Read",
            ["GetMine"] = "MyOfficialDuty:Read",
            ["GetMineById"] = "MyOfficialDuty:Read",
            ["DownloadAttachment"] = "MyOfficialDuty:Read",
            ["UploadAttachment"] = "MyOfficialDuty:Create",
            ["Create"] = "MyOfficialDuty:Create",
            ["Cancel"] = "MyOfficialDuty:Cancel",
        }.OrderBy(x => x.Key), metode.OrderBy(x => x.Key));

        var snapshot = PermissionRegistryDescriptor.BuildFromAssembly(typeof(OfficialDutySelfServiceController).Assembly);
        foreach (var aksi in new[] { "Read", "Create", "Cancel" })
        {
            Assert.Contains(snapshot.Actions, x => x.ModuleCode == "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE" && x.ResourceName == "MyOfficialDuty" && x.ActionName == aksi);
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(OfficialDutySelfServiceController).Assembly);
        using var provider = services.BuildServiceProvider();
        var hasil = new PermissionRegistryValidator(provider.GetRequiredService<IActionDescriptorCollectionProvider>(), NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);
        var temuan = hasil.UnregisterableEndpoints.Concat(hasil.DuplicateResourceIdentities).Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints).Concat(hasil.NakedBusinessEndpoints)
            .Where(x => x.StartsWith("MyOfficialDuty.", StringComparison.Ordinal) || x.Contains("'MyOfficialDuty'", StringComparison.Ordinal))
            .ToList();
        Assert.True(temuan.Count == 0, string.Join(Environment.NewLine, temuan));

        using var ctx = _db.CreateContext();
        Assert.False(await Pelaku.Izin(ctx).HasAccessAsync(Pelaku.Sebagai(_uSari).HttpContext!.User, "MyOfficialDuty", "Create"));
    }

    private sealed class LingkunganWebUji : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "OfficialDutySelfServiceTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class PenangkapLog : Microsoft.Extensions.Logging.ILogger<LoggerService>
    {
        public List<string> Pesan { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Pesan.Add(formatter(state, exception));
    }
}
