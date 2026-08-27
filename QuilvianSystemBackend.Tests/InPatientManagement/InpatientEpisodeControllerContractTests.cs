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

    /// <remarks>
    /// Lima belas endpoint, yaitu seluruh baris bagian Inpatient Episode pada api contract
    /// <c>0.4.0</c>: empat endpoint baca dari <c>BE-RWI-009</c>, tiga endpoint tulis dari
    /// <c>BE-RWI-007</c> dan <c>BE-RWI-008</c>, satu penetapan kebutuhan isolasi dari
    /// <c>BE-RWI-014</c>, dua penugasan DPJP dari <c>BE-RWI-017</c>, dua penugasan perawat dari
    /// <c>BE-RWI-018</c>, satu riwayat status dari <c>BE-RWI-028</c>, dan dua sesi koreksi dari
    /// <c>BE-RWI-030</c>.
    /// </remarks>
    [Fact]
    public void MenyediakanLimaBelasEndpointSesuaiApiContract()
    {
        Assert.Equal(15, EndpointsOf(typeof(InpatientEpisodeController)).Count);
    }

    /// <remarks>
    /// Riwayat status hanya dapat dibaca. Ketiadaan endpoint yang mengubah maupun menghapusnya
    /// adalah bagian dari kontrak — api contract bagian 8 dan <c>RWI-RULE-031</c> aturan 5 —
    /// bukan pekerjaan yang belum sempat dikerjakan.
    /// </remarks>
    [Fact]
    public void RiwayatStatusHanyaDapatDibaca()
    {
        var riwayat = EndpointsOf(typeof(InpatientEpisodeController))
            .Where(x => x
                .GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
                .Any(a => (a.Template ?? string.Empty)
                    .Contains("status-history", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var satuSatunya = Assert.Single(riwayat);

        Assert.NotNull(satuSatunya.GetCustomAttribute<HttpGetAttribute>());
        Assert.Equal("Read", PermissionActionOf(satuSatunya));
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

    /// <remarks>
    /// Pemetaan butir hak akses mengikuti permission matrix bagian 2.1 apa adanya: seluruh
    /// pembacaan memakai <c>Read</c>, pembukaan admisi memakai <c>Create</c>, penetapan
    /// kebutuhan isolasi memakai <c>SetIsolation</c>, dan sisanya <c>Update</c>. Butir
    /// <c>SetIsolation</c> sengaja terpisah karena ia diberikan kepada dokter, sementara
    /// <c>Update</c> diberikan kepada petugas admisi dan kepala ruangan.
    /// </remarks>
    [Fact]
    public void PemetaanButirHakAksesSesuaiPermissionMatrix()
    {
        var endpoints = EndpointsOf(typeof(InpatientEpisodeController));

        foreach (var endpoint in endpoints)
        {
            Assert.Equal("InpatientEpisode", PermissionResourceOf(endpoint));
        }

        var reads = endpoints
            .Where(x => x.GetCustomAttribute<HttpGetAttribute>() != null)
            .ToList();

        Assert.Equal(7, reads.Count);

        foreach (var endpoint in reads)
        {
            Assert.Equal("Read", PermissionActionOf(endpoint));
        }

        var creates = endpoints
            .Where(x => x.GetCustomAttribute<HttpPostAttribute>() != null)
            .Where(x => PermissionActionOf(x) == "Create")
            .ToList();

        Assert.Single(creates);

        var isolation = endpoints
            .Single(x => PermissionActionOf(x) == "SetIsolation");

        Assert.NotNull(isolation.GetCustomAttribute<HttpPatchAttribute>());

        // Sesi koreksi memakai butir tersendiri, bukan Update. Butir Reopen diberikan hanya
        // kepada supervisor; Update diberikan kepada petugas admisi dan kepala ruangan.
        var reopen = endpoints.Where(x => PermissionActionOf(x) == "Reopen").ToList();

        Assert.Equal(2, reopen.Count);

        var sisanya = endpoints
            .Except(reads)
            .Except(creates)
            .Except(new[] { isolation })
            .Except(reopen);

        foreach (var endpoint in sisanya)
        {
            Assert.Equal("Update", PermissionActionOf(endpoint));
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

        // 7 GET: filters/metadata, summary, daftar, detail, riwayat DPJP, riwayat perawat,
        // riwayat status.
        Assert.Equal(7, endpoints.Count(x => x.GetCustomAttribute<HttpGetAttribute>() != null));

        // 4 POST: buka admisi, alihkan DPJP, tugaskan perawat, buka sesi koreksi.
        Assert.Equal(4, endpoints.Count(x => x.GetCustomAttribute<HttpPostAttribute>() != null));

        // 1 PUT: ubah isian admisi.
        Assert.Single(endpoints.Where(x => x.GetCustomAttribute<HttpPutAttribute>() != null));

        // 3 PATCH: batalkan admisi, tetapkan kebutuhan isolasi, tutup sesi koreksi.
        Assert.Equal(3, endpoints.Count(x => x.GetCustomAttribute<HttpPatchAttribute>() != null));

        // Tidak ada DELETE. Episode tidak pernah dihapus, hanya dibatalkan atau ditutup.
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
        // Hanya rute yang MENULIS yang diperiksa. GET /{id}/status-history memang memuat kata
        // "status", tetapi ia pembacaan — dan justru pembacaan itulah yang menjadi bukti bahwa
        // riwayatnya utuh.
        var ruteTulis = EndpointsOf(typeof(InpatientEpisodeController))
            .Where(x =>
                x.GetCustomAttribute<HttpGetAttribute>() == null)
            .SelectMany(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(
            ruteTulis,
            x => x.EndsWith("/status", StringComparison.OrdinalIgnoreCase) ||
                 x.Equals("status", StringComparison.OrdinalIgnoreCase));
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
