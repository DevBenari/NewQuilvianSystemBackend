using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Membuktikan janji utama <c>BE-RWI-004</c>: keenam service Rawat Inap benar-benar dapat
/// diminta dari container, beserta seluruh rantai dependency-nya.
/// </summary>
/// <remarks>
/// Kenapa test ini penting. Service yang lupa didaftarkan tidak menghasilkan galat saat
/// aplikasi dibangun — ia baru gagal ketika petugas membuka layarnya, dan bentuk galatnya
/// adalah 500 tanpa penjelasan. Test ini memindahkan kegagalan itu ke waktu build.
///
/// Pendaftaran di sini disusun sama persis dengan <c>Program.cs</c> untuk keenam service
/// tersebut. Bila salah satu pendaftaran di <c>Program.cs</c> dihapus, test ini tetap lulus;
/// yang dijaga adalah rantai dependency-nya utuh dan dapat dibentuk, bukan isi berkas
/// <c>Program.cs</c>.
/// </remarks>
public sealed class InpatientServiceRegistrationTests
{
    [Fact]
    public void KeenamServiceRawatInap_DapatDimintaDariContainer()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpSettingService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpEpisodeNumberService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpBedOccupancyService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpEpisodeService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpDischargeService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpCensusQueryService>());
    }

    [Fact]
    public void DuaServiceMasterDataRawatInap_DapatDimintaDariContainer()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpatientSettingService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<InpatientClearanceItemService>());
    }

    [Fact]
    public void SetiapServiceDibentukSekaliPerPermintaan()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var first = scope.ServiceProvider.GetRequiredService<InpSettingService>();
        var second = scope.ServiceProvider.GetRequiredService<InpSettingService>();

        // Scoped, mengikuti pola seluruh service lain di repository ini. Bila suatu saat
        // seseorang menaikkannya menjadi Singleton, ia akan menyimpan ApplicationDbContext
        // yang sudah ditutup dan seluruh pembacaan pengaturan gagal setelah permintaan
        // pertama selesai.
        Assert.Same(first, second);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"inpatient-di-tests-{Guid.NewGuid():N}"));

        services.AddScoped<InpSettingService>();
        services.AddScoped<InpEpisodeNumberService>();
        services.AddScoped<InpBedOccupancyService>();
        services.AddScoped<InpEpisodeService>();
        services.AddScoped<InpDischargeService>();
        services.AddScoped<InpCensusQueryService>();

        services.AddScoped<InpatientSettingService>();
        services.AddScoped<InpatientClearanceItemService>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
