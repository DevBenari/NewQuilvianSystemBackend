using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using System.Reflection;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.HealthServices.ClinicalManagement;

/// <summary>
/// <c>BE-RWI-044</c> kriteria 4 dan 5 — setiap endpoint yang tersentuh sub-modul
/// <c>dokter-rawat-inap</c> dapat dipanggil peran <b>non-SuperAdmin</b> yang berhak, dan nama
/// pada penanda aksi sama persis dengan nama pada penanda hak akses.
/// </summary>
/// <remarks>
/// <para>
/// <b>Kenapa berkas ini ada.</b> <c>BE-RWI-034</c> pernah mengunci sembilan endpoint karena
/// <c>[AccessAction]</c> dan <c>[AccessPermission]</c> menyebut nama berbeda. Akibatnya bukan
/// galat yang terlihat, melainkan 403 permanen yang tidak dapat diperbaiki dari layar Akses Role
/// — barisnya untuk dicentang memang tidak pernah ada. Tujuh task frontend tertahan karenanya.
/// </para>
/// <para>
/// <b>Kenapa tidak cukup diuji lewat Swagger.</b> <c>AccessPermissionService.HasAccessAsync</c>
/// memulangkan <c>true</c> untuk SuperAdmin <b>sebelum satu baris hak akses pun dibaca</b>.
/// Bukti "terbukti berjalan" yang diambil sebagai SuperAdmin karena itu tidak membuktikan apa
/// pun tentang peran lain. Bagian 2 berkas ini memanggil jalur otorisasi yang sesungguhnya
/// memakai pengguna biasa.
/// </para>
/// </remarks>
public sealed class ClinicalRoleAccessContractTests
{
    /// <summary>
    /// Kedua controller yang disentuh <c>BE-RWI-044</c>, <c>BE-RWI-045</c>, dan
    /// <c>BE-RWI-046</c>.
    /// </summary>
    private static readonly Type[] TouchedControllers =
    {
        typeof(DoctorConsultationController),
        typeof(PatientAssessmentController)
    };

    /// <summary>
    /// Seluruh pasangan Resource–Action pada kedua controller, dihitung dari atributnya.
    /// </summary>
    public static TheoryData<string, string> SeluruhPasangan
    {
        get
        {
            var data = new TheoryData<string, string>();

            foreach (var (resource, action) in PasanganYangDiperiksaFilter())
            {
                data.Add(resource, action);
            }

            return data;
        }
    }

    // =====================================================================
    // Bagian 1 — metadata: nama yang diperiksa vs nama yang didaftarkan
    // =====================================================================

    /// <summary>
    /// Kriteria 5. Argumen pertama <c>[AccessPermission]</c> wajib sama persis dengan
    /// <c>ControllerName</c> pada <c>[AccessController]</c>, dan argumen keduanya sama persis
    /// dengan argumen pertama <c>[AccessAction]</c> pada method yang sama.
    /// </summary>
    /// <remarks>
    /// Menyimpang pada salah satunya menghasilkan 403 permanen. Pesan kegagalan uji ini sengaja
    /// menuliskan kedua nama berdampingan supaya selisihnya langsung terbaca.
    /// </remarks>
    [Fact]
    public void SetiapEndpoint_MemakaiNamaYangSamaPadaKeduaPenanda()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in TouchedControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>();

            if (controllerAttribute == null)
            {
                kesalahan.Add($"{controllerType.Name} belum diberi [AccessController].");
                continue;
            }

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (permission == null)
                {
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name} belum diberi [AccessPermission].");
                    continue;
                }

                if (action == null)
                {
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name} belum diberi [AccessAction].");
                    continue;
                }

                if (ResourceOf(permission) != controllerAttribute.ControllerName)
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memeriksa Resource " +
                        $"'{ResourceOf(permission)}' sedangkan controller-nya terdaftar sebagai " +
                        $"'{controllerAttribute.ControllerName}'.");
                }

                if (ActionOf(permission) != action.ActionName)
                {
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name} memeriksa Action " +
                        $"'{ActionOf(permission)}' sedangkan yang didaftarkan " +
                        $"'{action.ActionName}'.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Kriteria 4. Setiap pasangan yang diperiksa filter benar-benar terbentuk sebagai baris
    /// yang dapat dicentang <c>AccessMenuSeeder</c>.
    /// </summary>
    [Fact]
    public void SetiapPasangan_AdaSebagaiBarisYangDapatDicentang()
    {
        var terdaftar = PasanganYangDidaftarkanSeeder();
        var kesalahan = new List<string>();

        foreach (var (resource, action) in PasanganYangDiperiksaFilter())
        {
            if (!terdaftar.Contains((resource, action)))
            {
                kesalahan.Add(
                    $"'{resource} : {action}' diperiksa endpoint tetapi tidak pernah " +
                    "didaftarkan AccessMenuSeeder — hasilnya 403 permanen untuk semua peran.");
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Kriteria 4. Butir yang <c>AccessType</c>-nya di luar keempat kolom tidak muncul di layar
    /// Akses Role, sehingga tidak dapat diberikan kepada siapa pun.
    /// </summary>
    [Fact]
    public void SetiapAksi_MunculDanDapatDiberikanDiLayarAksesRole()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in TouchedControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;

            Assert.False(controllerAttribute.IsSystemOnly, controllerType.Name);
            Assert.True(controllerAttribute.VisibleInRoleAccess, controllerType.Name);

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var action = endpoint.GetCustomAttribute<AccessActionAttribute>();

                if (action == null)
                    continue;

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
    /// Label baris Akses Role tidak boleh bertambah ragamnya — penjaga ratchet atas cacat lama.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu <c>ActionName</c> boleh dipakai beberapa endpoint: <c>AccessMenuSeeder</c> memang
    /// menyatukannya menjadi <b>satu</b> baris. Masalahnya, <c>EnsureAction</c> menimpa
    /// <c>DisplayName</c>, <c>RoutePath</c>, dan <c>SortOrder</c> baris itu setiap kali endpoint
    /// berikutnya diproses — sehingga ketika dua endpoint memakai <c>ActionName</c> sama dengan
    /// label berbeda, label yang muncul di layar Akses Role <b>bergantung urutan pendaftaran</b>.
    /// </para>
    /// <para>
    /// <b>Ini cacat yang sudah ada, bukan cacat yang dibawa task ini.</b>
    /// <c>DoctorConsultation : Read</c> hari ini dipakai tiga label berbeda. Memperbaikinya
    /// berarti mengubah label butir hak akses yang sudah dicentang admin, dan itu keputusan
    /// pemilik modul — bukan efek samping task rawat inap. Temuannya dicatat pada laporan task.
    /// </para>
    /// <para>
    /// Yang dijaga uji ini karena itu adalah <b>ratchet</b>: ragam label yang ada hari ini
    /// dituliskan apa adanya, dan endpoint baru mana pun yang menambah ragamnya membuat uji ini
    /// gagal. Endpoint yang dibuat <c>BE-RWI-045</c> dan <c>BE-RWI-046</c> memakai label yang
    /// sudah ada, sehingga tidak menambah satu ragam pun.
    /// </para>
    /// </remarks>
    [Fact]
    public void RagamLabelPerActionName_TidakBertambahDariKeadaanSaatIni()
    {
        var ragamYangDiketahui = new Dictionary<string, string[]>
        {
            ["DoctorConsultation:Read"] =
            [
                "Read Doctor Consultation",
                "Read Active Doctor Consultation",
                "Validate Doctor Consultation Finalization"
            ],
            ["DoctorConsultation:Update"] =
            [
                "Update Doctor Consultation",
                "Autosave Doctor Consultation SOAP",
                "Complete Doctor Consultation",
                "Cancel Doctor Consultation"
            ],
            ["PatientAssessment:Read"] = ["Read Patient Assessment"],
            ["PatientAssessment:Update"] =
            [
                "Update Patient Assessment",
                "Complete Patient Assessment",
                "Cancel Patient Assessment"
            ]
        };

        var kesalahan = new List<string>();

        foreach (var controllerType in TouchedControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;

            var perNama = EndpointsOf(controllerType)
                .Select(x => x.GetCustomAttribute<AccessActionAttribute>())
                .Where(x => x != null)
                .GroupBy(x => x!.ActionName);

            foreach (var grup in perNama)
            {
                var kunci = $"{controllerAttribute.ControllerName}:{grup.Key}";
                var label = grup.Select(x => x!.DisplayName).Distinct().OrderBy(x => x).ToList();

                if (!ragamYangDiketahui.TryGetValue(kunci, out var diketahui))
                {
                    if (label.Count > 1)
                    {
                        kesalahan.Add(
                            $"{kunci} dipakai dengan {label.Count} DisplayName berbeda " +
                            $"({string.Join(", ", label)}) dan belum pernah tercatat.");
                    }

                    continue;
                }

                var baru = label.Except(diketahui).ToList();

                if (baru.Count > 0)
                {
                    kesalahan.Add(
                        $"{kunci} memperoleh label baru ({string.Join(", ", baru)}). " +
                        "Label baris Akses Role akan bergantung urutan pendaftaran seeder; " +
                        "pakai label yang sudah ada atau beri ActionName tersendiri.");
                }
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join("\n", kesalahan));
    }

    /// <summary>
    /// Kriteria 4 dan 5. Endpoint yang <b>dibuat</b> sub-modul ini benar-benar ada, dan hak
    /// aksesnya memakai butir yang sudah dipakai pembacaan lain — bukan butir baru yang belum
    /// pernah dicentang siapa pun.
    /// </summary>
    /// <remarks>
    /// Ditulis apa adanya supaya perubahan diam-diam pada route maupun butir hak aksesnya
    /// terlihat sebagai kegagalan uji, bukan sebagai 403 di layar pengguna.
    /// </remarks>
    [Theory]
    [InlineData(typeof(DoctorConsultationController), "GetSoapTimelineByEpisode", "episodes/{episodeId:guid}/soap-timeline", "DoctorConsultation", "Read")]
    [InlineData(typeof(PatientAssessmentController), "GetByEpisode", "episodes/{episodeId:guid}", "PatientAssessment", "Read")]
    public void EndpointBaru_AdaDenganButirHakAksesYangBenar(
        Type controllerType,
        string methodName,
        string route,
        string resource,
        string action)
    {
        var endpoint = EndpointsOf(controllerType).Single(x => x.Name == methodName);

        var http = endpoint.GetCustomAttribute<HttpMethodAttribute>()!;

        Assert.Equal(route, http.Template);
        Assert.Contains("GET", http.HttpMethods);

        var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>()!;

        Assert.Equal(resource, ResourceOf(permission));
        Assert.Equal(action, ActionOf(permission));

        var accessAction = endpoint.GetCustomAttribute<AccessActionAttribute>()!;

        Assert.Equal(action, accessAction.ActionName);
        Assert.Equal(AccessTypes.Read, accessAction.AccessType);
    }

    // =====================================================================
    // Bagian 2 — penegakan sesungguhnya, tanpa SuperAdmin
    // =====================================================================

    /// <summary>
    /// Kriteria 4. Untuk setiap pasangan: registry di-seed dari atribut yang sebenarnya, satu
    /// peran non-SuperAdmin diberi kebijakan, lalu jalur otorisasi sesungguhnya dipanggil.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeluruhPasangan))]
    public async Task SetiapPasangan_DapatDiberikanKepadaPeranNonSuperAdmin(
        string controllerName,
        string actionName)
    {
        await using var dbContext = NewDbContext();

        var registry = await SeedRegistryDariAtributAsync(dbContext);
        var user = await SeedPenggunaBiasaAsync(dbContext);

        Assert.True(
            registry.ContainsKey((controllerName, actionName)),
            $"'{controllerName} : {actionName}' tidak terbentuk dari atribut controller.");

        await BerikanKebijakanAsync(dbContext, user, registry, (controllerName, actionName));

        var service = CreateService(dbContext);

        Assert.True(
            await service.HasAccessAsync(PrincipalOf(user), controllerName, actionName),
            $"'{controllerName} : {actionName}' masih ditolak walau kebijakannya sudah diberikan.");
    }

    /// <summary>
    /// Kendali negatif. Tanpa kebijakan Akses Role, pasangan yang sama wajib ditolak — supaya
    /// uji di atas tidak lulus karena sebab yang salah.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeluruhPasangan))]
    public async Task SetiapPasangan_TetapDitolakBilaBelumDiberikan(
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
    /// Butir baca dan butir tulis benar-benar terpisah: peran yang hanya diberi
    /// <c>PatientAssessment : Read</c> tidak ikut memperoleh kemampuan membuat kajian.
    /// </summary>
    [Fact]
    public async Task ButirBaca_TidakIkutMemberiKemampuanMenulis()
    {
        await using var dbContext = NewDbContext();

        var registry = await SeedRegistryDariAtributAsync(dbContext);
        var pembaca = await SeedPenggunaBiasaAsync(dbContext);

        await BerikanKebijakanAsync(
            dbContext, pembaca, registry, ("PatientAssessment", "Read"));

        var service = CreateService(dbContext);
        var principal = PrincipalOf(pembaca);

        Assert.True(await service.HasAccessAsync(principal, "PatientAssessment", "Read"));
        Assert.False(await service.HasAccessAsync(principal, "PatientAssessment", "Create"));
        Assert.False(await service.HasAccessAsync(principal, "PatientAssessment", "Update"));
        Assert.False(await service.HasAccessAsync(principal, "DoctorConsultation", "Create"));
    }

    // =====================================================================
    // Perkakas
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

    /// <summary>Pasangan yang benar-benar diperiksa <c>AccessPermissionFilter</c>.</summary>
    private static HashSet<(string, string)> PasanganYangDiperiksaFilter()
    {
        var hasil = new HashSet<(string, string)>();

        foreach (var controllerType in TouchedControllers)
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (permission != null)
                    hasil.Add((ResourceOf(permission), ActionOf(permission)));
            }
        }

        return hasil;
    }

    /// <summary>
    /// Pasangan yang <b>akan</b> dibuat <c>AccessMenuSeeder</c>, dihitung dengan aturan yang
    /// sama: <c>ControllerName</c> dari <c>[AccessController]</c> dipasangkan dengan argumen
    /// pertama <c>[AccessAction]</c>.
    /// </summary>
    private static HashSet<(string, string)> PasanganYangDidaftarkanSeeder()
    {
        var hasil = new HashSet<(string, string)>();

        foreach (var controllerType in TouchedControllers)
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

    private static ApplicationDbContext NewDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"clinical-role-access-{Guid.NewGuid():N}")
            .Options);

    private static async Task<Dictionary<(string, string), (Guid ControllerAccessId, Guid ActionAccessId)>>
        SeedRegistryDariAtributAsync(ApplicationDbContext dbContext)
    {
        var registry = new Dictionary<(string, string), (Guid, Guid)>();
        var moduleId = Guid.NewGuid();

        foreach (var controllerType in TouchedControllers)
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
                    continue;

                var kunci = (controllerAttribute.ControllerName, action.ActionName);

                if (registry.ContainsKey(kunci))
                    continue;

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
            UserCode = "RWI-044",
            DisplayName = "Dokter Rawat Inap",
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
