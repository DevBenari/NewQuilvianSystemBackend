using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodBankProcedure;

/// <summary>
/// Bentuk kontrak endpoint <c>BE-BD-012</c> — tepat empat endpoint kontrak <c>v4</c> — beserta
/// pemetaan hasil service menjadi HTTP status.
/// </summary>
public partial class BloodBankProcedureServiceTests
{
    private const string BaseRoute = "api/v1/health-services/blood-bank-management/blood-bank-procedures";

    [Theory]
    [InlineData(nameof(BbkBloodBankProcedureController.GetAll), "Read", typeof(HttpGetAttribute), null)]
    [InlineData(nameof(BbkBloodBankProcedureController.GetById), "Read", typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData(nameof(BbkBloodBankProcedureController.Create), "Create", typeof(HttpPostAttribute), null)]
    [InlineData(nameof(BbkBloodBankProcedureController.Complete), "Update", typeof(HttpPostAttribute), "{id:guid}/complete")]
    public void Endpoint_MemakaiRouteDanButirHakAksesYangDikunciKontrak(string methodName, string aksi, Type verb, string? template)
    {
        var method = typeof(BbkBloodBankProcedureController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);
        Assert.Equal("BloodBankProcedure", arguments[0]);
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
    /// Tepat empat endpoint — tidak ada <c>GET /options</c>, <c>PUT</c>, <c>PATCH</c>,
    /// <c>DELETE</c>, maupun endpoint penyaluran biaya.
    /// </summary>
    [Fact]
    public void Controller_TepatEmpatEndpointKontrak_TanpaJalurBilling()
    {
        var type = typeof(BbkBloodBankProcedureController);

        Assert.Equal(BaseRoute, type.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.Equal("BloodBankProcedure", type.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName);
        Assert.Equal(
            new[] { "Health Services / Blood Bank Management / Blood Bank Procedure" },
            type.GetCustomAttribute<TagsAttribute>()!.Tags);

        var endpoints = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

        Assert.Equal(4, endpoints.Count);

        Assert.DoesNotContain(endpoints, x =>
            x.GetCustomAttributes<HttpDeleteAttribute>().Any() ||
            x.GetCustomAttributes<HttpPutAttribute>().Any() ||
            x.GetCustomAttributes<HttpPatchAttribute>().Any());

        var templates = endpoints
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(templates, x =>
            x.Contains("options", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("bill", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("charge", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("post", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Controller_PencatatanSah_Menjadi200DenganDetailLengkap()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await Controller(l, PetugasBdrs).Create(Catat(d.OrderVip, d.UjiSilang));

        var ok = Assert.IsType<OkObjectResult>(hasil);
        var body = Assert.IsType<ApiResponse<BloodBankProcedureDetailDto>>(ok.Value);

        Assert.True(body.Success);
        Assert.Equal("TND-00000001", body.Data!.ProcedureNumber);
        Assert.Equal(250_000m, body.Data.TariffAmountSnapshot);
        Assert.Equal("VIP", body.Data.PatientClassName);
        Assert.Equal(PetugasBdrs, body.Data.PerformedByUserId);
        Assert.Equal("Create", Assert.Single(body.Data.Transitions).Action);
    }

    [Fact]
    public async Task Controller_OrderTidakSah_Menjadi400_VAL_BD_026()
    {
        await using var l = await Lingkungan.BuatAsync();

        var hasil = await Controller(l, PetugasBdrs).Create(Catat(Guid.NewGuid(), l.D.UjiSilang));

        AssertGagal(hasil, StatusCodes.Status400BadRequest, PesanVal026);
    }

    [Fact]
    public async Task Controller_TarifTidakTersedia_Menjadi422_VAL_BD_084()
    {
        await using var l = await Lingkungan.BuatAsync();

        var hasil = await Controller(l, PetugasBdrs).Create(Catat(l.D.OrderVip, l.D.TindakanHanyaKelas1));

        AssertGagal(hasil, StatusCodes.Status422UnprocessableEntity, PesanVal084);
    }

    [Fact]
    public async Task Controller_SelesaiDuaKali_Menjadi422_DanTidakAda_Menjadi404()
    {
        await using var l = await Lingkungan.BuatAsync();
        var controller = Controller(l, PetugasBdrs);

        var id = (await l.Service().CreateAsync(Catat(l.D.OrderVip, l.D.UjiSilang), PetugasBdrs)).Entity!.Id;

        var pertama = Assert.IsType<OkObjectResult>(await controller.Complete(id));
        Assert.Equal(
            BbkProcedureStatus.Completed,
            Assert.IsType<ApiResponse<BloodBankProcedureDetailDto>>(pertama.Value).Data!.ProcedureStatus);

        AssertGagal(await controller.Complete(id), StatusCodes.Status422UnprocessableEntity);
        AssertGagal(await controller.Complete(Guid.NewGuid()), StatusCodes.Status404NotFound);
        AssertGagal(await controller.GetById(Guid.NewGuid()), StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Pelaku diturunkan dari klaim login; tanpa klaim, tidak ada tindakan yang lahir maupun selesai.
    /// </summary>
    [Fact]
    public async Task Controller_TanpaKlaimPengguna_TidakAdaTindakanYangLahirMaupunSelesai()
    {
        await using var l = await Lingkungan.BuatAsync();

        AssertGagal(await Controller(l, aktor: null).Create(Catat(l.D.OrderVip, l.D.UjiSilang)), StatusCodes.Status400BadRequest);

        var id = (await l.Service().CreateAsync(Catat(l.D.OrderVip, l.D.UjiSilang), PetugasBdrs)).Entity!.Id;

        AssertGagal(await Controller(l, aktor: null).Complete(id), StatusCodes.Status400BadRequest);
        Assert.Equal(BbkProcedureStatus.Recorded, (await l.Service().GetDetailAsync(id))!.ProcedureStatus);
    }

    [Fact]
    public async Task Controller_Daftar_Menjadi200()
    {
        await using var l = await Lingkungan.BuatAsync();
        await l.Service().CreateAsync(Catat(l.D.OrderVip, l.D.UjiSilang), PetugasBdrs);

        var ok = Assert.IsType<OkObjectResult>(await Controller(l, PetugasBdrs).GetAll(null, l.D.OrderVip, null, null, null, null));
        var body = Assert.IsType<ApiResponse<PagedResult<BloodBankProcedureListDto>>>(ok.Value);

        Assert.Equal(1, body.Data!.TotalData);
    }

    private static BbkBloodBankProcedureController Controller(Lingkungan l, Guid? aktor)
    {
        var claims = aktor.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, aktor.Value.ToString()) }
            : Array.Empty<Claim>();

        return new BbkBloodBankProcedureController(
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
