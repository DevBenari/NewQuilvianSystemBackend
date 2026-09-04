using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

/// <summary>
/// Mengunci perilaku saklar pelepas aturan klinis modul Operasi.
/// </summary>
/// <remarks>
/// Test terpenting di berkas ini adalah yang memastikan saklar ini TIDAK berlaku di
/// produksi. Aturan yang dilepasnya mencegah operasi salah pasien dan salah sisi, sehingga
/// satu nilai konfigurasi yang keliru terbawa tidak boleh cukup untuk mematikannya di
/// lingkungan yang melayani pasien sungguhan.
/// </remarks>
public sealed class OperatingRoomRuleRelaxationTests
{
    private static OperatingRoomRuleRelaxation Create(bool relax, string environmentName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OperatingRoom:RelaxClinicalRules"] = relax ? "true" : "false"
            })
            .Build();

        return new OperatingRoomRuleRelaxation(
            configuration,
            new HostingEnvironment { EnvironmentName = environmentName });
    }

    [Fact]
    public void Bawaan_AturanKlinisTetapBerlaku()
    {
        var configuration = new ConfigurationBuilder().Build();

        var relaxation = new OperatingRoomRuleRelaxation(
            configuration,
            new HostingEnvironment { EnvironmentName = "Development" });

        Assert.False(relaxation.IsRelaxed);
    }

    [Fact]
    public void SaklarNyala_MelepasAturan_DiLingkunganPengembangan()
    {
        Assert.True(Create(relax: true, environmentName: "Development").IsRelaxed);
    }

    [Fact]
    public void SaklarNyala_MelepasAturan_DiLingkunganStaging()
    {
        Assert.True(Create(relax: true, environmentName: "Staging").IsRelaxed);
    }

    [Fact]
    public void SaklarNyala_TETAPTidakBerlaku_DiProduksi()
    {
        // Batas yang paling menentukan pada berkas ini.
        Assert.False(Create(relax: true, environmentName: "Production").IsRelaxed);
    }

    [Fact]
    public void SaklarMati_AturanTetapBerlaku_DiLingkunganMana_Pun()
    {
        Assert.False(Create(relax: false, environmentName: "Development").IsRelaxed);
        Assert.False(Create(relax: false, environmentName: "Production").IsRelaxed);
    }
}
