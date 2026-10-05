using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Matriks izin modul Farmasi pada tingkat HTTP — 23 controller, 137 endpoint.
/// </summary>
/// <remarks>
/// <para>
/// Yang diperiksa di sini kontrak izin yang terpasang pada endpoint, bukan jawaban runtime-nya.
/// Pemisahan itu sama seperti pada Operasi dan disengaja: penolakan <c>403</c> bergantung pada
/// pemetaan peran-ke-izin milik lingkungan, dan memalsukannya di dalam uji hanya membuktikan
/// tiruannya bekerja. Yang tidak boleh berbeda antar lingkungan adalah bahwa setiap endpoint
/// Farmasi memang menuntut login, menuntut izin, dan menuntut izin atas sumber daya Farmasi.
/// </para>
/// <para>
/// Kegagalan uji ini berarti ada endpoint Farmasi yang dapat dicapai tanpa pemeriksaan izin,
/// atau memeriksa izin milik sumber daya modul lain. Farmasi menyentuh obat yang diserahkan ke
/// pasien dan saldo stok, jadi keduanya lubang otorisasi, bukan ketidakrapian penamaan.
/// </para>
/// <para>
/// <b>Mengapa refleksi, bukan pencarian teks.</b> Saat menyiapkan uji ini, pencarian teks
/// <c>^\[Authorize</c> salah melaporkan 15 controller "tanpa login" — padahal tiga di antaranya
/// menuliskannya gabung sebagai <c>[ApiController, Authorize]</c> dan sisanya ter-indentasi.
/// Refleksi membaca atribut yang benar-benar terpasang, sehingga tidak dapat tertipu tata letak.
/// </para>
/// </remarks>
public class PermissionMatrixTests
{
    /// <summary>
    /// Farmasi terdaftar pada <b>dua</b> kode modul, bukan satu. Keduanya ada dan aktif pada
    /// master modul, dan seluruh 23 controller terdaftar (12 + 11), sehingga tidak ada endpoint
    /// yang kehilangan izinnya. Yang diterima di sini kenyataannya, bukan pembenarannya —
    /// akibatnya dicatat pada `MODULE-STATUS.md` sebagai temuan terpisah.
    /// </summary>
    private static readonly string[] ModulFarmasi =
    [
        "HEALTH_SERVICE_PHARMACY",
        "HEALTH_SERVICE_PHARMACY_MANAGEMENT"
    ];

    private static readonly Assembly AssemblyFarmasi = typeof(DrugReturnService).Assembly;

    private static IEnumerable<Type> Controllers() =>
        AssemblyFarmasi.GetTypes()
            .Where(x => x.Namespace?.Contains("PharmacyManagement.Controllers") == true)
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

    private static Type Ambil(string nama) => AssemblyFarmasi.GetType(nama)!;

    // ── Sanity: uji di bawah tidak boleh lulus karena tidak menemukan apa pun ─────────────

    [Fact]
    public void Modul_farmasi_memiliki_controller_yang_terdeteksi()
    {
        Assert.NotEmpty(Controllers());
    }

    /// <summary>
    /// Angkanya dikunci supaya controller baru tidak lolos masuk tanpa ikut diperiksa uji di
    /// bawah. Kalau jumlahnya berubah, yang perlu diperiksa apakah yang baru itu juga dijaga.
    /// </summary>
    [Fact]
    public void Jumlah_controller_dan_endpoint_farmasi_sesuai_yang_tercatat()
    {
        var controllers = Controllers().ToList();
        var endpoint = controllers.Sum(x => Aksi(x).Count());

        Assert.Equal(23, controllers.Count);
        Assert.Equal(137, endpoint);
    }

    // ── Login dan pendaftaran modul ───────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_controller_menuntut_login(string namaController)
    {
        var controller = Ambil(namaController);

        Assert.True(controller.GetCustomAttribute<AuthorizeAttribute>() is not null,
            $"{controller.Name} tidak menuntut login.");
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_controller_terdaftar_pada_salah_satu_modul_farmasi(string namaController)
    {
        var controller = Ambil(namaController);

        var akses = controller.GetCustomAttribute<AccessControllerAttribute>();

        Assert.True(akses is not null,
            $"{controller.Name} tidak terdaftar pada modul izin mana pun.");
        Assert.Contains(akses!.ModuleCode, ModulFarmasi);
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Tidak_ada_controller_farmasi_yang_dibebaskan_dari_login(string namaController)
    {
        var controller = Ambil(namaController);

        Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());

        foreach (var aksi in Aksi(controller))
        {
            Assert.Null(aksi.GetCustomAttribute<AllowAnonymousAttribute>());
        }
    }

    // ── Izin per endpoint ─────────────────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_menuntut_izin(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            Assert.True(aksi.GetCustomAttributes<AccessPermissionAttribute>().Any(),
                $"{controller.Name}.{aksi.Name} dapat dipanggil tanpa pemeriksaan izin.");
        }
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_izin_menyebut_sumber_daya_dan_tindakan(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        foreach (var izin in aksi.GetCustomAttributes<AccessPermissionAttribute>())
        {
            var argumen = izin.Arguments;

            Assert.True(argumen is { Length: >= 2 },
                $"{controller.Name}.{aksi.Name} memasang izin tanpa sumber daya dan tindakan.");

            Assert.False(string.IsNullOrWhiteSpace(argumen![0] as string),
                $"{controller.Name}.{aksi.Name} memeriksa izin tanpa nama sumber daya.");
            Assert.False(string.IsNullOrWhiteSpace(argumen[1] as string),
                $"{controller.Name}.{aksi.Name} memeriksa izin tanpa nama tindakan.");
        }
    }

    /// <summary>
    /// Sumber daya milik modul lain berarti endpoint Farmasi dijaga izin orang lain: mencabut
    /// izin itu akan mematikan Farmasi, dan memberikannya akan membuka Farmasi lewat pintu
    /// modul lain.
    /// </summary>
    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_memakai_sumber_daya_milik_farmasi(string namaController)
    {
        // Kosakata sumber daya Farmasi, diambil dari source dan dikunci di sini supaya sumber
        // daya baru tidak masuk tanpa sengaja.
        string[] awalanFarmasi =
        [
            "Prescription", "Drug", "Medication", "SlidingScale", "Stock"
        ];

        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        foreach (var izin in aksi.GetCustomAttributes<AccessPermissionAttribute>())
        {
            var sumberDaya = (string)izin.Arguments![0]!;

            Assert.True(
                awalanFarmasi.Any(x => sumberDaya.StartsWith(x, StringComparison.Ordinal)),
                $"{controller.Name}.{aksi.Name} memeriksa izin `{sumberDaya}` yang bukan milik Farmasi.");
        }
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Endpoint_baca_tidak_pernah_menuntut_izin_tulis(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var metode = aksi.GetCustomAttributes<HttpMethodAttribute>()
                .SelectMany(x => x.HttpMethods).Distinct().ToList();

            if (!metode.Contains("GET")) continue;

            foreach (var izin in aksi.GetCustomAttributes<AccessPermissionAttribute>())
            {
                var tindakan = (string)izin.Arguments![1]!;

                Assert.False(
                    tindakan is "Create" or "Update" or "Delete",
                    $"{controller.Name}.{aksi.Name} hanya membaca tetapi menuntut izin `{tindakan}`, " +
                    "sehingga peran yang berhak membaca akan tertolak.");
            }
        }
    }

    /// <summary>
    /// Endpoint yang mengubah data tidak boleh cukup dengan izin baca.
    /// </summary>
    /// <remarks>
    /// Satu pengecualian sah dan sengaja didaftarkan di sini:
    /// <c>SlidingScaleExecutionController.Preview</c> memakai <c>POST</c> hanya karena perlu
    /// membawa badan permintaan, sementara <c>PreviewAsync</c> terbukti nol mutasi — tidak ada
    /// <c>SaveChanges</c> maupun <c>Add</c> di dalamnya. Memaksanya menuntut izin tulis akan
    /// menutup pratinjau dosis dari perawat yang hanya berhak membaca, dan itu justru
    /// memperburuk keselamatan.
    ///
    /// Pengecualiannya didaftarkan sebagai satu nama, bukan dilonggarkan untuk semua
    /// <c>Preview</c>, supaya endpoint pengubah data tidak bisa lolos hanya dengan menamai
    /// dirinya "preview".
    /// </remarks>
    [Theory]
    [MemberData(nameof(NamaController))]
    public void Endpoint_pengubah_data_tidak_pernah_cukup_dengan_izin_baca(string namaController)
    {
        (string Controller, string Aksi)[] pengecualian =
        [
            ("SlidingScaleExecutionController", "Preview")
        ];

        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var metode = aksi.GetCustomAttributes<HttpMethodAttribute>()
                .SelectMany(x => x.HttpMethods).Distinct().ToList();

            var mengubah = metode.Any(x => x is "POST" or "PUT" or "PATCH" or "DELETE");
            if (!mengubah) continue;

            if (pengecualian.Contains((controller.Name, aksi.Name))) continue;

            foreach (var izin in aksi.GetCustomAttributes<AccessPermissionAttribute>())
            {
                var tindakan = (string)izin.Arguments![1]!;

                Assert.False(
                    string.Equals(tindakan, "Read", StringComparison.OrdinalIgnoreCase),
                    $"{controller.Name}.{aksi.Name} mengubah data tetapi hanya menuntut izin baca.");
            }
        }
    }

    /// <summary>
    /// Menjaga daftar pengecualian di atas tetap jujur: kalau `Preview` suatu hari mulai
    /// mengubah data, pengecualiannya harus dicabut, bukan diwarisi diam-diam.
    /// </summary>
    [Fact]
    public void Pratinjau_sliding_scale_memang_hanya_membaca()
    {
        var controller = Controllers()
            .Single(x => x.Name == "SlidingScaleExecutionController");

        var preview = Aksi(controller).Single(x => x.Name == "Preview");

        var metode = preview.GetCustomAttributes<HttpMethodAttribute>()
            .SelectMany(x => x.HttpMethods).ToList();

        Assert.Contains("POST", metode);

        var izin = preview.GetCustomAttributes<AccessPermissionAttribute>().Single();
        Assert.Equal("SlidingScaleExecution", (string)izin.Arguments![0]!);
        Assert.Equal("Read", (string)izin.Arguments[1]!);
    }

    // ── Konsistensi penamaan ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Satu endpoint tidak boleh memasang dua izin sekaligus. Dua izin berarti dua keputusan
    /// otorisasi pada satu permintaan, dan filter hanya menolak bila salah satunya menolak —
    /// sehingga mana yang sebenarnya menjaganya menjadi tidak dapat dibaca dari source.
    /// </summary>
    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_memasang_tepat_satu_izin(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            Assert.Single(aksi.GetCustomAttributes<AccessPermissionAttribute>());
        }
    }

    /// <summary>
    /// `AccessAction` mendaftarkan tindakan ke registry izin; `AccessPermission` yang
    /// menegakkannya saat permintaan masuk. Bila keduanya menyebut nama tindakan yang berbeda,
    /// administrator memberi izin atas nama yang tidak pernah diperiksa — izinnya tampak
    /// diberikan tetapi endpointnya tetap tertolak.
    /// </summary>
    [Theory]
    [MemberData(nameof(NamaController))]
    public void Nama_tindakan_pada_pendaftaran_dan_penegakan_izin_sama(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var pendaftaran = aksi.GetCustomAttribute<AccessActionAttribute>();
            if (pendaftaran is null) continue;

            var penegakan = aksi.GetCustomAttributes<AccessPermissionAttribute>().Single();
            var tindakanDitegakkan = (string)penegakan.Arguments![1]!;

            Assert.Equal(pendaftaran.ActionName, tindakanDitegakkan);
        }
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_punya_rute_dan_metode_http(string namaController)
    {
        var controller = Ambil(namaController);

        Assert.NotNull(controller.GetCustomAttribute<RouteAttribute>());

        foreach (var aksi in Aksi(controller))
        {
            var metode = aksi.GetCustomAttributes<HttpMethodAttribute>()
                .SelectMany(x => x.HttpMethods).Distinct().ToList();

            Assert.NotEmpty(metode);

            // Satu aksi dengan dua metode HTTP berbeda membuat satu izin menjaga baca dan
            // tulis sekaligus.
            Assert.Single(metode);
        }
    }
}
