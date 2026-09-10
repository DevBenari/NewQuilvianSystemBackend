using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.Platform;

/// <summary>
/// Bukti untuk <c>PLT-BE-005</c> — layar pemantauan deret nomor, kontrak <c>v1</c>
/// (<c>api-contract.md</c> §2, <c>permission-audit-matrix.md</c> §1).
/// </summary>
/// <remarks>
/// <para>
/// <b>Kenapa berbentuk contract test, bukan seeder.</b> <c>Seeders/AccessMenuSeeder.cs</c> adalah
/// satu-satunya penulis <c>SysControllerAccess</c> dan <c>SysActionAccess</c> di production, dan
/// ia bekerja murni lewat refleksi atas controller yang benar-benar ada dan ter-routing. Menulis
/// daftar permission tangan akan melahirkan sumber kebenaran kedua.
/// </para>
/// <para>
/// <b>Kenapa pengujian lewat Swagger tidak cukup.</b>
/// <c>AccessPermissionService.HasAccessAsync</c> memulangkan <c>true</c> untuk SuperAdmin sebelum
/// satu baris hak akses pun dibaca. Cacat penamaan karena itu <b>tidak terlihat</b> saat dicoba
/// memakai akun SuperAdmin — persis yang meloloskan sembilan pasangan rusak di modul Rawat Inap
/// selama berbulan-bulan (<c>BE-RWI-034</c>).
/// </para>
/// </remarks>
public class NumberSeriesRoleAccessContractTests
{
    private static readonly Type[] ModuleControllers =
    {
        typeof(NumberSeriesController)
    };

    /// <summary>
    /// Seluruh pasangan hak akses modul ini pada kontrak <c>v1</c>. Satu Resource, satu Action —
    /// modul ini memang sesederhana itu dari sisi kewenangan, karena kemampuan intinya tidak
    /// dipaparkan kepada pengguna sama sekali.
    /// </summary>
    public static readonly (string Resource, string Action)[] KontrakV1 =
    {
        ("NumberSeries", "Read")
    };

    // =====================================================================
    // Bagian 1 — Kontrak penamaan
    // =====================================================================

    /// <summary>
    /// Argumen pertama <c>[AccessPermission]</c> wajib sama persis dengan <c>ControllerName</c>
    /// pada <c>[AccessController]</c>. Menyimpang berarti filter mencari resource yang tidak
    /// pernah didaftarkan seeder, dan hasilnya <b>403 permanen</b> yang tidak dapat diperbaiki
    /// dari layar Akses Role.
    /// </summary>
    [Fact]
    public void ResourcePadaPermission_SamaPersisDenganControllerName()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>();

            Assert.NotNull(controllerAttribute);

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (permission == null)
                    continue;

                var resource = ResourceOf(permission);

                if (!string.Equals(resource, controllerAttribute!.ControllerName, StringComparison.Ordinal))
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memakai resource '{resource}', " +
                        $"sedangkan ControllerName-nya '{controllerAttribute.ControllerName}'.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Argumen kedua <c>[AccessPermission]</c> wajib sama persis dengan argumen pertama
    /// <c>[AccessAction]</c> pada method yang sama.
    /// </summary>
    [Fact]
    public void AksiPadaPermission_SamaPersisDenganActionNamePadaMethodYangSama()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (permission == null || action == null)
                    continue;

                var aksi = ActionOf(permission);

                if (!string.Equals(aksi, action.ActionName, StringComparison.Ordinal))
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memeriksa aksi '{aksi}', " +
                        $"sedangkan [AccessAction] mendaftarkan '{action.ActionName}'.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Setiap endpoint wajib punya kedua atribut. Endpoint tanpa <c>[AccessPermission]</c> hanya
    /// terlindungi <c>[Authorize]</c>, artinya siapa pun yang punya login dapat memanggilnya.
    /// </summary>
    [Fact]
    public void TidakAdaEndpointYangHanyaTerlindungiAuthorize()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                if (endpoint.GetCustomAttribute<AccessPermissionAttribute>() == null)
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name} tanpa [AccessPermission].");

                if (endpoint.GetCustomAttribute<AccessActionAttribute>() == null)
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name} tanpa [AccessAction].");
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Butir yang <c>AccessType</c>-nya di luar keempat kolom tidak akan muncul di layar Akses
    /// Role, sehingga tidak dapat diberikan kepada siapa pun.
    /// </summary>
    [Fact]
    public void SetiapAksi_MunculDanDapatDiberikanDiLayarAksesRole()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;

            Assert.False(controllerAttribute.IsSystemOnly, controllerType.Name);
            Assert.True(controllerAttribute.VisibleInRoleAccess, controllerType.Name);

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (action == null)
                {
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name} belum diberi [AccessAction].");
                    continue;
                }

                if (!AccessTypes.AllowedForRoleAccess.Contains(action.AccessType))
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memakai AccessType " +
                        $"'{action.AccessType}' yang tidak ditampilkan layar Akses Role.");
                }

                if (action.IsSystemOnly || !action.VisibleInRoleAccess)
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} disembunyikan dari layar Akses Role.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Butir <c>NumberSeries : Read</c> benar-benar lahir dari controller yang ada, sehingga
    /// administrator dapat mencentangnya di layar Akses Role.
    /// </summary>
    [Fact]
    public void ButirKontrakV1_SeluruhnyaLahirDariControllerYangAda()
    {
        var terdaftar = PasanganYangDidaftarkanSeeder();

        var hilang = KontrakV1
            .Where(x => !terdaftar.Contains((x.Resource, x.Action)))
            .Select(x => $"{x.Resource} : {x.Action}")
            .ToList();

        Assert.True(hilang.Count == 0, "Butir kontrak yang belum lahir: " + string.Join(", ", hilang));
    }

    // =====================================================================
    // Bagian 2 — Penjaga permukaan baca-saja
    // =====================================================================

    /// <summary>
    /// <c>permission-audit-matrix.md</c> §1 — modul ini <b>hanya</b> punya butir <c>Read</c>.
    /// Munculnya butir <c>Create</c>, <c>Update</c>, atau <c>Delete</c> berarti ada endpoint tulis
    /// yang menyelinap masuk, dan setiap endpoint tulis pada modul ini melanggar
    /// <c>INV-PLT-001</c> atau <c>INV-PLT-002</c>.
    /// </summary>
    [Fact]
    public void ModulHanyaMendaftarkanButirRead_NolButirTulis()
    {
        var butirTulis = PasanganYangDidaftarkanSeeder()
            .Where(x => !string.Equals(x.Item2, "Read", StringComparison.Ordinal))
            .Select(x => $"{x.Item1} : {x.Item2}")
            .ToList();

        Assert.True(
            butirTulis.Count == 0,
            "Modul Number Series hanya boleh punya butir Read (permission-audit-matrix §1), " +
            "tetapi ditemukan: " + string.Join(", ", butirTulis));
    }

    /// <summary>
    /// <c>api-contract.md</c> §2 — keempat endpoint seluruhnya <c>GET</c>. Penjaga terhadap
    /// penambahan endpoint tulis di kemudian hari, ketika alasan ketiadaannya sudah tidak diingat
    /// lagi oleh yang menyuntingnya.
    /// </summary>
    /// <remarks>
    /// Yang dijaga di sini bukan kerapian, melainkan <c>INV-PLT-001</c>: sebuah
    /// <c>PATCH /{id}</c> yang menyunting <c>CurrentValue</c> akan menerbitkan ulang nomor yang
    /// sudah menempel pada order, invoice, atau tindakan milik orang lain.
    /// </remarks>
    [Fact]
    public void SeluruhEndpoint_HanyaGet_NolPostPutPatchDelete()
    {
        var pelanggar = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var metode = endpoint
                    .GetCustomAttributes<HttpMethodAttribute>()
                    .SelectMany(x => x.HttpMethods)
                    .Distinct()
                    .ToList();

                var selainGet = metode
                    .Where(x => !string.Equals(x, "GET", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (selainGet.Count > 0)
                {
                    pelanggar.Add(
                        $"{controllerType.Name}.{endpoint.Name} memakai {string.Join("/", selainGet)}.");
                }
            }
        }

        Assert.True(
            pelanggar.Count == 0,
            "Modul Number Series tidak boleh punya endpoint tulis (INV-PLT-001, INV-PLT-002), " +
            "tetapi ditemukan: " + string.Join(", ", pelanggar));
    }

    /// <summary>
    /// Keempat endpoint kontrak <c>v1</c> benar-benar ada, dan <c>GET /options</c> tidak ikut
    /// terbawa dari kebiasaan master data — deret bukan isi kotak pilihan.
    /// </summary>
    [Fact]
    public void EmpatEndpointKontrak_AdaPersisTanpaOptions()
    {
        var rute = EndpointsOf(typeof(NumberSeriesController))
            .Select(RouteTemplateOf)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            new[] { "", "filters/metadata", "summary", "{id:guid}" }.OrderBy(x => x, StringComparer.Ordinal),
            rute);

        Assert.DoesNotContain("options", rute);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static string ResourceOf(AccessPermissionAttribute permission)
        => (string)permission.Arguments![0]!;

    private static string ActionOf(AccessPermissionAttribute permission)
        => (string)permission.Arguments![1]!;

    private static string RouteTemplateOf(MethodInfo endpoint)
        => endpoint.GetCustomAttributes<HttpMethodAttribute>().FirstOrDefault()?.Template
           ?? string.Empty;

    private static List<MethodInfo> EndpointsOf(Type controllerType)
        => controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

    /// <summary>
    /// Pasangan yang <b>akan</b> dibuat <c>AccessMenuSeeder</c>, dihitung dengan aturan yang sama:
    /// <c>ControllerName</c> dari <c>[AccessController]</c> dipasangkan dengan argumen pertama
    /// <c>[AccessAction]</c>.
    /// </summary>
    private static HashSet<(string, string)> PasanganYangDidaftarkanSeeder()
    {
        var hasil = new HashSet<(string, string)>();

        foreach (var controllerType in ModuleControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>();

            if (controllerAttribute == null)
                continue;

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (action != null)
                    hasil.Add((controllerAttribute.ControllerName!, action.ActionName));
            }
        }

        return hasil;
    }
}
