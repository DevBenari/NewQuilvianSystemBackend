using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using System.Reflection;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-034</c> — membuktikan bahwa setiap pasangan hak akses yang diperiksa
/// <c>AccessPermissionFilter</c> pada modul Rawat Inap benar-benar ada sebagai baris yang
/// dapat dicentang admin di layar Akses Role.
/// </summary>
/// <remarks>
/// <para>
/// Sembilan endpoint modul ini pernah memeriksa pasangan yang <b>tidak pernah didaftarkan</b>
/// <c>AccessMenuSeeder</c>. Akibatnya bukan galat yang terlihat, melainkan 403 permanen yang
/// tidak dapat diperbaiki dari layar mana pun — barisnya untuk dicentang memang tidak ada.
/// </para>
/// <para>
/// <b>Kenapa tidak cukup diuji lewat atribut saja.</b> Bukti "terbukti berjalan" pada laporan
/// <c>BE-RWI-020</c> s.d. <c>BE-RWI-027</c> diambil lewat Swagger sebagai SuperAdmin, dan
/// <c>AccessPermissionService.HasAccessAsync</c> memulangkan <c>true</c> untuk SuperAdmin
/// <b>sebelum satu baris hak akses pun dibaca</b>. Itulah sebabnya cacat ini lolos berbulan-bulan.
/// Karena itu bagian kedua berkas ini memanggil jalur otorisasi yang sesungguhnya memakai
/// pengguna biasa — <c>UserType.Employee</c>, tanpa peran SuperAdmin.
/// </para>
/// </remarks>
public sealed class InpatientRoleAccessContractTests
{
    /// <summary>Kelima controller modul Rawat Inap.</summary>
    private static readonly Type[] ModuleControllers =
    {
        typeof(InpatientEpisodeController),
        typeof(InpatientBedOccupancyController),
        typeof(InpatientCensusController),
        typeof(InpatientMonitoringController),
        typeof(InpatientDischargeController)
    };

    /// <summary>
    /// Kesembilan pasangan yang dahulu rusak, ditambah endpoint baca kelayakan keuangan yang
    /// dibuka task ini. Ditulis apa adanya supaya test gagal bila salah satunya diam-diam
    /// dikembalikan ke bentuk lama.
    /// </summary>
    public static TheoryData<string, string> PasanganYangDahuluRusak => new()
    {
        { "InpatientDischarge", "Sign" },
        { "InpatientDischarge", "MarkFinancialClearance" },
        { "InpatientDischarge", "ReadFinancialClearance" },
        { "InpatientDischarge", "Close" },
        { "InpatientDischarge", "CloseOverride" },
        { "InpatientDischarge", "RecordDeparture" },
        { "InpatientEpisode", "SetIsolation" },
        { "InpatientEpisode", "Reopen" },
        { "InpatientBedOccupancy", "Transfer" }
    };

    // =====================================================================
    // Bagian 1 — Metadata: pasangan yang diperiksa vs pasangan yang didaftarkan
    // =====================================================================

    /// <summary>
    /// Kriteria 1 dan 5. Gagal bila ada endpoint modul yang memeriksa pasangan hak akses yang
    /// tidak pernah didaftarkan.
    /// </summary>
    [Fact]
    public void SetiapPasanganHakAksesModul_AdaSebagaiBarisYangDapatDicentang()
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
                        $"{controllerType.Name}.{endpoint.Name} memeriksa " +
                        $"'{resource} : {aksi}' yang tidak pernah didaftarkan AccessMenuSeeder — " +
                        "hasilnya 403 permanen untuk semua peran.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Kriteria 4. Butir yang <c>AccessType</c>-nya di luar keempat kolom tidak akan muncul di
    /// layar Akses Role, sehingga tidak dapat diberikan kepada siapa pun.
    /// </summary>
    [Fact]
    public void SetiapAksiModul_MunculDanDapatDiberikanDiLayarAksesRole()
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
    /// Satu <c>ActionName</c> boleh dipakai beberapa endpoint — seeder memang menyatukannya
    /// menjadi satu baris. Yang tidak boleh adalah dua endpoint memakai nama sama dengan
    /// <c>DisplayName</c> berbeda, karena label barisnya lalu bergantung urutan pendaftaran.
    /// </summary>
    [Fact]
    public void ActionNameKembar_MemakaiLabelYangSama()
    {
        foreach (var controllerType in ModuleControllers)
        {
            var perNama = EndpointsOf(controllerType)
                .Select(x => x.GetCustomAttribute<AccessActionAttribute>())
                .Where(x => x != null)
                .GroupBy(x => x!.ActionName);

            foreach (var grup in perNama)
            {
                var label = grup.Select(x => x!.DisplayName).Distinct().ToList();

                Assert.True(
                    label.Count == 1,
                    $"{controllerType.Name}: ActionName '{grup.Key}' dipakai dengan " +
                    $"{label.Count} DisplayName berbeda ({string.Join(", ", label)}).");
            }
        }
    }

    // =====================================================================
    // Bagian 2 — Penegakan sesungguhnya, tanpa SuperAdmin
    // =====================================================================

    /// <summary>
    /// Kriteria 2. Untuk tiap pasangan yang dahulu rusak: registry di-seed dari atribut yang
    /// sebenarnya, satu peran non-SuperAdmin diberi kebijakan, lalu jalur otorisasi
    /// sesungguhnya dipanggil.
    /// </summary>
    [Theory]
    [MemberData(nameof(PasanganYangDahuluRusak))]
    public async Task PasanganYangDahuluRusak_DapatDiberikanKepadaPeranNonSuperAdmin(
        string controllerName,
        string actionName)
    {
        await using var dbContext = NewDbContext();

        var registry = await SeedRegistryDariAtributAsync(dbContext);
        var user = await SeedPenggunaBiasaAsync(dbContext);

        Assert.True(
            registry.ContainsKey((controllerName, actionName)),
            $"'{controllerName} : {actionName}' tidak terbentuk dari atribut modul.");

        await BerikanKebijakanAsync(dbContext, user, registry, (controllerName, actionName));

        var service = CreateService(dbContext);

        Assert.True(
            await service.HasAccessAsync(PrincipalOf(user), controllerName, actionName),
            $"'{controllerName} : {actionName}' masih ditolak walau kebijakannya sudah diberikan.");
    }

    /// <summary>
    /// Kendali negatif. Tanpa kebijakan Akses Role, pasangan yang sama wajib ditolak — supaya
    /// test di atas tidak lulus karena sebab yang salah.
    /// </summary>
    [Theory]
    [MemberData(nameof(PasanganYangDahuluRusak))]
    public async Task PasanganYangDahuluRusak_TetapDitolakBilaBelumDiberikan(
        string controllerName,
        string actionName)
    {
        await using var dbContext = NewDbContext();

        await SeedRegistryDariAtributAsync(dbContext);
        var user = await SeedPenggunaBiasaAsync(dbContext);

        var service = CreateService(dbContext);

        Assert.False(await service.HasAccessAsync(PrincipalOf(user), controllerName, actionName));
    }

    /// <summary>
    /// Kriteria 3. Kasir yang diberi kemampuan kelayakan keuangan <b>tidak</b> ikut mendapat
    /// akses baca isi resume pulang.
    /// </summary>
    [Fact]
    public async Task KelayakanKeuangan_DapatDiberikanTanpaIkutMemberiBacaResumePulang()
    {
        await using var dbContext = NewDbContext();

        var registry = await SeedRegistryDariAtributAsync(dbContext);
        var kasir = await SeedPenggunaBiasaAsync(dbContext);

        await BerikanKebijakanAsync(
            dbContext,
            kasir,
            registry,
            ("InpatientDischarge", "MarkFinancialClearance"),
            ("InpatientDischarge", "ReadFinancialClearance"));

        var service = CreateService(dbContext);
        var principal = PrincipalOf(kasir);

        Assert.True(await service.HasAccessAsync(principal, "InpatientDischarge", "MarkFinancialClearance"));
        Assert.True(await service.HasAccessAsync(principal, "InpatientDischarge", "ReadFinancialClearance"));

        // 'Read' menggerbang GET /summary, yaitu isi resume pulang.
        Assert.False(await service.HasAccessAsync(principal, "InpatientDischarge", "Read"));
        Assert.False(await service.HasAccessAsync(principal, "InpatientDischarge", "Sign"));
    }

    /// <summary>
    /// Kriteria 3. Endpoint bacanya benar-benar ada, memulangkan
    /// <c>FinancialClearanceResponse</c>, dan hak aksesnya butir tersendiri — bukan
    /// <c>Read</c> yang sudah dipakai resume pulang.
    /// </summary>
    [Fact]
    public void EndpointBacaKelayakanKeuangan_AdaDenganButirHakAksesSendiri()
    {
        var endpoint = EndpointsOf(typeof(InpatientDischargeController))
            .Single(x => x.Name == "GetFinancialClearance");

        var route = endpoint.GetCustomAttribute<HttpMethodAttribute>()!;

        Assert.Equal("{episodeId:guid}/financial-clearance", route.Template);
        Assert.Contains("GET", route.HttpMethods);

        var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>()!;

        Assert.Equal("InpatientDischarge", ResourceOf(permission));
        Assert.Equal("ReadFinancialClearance", ActionOf(permission));

        var action = endpoint.GetCustomAttribute<AccessActionAttribute>()!;

        Assert.Equal("ReadFinancialClearance", action.ActionName);
        Assert.Equal(AccessTypes.Read, action.AccessType);
    }

    // =====================================================================
    // Perkakas
    // =====================================================================

    /// <summary>
    /// <c>AccessPermissionAttribute</c> adalah <c>TypeFilterAttribute</c>: pasangannya
    /// diteruskan lewat <c>Arguments</c>, persis seperti yang dibaca
    /// <c>AccessPermissionFilter</c> saat request masuk.
    /// </summary>
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

        return hasil;
    }

    private static ApplicationDbContext NewDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"inpatient-role-access-{Guid.NewGuid():N}")
            .Options);

    /// <summary>
    /// Menyalin aturan <c>AccessMenuSeeder</c>: satu baris controller per
    /// <c>[AccessController]</c>, dan satu baris action per <c>ActionName</c> unik.
    /// </summary>
    private static async Task<Dictionary<(string, string), (Guid ControllerAccessId, Guid ActionAccessId)>>
        SeedRegistryDariAtributAsync(ApplicationDbContext dbContext)
    {
        var registry = new Dictionary<(string, string), (Guid, Guid)>();
        var moduleId = Guid.NewGuid();

        foreach (var controllerType in ModuleControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;

            var controllerAccess = new SysControllerAccess
            {
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                ControllerName = controllerAttribute.ControllerName,
                DisplayName = controllerAttribute.DisplayName,
                IsActive = true,
                IsSystemOnly = false
            };

            dbContext.SysControllerAccesses.Add(controllerAccess);

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (action == null)
                {
                    continue;
                }

                var kunci = (controllerAttribute.ControllerName, action.ActionName);

                if (registry.ContainsKey(kunci))
                {
                    continue;
                }

                var actionAccess = new SysActionAccess
                {
                    Id = Guid.NewGuid(),
                    ControllerAccessId = controllerAccess.Id,
                    ActionName = action.ActionName,
                    DisplayName = action.DisplayName,
                    AccessType = action.AccessType,
                    IsActive = true,
                    IsSystemOnly = false,
                    VisibleInRoleAccess = true
                };

                dbContext.SysActionAccesses.Add(actionAccess);
                registry[kunci] = (controllerAccess.Id, actionAccess.Id);
            }
        }

        await dbContext.SaveChangesAsync();

        return registry;
    }

    /// <summary>Pengguna biasa: <c>UserType.Employee</c>, tanpa peran SuperAdmin.</summary>
    private static async Task<ApplicationUser> SeedPenggunaBiasaAsync(ApplicationDbContext dbContext)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"user-{Guid.NewGuid():N}",
            NormalizedUserName = $"USER-{Guid.NewGuid():N}",
            UserCode = "RWI-034",
            DisplayName = "Petugas Rawat Inap",
            UserType = UserType.Employee,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    private static async Task BerikanKebijakanAsync(
        ApplicationDbContext dbContext,
        ApplicationUser user,
        IReadOnlyDictionary<(string, string), (Guid ControllerAccessId, Guid ActionAccessId)> registry,
        params (string Controller, string Action)[] pasangan)
    {
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        dbContext.ApplicationUserOrganizations.Add(new ApplicationUserOrganization
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DepartmentId = departmentId,
            PositionId = positionId,
            IsActive = true
        });

        foreach (var (controller, action) in pasangan)
        {
            var baris = registry[(controller, action)];

            dbContext.SysAccessPolicies.Add(new SysAccessPolicy
            {
                Id = Guid.NewGuid(),
                DepartmentId = departmentId,
                PositionId = positionId,
                ControllerAccessId = baris.ControllerAccessId,
                ActionAccessId = baris.ActionAccessId,
                IsAllowed = true,
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static AccessPermissionService CreateService(ApplicationDbContext dbContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Authorization:EnforceClinicalPolicyForSuperAdmin"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddLogging();
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var userManager = services
            .BuildServiceProvider()
            .GetRequiredService<UserManager<ApplicationUser>>();

        return new AccessPermissionService(dbContext, userManager, configuration);
    }

    private static ClaimsPrincipal PrincipalOf(ApplicationUser user)
        => new(new ClaimsIdentity(
            new[] { new Claim("user_id", user.Id.ToString()) },
            authenticationType: "TestAuth"));
}
