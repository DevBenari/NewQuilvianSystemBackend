using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Menjaga bentuk <c>InpatientEpisodeController</c> terhadap api contract <c>0.4.0</c> bagian
/// Inpatient Episode dan permission matrix bagian 1.1 dan 2.1.
/// </summary>
/// <remarks>
/// Diuji lewat atribut, bukan lewat permintaan HTTP sungguhan, dengan alasan yang sama seperti
/// pada <c>InpatientMasterDataControllerContractTests</c>: hak akses di repository ini bekerja
/// dari atribut, dan endpoint yang lupa diberi atribut tidak menghasilkan galat apa pun — ia
/// justru terbuka untuk siapa saja yang sudah masuk.
///
/// Yang TIDAK dibuktikan di sini: bahwa permintaan tanpa hak akses benar-benar dibalas 403.
/// Itu memerlukan aplikasi berjalan beserta basis datanya, dan tercatat pada laporan task
/// sebagai verifikasi runtime yang masih tertunda.
/// </remarks>
public sealed class InpatientEpisodeControllerContractTests
{
    [Fact]
    public void MemakaiRouteDanGrupSwaggerSesuaiKontrak()
    {
        var type = typeof(InpatientEpisodeController);

        Assert.Equal(
            "api/v1/health-services/inpatient-management/episodes",
            type.GetCustomAttribute<RouteAttribute>()!.Template);

        Assert.Equal(
            "Health Services / Inpatient Management / Inpatient Episode",
            type.GetCustomAttribute<TagsAttribute>()!.Tags.Single());

        var access = type.GetCustomAttribute<AccessControllerAttribute>()!;

        Assert.Equal("HEALTH_SERVICE_INPATIENT", access.ModuleCode);
        Assert.Equal("InpatientEpisode", access.ControllerName);
        Assert.Equal("HealthServices", access.AreaName);

        Assert.NotNull(type.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
    }

    [Fact]
    public void MenyediakanTepatTigaEndpointYangDibukaBeRwi007DanBeRwi008()
    {
        // Endpoint baca — daftar, detail, ringkasan, penyaring, dan riwayat status — milik
        // BE-RWI-009 dan BE-RWI-028, dan sengaja belum ada di sini.
        Assert.Equal(3, EndpointsOf(typeof(InpatientEpisodeController)).Count);
    }

    [Fact]
    public void SetiapEndpointDiberiAtributHakAkses()
    {
        foreach (var endpoint in EndpointsOf(typeof(InpatientEpisodeController)))
        {
            Assert.True(
                endpoint.GetCustomAttribute<AccessActionAttribute>() != null,
                $"InpatientEpisodeController.{endpoint.Name} belum diberi [AccessAction].");

            Assert.True(
                endpoint.GetCustomAttribute<AccessPermissionAttribute>() != null,
                $"InpatientEpisodeController.{endpoint.Name} belum diberi [AccessPermission].");
        }
    }

    [Fact]
    public void MembukaAdmisiMemakaiHakAksesCreate_SisanyaUpdate()
    {
        var endpoints = EndpointsOf(typeof(InpatientEpisodeController));

        var create = endpoints
            .Where(x => x.GetCustomAttribute<HttpPostAttribute>() != null)
            .ToList();

        Assert.Single(create);
        Assert.Equal("Create", PermissionActionOf(create[0]));

        foreach (var endpoint in endpoints.Except(create))
        {
            Assert.Equal("Update", PermissionActionOf(endpoint));
        }

        foreach (var endpoint in endpoints)
        {
            Assert.Equal("InpatientEpisode", PermissionResourceOf(endpoint));
        }
    }

    /// <remarks>
    /// <c>AccessPermissionAttribute</c> menyimpan pasangan controller dan action pada
    /// <c>Arguments</c>, bukan pada properti bernama, karena ia adalah
    /// <c>TypeFilterAttribute</c>. Dibaca dari sana supaya test menguji nilai yang benar-benar
    /// dipakai <c>AccessPermissionFilter</c> saat permintaan masuk.
    /// </remarks>
    private static string PermissionResourceOf(MethodInfo endpoint)
        => (string)endpoint.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments![0]!;

    private static string PermissionActionOf(MethodInfo endpoint)
        => (string)endpoint.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments![1]!;

    [Fact]
    public void VerbSetiapEndpointSesuaiKontrak()
    {
        var endpoints = EndpointsOf(typeof(InpatientEpisodeController));

        Assert.Single(endpoints.Where(x => x.GetCustomAttribute<HttpPostAttribute>() != null));
        Assert.Single(endpoints.Where(x => x.GetCustomAttribute<HttpPutAttribute>() != null));
        Assert.Single(endpoints.Where(x => x.GetCustomAttribute<HttpPatchAttribute>() != null));
        Assert.Empty(endpoints.Where(x => x.GetCustomAttribute<HttpGetAttribute>() != null));
        Assert.Empty(endpoints.Where(x => x.GetCustomAttribute<HttpDeleteAttribute>() != null));
    }

    /// <remarks>
    /// <c>RWI-RULE-031</c> aturan 4: setiap perpindahan status punya endpoint bermakna
    /// sendiri. Endpoint bergaya <c>PATCH /episodes/{id}/status</c> yang menerima nilai bebas
    /// akan melubangi riwayat status, dan seluruh laporan yang dibaca dari riwayat itu ikut
    /// salah tanpa ada yang menyadarinya. Test ini menahan endpoint seperti itu dibuat orang
    /// lain kelak tanpa menyadari akibatnya.
    /// </remarks>
    [Fact]
    public void TidakAdaEndpointYangMenyetelStatusSecaraBebas()
    {
        var templates = EndpointsOf(typeof(InpatientEpisodeController))
            .SelectMany(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(
            templates,
            x => x.Contains("status", StringComparison.OrdinalIgnoreCase));
    }

    /// <remarks>QBE-SVC-001: controller tidak menyentuh <c>ApplicationDbContext</c>.</remarks>
    [Fact]
    public void ControllerTidakMenerimaApplicationDbContext()
    {
        var parameterTypes = typeof(InpatientEpisodeController)
            .GetConstructors()
            .SelectMany(x => x.GetParameters())
            .Select(x => x.ParameterType)
            .ToList();

        Assert.DoesNotContain(
            typeof(QuilvianSystemBackend.Repositories.ApplicationDbContext),
            parameterTypes);
    }

    private static List<MethodInfo> EndpointsOf(Type controllerType)
        => controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .Where(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any())
            .ToList();
}
