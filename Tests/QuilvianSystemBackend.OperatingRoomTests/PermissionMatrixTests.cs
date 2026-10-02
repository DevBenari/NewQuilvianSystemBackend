using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Matriks izin modul Operasi (`BE-OPR-011`).
/// </summary>
/// <remarks>
/// <para>
/// Yang diperiksa di sini adalah kontrak izin yang terpasang pada endpoint, bukan jawaban
/// runtime-nya. Pemisahan itu disengaja. Penolakan `403` bergantung pada peran dan pemetaan
/// izin milik lingkungan, yang berbeda antar pemasangan dan tidak boleh dipalsukan di dalam
/// uji. Yang tidak boleh berbeda antar lingkungan adalah bahwa setiap endpoint memang
/// menuntut izin, menuntut login, dan menuntut izin atas sumber daya yang benar.
/// </para>
/// <para>
/// Kegagalan uji ini berarti ada endpoint Operasi yang dapat dicapai tanpa pemeriksaan izin,
/// atau memeriksa izin milik sumber daya lain. Keduanya lubang otorisasi, bukan sekadar
/// ketidakrapian penamaan.
/// </para>
/// </remarks>
public class PermissionMatrixTests
{
    private const string ModulOperasi = "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT";

    private static readonly System.Reflection.Assembly AssemblyOperasi =
        typeof(OperatingRoomCaseService).Assembly;

    private static IEnumerable<Type> Controllers() =>
        AssemblyOperasi.GetTypes()
            .Where(x => x.Namespace?.Contains("OperatingRoomManagement.Controllers") == true)
            .Where(x => typeof(ControllerBase).IsAssignableFrom(x) && !x.IsAbstract);

    private static IEnumerable<MethodInfo> Aksi(Type controller) =>
        controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any());

    public static TheoryData<string> NamaController()
    {
        var data = new TheoryData<string>();
        foreach (var controller in Controllers()) data.Add(controller.FullName!);
        return data;
    }

    [Fact]
    public void Modul_operasi_memiliki_controller_yang_terdeteksi()
    {
        // Menjaga uji di bawah ini dari kemungkinan lulus karena tidak menemukan apa pun.
        Assert.NotEmpty(Controllers());
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_controller_menuntut_login_dan_terdaftar_pada_modul_operasi(string namaController)
    {
        var controller = AssemblyOperasi.GetType(namaController)!;

        Assert.True(controller.GetCustomAttribute<AuthorizeAttribute>() is not null,
            $"{controller.Name} tidak menuntut login.");

        var akses = controller.GetCustomAttribute<AccessControllerAttribute>();
        Assert.True(akses is not null, $"{controller.Name} tidak terdaftar pada modul izin mana pun.");
        Assert.Equal(ModulOperasi, akses!.ModuleCode);
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_menuntut_izin_atas_sumber_daya_operasi(string namaController)
    {
        var controller = AssemblyOperasi.GetType(namaController)!;

        foreach (var aksi in Aksi(controller))
        {
            var izin = aksi.GetCustomAttributes<AccessPermissionAttribute>().ToList();

            Assert.True(izin.Count > 0,
                $"{controller.Name}.{aksi.Name} dapat dipanggil tanpa pemeriksaan izin.");

            foreach (var satu in izin)
            {
                var argumen = satu.Arguments;
                Assert.True(argumen is { Length: >= 2 },
                    $"{controller.Name}.{aksi.Name} memasang izin tanpa sumber daya dan tindakan.");

                var sumberDaya = argumen![0] as string;
                var tindakan = argumen[1] as string;

                Assert.False(string.IsNullOrWhiteSpace(sumberDaya),
                    $"{controller.Name}.{aksi.Name} memeriksa izin tanpa nama sumber daya.");
                Assert.False(string.IsNullOrWhiteSpace(tindakan),
                    $"{controller.Name}.{aksi.Name} memeriksa izin tanpa nama tindakan.");

                // Sumber daya milik modul lain berarti endpoint Operasi dijaga izin orang lain.
                Assert.StartsWith("OperatingRoom", sumberDaya!, StringComparison.Ordinal);
            }
        }
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Endpoint_yang_mengubah_data_tidak_pernah_memakai_izin_baca(string namaController)
    {
        var controller = AssemblyOperasi.GetType(namaController)!;

        foreach (var aksi in Aksi(controller))
        {
            var metode = aksi.GetCustomAttributes<HttpMethodAttribute>()
                .SelectMany(x => x.HttpMethods).Distinct().ToList();

            var mengubah = metode.Any(x =>
                x is "POST" or "PUT" or "PATCH" or "DELETE");
            if (!mengubah) continue;

            foreach (var satu in aksi.GetCustomAttributes<AccessPermissionAttribute>())
            {
                var tindakan = satu.Arguments?[1] as string;
                Assert.False(string.Equals(tindakan, "Read", StringComparison.OrdinalIgnoreCase),
                    $"{controller.Name}.{aksi.Name} mengubah data tetapi hanya menuntut izin baca.");
            }
        }
    }
}
