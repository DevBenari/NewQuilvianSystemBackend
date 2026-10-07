using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Matriks izin modul Gizi pada tingkat HTTP — 6 controller, 45 endpoint.
/// </summary>
/// <remarks>
/// <para>
/// Yang diperiksa di sini kontrak izin yang terpasang pada endpoint, bukan jawaban runtime-nya.
/// Penolakan <c>403</c> bergantung pada pemetaan peran-ke-izin milik lingkungan, dan
/// memalsukannya di dalam uji hanya membuktikan tiruannya bekerja. Yang tidak boleh berbeda
/// antar lingkungan adalah bahwa setiap endpoint Gizi memang menuntut login, menuntut izin, dan
/// menuntut izin atas sumber daya Gizi.
/// </para>
/// <para>
/// Gizi menyentuh diet pasien rawat inap: pasien berpantang garam, berpantang protein, atau
/// puasa pra-operasi. Endpoint yang dapat diubah tanpa pemeriksaan izin berarti makanan yang
/// salah dapat sampai ke pasien yang salah, jadi kegagalan uji ini lubang otorisasi, bukan
/// ketidakrapian penamaan.
/// </para>
/// <para>
/// Mengikuti pola yang sudah terbukti pada Operasi dan Farmasi, termasuk memakai refleksi dan
/// bukan pencarian teks: atribut di repositori ini ditulis dengan tata letak yang beragam —
/// ter-indentasi, atau digabung seperti <c>[ApiController, Authorize]</c> — dan pencarian teks
/// pernah salah melaporkan controller "tanpa login" karenanya.
/// </para>
/// </remarks>
public class PermissionMatrixTests
{
    private const string ModulGizi = "HEALTH_SERVICE_NUTRITION_MANAGEMENT";

    private static readonly Assembly AssemblyGizi = typeof(NutritionOrderService).Assembly;

    private static IEnumerable<Type> Controllers() =>
        AssemblyGizi.GetTypes()
            .Where(x => x.Namespace?.Contains("NutritionManagement.Controllers") == true)
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

    private static Type Ambil(string nama) => AssemblyGizi.GetType(nama)!;

    // ── Sanity ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Modul_gizi_memiliki_controller_yang_terdeteksi()
    {
        // Menjaga uji di bawah dari kemungkinan lulus karena tidak menemukan apa pun.
        Assert.NotEmpty(Controllers());
    }

    /// <summary>
    /// Angkanya dikunci supaya controller atau endpoint baru tidak lolos masuk tanpa ikut
    /// diperiksa uji di bawah.
    /// </summary>
    [Fact]
    public void Jumlah_controller_dan_endpoint_gizi_sesuai_yang_tercatat()
    {
        var controllers = Controllers().ToList();

        Assert.Equal(6, controllers.Count);
        Assert.Equal(45, controllers.Sum(x => Aksi(x).Count()));
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
    public void Setiap_controller_terdaftar_pada_modul_gizi(string namaController)
    {
        var controller = Ambil(namaController);

        var akses = controller.GetCustomAttribute<AccessControllerAttribute>();

        Assert.True(akses is not null,
            $"{controller.Name} tidak terdaftar pada modul izin mana pun.");
        Assert.Equal(ModulGizi, akses!.ModuleCode);
    }

    /// <summary>
    /// Pelajaran dari Farmasi, dipasang di sini sebelum menjadi masalah.
    /// </summary>
    /// <remarks>
    /// Farmasi pernah terbelah pada dua kode modul, sehingga satu resep melewati dua modul izin
    /// dalam satu alur dan administrator yang memberi izin satu modul hanya memberi sebagian
    /// modul — lihat <c>docs/module-blueprints/pharmacy/temuan-modul-izin-terbelah.md</c>.
    /// Gizi hari ini memakai satu kode, dan uji ini yang menjaganya tetap satu.
    /// </remarks>
    [Fact]
    public void Seluruh_controller_gizi_memakai_satu_kode_modul_yang_sama()
    {
        var kode = Controllers()
            .Select(x => x.GetCustomAttribute<AccessControllerAttribute>()!.ModuleCode)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.Single(kode);
        Assert.Equal(ModulGizi, kode[0]);
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Tidak_ada_endpoint_gizi_yang_dibebaskan_dari_login(string namaController)
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
    public void Setiap_endpoint_menuntut_tepat_satu_izin(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var izin = aksi.GetCustomAttributes<AccessPermissionAttribute>().ToList();

            Assert.True(izin.Count > 0,
                $"{controller.Name}.{aksi.Name} dapat dipanggil tanpa pemeriksaan izin.");

            // Dua izin berarti dua keputusan otorisasi pada satu permintaan, dan mana yang
            // sebenarnya menjaganya tidak lagi dapat dibaca dari source.
            Assert.Single(izin);
        }
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_izin_menyebut_sumber_daya_dan_tindakan(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var izin = aksi.GetCustomAttributes<AccessPermissionAttribute>().Single();
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
    /// Sumber daya milik modul lain berarti endpoint Gizi dijaga izin orang lain: mencabut izin
    /// itu akan mematikan Gizi, dan memberikannya akan membuka Gizi lewat pintu modul lain.
    /// </summary>
    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_memakai_sumber_daya_milik_gizi(string namaController)
    {
        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var sumberDaya = (string)aksi.GetCustomAttributes<AccessPermissionAttribute>()
                .Single().Arguments![0]!;

            Assert.StartsWith("Nutrition", sumberDaya, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Sumber daya izin <b>tidak wajib</b> sama dengan <c>ControllerName</c>.
    /// </summary>
    /// <remarks>
    /// <c>NutritionOrderController</c> terdaftar sebagai <c>NutritionOrder</c>, tetapi satu
    /// endpoint-nya menuntut <c>NutritionCareRecord</c>. Itu sah: registry mengambil sumber daya
    /// dari <c>AccessPermission</c>, bukan hanya dari <c>AccessController.ControllerName</c>,
    /// dan pembacaan read-only pada basis data dev memastikan <c>NutritionCareRecord</c> memang
    /// terdaftar aktif. Uji ini mengunci kenyataan itu supaya tidak ada yang "merapikan"
    /// sumber daya tersebut menjadi <c>NutritionOrder</c> dan dengan begitu menyatukan izin
    /// catatan kunjungan ahli gizi dengan izin order-nya.
    /// </remarks>
    [Fact]
    public void Catatan_kunjungan_gizi_memakai_sumber_daya_terpisah_dari_order()
    {
        var controller = Controllers().Single(x => x.Name == "NutritionOrderController");
        var aksi = Aksi(controller).Single(x => x.Name == "SaveCareRecord");

        var izin = aksi.GetCustomAttributes<AccessPermissionAttribute>().Single();

        Assert.Equal("NutritionCareRecord", (string)izin.Arguments![0]!);
        Assert.Equal("Update", (string)izin.Arguments[1]!);
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

            var tindakan = (string)aksi.GetCustomAttributes<AccessPermissionAttribute>()
                .Single().Arguments![1]!;

            Assert.False(tindakan is "Create" or "Update" or "Delete",
                $"{controller.Name}.{aksi.Name} hanya membaca tetapi menuntut izin `{tindakan}`, " +
                "sehingga peran yang berhak membaca akan tertolak.");
        }
    }

    /// <summary>
    /// Endpoint yang mengubah data tidak boleh cukup dengan izin baca.
    /// </summary>
    /// <remarks>
    /// Satu pengecualian sah didaftarkan per nama:
    /// <c>NutritionRequirementController.Calculate</c> memakai <c>POST</c> hanya karena perlu
    /// membawa badan permintaan, dan <c>CalculateAsync</c> terbukti nol mutasi — tidak ada
    /// <c>SaveChanges</c>, <c>Add</c>, <c>Update</c>, maupun <c>Remove</c> di dalamnya; jenis
    /// balasannya pun <c>GziCalculationPreviewResponse</c>. Memaksanya menuntut izin tulis akan
    /// menutup pratinjau kebutuhan nutrisi dari ahli gizi yang baru menelaah dan belum
    /// memutuskan apa pun.
    ///
    /// Didaftarkan sebagai satu nama, bukan dilonggarkan untuk semua <c>Calculate</c>, supaya
    /// endpoint pengubah data tidak bisa lolos hanya dengan menamai dirinya begitu.
    /// </remarks>
    [Theory]
    [MemberData(nameof(NamaController))]
    public void Endpoint_pengubah_data_tidak_pernah_cukup_dengan_izin_baca(string namaController)
    {
        (string Controller, string Aksi)[] pengecualian =
        [
            ("NutritionRequirementController", "Calculate")
        ];

        var controller = Ambil(namaController);

        foreach (var aksi in Aksi(controller))
        {
            var metode = aksi.GetCustomAttributes<HttpMethodAttribute>()
                .SelectMany(x => x.HttpMethods).Distinct().ToList();

            if (!metode.Any(x => x is "POST" or "PUT" or "PATCH" or "DELETE")) continue;
            if (pengecualian.Contains((controller.Name, aksi.Name))) continue;

            var tindakan = (string)aksi.GetCustomAttributes<AccessPermissionAttribute>()
                .Single().Arguments![1]!;

            Assert.False(string.Equals(tindakan, "Read", StringComparison.OrdinalIgnoreCase),
                $"{controller.Name}.{aksi.Name} mengubah data tetapi hanya menuntut izin baca.");
        }
    }

    /// <summary>
    /// Menjaga pengecualian di atas tetap jujur: kalau <c>Calculate</c> suatu hari mulai
    /// menyimpan hasil kalkulasinya, pengecualiannya harus dicabut, bukan diwarisi diam-diam.
    /// </summary>
    [Fact]
    public void Pratinjau_kalkulasi_kebutuhan_nutrisi_memang_hanya_membaca()
    {
        var controller = Controllers().Single(x => x.Name == "NutritionRequirementController");
        var calculate = Aksi(controller).Single(x => x.Name == "Calculate");

        Assert.Contains("POST", calculate.GetCustomAttributes<HttpMethodAttribute>()
            .SelectMany(x => x.HttpMethods));

        var izin = calculate.GetCustomAttributes<AccessPermissionAttribute>().Single();
        Assert.Equal("NutritionRequirement", (string)izin.Arguments![0]!);
        Assert.Equal("Read", (string)izin.Arguments[1]!);

        // Dan `Save` pada controller yang sama — yang memang menyimpan — tetap menuntut tulis,
        // supaya pengecualian di atas tidak terbaca sebagai "controller ini boleh baca saja".
        var save = Aksi(controller).Single(x => x.Name == "Save");
        var izinSave = save.GetCustomAttributes<AccessPermissionAttribute>().Single();
        Assert.Equal("Update", (string)izinSave.Arguments![1]!);
    }

    // ── Konsistensi penamaan ──────────────────────────────────────────────────────────────

    /// <summary>
    /// <c>AccessAction</c> mendaftarkan tindakan ke registry; <c>AccessPermission</c> yang
    /// menegakkannya saat permintaan masuk. Bila keduanya menyebut nama berbeda, administrator
    /// memberi izin atas nama yang tidak pernah diperiksa — izinnya tampak diberikan tetapi
    /// endpointnya tetap tertolak.
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

            var ditegakkan = (string)aksi.GetCustomAttributes<AccessPermissionAttribute>()
                .Single().Arguments![1]!;

            Assert.Equal(pendaftaran.ActionName, ditegakkan);
        }
    }

    [Theory]
    [MemberData(nameof(NamaController))]
    public void Setiap_endpoint_punya_rute_dan_tepat_satu_metode_http(string namaController)
    {
        var controller = Ambil(namaController);

        Assert.NotNull(controller.GetCustomAttribute<RouteAttribute>());

        foreach (var aksi in Aksi(controller))
        {
            var metode = aksi.GetCustomAttributes<HttpMethodAttribute>()
                .SelectMany(x => x.HttpMethods).Distinct().ToList();

            // Satu aksi dengan dua metode HTTP berbeda membuat satu izin menjaga baca dan
            // tulis sekaligus.
            Assert.Single(metode);
        }
    }

    /// <summary>
    /// 45 endpoint memakai 14 pasangan <c>resource:action</c> unik; beberapa endpoint memang sah
    /// berbagi pasangan yang sama, misalnya daftar dan detail yang keduanya <c>Read</c>. Yang
    /// dikunci di sini jumlahnya, supaya pasangan baru tidak masuk tanpa diperiksa.
    /// </summary>
    [Fact]
    public void Jumlah_pasangan_sumber_daya_dan_tindakan_sesuai_yang_tercatat()
    {
        var pasangan = new List<string>();

        foreach (var controller in Controllers())
        foreach (var aksi in Aksi(controller))
        {
            var izin = aksi.GetCustomAttributes<AccessPermissionAttribute>().Single();
            pasangan.Add($"{(string)izin.Arguments![0]!}:{(string)izin.Arguments[1]!}");
        }

        Assert.Equal(45, pasangan.Count);
        Assert.Equal(14, pasangan.Distinct(StringComparer.Ordinal).Count());
    }
}
