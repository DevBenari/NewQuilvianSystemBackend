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

// BE-HRD-333 / AT-HRD-TRV-22 dan BE-HRD-380 / AT-HRD-TRV-38 — kepatuhan 25 endpoint API Kontrak v25 + v28 (§34.1 POST /requests/{id}/submit) (api-contract.md §31, 5 tag Swagger): setiap endpoint kontrak ada
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

    // §31.1–§31.5 (24 endpoint v25) dan §34.1 (endpoint Ajukan v28) beserta tag-nya.
    [Theory]
    [InlineData("GET", "/api/v1/hrd/travel/requests", "Corporate - HRD Business Travel Requests")]
    [InlineData("GET", "/api/v1/hrd/travel/requests/{id}", "Corporate - HRD Business Travel Requests")]
    [InlineData("POST", "/api/v1/hrd/travel/requests", "Corporate - HRD Business Travel Requests")]
    [InlineData("PUT", "/api/v1/hrd/travel/requests/{id}", "Corporate - HRD Business Travel Requests")]
    [InlineData("PUT", "/api/v1/hrd/travel/requests/{id}/cancel", "Corporate - HRD Business Travel Requests")]
    [InlineData("POST", "/api/v1/hrd/travel/requests/{id}/submit", "Corporate - HRD Business Travel Requests")]
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
        Assert.True(Semua.Count >= 25, $"endpoint source {Semua.Count} < 25 kontrak (v25 + v28)");
        Assert.All(Semua, e => Assert.NotNull(e.Method.GetCustomAttribute<AccessPermissionAttribute>()));
    }

    // §34.1–§34.5 (v28): kode status yang ditambahkan amandemen terdokumentasi pada [ProducesResponseType].
    [Theory]
    [InlineData("POST", "/api/v1/hrd/travel/requests/{id}/submit", new[] { 200, 400, 403, 404, 409 })]
    [InlineData("PUT", "/api/v1/hrd/travel/requests/{id}", new[] { 404, 409 })]
    [InlineData("PUT", "/api/v1/hrd/travel/requests/{id}/cancel", new[] { 404, 409 })]
    [InlineData("POST", "/api/v1/hrd/travel/requests", new[] { 201 })]
    [InlineData("POST", "/api/v1/hrd/travel/advances", new[] { 409 })]
    [InlineData("POST", "/api/v1/hrd/travel/settlements", new[] { 409 })]
    public void Kode_status_v28_terdokumentasi(string verb, string path, int[] kode)
    {
        var e = Assert.Single(Semua, x => x.Verb == verb && Normal(x.Path) == Normal(path));
        var ada = e.Method.GetCustomAttributes<ProducesResponseTypeAttribute>().Select(x => x.StatusCode).ToHashSet();
        Assert.All(kode, k => Assert.Contains(k, ada));
    }

    // §34.1: Ajukan dijaga hak yang sama dengan membuat SPPD; hak Approve tidak menjadi endpoint modul (persetujuan lewat Workflow V2).
    [Fact]
    public void Ajukan_memakai_hak_Create_dan_tidak_ada_endpoint_setujui_modul()
    {
        var buat = Assert.Single(Semua, x => x.Verb == "POST" && Normal(x.Path) == "/api/v1/hrd/travel/requests");
        var ajukan = Assert.Single(Semua, x => x.Verb == "POST" && Normal(x.Path) == "/api/v1/hrd/travel/requests/{id}/submit");
        var a = buat.Method.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments!;
        var b = ajukan.Method.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments!;
        Assert.Equal((a[0], a[1]), (b[0], b[1]));
        Assert.DoesNotContain(Semua, x => x.Controller == typeof(BusinessTravelRequestsController) && (x.Path.EndsWith("/approve") || x.Path.EndsWith("/reject")));
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
