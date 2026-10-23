using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Endpoint membership baru (termasuk aksi khusus <c>Cancel</c> dan <c>Verify</c>) harus lolos
/// pemeriksa registri izin yang dijalankan saat startup — crm-membership `BE-CRM-011`…`013`.
/// </summary>
public class MembershipPermissionRegistryTests
{
    // Endpoint lama ber-policy KioskRead (metadata, daftar, pilihan, CreatePatientMembership) sudah
    // tanpa [AccessPermission] sebelum crm-membership; yang diperiksa di sini hanya aksi baru.
    private static readonly string[] AksiBaru =
    [
        "RegisterPatientMembershipForAdmin",
        "RenewPatientMembership",
        "UpgradePatientMembership",
        "CancelPatientMembership",
        "VerifyLegacyPatientMembership",
        "UpdatePatientMembershipNotes",
        "GetPatientMembershipStatusHistories",
        "LookupPatientsForMembership",
        "GetPatientActiveMembership"
    ];

    [Fact]
    public void EndpointMembershipBaru_LolosPermissionRegistryValidator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(PatientMembershipController).Assembly);

        using var provider = services.BuildServiceProvider();
        var actions = provider.GetRequiredService<IActionDescriptorCollectionProvider>();

        var hasil = new PermissionRegistryValidator(actions, NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);

        var temuanMembership = hasil.UnregisterableEndpoints
            .Concat(hasil.DuplicateResourceIdentities)
            .Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints)
            .Where(x => AksiBaru.Any(aksi => x.Contains("." + aksi, StringComparison.Ordinal)))
            .ToList();

        Assert.True(temuanMembership.Count == 0, string.Join(Environment.NewLine, temuanMembership));

        // Pastikan endpoint baru memang ikut diperiksa.
        var rute = actions.ActionDescriptors.Items.Select(x => x.AttributeRouteInfo?.Template ?? string.Empty).ToList();
        Assert.Contains(rute, x => x.EndsWith("patient-memberships/admin/{id:guid}/cancel", StringComparison.Ordinal));
        Assert.Contains(rute, x => x.EndsWith("patient-memberships/admin/{id:guid}/verify", StringComparison.Ordinal));
    }
}
