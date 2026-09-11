using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodUnitStorage;

/// <summary>
/// Bentuk kontrak ketiga endpoint penyimpanan <c>BE-BD-015</c> beserta pemetaan hasil service
/// menjadi HTTP status.
/// </summary>
public partial class BloodUnitStorageServiceTests
{
    [Theory]
    [InlineData(nameof(BbkBloodUnitController.GetPlacements), "Read", typeof(HttpGetAttribute), "{id:guid}/placements")]
    [InlineData(nameof(BbkBloodUnitController.AssignStorageLocation), "Store", typeof(HttpPostAttribute), "{id:guid}/storage-location")]
    [InlineData(nameof(BbkBloodUnitController.MoveStorageLocation), "Store", typeof(HttpPutAttribute), "{id:guid}/storage-location")]
    public void Endpoint_MemakaiRouteDanButirHakAksesYangDikunciKontrak(string methodName, string aksi, Type verb, string template)
    {
        var method = typeof(BbkBloodUnitController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);
        Assert.Equal("BloodUnit", arguments[0]);
        Assert.Equal(aksi, arguments[1]);

        // Argumen ke-2 [AccessPermission] wajib sama persis dengan argumen ke-1 [AccessAction].
        var action = method.GetCustomAttribute<AccessActionAttribute>();
        Assert.NotNull(action);
        Assert.Equal(aksi, action!.ActionName);
        Assert.Contains(action.AccessType, AccessTypes.AllowedForRoleAccess);
        Assert.False(action.IsSystemOnly);
        Assert.True(action.VisibleInRoleAccess);

        var verbAttribute = method.GetCustomAttributes(verb, inherit: false).SingleOrDefault();
        Assert.NotNull(verbAttribute);
        Assert.Equal(template, ((IRouteTemplateProvider)verbAttribute!).Template);
    }

    /// <summary>
    /// <c>Store</c> terpisah dari <c>Allocate</c> (<c>permission-audit-matrix</c> §1), dan tidak ada
    /// endpoint <c>DELETE</c>, <c>PATCH</c>, alokasi, maupun pemindahan massal pada slice ini.
    /// </summary>
    [Fact]
    public void Controller_DelapanEndpoint_TanpaHapusSuntingAlokasiMaupunPemindahanMassal()
    {
        var endpoints = typeof(BbkBloodUnitController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

        Assert.Equal(8, endpoints.Count);

        Assert.DoesNotContain(endpoints, x =>
            x.GetCustomAttributes<HttpDeleteAttribute>().Any() ||
            x.GetCustomAttributes<HttpPatchAttribute>().Any());

        var templates = endpoints
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(templates, x =>
            x.Contains("allocate", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("bulk", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("batch", StringComparison.OrdinalIgnoreCase));

        var aksi = endpoints
            .Select(x => x.GetCustomAttribute<AccessPermissionAttribute>()!)
            .Select(x => (string)Assert.IsType<object[]>(x.Arguments)[1])
            .Distinct()
            .OrderBy(x => x);

        Assert.Equal(new[] { "Read", "Store" }, aksi);
    }

    [Fact]
    public async Task Controller_PenetapanPertama_Menjadi200DenganLokasiDanStatusAvailable()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        var ok = Assert.IsType<OkObjectResult>(await Controller(l, PetugasBdrs).AssignStorageLocation(id, Simpan(l.D.KulkasBesar)));
        var body = Assert.IsType<ApiResponse<BloodUnitDetailDto>>(ok.Value);

        Assert.True(body.Success);
        Assert.Equal(BbkBloodUnitStatus.Available, body.Data!.UnitStatus);
        Assert.Equal("Kulkas Besar", body.Data.CurrentStorageLocationName);
        Assert.Equal(new[] { "MoveStorageLocation" }, body.Data.AvailableActions);
    }

    [Fact]
    public async Task Controller_JalurGagal_DipetakanKe400_404_409_422()
    {
        await using var l = await Lingkungan.BuatAsync();
        var kantong = await l.SiapkanKantongAsync(2);
        var controller = Controller(l, PetugasBdrs);

        AssertGagal(await controller.AssignStorageLocation(kantong[0], Simpan(l.D.KulkasRusak)), StatusCodes.Status422UnprocessableEntity, PesanVal060);
        AssertGagal(await controller.MoveStorageLocation(kantong[0], Pindah(l.D.KulkasBesar)), StatusCodes.Status422UnprocessableEntity, PesanVal062);
        AssertGagal(await controller.AssignStorageLocation(kantong[0], Simpan(Guid.Empty)), StatusCodes.Status400BadRequest);
        AssertGagal(await controller.AssignStorageLocation(Guid.NewGuid(), Simpan(l.D.KulkasBesar)), StatusCodes.Status404NotFound);
        AssertGagal(await controller.AssignStorageLocation(kantong[0], Simpan(l.D.KulkasBesar, versi: 9)), StatusCodes.Status409Conflict);

        Assert.IsType<OkObjectResult>(await controller.AssignStorageLocation(kantong[0], Simpan(l.D.KulkasBesar)));

        AssertGagal(await controller.AssignStorageLocation(kantong[0], Simpan(l.D.KulkasKecil)), StatusCodes.Status422UnprocessableEntity, PesanVal061);
        AssertGagal(await controller.MoveStorageLocation(kantong[0], Pindah(l.D.KulkasRusak)), StatusCodes.Status422UnprocessableEntity, PesanVal060);
        AssertGagal(await controller.GetPlacements(Guid.NewGuid()), StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Controller_PerpindahanDanRiwayatPenempatan_Menjadi200()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];
        var controller = Controller(l, PetugasBdrs);

        await controller.AssignStorageLocation(id, Simpan(l.D.KulkasBesar));

        var pindah = Assert.IsType<OkObjectResult>(await controller.MoveStorageLocation(id, Pindah(l.D.KulkasKecil)));
        Assert.Equal("Kulkas Kecil", Assert.IsType<ApiResponse<BloodUnitDetailDto>>(pindah.Value).Data!.CurrentStorageLocationName);

        var riwayat = Assert.IsType<OkObjectResult>(await controller.GetPlacements(id));
        var body = Assert.IsType<ApiResponse<List<BloodUnitPlacementDto>>>(riwayat.Value);
        Assert.Equal(new[] { "KLK-BSR", "KLK-KCL" }, body.Data!.Select(x => x.StorageLocationCode));
    }

    /// <summary>
    /// <c>AC-BD-032</c> lewat endpoint — daftar kerja #2 adalah <c>GET /blood-units?unitStatus=PendingReview</c>.
    /// </summary>
    [Fact]
    public async Task Controller_DaftarKerjaDua_MemuatKantongBerlebihSesudahDisimpan()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 1));
        var kantong = await l.TerimaAsync(permintaan.Id, (l.D.Prc, "PMI-E-001"), (l.D.Prc, "PMI-E-002"));
        var controller = Controller(l, PetugasBdrs);

        foreach (var id in kantong)
            await controller.AssignStorageLocation(id, Simpan(l.D.KulkasBesar));

        var ok = Assert.IsType<OkObjectResult>(await controller.GetAll(null, BbkBloodUnitStatus.PendingReview, null, null, null, null, null));
        var body = Assert.IsType<ApiResponse<PagedResult<BloodUnitListDto>>>(ok.Value);

        var baris = Assert.Single(body.Data!.Items);
        Assert.Equal(kantong[1], baris.Id);
        Assert.True(baris.IsExcess);
    }

    /// <summary>Pelaku diturunkan dari klaim login; tanpa klaim, tidak ada kantong yang disimpan.</summary>
    [Fact]
    public async Task Controller_TanpaKlaimPengguna_Ditolak400_KantongTetapReceived()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        AssertGagal(await Controller(l, aktor: null).AssignStorageLocation(id, Simpan(l.D.KulkasBesar)), StatusCodes.Status400BadRequest);

        await AssertBelumPernahDisimpanAsync(l, id);
    }

    [Fact]
    public async Task ControllerLokasi_HapusLokasiYangPernahDipakai_Menjadi422()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        var controller = new BloodStorageLocationController(
            l.LocationService(),
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()))
        {
            ControllerContext = KonteksDengan(PetugasBdrs)
        };

        AssertGagal(await controller.Delete(l.D.KulkasBesar), StatusCodes.Status422UnprocessableEntity);
    }

    /// <summary>
    /// <c>FE-BD-015</c> — detail lokasi menyebut jumlah kantong yang akan tertahan <b>sebelum</b>
    /// penonaktifan, supaya konfirmasi dapat menyebutnya. Kantong yang belum disimpan dan kantong di
    /// lokasi lain tidak terhitung.
    /// </summary>
    [Fact]
    public async Task ControllerLokasi_DetailMenyebutJumlahKantongYangAkanTertahan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var kantong = await l.SiapkanKantongAsync(4);

        await l.Service().AssignStorageLocationAsync(kantong[0], Simpan(l.D.KulkasBesar), PetugasBdrs);
        await l.Service().AssignStorageLocationAsync(kantong[1], Simpan(l.D.KulkasBesar), PetugasBdrs);
        await l.Service().AssignStorageLocationAsync(kantong[2], Simpan(l.D.KulkasKecil), PetugasBdrs);

        var controller = new BloodStorageLocationController(
            l.LocationService(),
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()))
        {
            ControllerContext = KonteksDengan(PetugasBdrs)
        };

        var ok = Assert.IsType<OkObjectResult>(await controller.GetById(l.D.KulkasBesar));
        var body = Assert.IsType<ApiResponse<QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BloodStorageLocationResponse>>(ok.Value);

        Assert.True(body.Data!.IsActive);
        Assert.Equal(2, body.Data.HeldUnitCount);
    }

    private static BbkBloodUnitController Controller(Lingkungan l, Guid? aktor)
        => new(l.Service(), new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()))
        {
            ControllerContext = KonteksDengan(aktor)
        };

    private static ControllerContext KonteksDengan(Guid? aktor)
    {
        var claims = aktor.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, aktor.Value.ToString()) }
            : Array.Empty<Claim>();

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Uji"))
            }
        };
    }

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
