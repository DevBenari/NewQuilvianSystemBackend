using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Attributes;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Menjaga bentuk kedua controller master Rawat Inap terhadap api contract <c>0.4.0</c> dan
/// permission matrix bagian 2.5.
/// </summary>
/// <remarks>
/// Kenapa diuji lewat atribut, bukan lewat permintaan HTTP sungguhan. Hak akses di
/// repository ini bekerja dari atribut: <c>AccessMenuSeeder</c> menyisir seluruh endpoint
/// saat aplikasi menyala lalu membuat butir haknya, dan <c>AccessPermissionFilter</c>
/// memeriksa pasangan controller dan action saat permintaan masuk. Endpoint yang lupa diberi
/// atribut tidak menghasilkan galat apa pun — ia justru terbuka untuk siapa saja yang sudah
/// masuk. Test ini menutup celah itu pada waktu build.
///
/// Yang TIDAK dibuktikan di sini: bahwa permintaan tanpa hak akses benar-benar dibalas 403.
/// Itu memerlukan aplikasi berjalan beserta basis datanya, dan tercatat pada laporan task
/// sebagai verifikasi runtime yang masih tertunda.
/// </remarks>
public sealed class InpatientMasterDataControllerContractTests
{
    [Fact]
    public void InpatientSettingController_MemakaiRouteDanGrupSwaggerSesuaiKontrak()
    {
        var type = typeof(InpatientSettingController);

        Assert.Equal(
            "api/v1/health-services/master-data/inpatient-settings",
            type.GetCustomAttribute<RouteAttribute>()!.Template);

        Assert.Equal(
            "Health Services / Master Data / Inpatient Setting",
            type.GetCustomAttribute<TagsAttribute>()!.Tags.Single());

        var access = type.GetCustomAttribute<AccessControllerAttribute>()!;
        Assert.Equal("HEALTH_SERVICE_MASTER_DATA", access.ModuleCode);
        Assert.Equal("InpatientSetting", access.ControllerName);

        Assert.NotNull(type.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
    }

    [Fact]
    public void InpatientClearanceItemController_MemakaiRouteDanGrupSwaggerSesuaiKontrak()
    {
        var type = typeof(InpatientClearanceItemController);

        Assert.Equal(
            "api/v1/health-services/master-data/inpatient-clearance-items",
            type.GetCustomAttribute<RouteAttribute>()!.Template);

        Assert.Equal(
            "Health Services / Master Data / Inpatient Clearance Item",
            type.GetCustomAttribute<TagsAttribute>()!.Tags.Single());

        var access = type.GetCustomAttribute<AccessControllerAttribute>()!;
        Assert.Equal("HEALTH_SERVICE_MASTER_DATA", access.ModuleCode);
        Assert.Equal("InpatientClearanceItem", access.ControllerName);

        Assert.NotNull(type.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
    }

    [Fact]
    public void ControllerMenyediakanVarianSingletonDanSembilanEndpointBaseline()
    {
        Assert.Equal(2, EndpointsOf(typeof(InpatientSettingController)).Count);
        Assert.Equal(9, EndpointsOf(typeof(InpatientClearanceItemController)).Count);
    }

    [Fact]
    public void SetiapEndpointDiberiAtributHakAkses()
    {
        foreach (var type in new[]
                 {
                     typeof(InpatientSettingController),
                     typeof(InpatientClearanceItemController)
                 })
        {
            foreach (var endpoint in EndpointsOf(type))
            {
                Assert.True(
                    endpoint.GetCustomAttribute<AccessActionAttribute>() != null,
                    $"{type.Name}.{endpoint.Name} belum diberi [AccessAction].");

                Assert.True(
                    endpoint.GetCustomAttribute<AccessPermissionAttribute>() != null,
                    $"{type.Name}.{endpoint.Name} belum diberi [AccessPermission].");
            }
        }
    }

    [Fact]
    public void InpatientSettingController_TidakPunyaEndpointMenambahBaris()
    {
        // Tabel pengaturan dipakai sebagai satu baris tunggal berkode DEFAULT. Baris kedua
        // akan membuat modul membaca angka yang berbeda dari yang disetel admin pada layar
        // ini, dan tidak ada satu pun layar yang menampilkan hal itu sebagai kesalahan.
        //
        // Test ini menahan endpoint tambah yang mungkin dibuat orang lain kelak tanpa
        // menyadari akibatnya.
        var type = typeof(InpatientSettingController);

        Assert.Empty(EndpointsOf(type).Where(x => x.GetCustomAttribute<HttpPostAttribute>() != null));
        Assert.Empty(EndpointsOf(type).Where(x => x.GetCustomAttribute<HttpDeleteAttribute>() != null));
    }

    [Fact]
    public void SetiapEndpointYangMengubahData_MemakaiVerbYangSesuaiKontrak()
    {
        var setting = EndpointsOf(typeof(InpatientSettingController));

        Assert.Single(setting.Where(x => x.GetCustomAttribute<HttpGetAttribute>() != null));
        Assert.Single(setting.Where(x => x.GetCustomAttribute<HttpPutAttribute>() != null));

        var clearance = EndpointsOf(typeof(InpatientClearanceItemController));

        Assert.Equal(5, clearance.Count(x => x.GetCustomAttribute<HttpGetAttribute>() != null));
        Assert.Single(clearance.Where(x => x.GetCustomAttribute<HttpPostAttribute>() != null));
        Assert.Single(clearance.Where(x => x.GetCustomAttribute<HttpPutAttribute>() != null));
        Assert.Single(clearance.Where(x => x.GetCustomAttribute<HttpPatchAttribute>() != null));
        Assert.Single(clearance.Where(x => x.GetCustomAttribute<HttpDeleteAttribute>() != null));
    }

    /// <remarks>
    /// QBE-SVC-001: controller tidak boleh menyentuh <c>ApplicationDbContext</c> langsung.
    /// Seluruh pembacaan dan perubahan tabel master lewat service pemiliknya.
    /// </remarks>
    [Fact]
    public void KeduaControllerTidakMenerimaApplicationDbContext()
    {
        foreach (var type in new[]
                 {
                     typeof(InpatientSettingController),
                     typeof(InpatientClearanceItemController)
                 })
        {
            var parameterTypes = type
                .GetConstructors()
                .SelectMany(x => x.GetParameters())
                .Select(x => x.ParameterType)
                .ToList();

            Assert.DoesNotContain(
                typeof(QuilvianSystemBackend.Repositories.ApplicationDbContext),
                parameterTypes);
        }
    }

    private static List<MethodInfo> EndpointsOf(Type controllerType)
        => controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .Where(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any())
            .ToList();
}
