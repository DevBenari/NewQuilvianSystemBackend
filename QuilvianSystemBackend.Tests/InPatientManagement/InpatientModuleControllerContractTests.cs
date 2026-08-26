using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Menjaga bentuk empat controller Rawat Inap yang dibuka <c>BE-RWI-010</c> sampai
/// <c>BE-RWI-022</c> terhadap api contract <c>0.4.0</c> dan permission matrix bagian 1.1
/// dan 2.
/// </summary>
/// <remarks>
/// Diuji lewat atribut, bukan lewat permintaan HTTP sungguhan. Alasannya sama seperti pada
/// <c>InpatientEpisodeControllerContractTests</c>: hak akses di repository ini bekerja dari
/// atribut, dan endpoint yang lupa diberi atribut <b>tidak menghasilkan galat apa pun</b> —
/// ia justru terbuka untuk siapa saja yang sudah masuk.
/// </remarks>
public sealed class InpatientModuleControllerContractTests
{
    public static TheoryData<Type, string, string, string> ControllerData => new()
    {
        {
            typeof(InpatientBedOccupancyController),
            "api/v1/health-services/inpatient-management/bed-occupancies",
            "InpatientBedOccupancy",
            "Health Services / Inpatient Management / Bed Occupancy"
        },
        {
            typeof(InpatientCensusController),
            "api/v1/health-services/inpatient-management/census",
            "InpatientCensus",
            "Health Services / Inpatient Management / Inpatient Census"
        },
        {
            typeof(InpatientMonitoringController),
            "api/v1/health-services/inpatient-management/monitoring",
            "InpatientMonitoring",
            "Health Services / Inpatient Management / Inpatient Monitoring"
        },
        {
            typeof(InpatientDischargeController),
            "api/v1/health-services/inpatient-management/discharges",
            "InpatientDischarge",
            "Health Services / Inpatient Management / Inpatient Discharge"
        }
    };

    [Theory]
    [MemberData(nameof(ControllerData))]
    public void MemakaiRouteGrupSwaggerDanMetadataHakAksesSesuaiKontrak(
        Type controllerType,
        string route,
        string controllerName,
        string swaggerTag)
    {
        Assert.Equal(route, controllerType.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.Equal(swaggerTag, controllerType.GetCustomAttribute<TagsAttribute>()!.Tags.Single());

        var access = controllerType.GetCustomAttribute<AccessControllerAttribute>()!;

        Assert.Equal("HEALTH_SERVICE_INPATIENT", access.ModuleCode);
        Assert.Equal(controllerName, access.ControllerName);
        Assert.Equal("HealthServices", access.AreaName);

        Assert.NotNull(controllerType.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(controllerType.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Theory]
    [MemberData(nameof(ControllerData))]
    public void SetiapEndpointDiberiAtributHakAkses(
        Type controllerType,
        string route,
        string controllerName,
        string swaggerTag)
    {
        _ = route;
        _ = swaggerTag;

        foreach (var endpoint in EndpointsOf(controllerType))
        {
            Assert.True(
                endpoint.GetCustomAttribute<AccessActionAttribute>() != null,
                $"{controllerType.Name}.{endpoint.Name} belum diberi [AccessAction].");

            var permission = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

            Assert.True(
                permission != null,
                $"{controllerType.Name}.{endpoint.Name} belum diberi [AccessPermission].");
        }

        _ = controllerName;
    }

    /// <remarks>QBE-SVC-001: controller tidak menyentuh <c>ApplicationDbContext</c>.</remarks>
    [Theory]
    [MemberData(nameof(ControllerData))]
    public void ControllerTidakMenerimaApplicationDbContext(
        Type controllerType,
        string route,
        string controllerName,
        string swaggerTag)
    {
        _ = route;
        _ = controllerName;
        _ = swaggerTag;

        var parameterTypes = controllerType
            .GetConstructors()
            .SelectMany(x => x.GetParameters())
            .Select(x => x.ParameterType)
            .ToList();

        Assert.DoesNotContain(typeof(ApplicationDbContext), parameterTypes);
    }

    /// <remarks>
    /// Tujuh endpoint sesuai api contract bagian Bed Occupancy: dua pembacaan, dua pemesanan,
    /// penempatan, perpindahan, dan riwayat penempatan.
    /// </remarks>
    [Fact]
    public void BedOccupancyMenyediakanTujuhEndpointSesuaiKontrak()
    {
        var endpoints = EndpointsOf(typeof(InpatientBedOccupancyController));

        Assert.Equal(7, endpoints.Count);

        Assert.Equal(3, endpoints.Count(x => x.GetCustomAttribute<HttpGetAttribute>() != null));
        Assert.Equal(3, endpoints.Count(x => x.GetCustomAttribute<HttpPostAttribute>() != null));
        Assert.Single(endpoints.Where(x => x.GetCustomAttribute<HttpPatchAttribute>() != null));
        Assert.Empty(endpoints.Where(x => x.GetCustomAttribute<HttpDeleteAttribute>() != null));

        var perpindahan = endpoints.Single(x => x.Name == "TransferPatient");

        Assert.Equal("Transfer", PermissionActionOf(perpindahan));
    }

    /// <remarks>
    /// Census hanya membaca. Bila suatu saat ada endpoint tulis muncul di sini, census berhenti
    /// menjadi angka yang dihitung dan mulai menjadi angka yang disimpan — dan sejak saat itu
    /// ada dua versi kebenaran yang harus disamakan terus-menerus.
    /// </remarks>
    [Fact]
    public void CensusDanMonitoringSeluruhnyaHanyaMembaca()
    {
        foreach (var controllerType in new[]
        {
            typeof(InpatientCensusController),
            typeof(InpatientMonitoringController)
        })
        {
            var endpoints = EndpointsOf(controllerType);

            Assert.NotEmpty(endpoints);

            foreach (var endpoint in endpoints)
            {
                Assert.NotNull(endpoint.GetCustomAttribute<HttpGetAttribute>());
                Assert.Equal("Read", PermissionActionOf(endpoint));
            }
        }

        Assert.Equal(3, EndpointsOf(typeof(InpatientCensusController)).Count);

        // Lima daftar pantau: satu dibuka BE-RWI-015, empat sisanya BE-RWI-029.
        Assert.Equal(5, EndpointsOf(typeof(InpatientMonitoringController)).Count);
    }

    /// <remarks>
    /// Sebelas endpoint, yaitu seluruh baris bagian Inpatient Discharge pada api contract
    /// <c>0.4.0</c>.
    /// </remarks>
    [Fact]
    public void DischargeMenyediakanSebelasEndpointSesuaiApiContract()
    {
        var endpoints = EndpointsOf(typeof(InpatientDischargeController));

        Assert.Equal(11, endpoints.Count);

        var templates = endpoints
            .SelectMany(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.Contains(templates, x => x.Contains("decide", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(templates, x => x.Contains("summary", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(templates, x => x.Contains("clearance", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(templates, x => x.Contains("closure-readiness", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(templates, x => x.Contains("close-with-override", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(templates, x => x.Contains("record-departure", StringComparison.OrdinalIgnoreCase));

        var tandaTangan = endpoints.Single(x => x.Name == "SignSummary");
        Assert.Equal("Sign", PermissionActionOf(tandaTangan));

        var kepergian = endpoints.Single(x => x.Name == "RecordDeparture");
        Assert.Equal("RecordDeparture", PermissionActionOf(kepergian));

        var keuangan = endpoints.Single(x => x.Name == "MarkFinancialClearance");
        Assert.Equal("InpatientFinancialClearance", PermissionResourceOf(keuangan));
        Assert.Equal("Update", PermissionActionOf(keuangan));

        // Penutupan memakai butir milik episode, bukan milik discharge — permission matrix
        // bagian 2.3. Yang menutup episode adalah petugas admisi, dan haknya melekat pada
        // episode itu sendiri.
        var tutup = endpoints.Single(x => x.Name == "CloseEpisode");
        Assert.Equal("InpatientEpisode", PermissionResourceOf(tutup));
        Assert.Equal("Close", PermissionActionOf(tutup));

        var tutupMenembus = endpoints.Single(x => x.Name == "CloseEpisodeWithOverride");
        Assert.Equal("InpatientEpisode", PermissionResourceOf(tutupMenembus));
        Assert.Equal("CloseOverride", PermissionActionOf(tutupMenembus));
    }

    /// <remarks>
    /// Api contract bagian 8 menyatakan tidak ada endpoint yang dapat mengubah maupun
    /// menghapus salinan versi resume dan baris riwayat status. Ketiadaan <c>DELETE</c> pada
    /// seluruh controller modul ini adalah bentuk penegakannya.
    /// </remarks>
    [Fact]
    public void TidakAdaSatuPunEndpointDeleteDiSeluruhModul()
    {
        foreach (var controllerType in new[]
        {
            typeof(InpatientBedOccupancyController),
            typeof(InpatientCensusController),
            typeof(InpatientMonitoringController),
            typeof(InpatientDischargeController)
        })
        {
            Assert.Empty(EndpointsOf(controllerType)
                .Where(x => x.GetCustomAttribute<HttpDeleteAttribute>() != null));
        }
    }

    private static string PermissionResourceOf(MethodInfo endpoint)
        => (string)endpoint.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments![0]!;

    private static string PermissionActionOf(MethodInfo endpoint)
        => (string)endpoint.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments![1]!;

    private static List<MethodInfo> EndpointsOf(Type controllerType)
        => controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .Where(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any())
            .ToList();
}
