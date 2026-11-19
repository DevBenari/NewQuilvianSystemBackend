using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-333 / AT-HRD-TRV-22 — kepatuhan 24 endpoint API Kontrak v25 (api-contract.md §31, 5 tag Swagger): setiap endpoint kontrak ada
// dengan verb dan path yang sama, di controller bertag sesuai, membutuhkan login, dan dijaga [AccessPermission] yang identitasnya sama
// dengan ControllerName + nama [AccessAction]; kelima controller lolos validator registri izin saat startup.
public sealed class TravelContractV25Tests
{
    private static readonly Type[] Controllers =
    {
        typeof(BusinessTravelRequestsController), typeof(BusinessTravelAdvancesController), typeof(BusinessTravelSettlementsController),
        typeof(DirectExpenseClaimsController), typeof(TravelRatesAndPoliciesController),
    };

    private sealed record Endpoint(Type Controller, MethodInfo Method, string Verb, string Path);

    private static readonly List<Endpoint> Semua = Controllers.SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>().Select(h => new Endpoint(c, m, h.HttpMethods.Single(),
                "/" + c.GetCustomAttribute<RouteAttribute>()!.Template + (string.IsNullOrEmpty(h.Template) ? "" : "/" + h.Template)))))
        .ToList();

    // Normalisasi `{id}` / `{id:guid}` agar path kontrak dan route source dibandingkan setara.
    private static string Normal(string path) => System.Text.RegularExpressions.Regex.Replace(path, @"\{(\w+)(:[^}]*)?\}", "{$1}");

    // §31.1–§31.5: 24 endpoint kontrak beserta tag-nya.
    [Theory]
    [InlineData("GET", "/api/v1/hrd/travel/requests", "Corporate - HRD Business Travel Requests")]
    [InlineData("GET", "/api/v1/hrd/travel/requests/{id}", "Corporate - HRD Business Travel Requests")]
    [InlineData("POST", "/api/v1/hrd/travel/requests", "Corporate - HRD Business Travel Requests")]
    [InlineData("PUT", "/api/v1/hrd/travel/requests/{id}", "Corporate - HRD Business Travel Requests")]
    [InlineData("PUT", "/api/v1/hrd/travel/requests/{id}/cancel", "Corporate - HRD Business Travel Requests")]
    [InlineData("GET", "/api/v1/hrd/travel/requests/{id}/document-pdf", "Corporate - HRD Business Travel Requests")]
    [InlineData("GET", "/api/v1/hrd/travel/advances", "Corporate - HRD Business Travel Advances")]
    [InlineData("POST", "/api/v1/hrd/travel/advances", "Corporate - HRD Business Travel Advances")]
    [InlineData("POST", "/api/v1/hrd/travel/advances/{id}/approve", "Corporate - HRD Business Travel Advances")]
    [InlineData("POST", "/api/v1/hrd/travel/advances/{id}/disburse", "Corporate - HRD Business Travel Advances")]
    [InlineData("GET", "/api/v1/hrd/travel/settlements", "Corporate - HRD Business Travel Settlements")]
    [InlineData("GET", "/api/v1/hrd/travel/settlements/{id}", "Corporate - HRD Business Travel Settlements")]
    [InlineData("POST", "/api/v1/hrd/travel/settlements", "Corporate - HRD Business Travel Settlements")]
    [InlineData("POST", "/api/v1/hrd/travel/settlements/{id}/verify", "Corporate - HRD Business Travel Settlements")]
    [InlineData("POST", "/api/v1/hrd/travel/settlements/{id}/close", "Corporate - HRD Business Travel Settlements")]
    [InlineData("GET", "/api/v1/hrd/expenses/claims", "Corporate - HRD Direct Non-Trip Expenses")]
    [InlineData("POST", "/api/v1/hrd/expenses/claims", "Corporate - HRD Direct Non-Trip Expenses")]
    [InlineData("GET", "/api/v1/hrd/expenses/claims/{id}", "Corporate - HRD Direct Non-Trip Expenses")]
    [InlineData("POST", "/api/v1/hrd/expenses/claims/{id}/verify", "Corporate - HRD Direct Non-Trip Expenses")]
    [InlineData("POST", "/api/v1/hrd/expenses/claims/{id}/pay", "Corporate - HRD Direct Non-Trip Expenses")]
    [InlineData("GET", "/api/v1/hrd/travel/rates", "Corporate - HRD Travel Rates & Policies")]
    [InlineData("POST", "/api/v1/hrd/travel/rates", "Corporate - HRD Travel Rates & Policies")]
    [InlineData("GET", "/api/v1/hrd/travel/zones", "Corporate - HRD Travel Rates & Policies")]
    [InlineData("GET", "/api/v1/hrd/travel/overdue-reports", "Corporate - HRD Travel Rates & Policies")]
    public void Endpoint_kontrak_v25_ada_bertag_berlogin_dan_berizin(string verb, string path, string tag)
    {
        var e = Assert.Single(Semua, x => x.Verb == verb && Normal(x.Path) == Normal(path));
        Assert.Equal(new[] { tag }, e.Controller.GetCustomAttribute<TagsAttribute>()!.Tags);
        Assert.True(e.Controller.GetCustomAttribute<AuthorizeAttribute>() != null || e.Method.GetCustomAttribute<AuthorizeAttribute>() != null, $"{verb} {path} tanpa [Authorize]");
        Assert.Null(e.Method.GetCustomAttribute<AllowAnonymousAttribute>());

        var izin = e.Method.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.True(izin != null, $"{verb} {path} tanpa [AccessPermission]");
        var aksi = e.Method.GetCustomAttribute<AccessActionAttribute>()!;
        var namaController = e.Controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;
        Assert.Equal((namaController, aksi.ActionName), ((string)izin!.Arguments![0], (string)izin.Arguments[1]));
    }

    [Fact]
    public void Lima_tag_dan_setiap_endpoint_source_dijaga_izin()
    {
        Assert.Equal(5, Controllers.Select(c => c.GetCustomAttribute<TagsAttribute>()!.Tags.Single()).Distinct().Count());
        Assert.True(Semua.Count >= 24, $"endpoint source {Semua.Count} < 24 kontrak");
        Assert.All(Semua, e => Assert.NotNull(e.Method.GetCustomAttribute<AccessPermissionAttribute>()));
    }

    // Startup memvalidasi registri izin; kelima controller tidak boleh menyumbang temuan.
    [Fact]
    public void Kelima_controller_lolos_validator_registri_izin()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(BusinessTravelRequestsController).Assembly);
        using var provider = services.BuildServiceProvider();
        var hasil = new PermissionRegistryValidator(provider.GetRequiredService<IActionDescriptorCollectionProvider>(), NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);
        var nama = Controllers.Select(c => c.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName!).Concat(Controllers.Select(c => c.Name)).ToList();
        var temuan = hasil.UnregisterableEndpoints.Concat(hasil.DuplicateResourceIdentities).Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints).Concat(hasil.NakedBusinessEndpoints)
            .Where(x => nama.Any(n => x.Contains(n, StringComparison.Ordinal)))
            .ToList();
        Assert.True(temuan.Count == 0, string.Join(Environment.NewLine, temuan));
    }
}
