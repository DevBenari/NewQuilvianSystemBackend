using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.ProviderRequest;

/// <summary>
/// Bentuk kontrak endpoint <c>BE-BD-004</c> dan pemetaan hasil service menjadi HTTP status,
/// sesuai <c>contracts/api-contract.md</c> grup Provider Request dan Blood Unit.
/// </summary>
/// <remarks>
/// Bagian dari kelas yang sama dengan <see cref="ProviderRequestServiceTests"/> supaya memakai
/// lingkungan uji dan data semai yang persis sama.
/// </remarks>
public partial class ProviderRequestServiceTests
{
    private const string BaseRouteProviderRequest = "api/v1/health-services/blood-bank-management/provider-requests";
    private const string BaseRouteBloodUnit = "api/v1/health-services/blood-bank-management/blood-units";

    // =====================================================================
    // 7. Bentuk kontrak endpoint
    // =====================================================================

    [Theory]
    [InlineData(nameof(BbkProviderRequestController.GetFilterMetadata), "Read", typeof(HttpGetAttribute), "filters/metadata")]
    [InlineData(nameof(BbkProviderRequestController.GetSummary), "Read", typeof(HttpGetAttribute), "summary")]
    [InlineData(nameof(BbkProviderRequestController.GetAll), "Read", typeof(HttpGetAttribute), null)]
    [InlineData(nameof(BbkProviderRequestController.GetById), "Read", typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData(nameof(BbkProviderRequestController.GetStatusHistory), "Read", typeof(HttpGetAttribute), "{id:guid}/status-history")]
    [InlineData(nameof(BbkProviderRequestController.Create), "Create", typeof(HttpPostAttribute), null)]
    [InlineData(nameof(BbkProviderRequestController.RecordReceipt), "Process", typeof(HttpPostAttribute), "{id:guid}/receipts")]
    [InlineData(nameof(BbkProviderRequestController.Cancel), "Update", typeof(HttpPostAttribute), "{id:guid}/cancel")]
    public void EndpointPermintaan_MemakaiRouteDanButirHakAksesYangDikunciKontrak(
        string methodName,
        string aksi,
        Type verb,
        string? template)
        => AssertEndpoint(typeof(BbkProviderRequestController), "BloodProviderRequest", methodName, aksi, verb, template);

    [Theory]
    [InlineData(nameof(BbkBloodUnitController.GetFilterMetadata), typeof(HttpGetAttribute), "filters/metadata")]
    [InlineData(nameof(BbkBloodUnitController.GetSummary), typeof(HttpGetAttribute), "summary")]
    [InlineData(nameof(BbkBloodUnitController.GetAll), typeof(HttpGetAttribute), null)]
    [InlineData(nameof(BbkBloodUnitController.GetById), typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData(nameof(BbkBloodUnitController.GetStatusHistory), typeof(HttpGetAttribute), "{id:guid}/status-history")]
    public void EndpointKantong_HanyaMembaca_DenganButirBloodUnitRead(string methodName, Type verb, string? template)
        => AssertEndpoint(typeof(BbkBloodUnitController), "BloodUnit", methodName, "Read", verb, template);

    /// <summary>
    /// Bentuk transaksi, bukan master data: tidak ada <c>GET /options</c>, <c>PATCH /{id}/status</c>
    /// generik, maupun <c>DELETE</c>. <c>PUT</c> hanya satu, <c>PUT /{id}/storage-location</c> milik
    /// kontrak <c>v4</c> untuk perpindahan lokasi kantong (<c>BE-BD-015</c>). Jumlah endpoint dikunci
    /// supaya penambahan tanpa pembaruan kontrak ketahuan.
    /// </summary>
    [Theory]
    [InlineData(typeof(BbkProviderRequestController), BaseRouteProviderRequest, "BloodProviderRequest", "Health Services / Blood Bank Management / Provider Request", 8)]
    [InlineData(typeof(BbkBloodUnitController), BaseRouteBloodUnit, "BloodUnit", "Health Services / Blood Bank Management / Blood Unit", 8)]
    public void Controller_BerbentukTransaksi_DenganBaseRouteDanTagKontrak(
        Type type,
        string route,
        string controllerName,
        string tag,
        int jumlahEndpoint)
    {
        Assert.Equal(route, type.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.Equal(controllerName, type.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName);
        Assert.Equal(new[] { tag }, type.GetCustomAttribute<TagsAttribute>()!.Tags);

        var endpoints = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

        Assert.Equal(jumlahEndpoint, endpoints.Count);

        Assert.DoesNotContain(endpoints, x =>
            x.GetCustomAttributes<HttpDeleteAttribute>().Any() ||
            x.GetCustomAttributes<HttpPatchAttribute>().Any());

        Assert.All(
            endpoints.SelectMany(x => x.GetCustomAttributes<HttpPutAttribute>()),
            x => Assert.Equal("{id:guid}/storage-location", x.Template));

        var templates = endpoints
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("options", templates);
        Assert.DoesNotContain("{id:guid}/status", templates);
    }

    /// <summary>
    /// Butir <c>Process</c> dan <c>Update</c> memakai <c>AccessType</c> yang ditampilkan layar
    /// Akses Role, sehingga keduanya benar-benar dapat diberikan admin.
    /// </summary>
    [Theory]
    [InlineData(nameof(BbkProviderRequestController.RecordReceipt), "Process")]
    [InlineData(nameof(BbkProviderRequestController.Cancel), "Update")]
    public void ButirPenerimaanDanPembatalan_DapatDiberikanDiLayarAksesRole(string methodName, string aksi)
    {
        var action = typeof(BbkProviderRequestController).GetMethod(methodName)!.GetCustomAttribute<AccessActionAttribute>()!;

        Assert.Equal(aksi, action.ActionName);
        Assert.Equal(AccessTypes.Update, action.AccessType);
        Assert.Contains(action.AccessType, AccessTypes.AllowedForRoleAccess);
        Assert.False(action.IsSystemOnly);
        Assert.True(action.VisibleInRoleAccess);
    }

    // =====================================================================
    // 8. Pemetaan hasil service menjadi HTTP status
    // =====================================================================

    [Fact]
    public async Task Controller_PermintaanSah_Menjadi200DenganDetailLengkap()
    {
        await using var l = await Lingkungan.BuatAsync();
        var orderId = await l.BuatOrderAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await Controller(l, PetugasBdrs).Create(new CreateProviderRequestRequest { BloodOrderId = orderId });

        var ok = Assert.IsType<OkObjectResult>(hasil);
        var body = Assert.IsType<ApiResponse<ProviderRequestDetailDto>>(ok.Value);

        Assert.True(body.Success);
        Assert.Equal("PMI-00000001", body.Data!.RequestNumber);
        Assert.Equal("Pasien A", body.Data.PatientName);
        Assert.Equal("RI-001", body.Data.EncounterNumber);
        Assert.Equal(2, body.Data.TotalOutstandingQuantity);
        Assert.Equal("Create", Assert.Single(body.Data.Transitions).Action);
    }

    [Fact]
    public async Task Controller_PermintaanGanda_Menjadi422_VAL_BD_006()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await Controller(l, PetugasBdrs).Create(
            new CreateProviderRequestRequest { BloodOrderId = permintaan.BloodOrderId });

        AssertGagal(hasil, StatusCodes.Status422UnprocessableEntity, PesanVal006);
    }

    /// <summary>
    /// Kelebihan kiriman bukan kegagalan: <c>200</c> dengan peringatan <c>VAL-BD-014</c>, dan
    /// detail yang dikembalikan sudah memuat kantong berlebihnya.
    /// </summary>
    [Fact]
    public async Task Controller_PenerimaanBerlebih_Menjadi200DenganPeringatan_VAL_BD_014()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 1));

        var hasil = await Controller(l, PetugasBdrs).RecordReceipt(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-P-001"), (l.D.Prc, "PMI-P-002")));

        var ok = Assert.IsType<OkObjectResult>(hasil);
        var body = Assert.IsType<ApiResponse<ProviderRequestDetailDto>>(ok.Value);

        Assert.True(body.Success);
        Assert.Equal(PesanVal014, body.Message);
        Assert.Equal(1, body.Data!.TotalExcessQuantity);
        Assert.Equal(0, body.Data.TotalOutstandingQuantity);
    }

    [Fact]
    public async Task Controller_NomorKantongGanda_Menjadi422()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await Controller(l, PetugasBdrs).RecordReceipt(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-Q-001"), (l.D.Prc, "PMI-Q-001")));

        AssertGagal(hasil, StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task Controller_PembatalanTokenUsang_Menjadi409()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await Controller(l, PetugasBdrs).Cancel(
            permintaan.Id,
            new CancelProviderRequestRequest { ReasonCode = AlasanBatalPmi, Version = 5 });

        AssertGagal(hasil, StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Controller_PembatalanTanpaAlasanTerkendali_Menjadi400_VAL_BD_016()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await Controller(l, PetugasBdrs).Cancel(permintaan.Id, Batal("KETIKAN-BEBAS"));

        AssertGagal(hasil, StatusCodes.Status400BadRequest, PesanVal016);
    }

    [Fact]
    public async Task Controller_DataTidakAda_Menjadi404PadaSeluruhJalurBacaDanTulis()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = Controller(l, PetugasBdrs);
        var kantong = KontrolerKantong(l);
        var acak = Guid.NewGuid();

        AssertGagal(await permintaan.GetById(acak), StatusCodes.Status404NotFound);
        AssertGagal(await permintaan.GetStatusHistory(acak), StatusCodes.Status404NotFound);
        AssertGagal(await permintaan.RecordReceipt(acak, Terima((l.D.Prc, "PMI-R-001"))), StatusCodes.Status404NotFound);
        AssertGagal(await permintaan.Cancel(acak, Batal(AlasanBatalPmi)), StatusCodes.Status404NotFound);
        AssertGagal(await kantong.GetById(acak), StatusCodes.Status404NotFound);
        AssertGagal(await kantong.GetStatusHistory(acak), StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Petugas penerima diturunkan dari klaim pengguna. Tanpa klaim itu, tidak ada kantong yang
    /// lahir — tidak ada penerimaan tanpa jejak penerimanya.
    /// </summary>
    [Fact]
    public async Task Controller_TanpaKlaimPengguna_TidakAdaPermintaanMaupunKantongYangLahir()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        AssertGagal(
            await Controller(l, aktor: null).RecordReceipt(permintaan.Id, Terima((l.D.Prc, "PMI-S-001"))),
            StatusCodes.Status400BadRequest);

        AssertGagal(
            await Controller(l, aktor: null).Create(new CreateProviderRequestRequest { BloodOrderId = permintaan.BloodOrderId }),
            StatusCodes.Status400BadRequest);

        Assert.Equal(1, await HitungPermintaanAsync(l));
        Assert.Equal(0, (await l.UnitService().GetSummaryAsync()).TotalUnit);
    }

    [Fact]
    public async Task ControllerKantong_DaftarDanDetail_Menjadi200()
    {
        await using var l = await Lingkungan.BuatAsync();
        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 1));
        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-T-001")), PetugasBdrs);

        var kantong = KontrolerKantong(l);

        var daftar = Assert.IsType<OkObjectResult>(await kantong.GetAll(null, null, null, permintaan.Id, null, null, null));
        var isi = Assert.IsType<ApiResponse<PagedResult<BloodUnitListDto>>>(daftar.Value);
        var baris = Assert.Single(isi.Data!.Items);

        Assert.Equal(permintaan.RequestNumber, baris.RequestNumber);
        Assert.Equal("Pasien A", baris.PatientName);

        var detail = Assert.IsType<OkObjectResult>(await kantong.GetById(baris.Id));
        Assert.Equal("PMI-T-001", Assert.IsType<ApiResponse<BloodUnitDetailDto>>(detail.Value).Data!.PmiBagNumber);
    }

    // =====================================================================
    // Penolong endpoint
    // =====================================================================

    private static void AssertEndpoint(
        Type controller,
        string resource,
        string methodName,
        string aksi,
        Type verb,
        string? template)
    {
        var method = controller.GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);
        Assert.Equal(resource, arguments[0]);
        Assert.Equal(aksi, arguments[1]);

        // Argumen ke-2 [AccessPermission] wajib sama persis dengan argumen ke-1 [AccessAction];
        // menyimpang berarti 403 permanen yang tidak dapat diperbaiki dari layar Akses Role.
        var action = method.GetCustomAttribute<AccessActionAttribute>();
        Assert.NotNull(action);
        Assert.Equal(aksi, action!.ActionName);

        var verbAttribute = method.GetCustomAttributes(verb, inherit: false).SingleOrDefault();
        Assert.NotNull(verbAttribute);
        Assert.Equal(template, ((IRouteTemplateProvider)verbAttribute!).Template);
    }

    private static BbkProviderRequestController Controller(Lingkungan l, Guid? aktor)
    {
        var claims = aktor.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, aktor.Value.ToString()) }
            : Array.Empty<Claim>();

        return new BbkProviderRequestController(
            l.Service(),
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Uji"))
                }
            }
        };
    }

    private static BbkBloodUnitController KontrolerKantong(Lingkungan l)
        => new(l.UnitService(), new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

    private static void AssertGagal(IActionResult hasil, int status, string? pesan = null)
    {
        var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
        Assert.Equal(status, objek.StatusCode);

        var body = Assert.IsType<ApiResponse<object>>(objek.Value);
        Assert.False(body.Success);
        Assert.Equal(status, body.StatusCode);

        if (pesan != null)
            Assert.Equal(pesan, body.Message);
    }
}
