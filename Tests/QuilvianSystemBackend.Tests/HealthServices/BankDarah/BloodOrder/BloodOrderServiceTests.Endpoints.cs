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

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodOrder;

/// <summary>
/// Bentuk kontrak endpoint <c>BE-BD-003</c> dan pemetaan hasil service menjadi HTTP status,
/// sesuai <c>contracts/api-contract.md</c> grup Blood Order dan konvensi kode status kontrak
/// <c>v4</c>.
/// </summary>
/// <remarks>
/// Berkas ini bagian dari kelas yang sama dengan <see cref="BloodOrderServiceTests"/> supaya
/// memakai lingkungan uji dan data semai yang persis sama. <c>LoggerService</c> hanya menulis
/// ke <c>ILogger</c>, sehingga controller dapat diuji utuh memakai <c>NullLogger</c> tanpa
/// meninggalkan berkas atau baris apa pun.
/// </remarks>
public partial class BloodOrderServiceTests
{
    private const string BaseRouteBloodOrder = "api/v1/health-services/blood-bank-management/blood-orders";

    // =====================================================================
    // 9. Bentuk kontrak endpoint
    // =====================================================================

    [Theory]
    [InlineData(nameof(BbkBloodOrderController.GetFilterMetadata), "Read", typeof(HttpGetAttribute), "filters/metadata")]
    [InlineData(nameof(BbkBloodOrderController.GetSummary), "Read", typeof(HttpGetAttribute), "summary")]
    [InlineData(nameof(BbkBloodOrderController.GetAll), "Read", typeof(HttpGetAttribute), null)]
    [InlineData(nameof(BbkBloodOrderController.GetById), "Read", typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData(nameof(BbkBloodOrderController.GetFulfillment), "Read", typeof(HttpGetAttribute), "{id:guid}/fulfillment")]
    [InlineData(nameof(BbkBloodOrderController.GetStatusHistory), "Read", typeof(HttpGetAttribute), "{id:guid}/status-history")]
    [InlineData(nameof(BbkBloodOrderController.Create), "Create", typeof(HttpPostAttribute), null)]
    [InlineData(nameof(BbkBloodOrderController.CreateManual), "Create", typeof(HttpPostAttribute), "manual")]
    [InlineData(nameof(BbkBloodOrderController.ConfirmDuplicate), "Create", typeof(HttpPostAttribute), "confirm-duplicate")]
    [InlineData(nameof(BbkBloodOrderController.Cancel), "Cancel", typeof(HttpPostAttribute), "{id:guid}/cancel")]
    public void Endpoint_MemakaiRouteDanButirHakAksesYangDikunciKontrak(
        string methodName,
        string aksi,
        Type verb,
        string? template)
    {
        var method = typeof(BbkBloodOrderController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);
        Assert.Equal("BloodOrder", arguments[0]);
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

    /// <summary>
    /// Bentuk transaksi, bukan master data: tidak ada <c>GET /options</c>, tidak ada
    /// <c>PATCH /{id}/status</c> generik, tidak ada <c>PUT</c>, dan tidak ada <c>DELETE</c>.
    /// Jumlah endpoint dikunci supaya penambahan tanpa pembaruan kontrak ketahuan.
    /// </summary>
    [Fact]
    public void ControllerOrderDarah_BerbentukTransaksi_DenganBaseRouteDanTagKontrak()
    {
        var type = typeof(BbkBloodOrderController);

        Assert.Equal(BaseRouteBloodOrder, type.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.Equal("BloodOrder", type.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName);
        Assert.Equal(
            new[] { "Health Services / Blood Bank Management / Blood Order" },
            type.GetCustomAttribute<TagsAttribute>()!.Tags);

        var endpoints = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

        Assert.Equal(10, endpoints.Count);

        Assert.DoesNotContain(endpoints, x =>
            x.GetCustomAttributes<HttpDeleteAttribute>().Any() ||
            x.GetCustomAttributes<HttpPutAttribute>().Any() ||
            x.GetCustomAttributes<HttpPatchAttribute>().Any());

        var templates = endpoints
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("options", templates);
        Assert.DoesNotContain("{id:guid}/status", templates);
    }

    /// <summary>
    /// <c>DEC-BD-044</c> pada tingkat penegakan — pembatalan memakai butir <c>Cancel</c>
    /// sendiri, bukan <c>Update</c>, dan butir itu tetap muncul di layar Akses Role karena
    /// <c>AccessType</c>-nya salah satu dari empat kolom yang ditampilkan.
    /// </summary>
    [Fact]
    public void ButirPembatalan_TerpisahDariUpdate_DanDapatDiberikanDiLayarAksesRole()
    {
        var cancel = typeof(BbkBloodOrderController).GetMethod(nameof(BbkBloodOrderController.Cancel))!;
        var action = cancel.GetCustomAttribute<AccessActionAttribute>()!;

        Assert.Equal("Cancel", action.ActionName);
        Assert.Equal(AccessTypes.Update, action.AccessType);
        Assert.Contains(action.AccessType, AccessTypes.AllowedForRoleAccess);
        Assert.False(action.IsSystemOnly);
        Assert.True(action.VisibleInRoleAccess);
    }

    // =====================================================================
    // 10. Pemetaan hasil service menjadi HTTP status
    // =====================================================================

    [Fact]
    public async Task Controller_OrderSah_Menjadi200DenganDetailLengkap()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await Controller(l, PetugasUnit).Create(OrderPrc(d));

        var ok = Assert.IsType<OkObjectResult>(hasil);
        var body = Assert.IsType<ApiResponse<BloodOrderDetailDto>>(ok.Value);

        Assert.True(body.Success);
        Assert.Equal("ORD-00000001", body.Data!.OrderNumber);
        Assert.Equal("Pasien A", body.Data.PatientName);
        Assert.Equal("dr. Peminta", body.Data.RequestingDoctorName);
        Assert.Equal(PetugasUnit, body.Data.InputByUserId);
        Assert.Equal("Create", Assert.Single(body.Data.Transitions).Action);
    }

    [Fact]
    public async Task Controller_UnitTanpaKewenangan_Menjadi403_VAL_BD_013()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await Controller(l, PetugasUnit).Create(
            Order(d.PasienA, d.KunjunganRi001, d.UnitTakBerwenang, d.DokterPeminta, (d.Prc, 2)));

        AssertGagal(hasil, StatusCodes.Status403Forbidden, PesanVal013);
    }

    [Fact]
    public async Task Controller_OrderGanda_Menjadi422_VAL_BD_001()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        await l.Service().CreateAsync(OrderPrc(d), PetugasUnit);

        var hasil = await Controller(l, PetugasUnit).Create(OrderPrc(d));

        AssertGagal(hasil, StatusCodes.Status422UnprocessableEntity, PesanVal001);
    }

    [Fact]
    public async Task Controller_OrderManualTakLengkap_Menjadi400_VAL_BD_010()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var request = Isi(
            new CreateManualBloodOrderRequest(),
            d.PasienA, Guid.Empty, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) });

        var hasil = await Controller(l, PetugasBdrs).CreateManual(request);

        AssertGagal(hasil, StatusCodes.Status400BadRequest, PesanVal010);
    }

    [Fact]
    public async Task Controller_PembatalanTokenUsang_Menjadi409()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var order = (await l.Service().CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var hasil = await Controller(l, PetugasBdrs).Cancel(
            order.Id,
            new CancelBloodOrderRequest { ReasonCode = AlasanOperasional, Version = 3 });

        AssertGagal(hasil, StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Controller_KategoriAlasanTidakSesuaiPelaku_Menjadi422_VAL_BD_083()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var order = (await l.Service().CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var hasil = await Controller(l, PetugasBdrs).Cancel(order.Id, Batal(AlasanKlinis));

        AssertGagal(hasil, StatusCodes.Status422UnprocessableEntity, PesanVal083);
    }

    [Fact]
    public async Task Controller_PembatalanTanpaAlasanTerkendali_Menjadi400_VAL_BD_016()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var order = (await l.Service().CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var hasil = await Controller(l, PetugasBdrs).Cancel(order.Id, Batal("KETIKAN-BEBAS"));

        AssertGagal(hasil, StatusCodes.Status400BadRequest, PesanVal016);
    }

    [Fact]
    public async Task Controller_OrderTidakAda_Menjadi404PadaSeluruhJalurBacaDanBatal()
    {
        await using var l = await Lingkungan.BuatAsync();
        var controller = Controller(l, PetugasBdrs);
        var acak = Guid.NewGuid();

        AssertGagal(await controller.GetById(acak), StatusCodes.Status404NotFound);
        AssertGagal(await controller.GetFulfillment(acak), StatusCodes.Status404NotFound);
        AssertGagal(await controller.GetStatusHistory(acak), StatusCodes.Status404NotFound);
        AssertGagal(await controller.Cancel(acak, Batal(AlasanOperasional)), StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Pelaku diturunkan dari klaim pengguna terautentikasi. Tanpa klaim itu, tidak ada order
    /// yang lahir — tidak ada jalan menyimpan order tanpa jejak pembuatnya (<c>VAL-BD-011</c>).
    /// </summary>
    [Fact]
    public async Task Controller_TanpaKlaimPengguna_OrderTidakTersimpan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await Controller(l, aktor: null).Create(OrderPrc(d));

        AssertGagal(hasil, StatusCodes.Status400BadRequest);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    // =====================================================================
    // Penolong endpoint
    // =====================================================================

    private static BbkBloodOrderController Controller(Lingkungan l, Guid? aktor)
    {
        var claims = aktor.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, aktor.Value.ToString()) }
            : Array.Empty<Claim>();

        return new BbkBloodOrderController(
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
