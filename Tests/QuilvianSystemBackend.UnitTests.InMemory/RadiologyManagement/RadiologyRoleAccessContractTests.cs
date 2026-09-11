using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Seeders;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Uji kontrak hak akses Radiologi — <c>BE-RAD-14</c>, menutup <c>RAD-CAP-025</c>.
///
/// <b>Mengapa berkas ini ada.</b> Hak akses adalah satu-satunya hal yang memisahkan admin yang
/// menyusun aturan keselamatan dari penanggung jawab klinis yang mengesahkannya. Pemisahan itu
/// dapat bergeser diam-diam dengan tiga cara, dan ketiganya tidak menghasilkan galat yang
/// terlihat:
///
/// <list type="number">
/// <item>Sebuah endpoint lupa diberi <c>[AccessPermission]</c> — siapa pun yang login dapat
/// memanggilnya.</item>
/// <item>Endpoint memeriksa pasangan yang tidak pernah didaftarkan <c>AccessMenuSeeder</c> —
/// hasilnya <c>403</c> permanen yang <b>tidak dapat diperbaiki dari layar mana pun</b>, karena
/// baris untuk dicentangnya memang tidak ada.</item>
/// <item>Aksinya disembunyikan dari layar Akses Role, sehingga tidak dapat diberikan kepada
/// siapa pun.</item>
/// </list>
///
/// Empat butir <c>RAD-PERM-001</c> bagian 9 dibuktikan di sini. Butir keempat —
/// <c>RadSafetyRule : Deactivate</c> hanya boleh diberikan kepada peran yang juga memegang
/// <c>Approve</c> — membaca susunan peran, bukan source, sehingga yang diuji di sini adalah
/// pemeriksanya. Lihat bagian 4.
/// </summary>
public sealed class RadiologyRoleAccessContractTests
{
    /// <summary>Keenam controller modul Radiologi.</summary>
    private static readonly Type[] ModuleControllers =
    {
        typeof(RadOrderController),
        typeof(RadStudyController),
        typeof(RadReportController),
        typeof(RadSafetyRuleController),
        typeof(RadModalityController),
        typeof(RadSafetyRequirementController),
    };

    /// <summary>
    /// Pasangan hak akses yang <b>wajib</b> ada, ditulis apa adanya supaya uji gagal bila
    /// salah satunya hilang atau berganti nama diam-diam.
    ///
    /// Empat di antaranya menyangga pemisahan wewenang <c>RAD-DEC-005</c>: menyusun,
    /// mengajukan, mengesahkan, dan menghentikan aturan keselamatan.
    /// </summary>
    public static TheoryData<string, string> PasanganYangWajibAda => new()
    {
        { "RadSafetyRule", "Create" },
        { "RadSafetyRule", "Submit" },
        { "RadSafetyRule", "Approve" },
        { "RadSafetyRule", "Reject" },
        { "RadSafetyRule", "Deactivate" },
        { "RadStudy", "Safety" },
        { "RadStudy", "Acquire" },
        { "RadModality", "Delete" },
        { "RadSafetyRequirement", "Delete" },

        // Ditambahkan BE-RAD-09. Dua di antaranya menyangga pemisahan wewenang RAD-DEC-003:
        // mengesahkan dan merilis hasil bacaan wajib menjadi kewenangan yang dapat diberikan
        // dan dicabut sendiri-sendiri, terpisah dari kewenangan menulis draf.
        { "RadReport", "Create" },
        { "RadReport", "Update" },
        { "RadReport", "Validate" },
        { "RadReport", "Release" },

        // Ditambahkan BE-RAD-10. Kewenangan mengoreksi bacaan yang sudah dirilis wajib dapat
        // diberikan dan dicabut sendiri, terpisah dari kewenangan menulis draf pertama.
        { "RadReport", "Amend" },

        // Penanda peran klinis — RAD-DEC-015. Tidak menempel pada endpoint mana pun, dan
        // justru karena itu ia yang paling mudah hilang: sampai penghalangnya ditutup, pasangan
        // ini tidak pernah masuk SysActionAccess dan seluruh pengesahan hasil bacaan ditolak
        // 403 untuk semua orang kecuali SuperAdmin.
        { "RadReport", "ActAsRadiologist" },
    };

    /* ================================================================== *
     * 1 — Setiap endpoint punya atribut, dan pasangannya dapat dicentang
     * ================================================================== */

    [Fact]
    public void SetiapEndpointRadiologi_PunyaAtributHakAkses()
    {
        // Butir 2 RAD-PERM-001 bagian 9. Endpoint tanpa atribut hanya dijaga [Authorize] —
        // artinya siapa pun yang berhasil login dapat memanggilnya.
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                if (endpoint.GetCustomAttribute<AccessPermissionAttribute>() == null)
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} belum diberi [AccessPermission].");
                }

                if (endpoint.GetCustomAttribute<AccessActionAttribute>() == null)
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} belum diberi [AccessAction].");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    [Fact]
    public void SetiapPasanganHakAkses_AdaSebagaiBarisYangDapatDicentang()
    {
        // Butir 1. Pasangan yang diperiksa filter wajib sama persis dengan pasangan yang
        // didaftarkan AccessMenuSeeder — kalau tidak, hasilnya 403 permanen.
        var terdaftar = PasanganYangDidaftarkanSeeder();
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (permission == null)
                {
                    continue;
                }

                var pasangan = (ResourceOf(permission), ActionOf(permission));

                if (!terdaftar.Contains(pasangan))
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memeriksa " +
                        $"'{pasangan.Item1} : {pasangan.Item2}' yang tidak pernah didaftarkan " +
                        "AccessMenuSeeder — hasilnya 403 permanen untuk semua peran.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    [Theory]
    [MemberData(nameof(PasanganYangWajibAda))]
    public void PasanganYangMenyanggaPemisahanWewenang_TidakBolehHilang(
        string resource,
        string aksi)
    {
        var terpakai = SeluruhPasanganYangDiperiksa();

        Assert.True(
            terpakai.Contains((resource, aksi)),
            $"Tidak ada satu pun endpoint radiologi yang memeriksa '{resource} : {aksi}'. " +
            "Bila endpointnya memang dihapus, perbarui RAD-PERM-001 lebih dulu.");
    }

    /* ================================================================== *
     * 2 — Aksinya benar-benar dapat diberikan di layar Akses Role
     * ================================================================== */

    [Fact]
    public void SetiapAksiRadiologi_MunculDanDapatDiberikanDiLayarAksesRole()
    {
        // Butir 3. Aksi yang tersembunyi dari layar Akses Role tidak dapat diberikan kepada
        // siapa pun — pengaruhnya sama dengan endpoint yang mati.
        var kesalahan = new List<string>();

        foreach (var controllerType in ModuleControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>();

            Assert.NotNull(controllerAttribute);
            Assert.False(controllerAttribute!.IsSystemOnly, controllerType.Name);
            Assert.True(controllerAttribute.VisibleInRoleAccess, controllerType.Name);

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (action == null)
                {
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

    [Fact]
    public void SetiapControllerRadiologi_TerdaftarPadaModulYangSama()
    {
        // Controller yang salah modul membuat hak aksesnya muncul di tempat yang salah pada
        // layar Akses Role, dan admin mencarinya di pohon yang keliru.
        foreach (var controllerType in ModuleControllers)
        {
            var attr = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;

            Assert.Equal("HEALTH_SERVICE_RADIOLOGY_MANAGEMENT", attr.ModuleCode);
            Assert.Equal("HealthServices", attr.AreaName);
        }
    }

    /* ================================================================== *
     * 3 — Pemisahan wewenang ditegakkan service, bukan hanya atribut
     * ================================================================== */

    [Fact]
    public async Task PenyusunTidakDapatMengesahkan_WalauSeluruhHakAksesnyaLengkap()
    {
        // Butir 3 RAD-PERM-001 bagian 9, dan inti RAD-DEC-005.
        //
        // Atribut endpoint hanya menjawab "boleh mengesahkan atau tidak". Ia tidak pernah
        // membandingkan siapa pelaku sebelumnya pada baris yang sama. Karena itu pemisahan
        // wewenang wajib berupa kode di service — dan dibuktikan di sini dengan memanggil
        // service langsung, tanpa melewati satu pun atribut hak akses.
        //
        // Artinya: sekalipun seseorang memegang seluruh hak akses RadSafetyRule sekaligus,
        // ia tetap tidak dapat mengesahkan aturan yang ia susun sendiri.
        await using var db = Konteks();
        var (alat, butir) = await SeedMasterAsync(db);

        var penyusun = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var draf = await PolicyService(db, penyusun).CreateDraftAsync(
            new CreateRadSafetyRuleRequest
            {
                ModalityId = alat,
                SafetyRequirementId = butir,
                IsMandatory = true,
            });

        await PolicyService(db, penyusun).SubmitAsync(draf.Value!.Id);

        var hasil = await PolicyService(db, penyusun).ApproveAsync(draf.Value.Id);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfApprovalNotAllowed, hasil.ErrorCode);

        // Penolakannya benar-benar menahan: aturannya tidak berlaku.
        var tersimpan = await db.MstRadModalitySafetyRules.AsNoTracking().SingleAsync();
        Assert.NotEqual(RadSafetyRuleStatus.Active, tersimpan.RuleStatus);
    }

    [Fact]
    public async Task OrangKeduaDapatMengesahkan_SehinggaPemisahannyaBukanJalanBuntu()
    {
        // Sisi lain yang sama pentingnya: pengaman ini menahan orang yang salah, bukan
        // menahan semua orang. Tanpa uji ini, sebuah bug yang menolak siapa pun akan terbaca
        // sebagai "pengaman bekerja".
        await using var db = Konteks();
        var (alat, butir) = await SeedMasterAsync(db);

        var penyusun = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var penanggungJawabKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var draf = await PolicyService(db, penyusun).CreateDraftAsync(
            new CreateRadSafetyRuleRequest
            {
                ModalityId = alat,
                SafetyRequirementId = butir,
                IsMandatory = true,
            });

        await PolicyService(db, penyusun).SubmitAsync(draf.Value!.Id);

        var hasil = await PolicyService(db, penanggungJawabKlinis).ApproveAsync(draf.Value.Id);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadSafetyRuleStatus.Active), hasil.Value!.RuleStatus);
    }

    /* ================================================================== *
     * 4 — Deactivate hanya untuk peran yang juga memegang Approve
     * ================================================================== */

    [Fact]
    public async Task PeranYangMemegangDeactivateTanpaApprove_Terdeteksi()
    {
        // RAD-PERM-001 bagian 5.3, ditetapkan revision 3.
        //
        // Menghentikan aturan keselamatan berarti menghapus pertanyaan yang menahan
        // penyinaran — bahayanya setara dengan memberlakukannya. Peran yang hanya memegang
        // Deactivate dapat mencabut kebijakan klinis tanpa satu pun penanggung jawab klinis
        // menyetujuinya.
        await using var db = Konteks();
        await SeedPeranAsync(db, punyaApprove: false, punyaDeactivate: true);

        var pelanggar = await PeranYangMelanggarDisiplinDeactivateAsync(db);

        var baris = Assert.Single(pelanggar);
        Assert.Contains("Deactivate", baris);
    }

    [Fact]
    public async Task PeranYangMemegangKeduanya_TidakDianggapMelanggar()
    {
        await using var db = Konteks();
        await SeedPeranAsync(db, punyaApprove: true, punyaDeactivate: true);

        var pelanggar = await PeranYangMelanggarDisiplinDeactivateAsync(db);

        Assert.Empty(pelanggar);
    }

    [Fact]
    public async Task PeranYangHanyaMemegangApprove_TidakDianggapMelanggar()
    {
        // Memegang Approve tanpa Deactivate bukan pelanggaran: ia hanya berarti peran itu
        // dapat mengesahkan tetapi tidak menghentikan. Yang dilarang adalah arah sebaliknya.
        await using var db = Konteks();
        await SeedPeranAsync(db, punyaApprove: true, punyaDeactivate: false);

        var pelanggar = await PeranYangMelanggarDisiplinDeactivateAsync(db);

        Assert.Empty(pelanggar);
    }

    /// <summary>
    /// Pemeriksa disiplin <c>RAD-PERM-001</c> bagian 5.3.
    ///
    /// Dituliskan sebagai fungsi yang menerima satu <see cref="ApplicationDbContext"/> supaya
    /// dapat dijalankan juga terhadap database sungguhan ketika susunan peran hendak
    /// diperiksa — bukan hanya di dalam uji ini.
    /// </summary>
    public static async Task<List<string>> PeranYangMelanggarDisiplinDeactivateAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        var izin = await db.SysAccessPolicies
            .AsNoTracking()
            .Where(x => x.IsAllowed && x.IsActive && !x.IsDelete)
            .Join(
                db.SysActionAccesses.AsNoTracking().Where(a => a.IsActive && !a.IsDelete),
                p => p.ActionAccessId,
                a => a.Id,
                (p, a) => new { p.DepartmentId, p.PositionId, a.ActionName, a.ControllerAccessId })
            .Join(
                db.SysControllerAccesses.AsNoTracking().Where(c => c.IsActive && !c.IsDelete),
                x => x.ControllerAccessId,
                c => c.Id,
                (x, c) => new { x.DepartmentId, x.PositionId, c.ControllerName, x.ActionName })
            .Where(x => x.ControllerName == "RadSafetyRule")
            .ToListAsync(cancellationToken);

        return izin
            .GroupBy(x => new { x.DepartmentId, x.PositionId })
            .Where(g =>
                g.Any(x => x.ActionName == "Deactivate") &&
                !g.Any(x => x.ActionName == "Approve"))
            .Select(g =>
                $"Peran departemen {g.Key.DepartmentId} posisi {g.Key.PositionId} memegang " +
                "'RadSafetyRule : Deactivate' tanpa 'RadSafetyRule : Approve' — " +
                "melanggar RAD-PERM-001 bagian 5.3.")
            .ToList();
    }

    /* ================================================================== *
     * 5 — Hak akses yang dibaca service, bukan atribut endpoint
     *
     * Ditambahkan saat penghalang ActAsRadiologist ditutup. Butir 1 sampai 4 hanya memeriksa
     * pasangan yang dipakai [AccessPermission] pada endpoint — dan justru karena itu keempatnya
     * BUTA terhadap penghalang yang menahan seluruh pengesahan hasil bacaan selama empat task.
     * Bagian ini menutup celah itu.
     * ================================================================== */

    [Fact]
    public void SetiapHakAksesYangDibacaServiceTerdaftarSebagaiBarisYangDapatDicentang()
    {
        // Bentuk kegagalan yang ditangkap: sebuah service memanggil HasAccessAsync dengan
        // pasangan yang tidak pernah didaftarkan AccessMenuSeeder. Hasilnya 403 permanen yang
        // TIDAK dapat diperbaiki dari layar mana pun, karena baris untuk dicentangnya memang
        // tidak ada — dan tidak ada satu pun galat yang terlihat.
        var terdaftar = PasanganYangDidaftarkanSeeder();
        var dibaca = PasanganYangDibacaServiceRadiologi();

        Assert.True(
            dibaca.Count > 0,
            "Tidak satu pun pemanggilan HasAccessAsync ditemukan pada source Radiologi; " +
            "pola pemindaiannya kemungkinan sudah tidak cocok.");

        var kesalahan = dibaca
            .Where(x => !terdaftar.Contains(x))
            .Select(x =>
                $"Service memeriksa '{x.Item1} : {x.Item2}' yang tidak pernah didaftarkan " +
                "AccessMenuSeeder — hasilnya 403 permanen untuk semua peran.")
            .ToList();

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    [Fact]
    public void PenandaActAsRadiologistDidaftarkanTanpaRouteDanTerlihatDiLayarAksesRole()
    {
        var penanda = Assert.Single(
            AccessMenuSeeder.PenandaTanpaEndpointYangDidaftarkan,
            x => x.ControllerName == "RadReport" && x.ActionName == "ActAsRadiologist");

        // Tidak punya route, dan memang tidak boleh punya — RAD-PERM-001 bagian 6 menyatakan
        // penanda ini bukan endpoint. Mengisinya dengan alamat karangan akan membuat layar
        // Akses Role menampilkan jalur yang tidak dapat dipanggil siapa pun.
        Assert.Equal("HEALTH_SERVICE_RADIOLOGY_MANAGEMENT", penanda.ModuleCode);
        Assert.NotEmpty(penanda.DisplayName);
        Assert.NotEmpty(penanda.Description);
    }

    [Fact]
    public void PenandaActAsRadiologistTidakDigabungDenganValidate()
    {
        // RAD-PERM-001 bagian 6. "Boleh mencoba mengesahkan" dan "dihitung sebagai dokter
        // radiolog" wajib tetap dua pasangan terpisah: seorang residen dapat diberi Validate
        // supaya dapat mengesahkan draf radiografer, dan tanpa penanda ini draf yang ia tulis
        // sendiri tetap ditolak. Menggabungkannya menghapus aturan RAD-DEC-003.
        var terdaftar = PasanganYangDidaftarkanSeeder();

        Assert.Contains(("RadReport", "Validate"), terdaftar);
        Assert.Contains(("RadReport", "ActAsRadiologist"), terdaftar);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    /// <summary>
    /// Pasangan hak akses yang dipanggil langsung <c>HasAccessAsync</c> pada source modul
    /// Radiologi.
    /// </summary>
    /// <remarks>
    /// Dibaca dari source, bukan dari reflection: pemanggilan ini terjadi di dalam badan method
    /// dan tidak meninggalkan jejak pada metadata tipe mana pun. Itu pula sebabnya ia luput dari
    /// keempat butir <c>RAD-PERM-001</c> bagian 9.
    /// </remarks>
    private static HashSet<(string, string)> PasanganYangDibacaServiceRadiologi()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null &&
               !File.Exists(Path.Combine(dir.FullName, "QuilvianSystemBackend.csproj")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);

        var akar = Path.Combine(
            dir!.FullName, "Areas", "HealthServices", "RadiologyManagement");

        var berkas = Directory.EnumerateFiles(akar, "*.cs", SearchOption.AllDirectories).ToList();

        Assert.True(berkas.Count > 10, $"Hanya {berkas.Count} berkas terpindai; jalurnya salah.");

        var hasil = new HashSet<(string, string)>();

        foreach (var jalur in berkas)
        {
            var cocok = System.Text.RegularExpressions.Regex.Matches(
                File.ReadAllText(jalur),
                @"HasAccessAsync\s*\(\s*[^,]+,\s*""(?<resource>[^""]+)""\s*,\s*""(?<action>[^""]+)""");

            foreach (System.Text.RegularExpressions.Match x in cocok)
            {
                hasil.Add((x.Groups["resource"].Value, x.Groups["action"].Value));
            }
        }

        return hasil;
    }

    private static ApplicationDbContext Konteks()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-role-access-{Guid.NewGuid():N}")
            .Options);

    /// <summary>
    /// Service siklus pengesahan atas nama satu pelaku. Dibuat ulang pada setiap pemakaian:
    /// <c>HttpContextAccessor</c> memakai <c>AsyncLocal</c> bersama, dan setternya
    /// mengosongkan context accessor sebelumnya.
    /// </summary>
    private static RadSafetyPolicyService PolicyService(
        ApplicationDbContext db,
        Guid actorUserId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new RadSafetyPolicyService(
            db, accessor, new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static async Task<(Guid AlatId, Guid ButirId)> SeedMasterAsync(
        ApplicationDbContext db)
    {
        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = "CT",
            ModalityName = "CT-Scan",
            IsActive = true,
        };

        var butir = new MstRadSafetyRequirement
        {
            Id = Guid.NewGuid(),
            RequirementCode = "PREGNANCY_SCREENING",
            RequirementName = "Skrining kehamilan",
            IsActive = true,
        };

        db.MstRadModalities.Add(alat);
        db.MstRadSafetyRequirements.Add(butir);
        await db.SaveChangesAsync();

        return (alat.Id, butir.Id);
    }

    /// <summary>Satu peran — pasangan departemen dan posisi — beserta izin yang dipegangnya.</summary>
    private static async Task SeedPeranAsync(
        ApplicationDbContext db,
        bool punyaApprove,
        bool punyaDeactivate)
    {
        var controllerId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        db.SysControllerAccesses.Add(new SysControllerAccess
        {
            Id = controllerId,
            ControllerName = "RadSafetyRule",
            DisplayName = "Rad Safety Rule",
            IsActive = true,
        });

        if (punyaApprove)
        {
            TambahIzin(db, controllerId, departmentId, positionId, "Approve");
        }

        if (punyaDeactivate)
        {
            TambahIzin(db, controllerId, departmentId, positionId, "Deactivate");
        }

        await db.SaveChangesAsync();
    }

    private static void TambahIzin(
        ApplicationDbContext db,
        Guid controllerId,
        Guid departmentId,
        Guid positionId,
        string actionName)
    {
        var actionId = Guid.NewGuid();

        db.SysActionAccesses.Add(new SysActionAccess
        {
            Id = actionId,
            ControllerAccessId = controllerId,
            ActionName = actionName,
            DisplayName = $"{actionName} Rad Safety Rule",
            IsActive = true,
        });

        db.SysAccessPolicies.Add(new SysAccessPolicy
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            PositionId = positionId,
            ControllerAccessId = controllerId,
            ActionAccessId = actionId,
            IsAllowed = true,
            IsActive = true,
        });
    }

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
    /// Pasangan yang <b>akan</b> dibuat <c>AccessMenuSeeder</c>, dihitung dengan aturan yang
    /// sama: <c>ControllerName</c> dari <c>[AccessController]</c> dipasangkan dengan argumen
    /// pertama <c>[AccessAction]</c>.
    /// </summary>
    private static HashSet<(string, string)> PasanganYangDidaftarkanSeeder()
    {
        var hasil = new HashSet<(string, string)>();

        foreach (var controllerType in ModuleControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>();

            if (controllerAttribute == null)
            {
                continue;
            }

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (action != null)
                {
                    hasil.Add((controllerAttribute.ControllerName, action.ActionName));
                }
            }
        }

        // Hak akses penanda tidak menempel pada endpoint mana pun, sehingga tidak tertangkap
        // pemindaian di atas. Yang dibaca di sini adalah daftar yang benar-benar dipakai
        // AccessMenuSeeder — bukan salinannya — supaya uji ini tidak dapat lulus terhadap
        // daftar yang sudah berbeda dari yang dijalankan aplikasi.
        foreach (var penanda in AccessMenuSeeder.PenandaTanpaEndpointYangDidaftarkan)
        {
            hasil.Add((penanda.ControllerName, penanda.ActionName));
        }

        return hasil;
    }

    private static HashSet<(string, string)> SeluruhPasanganYangDiperiksa()
    {
        var hasil = new HashSet<(string, string)>();

        foreach (var controllerType in ModuleControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (permission != null)
                {
                    hasil.Add((ResourceOf(permission), ActionOf(permission)));
                }
            }
        }

        // Sebuah pasangan dapat "dipakai modul ini" tanpa menempel pada endpoint mana pun.
        // Penanda peran klinis dibaca service lewat HasAccessAsync, dan mengabaikannya di sini
        // akan membuat uji ini menyatakan pasangan yang benar-benar menentukan keselamatan
        // sebagai "tidak dipakai".
        foreach (var pasangan in PasanganYangDibacaServiceRadiologi())
        {
            hasil.Add(pasangan);
        }

        return hasil;
    }
}
