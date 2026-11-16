using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-351: registri hak akses amandemen A52 dan mekanisme 403 untuk akun non-superadmin.
public sealed class A52PermissionRegistryTests : IDisposable
{
    private static readonly Type[] ControllerA52 =
    {
        typeof(AttendanceDeviceController), typeof(HolidayController), typeof(MyScheduleSelfServiceController),
        typeof(AttendanceReportController), typeof(AttendancePeriodController), typeof(AttendanceRawLogController),
        typeof(AttendanceSettingController), typeof(RosterPeriodController), typeof(ShiftReplacementController),
        typeof(OnCallAssignmentController)
    };

    private readonly TestDatabase _db = TestDatabase.Create();

    public void Dispose() => _db.Dispose();

    // Startup Development menghentikan aplikasi bila validator menemukan anotasi izin yang salah; controller A52 harus bersih.
    [Fact]
    public void Controller_A52_lolos_PermissionRegistryValidator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(AttendanceReportController).Assembly);
        using var provider = services.BuildServiceProvider();
        var actions = provider.GetRequiredService<IActionDescriptorCollectionProvider>();

        var hasil = new PermissionRegistryValidator(actions, NullLogger<PermissionRegistryValidator>.Instance).ValidateAndReport(throwOnFailure: false);
        var nama = ControllerA52.Select(t => t.FullName!).ToList();

        // Temuan validator memakai nama resource [AccessController] ("Holiday.Delete", "Resource 'Holiday'"), bukan nama tipe.
        var resource = ControllerA52.Select(t => t.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName!).ToList();
        bool MilikA52(string temuan) => resource.Any(r =>
            temuan.StartsWith(r + ".", StringComparison.Ordinal) || temuan.Contains($"'{r}'", StringComparison.Ordinal));
        var temuan = hasil.UnregisterableEndpoints
            .Concat(hasil.DuplicateResourceIdentities)
            .Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints)
            .Concat(hasil.NakedBusinessEndpoints)
            .Where(MilikA52)
            .ToList();
        Assert.True(temuan.Count == 0, string.Join(Environment.NewLine, temuan));

        // Validator benar-benar melihat controller A52.
        var terlihat = actions.ActionDescriptors.Items.Count(x => x.DisplayName != null && nama.Any(n => x.DisplayName.Contains(n, StringComparison.Ordinal)));
        Assert.True(terlihat >= 40, $"Endpoint A52 yang terlihat validator: {terlihat}");
    }

    // Resource dan aksi yang dijanjikan permission-audit-matrix 30.1 terdaftar, termasuk dua penanda.
    [Theory]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "AttendanceDevice", "Read")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "AttendanceDevice", "Create")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "AttendanceDevice", "Update")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "AttendanceDevice", "Delete")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "Holiday", "Read")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "Holiday", "Create")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "Holiday", "Update")]
    [InlineData("HUMAN_RESOURCE_MASTER_DATA", "Holiday", "Delete")]
    [InlineData("HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE", "MySchedule", "Read")]
    [InlineData("HUMAN_RESOURCE_ATTENDANCE", "AttendanceReport", "Read")]
    [InlineData("HUMAN_RESOURCE_ATTENDANCE", "AttendanceReport", "Export")]
    [InlineData("HUMAN_RESOURCE_ATTENDANCE", "AttendanceReport", "ReadAllUnits")]
    [InlineData("HUMAN_RESOURCE_SCHEDULING_MANAGEMENT", "RosterPeriod", "ManageAllUnits")]
    [InlineData("HUMAN_RESOURCE_SCHEDULING_MANAGEMENT", "RosterPeriod", "RemoveShift")]
    [InlineData("HUMAN_RESOURCE_SCHEDULING_MANAGEMENT", "OnCallAssignment", "ManageBackup")]
    [InlineData("HUMAN_RESOURCE_SCHEDULING_MANAGEMENT", "OnCallAssignment", "RecordCall")]
    public void Registri_memuat_hak_akses_A52(string modul, string resource, string aksi)
    {
        var snapshot = PermissionRegistryDescriptor.BuildFromAssembly(typeof(AttendanceReportController).Assembly);
        Assert.Contains(snapshot.Resources, x => x.ModuleCode == modul && x.ResourceName == resource);
        Assert.Contains(snapshot.Actions, x => x.ModuleCode == modul && x.ResourceName == resource && x.ActionName == aksi);
    }

    public static IEnumerable<object[]> EndpointA52Baru() =>
        new[] { typeof(AttendanceDeviceController), typeof(HolidayController), typeof(MyScheduleSelfServiceController), typeof(AttendanceReportController), typeof(OnCallAssignmentController) }
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Select(m => (m, izin: m.GetCustomAttribute<AccessPermissionAttribute>()))
            .Where(x => x.izin != null)
            .Select(x => new object[] { $"{x.m.DeclaringType!.Name}.{x.m.Name}" });

    // Uji 403 akun non-superadmin (acceptance-test-matrix: "superadmin menutupi kegagalan hak akses"):
    // setiap endpoint A52 baru dijaga [AccessPermission]; pegawai biasa tanpa kebijakan Akses Role → ditolak,
    // superadmin → lolos, pegawai berkebijakan → lolos.
    [Theory]
    [MemberData(nameof(EndpointA52Baru))]
    public async Task Endpoint_A52_menolak_pegawai_tanpa_izin(string endpoint)
    {
        var (tipe, metode) = (endpoint.Split('.')[0], endpoint.Split('.')[1]);
        var controller = typeof(AttendanceReportController).Assembly.GetTypes().Single(t => t.Name == tipe);
        var izin = controller.GetMethod(metode)!.GetCustomAttribute<AccessPermissionAttribute>()!;
        var (resource, aksi) = Izin(izin);

        var pegawai = Guid.NewGuid();
        var berizin = Guid.NewGuid();
        var super = Guid.NewGuid();
        var dept = Guid.NewGuid();
        var pos = Guid.NewGuid();
        using (var ctx = _db.CreateContext())
        {
            ApplicationUser Akun(Guid id, UserType tipeAkun) => new() { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipeAkun, IsActive = true };
            ctx.Users.AddRange(Akun(pegawai, UserType.Employee), Akun(berizin, UserType.Employee), Akun(super, UserType.SuperAdmin));
            var c = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = resource, DisplayName = resource };
            var a = new SysActionAccess { ControllerAccessId = c.Id, ActionName = aksi, DisplayName = aksi, AccessType = "Read" };
            ctx.SysControllerAccesses.Add(c);
            ctx.SysActionAccesses.Add(a);
            ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = berizin, DepartmentId = dept, PositionId = pos, IsActive = true });
            ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = dept, PositionId = pos, ControllerAccessId = c.Id, ActionAccessId = a.Id, IsAllowed = true, IsActive = true });
            ctx.SaveChanges();
        }

        async Task<bool> Boleh(Guid user)
        {
            using var ctx = _db.CreateContext();
            return await Pelaku.Izin(ctx).HasAccessAsync(Pelaku.Sebagai(user).HttpContext!.User, resource, aksi);
        }

        Assert.False(await Boleh(pegawai));
        Assert.True(await Boleh(berizin));
        Assert.True(await Boleh(super));
    }

    // AccessPermissionAttribute menyerahkan (resource, aksi) ke AccessPermissionFilter lewat Arguments[0..1].
    private static (string Resource, string Action) Izin(AccessPermissionAttribute a) =>
        ((string)a.Arguments![0], (string)a.Arguments[1]);
}
