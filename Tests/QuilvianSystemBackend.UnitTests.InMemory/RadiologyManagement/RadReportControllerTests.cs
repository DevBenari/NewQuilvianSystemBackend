using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Endpoint hasil bacaan — <c>BE-RAD-09</c>.
///
/// <para>
/// Dua hal dibuktikan di sini, dan keduanya berbeda dari yang dibuktikan
/// <c>RadReportServiceTests</c>. Yang pertama: <b>bentuk permukaannya sesuai
/// <c>RAD-API-001</c></b> — route, kata kerja HTTP, dan hak akses tiap endpoint. Yang kedua:
/// <b>aturan keselamatan tetap berlaku lewat jalur HTTP</b>, dan kode statusnya sesuai
/// <c>RAD-API-001</c> bagian 4.
/// </para>
///
/// <para>
/// Yang kedua itu bukan pengulangan. Sebuah aturan dapat berjalan sempurna di service lalu
/// hilang di controller — karena hasil penolakan dipetakan menjadi <c>200</c>, karena sebuah
/// endpoint lupa memanggil service dan menyentuh database sendiri, atau karena pemetaan
/// kodenya berbeda dari controller radiologi lainnya sehingga layar menangani dua bentuk
/// kegagalan untuk keadaan yang sama.
/// </para>
/// </summary>
public sealed class RadReportControllerTests
{
    private const string BaseUrl = "api/v1/health-services/radiology-management/rad-reports";

    private static readonly Guid Radiolog = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RadiologLain = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Residen = Guid.Parse("33333333-3333-3333-3333-333333333333");

    /* ================================================================== *
     * 1 — Bentuk permukaan sesuai RAD-API-001
     * ================================================================== */

    [Fact]
    public void BaseUrlSesuaiKontrak()
    {
        var route = typeof(RadReportController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(route);
        Assert.Equal(BaseUrl, route!.Template);
    }

    [Theory]
    // Delapan endpoint yang menjadi bagian BE-RAD-09, ditulis apa adanya seperti RAD-API-001.
    [InlineData("GET", null)]
    [InlineData("GET", "{id:guid}")]
    [InlineData("GET", "by-study/{radStudyId:guid}")]
    [InlineData("GET", "by-encounter/{encounterId:guid}")]
    [InlineData("POST", "by-study/{radStudyId:guid}/draft")]
    [InlineData("PUT", "{id:guid}/draft")]
    [InlineData("POST", "{id:guid}/validate")]
    [InlineData("POST", "{id:guid}/release")]
    // Dua endpoint koreksi berversi, ditambahkan BE-RAD-10.
    [InlineData("GET", "{id:guid}/versions")]
    [InlineData("POST", "{id:guid}/amendments")]
    // Dua endpoint baseline wajib standar endpoint transaksi.
    [InlineData("GET", "filters/metadata")]
    [InlineData("GET", "summary")]
    public void EndpointKontrakTersedia(string metode, string? template)
    {
        var ada = EndpointsOf(typeof(RadReportController))
            .Any(x => MetodeDan(x) == (metode, template));

        Assert.True(ada, $"{metode} /{template ?? string.Empty} tidak ditemukan pada controller.");
    }

    [Fact]
    public void TidakAdaEndpointYangDapatMengubahAtauMenghapusVersi()
    {
        // Penjaga paling penting pada controller ini — RJ-BIL-GATE-DEC-004. Satu-satunya
        // endpoint yang boleh menyentuh isi versi adalah PUT /{id}/draft, dan itu pun hanya
        // atas draf yang belum disahkan. Tidak boleh ada DELETE apa pun, dan tidak boleh ada
        // PUT maupun PATCH yang menunjuk sebuah versi.
        var jalur = EndpointsOf(typeof(RadReportController))
            .Select(MetodeDan)
            .ToList();

        Assert.DoesNotContain(jalur, x => x.Metode == "DELETE");

        var penyuntingVersi = jalur
            .Where(x => x.Metode is "PUT" or "PATCH")
            .Select(x => x.Template)
            .ToList();

        Assert.Equal(new[] { "{id:guid}/draft" }, penyuntingVersi);
    }

    [Fact]
    public void HakAksesTiapEndpointSesuaiRadPerm001Bagian3()
    {
        var diharapkan = new Dictionary<string, string>
        {
            ["GetFilterMetadata"] = "Read",
            ["GetSummary"] = "Read",
            ["GetList"] = "Read",
            ["GetById"] = "Read",
            ["GetVersions"] = "Read",
            ["GetByStudy"] = "Read",
            ["GetByEncounter"] = "Read",
            ["CreateDraft"] = "Create",
            ["UpdateDraft"] = "Update",
            ["Validate"] = "Validate",
            ["Release"] = "Release",
            ["CreateAmendment"] = "Amend",
        };

        foreach (var endpoint in EndpointsOf(typeof(RadReportController)))
        {
            Assert.True(
                diharapkan.ContainsKey(endpoint.Name),
                $"Endpoint {endpoint.Name} tidak tercantum pada RAD-PERM-001 bagian 3.");

            var permission = endpoint
                .GetCustomAttributes()
                .Single(x => x.GetType().Name == "AccessPermissionAttribute");

            var arguments = (object[])permission
                .GetType()
                .GetProperty("Arguments")!
                .GetValue(permission)!;

            Assert.Equal("RadReport", arguments[0]);
            Assert.Equal(diharapkan[endpoint.Name], arguments[1]);
        }
    }

    /* ================================================================== *
     * 2 — Kode status sesuai RAD-API-001 bagian 4
     * ================================================================== */

    [Fact]
    public async Task DaftarBacaanDijawab200()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);
        await Controller(db, Radiolog, radiolog: true).CreateDraft(study, Draf());

        var hasil = await Controller(db, Radiolog, radiolog: true).GetList();

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));

        var isi = Muatan<PagedResult<RadReportListResponse>>(hasil);
        Assert.Equal(1, isi.TotalData);
        Assert.Equal(1, isi.PageNumber);
    }

    [Fact]
    public async Task DaftarBacaanTidakMembawaIsiBacaan()
    {
        // Daftar sering terbuka pada layar yang terlihat banyak orang. Kesimpulan klinis
        // seorang pasien dibuka lewat GET /{id}, ketika memang hendak dibaca.
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);
        await Controller(db, Radiolog, radiolog: true).CreateDraft(study, Draf());

        var isi = Muatan<PagedResult<RadReportListResponse>>(
            await Controller(db, Radiolog, radiolog: true).GetList());

        var nama = typeof(RadReportListResponse).GetProperties().Select(x => x.Name).ToList();

        Assert.DoesNotContain(nameof(RadReportVersion.Findings), nama);
        Assert.DoesNotContain(nameof(RadReportVersion.Impression), nama);
        Assert.DoesNotContain(nameof(RadReportVersion.Recommendation), nama);
        Assert.Single(isi.Items);
    }

    [Fact]
    public async Task BacaanYangTidakAdaDijawab404()
    {
        await using var db = Konteks();

        var hasil = await Controller(db, Radiolog, radiolog: true).GetById(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, Kode(hasil));
    }

    [Fact]
    public async Task PemeriksaanYangBelumPunyaBacaanDijawab404DenganPesanYangMembedakan()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Controller(db, Radiolog, radiolog: true).GetByStudy(study);

        Assert.Equal(StatusCodes.Status404NotFound, Kode(hasil));
        Assert.Contains("belum memiliki bacaan", Pesan(hasil));
    }

    [Fact]
    public async Task KunjunganTanpaBacaanDijawab200DenganDaftarKosong()
    {
        // Bukan 404. Kunjungan yang belum punya bacaan adalah keadaan wajar, dan rekam medis
        // perlu membedakannya dari kunjungan yang memang tidak ada.
        await using var db = Konteks();

        var hasil = await Controller(db, Radiolog, radiolog: true).GetByEncounter(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));
        Assert.Empty(Muatan<List<RadReportListResponse>>(hasil));
    }

    [Fact]
    public async Task DrafTersimpanDijawab200()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Controller(db, Radiolog, radiolog: true).CreateDraft(study, Draf());

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));

        var isi = Muatan<RadReportDetailResponse>(hasil);
        Assert.Equal(nameof(RadReportStatus.Drafted), isi.ReportStatus);
    }

    [Fact]
    public async Task DrafTanpaKesimpulanDijawab400()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var permintaan = Draf();
        permintaan.Impression = "  ";

        var hasil = await Controller(db, Radiolog, radiolog: true).CreateDraft(study, permintaan);

        Assert.Equal(StatusCodes.Status400BadRequest, Kode(hasil));
        Assert.Equal("Kesimpulan bacaan wajib diisi.", Pesan(hasil));
    }

    [Fact]
    public async Task DrafAtasCitraYangDinyatakanTidakLayakDijawab422()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db, isUsable: false);

        var hasil = await Controller(db, Radiolog, radiolog: true).CreateDraft(study, Draf());

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, Kode(hasil));
        Assert.Contains("tidak layak dibaca", Pesan(hasil));
    }

    [Fact]
    public async Task DrafAtasCitraYangMutunyaBelumDinilaiDijawab422()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db, isUsable: null);

        var hasil = await Controller(db, Radiolog, radiolog: true).CreateDraft(study, Draf());

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, Kode(hasil));
        Assert.Contains("belum dinilai", Pesan(hasil));
    }

    [Fact]
    public async Task BacaanKeduaAtasPemeriksaanYangSamaDijawab409()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);
        await Controller(db, Radiolog, radiolog: true).CreateDraft(study, Draf());

        var hasil = await Controller(db, RadiologLain, radiolog: true).CreateDraft(study, Draf());

        Assert.Equal(StatusCodes.Status409Conflict, Kode(hasil));
    }

    [Fact]
    public async Task MengakuDokterRadiologTanpaPenandanyaDijawab403()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Controller(db, Residen)
            .CreateDraft(study, Draf(RadReportAuthorRole.Radiologist));

        Assert.Equal(StatusCodes.Status403Forbidden, Kode(hasil));
    }

    [Fact]
    public async Task MengubahDrafOrangLainDijawab403()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Controller(db, Radiolog, radiolog: true).UpdateDraft(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Diubah orang lain." });

        Assert.Equal(StatusCodes.Status403Forbidden, Kode(hasil));
        Assert.Equal("Hanya penulis draf yang dapat mengubahnya sebelum disahkan.", Pesan(hasil));
    }

    /* ================================================================== *
     * 3 — AC-1 sampai AC-4 lewat jalur HTTP
     * ================================================================== */

    [Fact]
    public async Task AC1_ResidenMengesahkanDrafnyaSendiriDijawab403LewatHttp()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Controller(db, Residen).Validate(laporan);

        Assert.Equal(StatusCodes.Status403Forbidden, Kode(hasil));
        Assert.Equal("Draf yang Anda tulis harus disahkan dokter radiolog.", Pesan(hasil));

        // Penolakannya benar-benar menahan, bukan sekadar pesan.
        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(RadReportVersionStatus.Drafted, tersimpan.VersionStatus);
        Assert.Null(tersimpan.ValidatorUserId);
    }

    [Fact]
    public async Task AC2_RadiologMengesahkanDrafnyaSendiriDijawab200LewatHttp()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Radiolog, RadReportAuthorRole.Radiologist, radiolog: true);

        var hasil = await Controller(db, Radiolog, radiolog: true).Validate(laporan);

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));
        Assert.Equal(
            nameof(RadReportStatus.Validated),
            Muatan<RadReportDetailResponse>(hasil).ReportStatus);
    }

    [Fact]
    public async Task AC3_ResidenYangKemudianMenjadiRadiologTetapDijawab403LewatHttp()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Controller(db, Residen, radiolog: true).Validate(laporan);

        Assert.Equal(StatusCodes.Status403Forbidden, Kode(hasil));
        Assert.Equal("Draf yang Anda tulis harus disahkan dokter radiolog.", Pesan(hasil));
    }

    [Fact]
    public async Task PengesahYangBukanDokterRadiologDijawab403LewatHttp()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Controller(db, RadiologLain).Validate(laporan);

        Assert.Equal(StatusCodes.Status403Forbidden, Kode(hasil));
        Assert.Equal("Hanya dokter radiolog yang boleh mengesahkan hasil bacaan.", Pesan(hasil));
    }

    [Fact]
    public async Task PengesahanKeduaDijawab409LewatHttp()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);
        await Controller(db, Radiolog, radiolog: true).Validate(laporan);

        var hasil = await Controller(db, RadiologLain, radiolog: true).Validate(laporan);

        Assert.Equal(StatusCodes.Status409Conflict, Kode(hasil));

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(Radiolog, tersimpan.ValidatorUserId);
    }

    /* ================================================================== *
     * 4 — Rilis
     * ================================================================== */

    [Fact]
    public async Task MerilisBacaanYangBelumDisahkanDijawab409()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Controller(db, Radiolog, radiolog: true).Release(laporan);

        Assert.Equal(StatusCodes.Status409Conflict, Kode(hasil));
        Assert.Equal("Bacaan harus disahkan lebih dulu sebelum dirilis.", Pesan(hasil));
    }

    [Fact]
    public async Task RilisOlehBukanDokterRadiologDijawab403()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);
        await Controller(db, Radiolog, radiolog: true).Validate(laporan);

        var hasil = await Controller(db, Residen).Release(laporan);

        Assert.Equal(StatusCodes.Status403Forbidden, Kode(hasil));
    }

    [Fact]
    public async Task RilisBerhasilDijawab200DanTerbacaPadaDaftar()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);
        await Controller(db, Radiolog, radiolog: true).Validate(laporan);

        var hasil = await Controller(db, Radiolog, radiolog: true).Release(laporan);

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));

        var isi = Muatan<RadReportDetailResponse>(hasil);
        Assert.Equal(nameof(RadReportStatus.Released), isi.ReportStatus);
        Assert.NotNull(isi.FirstReleasedAt);

        var daftar = Muatan<PagedResult<RadReportListResponse>>(
            await Controller(db, Radiolog, radiolog: true).GetList());

        Assert.Equal("Sudah dirilis ke dokter pengirim", daftar.Items[0].ReportStatusLabel);
    }

    /* ================================================================== *
     * 5 — Metadata, rekap, dan daftar aksi
     * ================================================================== */

    [Fact]
    public void MetadataPenyaringDijawab200DanMemuatSeluruhKeadaanBacaan()
    {
        using var db = Konteks();

        var hasil = Controller(db, Radiolog, radiolog: true).GetFilterMetadata();

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));

        var isi = Muatan<RadReportFilterMetadataResponse>(hasil);
        Assert.Equal(7, isi.ReportStatuses.Count);
        Assert.Equal(4, isi.VersionStatuses.Count);
        Assert.Equal(4, isi.AuthorRoles.Count);
        Assert.NotEmpty(isi.Actions);
    }

    [Fact]
    public async Task RekapMenghitungBacaanYangBelumSampaiKeDokterPengirim()
    {
        await using var db = Konteks();

        var menunggu = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);
        await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        await Controller(db, Radiolog, radiolog: true).Validate(menunggu);
        await Controller(db, Radiolog, radiolog: true).Release(menunggu);

        var isi = Muatan<RadReportSummaryResponse>(
            await Controller(db, Radiolog, radiolog: true).GetSummary());

        Assert.Equal(2, isi.TotalBacaan);
        Assert.Equal(1, isi.SudahDirilis);
        Assert.Equal(1, isi.Draf);
        Assert.Equal(1, isi.BelumDirilis);
    }

    [Fact]
    public async Task DaftarAksiMengikutiKeadaanBacaan()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var draf = Muatan<RadReportDetailResponse>(
            await Controller(db, Radiolog, radiolog: true).GetById(laporan));
        Assert.Equal(new[] { "UpdateDraft", "Validate" }, draf.AvailableActions);

        var disahkan = Muatan<RadReportDetailResponse>(
            await Controller(db, Radiolog, radiolog: true).Validate(laporan));
        Assert.Equal(new[] { "Release" }, disahkan.AvailableActions);

        var dirilis = Muatan<RadReportDetailResponse>(
            await Controller(db, Radiolog, radiolog: true).Release(laporan));
        Assert.Equal(new[] { "Amend" }, dirilis.AvailableActions);
    }

    [Fact]
    public async Task DaftarAksiHanyaBantuanTampilanBukanPengaman()
    {
        // Residen tetap melihat tombol Sahkan pada drafnya sendiri — daftar aksi tidak tahu
        // siapa yang melihat. Yang menahan adalah backend, dan itu yang dibuktikan di sini.
        await using var db = Konteks();
        var laporan = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var terlihat = Muatan<RadReportDetailResponse>(
            await Controller(db, Residen).GetById(laporan));

        Assert.Contains("Validate", terlihat.AvailableActions);
        Assert.Equal(
            StatusCodes.Status403Forbidden,
            Kode(await Controller(db, Residen).Validate(laporan)));
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private const string IsiKesimpulan = "Tidak tampak perdarahan intrakranial.";

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-report-api-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Controller sungguhan di atas service sungguhan. Satu-satunya bagian yang digantikan
    /// adalah pemeriksaan penanda <c>RadReport : ActAsRadiologist</c>.
    /// </summary>
    private sealed class BacaanDenganKewenangan(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService,
        RadReportNumberService reportNumberService,
        bool radiolog)
        : RadReportService(dbContext, httpContextAccessor, null!, loggerService, reportNumberService)
    {
        protected override Task<bool> HasRadiologistAuthorityAsync(CancellationToken cancellationToken) =>
            Task.FromResult(radiolog);
    }

    private static RadReportController Controller(
        ApplicationDbContext db,
        Guid actorUserId,
        bool radiolog = false)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        var service = new BacaanDenganKewenangan(
            db,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor),
            new RadReportNumberService(),
            radiolog);

        return new RadReportController(service);
    }

    private static int Kode(IActionResult hasil) =>
        Assert.IsAssignableFrom<ObjectResult>(hasil).StatusCode
        ?? throw new InvalidOperationException("Hasil tidak membawa kode status.");

    private static T Muatan<T>(IActionResult hasil)
    {
        var isi = Assert.IsAssignableFrom<ObjectResult>(hasil).Value;
        var pembungkus = Assert.IsType<ApiResponse<T>>(isi);

        Assert.True(pembungkus.Success, "Muatan diminta dari jawaban yang gagal.");

        return pembungkus.Data!;
    }

    private static string Pesan(IActionResult hasil)
    {
        var isi = Assert.IsAssignableFrom<ObjectResult>(hasil).Value;

        return (string)isi!.GetType().GetProperty(nameof(ApiResponse<object>.Message))!
            .GetValue(isi)!;
    }

    private static IEnumerable<MethodInfo> EndpointsOf(Type controllerType) =>
        controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName);

    private static (string Metode, string? Template) MetodeDan(MethodInfo endpoint)
    {
        if (endpoint.GetCustomAttribute<HttpGetAttribute>() is { } get)
        {
            return ("GET", get.Template);
        }

        if (endpoint.GetCustomAttribute<HttpPostAttribute>() is { } post)
        {
            return ("POST", post.Template);
        }

        if (endpoint.GetCustomAttribute<HttpPutAttribute>() is { } put)
        {
            return ("PUT", put.Template);
        }

        return ("NONE", null);
    }

    private static async Task<Guid> SeedStudyAsync(
        ApplicationDbContext db,
        bool? isUsable = true)
    {
        var order = new RadOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = Guid.NewGuid(),
            ModalityId = Guid.NewGuid(),
            OrderStatus = RadOrderStatus.InProgress,
        };

        var study = new RadStudy
        {
            Id = Guid.NewGuid(),
            RadOrderId = order.Id,
            EncounterId = order.EncounterId,
            ProcedureId = order.ProcedureId,
            ModalityId = order.ModalityId,
            StudySequence = 1,
            StudyNumber = $"RAD-{order.Id:N}-001".ToUpperInvariant(),
            StudyStatus = isUsable == true
                ? RadStudyStatus.QualityAccepted
                : RadStudyStatus.Acquired,
            IsUsable = isUsable,
        };

        db.RadOrders.Add(order);
        db.RadStudies.Add(study);
        await db.SaveChangesAsync();

        return study.Id;
    }

    private static CreateRadReportDraftRequest Draf(RadReportAuthorRole? peran = null) => new()
    {
        AuthorRole = peran,
        Findings = "Sulkus dan girus dalam batas normal.",
        Impression = IsiKesimpulan,
        Recommendation = "Tidak diperlukan pemeriksaan lanjutan.",
    };

    /// <summary>Satu pemeriksaan beserta draf pertamanya. Mengembalikan id bacaan.</summary>
    private static async Task<Guid> DrafAsync(
        ApplicationDbContext db,
        Guid penulis,
        RadReportAuthorRole peran,
        bool radiolog = false)
    {
        var study = await SeedStudyAsync(db);

        var hasil = await Controller(db, penulis, radiolog).CreateDraft(study, Draf(peran));

        Assert.Equal(StatusCodes.Status200OK, Kode(hasil));

        return Muatan<RadReportDetailResponse>(hasil).Id;
    }
}
