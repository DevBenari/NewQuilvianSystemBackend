using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.Access;

/// <summary>
/// <c>BE-BD-016</c> — membuktikan bahwa setiap pasangan hak akses Bank Darah yang diperiksa
/// <c>AccessPermissionFilter</c> benar-benar ada sebagai baris yang dapat dicentang admin di
/// layar Akses Role, dan bahwa pemisahan butir yang diputuskan <c>DEC-BD-043</c> tidak
/// dibatalkan lewat pintu belakang.
/// </summary>
/// <remarks>
/// <para>
/// <b>Kenapa berkas ini berbentuk contract test, bukan seeder.</b> Roadmap menuliskan
/// <c>BE-BD-016</c> sebagai "seeder resource + action". Source memakai mekanisme yang berbeda:
/// <c>Seeders/AccessMenuSeeder.cs</c> adalah <b>satu-satunya</b> penulis
/// <c>SysControllerAccess</c> dan <c>SysActionAccess</c> di production, dan ia bekerja murni
/// lewat refleksi atas controller yang benar-benar ada dan ter-routing. Tidak ada daftar
/// permission yang ditulis tangan di mana pun.
/// </para>
/// <para>
/// Konsekuensinya mengikat: sebuah butir hak akses hanya lahir ketika endpoint yang memakainya
/// sudah ada. Menuliskan seeder tandingan yang menyisipkan baris untuk controller yang belum
/// dibuat akan menghasilkan dua sumber kebenaran, dan baris yatim yang dapat dicentang admin
/// tetapi tidak menjaga apa pun. Rinciannya di laporan task bagian 8.
/// </para>
/// <para>
/// <b>Kenapa pengujian lewat Swagger tidak cukup.</b>
/// <c>AccessPermissionService.HasAccessAsync</c> memulangkan <c>true</c> untuk SuperAdmin
/// sebelum satu baris hak akses pun dibaca. Cacat penamaan karena itu tidak terlihat saat
/// dicoba memakai akun SuperAdmin — persis yang terjadi pada modul Rawat Inap dan menghasilkan
/// sembilan pasangan rusak yang lolos berbulan-bulan (<c>BE-RWI-034</c>).
/// </para>
/// </remarks>
public sealed class BloodBankRoleAccessContractTests
{
    /// <summary>
    /// Controller Bank Darah yang <b>sudah ada</b> di source. Daftar ini bertambah seiring
    /// task berikutnya; setiap penambahan otomatis ikut diuji seluruh kriteria di bawah.
    /// </summary>
    private static readonly Type[] ModuleControllers =
    {
        typeof(BloodComponentController),
        typeof(BloodStorageLocationController),
        typeof(BloodBankReasonController)
    };

    /// <summary>
    /// Seluruh pasangan hak akses Bank Darah pada kontrak <c>v4</c>, beserta task yang
    /// mendaftarkannya. Ditulis apa adanya supaya cakupan yang belum selesai terbaca sebagai
    /// angka, bukan sebagai ingatan.
    /// </summary>
    public static readonly (string Resource, string Action, string Task)[] KontrakV4 =
    {
        ("BloodComponent", "Read", "BE-BD-001"),
        ("BloodComponent", "Create", "BE-BD-001"),
        ("BloodComponent", "Update", "BE-BD-001"),
        ("BloodComponent", "Delete", "BE-BD-001"),

        ("BloodStorageLocation", "Read", "BE-BD-014"),
        ("BloodStorageLocation", "Create", "BE-BD-014"),
        ("BloodStorageLocation", "Update", "BE-BD-014"),
        ("BloodStorageLocation", "Delete", "BE-BD-014"),

        ("BloodBankReason", "Read", "BE-BD-001"),
        ("BloodBankReason", "Create", "BE-BD-001"),
        ("BloodBankReason", "Update", "BE-BD-001"),
        ("BloodBankReason", "Delete", "BE-BD-001"),

        ("BloodOrder", "Read", "BE-BD-003"),
        ("BloodOrder", "Create", "BE-BD-003"),
        ("BloodOrder", "Update", "BE-BD-003"),
        ("BloodOrder", "Cancel", "BE-BD-003"),

        ("BloodProviderRequest", "Read", "BE-BD-004"),
        ("BloodProviderRequest", "Create", "BE-BD-004"),
        ("BloodProviderRequest", "Update", "BE-BD-004"),
        ("BloodProviderRequest", "Process", "BE-BD-004"),

        ("BloodUnit", "Read", "BE-BD-004"),
        ("BloodUnit", "Store", "BE-BD-015"),
        ("BloodUnit", "Allocate", "BE-BD-006"),
        ("BloodUnit", "Compatibility", "BE-BD-007"),
        ("BloodUnit", "Issue", "BE-BD-007"),
        ("BloodUnit", "EmergencyIssue", "BE-BD-008"),
        ("BloodUnit", "ResolveReallocate", "BE-BD-009"),
        ("BloodUnit", "ResolveReturn", "BE-BD-009"),
        ("BloodUnit", "ResolveNotUsable", "BE-BD-009"),
        ("BloodUnit", "Correct", "BE-BD-010"),
        ("BloodUnit", "ApproveCorrection", "BE-BD-010"),

        ("BloodGroupExam", "Read", "BE-BD-005"),
        ("BloodGroupExam", "Create", "BE-BD-005"),
        ("BloodGroupExam", "Update", "BE-BD-005"),
        ("BloodGroupExam", "Validate", "BE-BD-005"),
        ("BloodGroupExam", "ResolveConflict", "BE-BD-011"),

        ("BloodBankProcedure", "Read", "BE-BD-012"),
        ("BloodBankProcedure", "Create", "BE-BD-012"),
        ("BloodBankProcedure", "Update", "BE-BD-012")
    };

    // =====================================================================
    // Bagian 1 — Kontrak penamaan pada controller yang sudah ada
    // =====================================================================

    /// <summary>
    /// Gagal bila ada endpoint Bank Darah yang memeriksa pasangan hak akses yang tidak pernah
    /// didaftarkan <c>AccessMenuSeeder</c>. Akibat kesalahan ini bukan galat yang terlihat,
    /// melainkan <b>403 permanen</b> yang tidak dapat diperbaiki dari layar mana pun.
    /// </summary>
    [Fact]
    public void SetiapPasanganHakAkses_AdaSebagaiBarisYangDapatDicentang()
    {
        var terdaftar = PasanganYangDidaftarkanSeeder();
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (permission == null)
                {
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name} belum diberi [AccessPermission].");
                    continue;
                }

                var resource = ResourceOf(permission);
                var aksi = ActionOf(permission);

                if (!terdaftar.Contains((resource, aksi)))
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memeriksa '{resource} : {aksi}' " +
                        "yang tidak pernah didaftarkan AccessMenuSeeder — hasilnya 403 permanen " +
                        "untuk semua peran.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Argumen pertama <c>[AccessPermission]</c> wajib sama persis dengan <c>ControllerName</c>
    /// pada <c>[AccessController]</c>. Menyimpang berarti filter mencari resource yang tidak
    /// pernah ada.
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
    /// <c>[AccessAction]</c> pada method yang sama. Inilah cacat yang benar-benar ada di
    /// <c>LabSpecimenController</c> dan disebut aturan hak akses sebagai contoh.
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
    /// Setiap endpoint Bank Darah wajib punya kedua atribut. Endpoint tanpa
    /// <c>[AccessPermission]</c> hanya terlindungi <c>[Authorize]</c>, artinya siapa pun yang
    /// punya login dapat memanggilnya — termasuk petugas dari unit yang tidak berkepentingan.
    /// </summary>
    [Fact]
    public void TidakAdaEndpointBankDarahYangHanyaTerlindungiAuthorize()
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

    // =====================================================================
    // Bagian 2 — Penjaga pemisahan butir DEC-BD-043 dan DEC-BD-044
    // =====================================================================

    /// <summary>
    /// <c>DEC-BD-043</c> dan <c>INV-BD-034</c> — butir gabungan <c>BloodUnit : Resolve</c>
    /// <b>MUST NOT</b> didaftarkan.
    /// </summary>
    /// <remarks>
    /// Membiarkannya hidup berdampingan dengan ketiga penggantinya menjadi jalan pintas yang
    /// membatalkan pemisahan: siapa pun yang boleh membuang kantong rusak otomatis boleh
    /// mengalihkan darah ke pasien lain — dan itu justru tindakan paling berisiko di antara
    /// ketiganya.
    ///
    /// Test ini menjaga seluruh source, bukan hanya controller Bank Darah, karena butir itu
    /// dapat muncul dari mana saja.
    /// </remarks>
    [Fact]
    public void ButirGabunganBloodUnitResolve_TidakPernahDidaftarkanDiSeluruhSource()
    {
        var pelanggar = SeluruhPasanganPadaSource()
            .Where(x =>
                x.Resource == "BloodUnit" &&
                string.Equals(x.Action, "Resolve", StringComparison.Ordinal))
            .Select(x => x.Lokasi)
            .ToList();

        Assert.True(
            pelanggar.Count == 0,
            "Butir gabungan 'BloodUnit : Resolve' dilarang DEC-BD-043 dan INV-BD-034, " +
            "tetapi ditemukan pada: " + string.Join(", ", pelanggar));
    }

    /// <summary>
    /// <c>DEC-BD-044</c> — <c>BloodOrder : Cancel</c> wajib terpisah dari
    /// <c>BloodOrder : Update</c>. Bila kelak ada endpoint pembatalan yang memakai
    /// <c>Update</c>, pemisahannya batal tanpa ada yang menyadarinya.
    /// </summary>
    [Fact]
    public void PembatalanOrder_TidakDipetakanKeButirUpdate()
    {
        var pelanggar = SeluruhPasanganPadaSource()
            .Where(x =>
                x.Resource == "BloodOrder" &&
                string.Equals(x.Action, "Update", StringComparison.Ordinal) &&
                x.Lokasi.Contains("Cancel", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Lokasi)
            .ToList();

        Assert.True(
            pelanggar.Count == 0,
            "Pembatalan order wajib memakai butir 'BloodOrder : Cancel' terpisah (DEC-BD-044), " +
            "tetapi ditemukan memakai 'Update' pada: " + string.Join(", ", pelanggar));
    }

    /// <summary>
    /// Ketiga butir penyelesaian <c>PendingReview</c> tercatat sebagai tiga butir berbeda pada
    /// daftar kontrak, bukan satu. Penjaga terhadap penggabungan diam-diam saat daftar ini
    /// disunting.
    /// </summary>
    [Fact]
    public void KontrakMemuatTigaButirPenyelesaianYangTerpisah()
    {
        var penyelesaian = KontrakV4
            .Where(x => x.Resource == "BloodUnit" && x.Action.StartsWith("Resolve", StringComparison.Ordinal))
            .Select(x => x.Action)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(
            new[] { "ResolveNotUsable", "ResolveReallocate", "ResolveReturn" },
            penyelesaian);
    }

    /// <summary>
    /// <c>DEC-BD-039</c> — validasi rutin dan penyelesaian konflik golongan darah adalah dua
    /// butir berbeda.
    /// </summary>
    [Fact]
    public void KontrakMemisahkanValidasiRutinDariPenyelesaianKonflik()
    {
        var butir = KontrakV4
            .Where(x => x.Resource == "BloodGroupExam")
            .Select(x => x.Action)
            .ToList();

        Assert.Contains("Validate", butir);
        Assert.Contains("ResolveConflict", butir);
    }

    // =====================================================================
    // Bagian 3 — Cakupan pendaftaran, dan gap yang tersisa
    // =====================================================================

    /// <summary>
    /// Butir kontrak yang task-nya <b>sudah</b> dikerjakan wajib benar-benar terdaftar. Gagal
    /// bila sebuah task mengaku selesai tetapi butirnya tidak lahir.
    /// </summary>
    [Fact]
    public void ButirMilikTaskYangSudahSelesai_SeluruhnyaTerdaftar()
    {
        string[] taskSelesai = { "BE-BD-001", "BE-BD-014" };

        var terdaftar = PasanganYangDidaftarkanSeeder();

        var hilang = KontrakV4
            .Where(x => taskSelesai.Contains(x.Task))
            .Where(x => !terdaftar.Contains((x.Resource, x.Action)))
            .Select(x => $"{x.Resource} : {x.Action} (milik {x.Task})")
            .ToList();

        Assert.True(hilang.Count == 0, "Butir yang seharusnya sudah lahir tetapi belum: " + string.Join(", ", hilang));
    }

    /// <summary>
    /// Butir yang controller-nya belum dibuat <b>belum boleh</b> terdaftar. Terdaftarnya butir
    /// semacam itu berarti ada seeder tandingan yang membuat baris yatim — dapat dicentang
    /// admin tetapi tidak menjaga endpoint mana pun.
    /// </summary>
    [Fact]
    public void ButirMilikTaskYangBelumDikerjakan_BelumTerdaftarDanItuBenar()
    {
        string[] taskSelesai = { "BE-BD-001", "BE-BD-014" };

        var terdaftar = PasanganYangDidaftarkanSeeder();

        var yatim = KontrakV4
            .Where(x => !taskSelesai.Contains(x.Task))
            .Where(x => terdaftar.Contains((x.Resource, x.Action)))
            .Select(x => $"{x.Resource} : {x.Action}")
            .ToList();

        Assert.True(yatim.Count == 0, "Butir yatim terdaftar tanpa endpoint pemakainya: " + string.Join(", ", yatim));
    }

    /// <summary>
    /// Angka cakupan yang dibaca laporan task. Dikunci supaya perubahannya disadari, bukan
    /// bergeser diam-diam.
    /// </summary>
    [Fact]
    public void CakupanPendaftaranButirKontrak_DuaBelasDariTigaPuluhSembilan()
    {
        var terdaftar = PasanganYangDidaftarkanSeeder();

        var sudah = KontrakV4.Count(x => terdaftar.Contains((x.Resource, x.Action)));

        Assert.Equal(39, KontrakV4.Length);
        Assert.Equal(12, sudah);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static string ResourceOf(AccessPermissionAttribute permission)
        => (string)permission.Arguments![0]!;

    private static string ActionOf(AccessPermissionAttribute permission)
        => (string)permission.Arguments![1]!;

    private static List<MethodInfo> EndpointsOf(Type controllerType)
        => controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

    /// <summary>
    /// Pasangan yang <b>akan</b> dibuat <c>AccessMenuSeeder</c> untuk controller Bank Darah,
    /// dihitung dengan aturan yang sama: <c>ControllerName</c> dari <c>[AccessController]</c>
    /// dipasangkan dengan argumen pertama <c>[AccessAction]</c>.
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
                    hasil.Add((controllerAttribute.ControllerName, action.ActionName));
            }
        }

        return hasil;
    }

    /// <summary>
    /// Seluruh pasangan <c>[AccessPermission]</c> pada assembly aplikasi, dipakai penjaga
    /// larangan butir gabungan. Menyapu seluruh source karena butir terlarang dapat muncul dari
    /// controller mana pun, bukan hanya milik Bank Darah.
    /// </summary>
    private static List<(string Resource, string Action, string Lokasi)> SeluruhPasanganPadaSource()
    {
        var hasil = new List<(string, string, string)>();

        var controllerTypes = typeof(AccessControllerAttribute).Assembly
            .GetTypes()
            .Where(x => x.GetCustomAttribute<AccessControllerAttribute>() != null);

        foreach (var controllerType in controllerTypes)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (permission == null)
                    continue;

                hasil.Add((
                    ResourceOf(permission),
                    ActionOf(permission),
                    $"{controllerType.Name}.{endpoint.Name}"));
            }
        }

        return hasil;
    }
}
