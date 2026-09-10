using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-11</c> — jenis pemeriksaan dan salinan tarif berpindah dari wadah ke
/// baris pemeriksaan (<c>FR-02.4</c>, <c>FR-02.6</c>; <c>LAB-DEC-024</c>).
///
/// Yang dibuktikan di sini:
///   1. keenam kolom itu benar-benar hilang dari <c>LabSpecimen</c>, beserta relasinya ke
///      <c>MstProcedure</c> dan index yang menyertainya;
///   2. keenamnya utuh pada <c>LabExamination</c>, sehingga yang terjadi adalah pemindahan dan
///      bukan penghapusan;
///   3. jawaban wadah tidak lagi membawa jenis pemeriksaan maupun tarif, sesuai
///      <c>contracts/api-contract.md</c> bagian 3 yang menyebutnya <b>breaking</b>.
///
/// Pemeriksaan dilakukan atas model relasional Npgsql yang dibangun sepenuhnya di memori:
/// tidak ada koneksi yang dibuka dan tidak ada perintah database yang dijalankan, sehingga
/// bentuk schema yang sebenarnya dapat diperiksa tanpa wewenang database.
/// </summary>
public class LabSpecimenColumnSplitTests
{
    /// <summary>
    /// Keenam kolom yang dipindahkan. Daftarnya ditulis sebagai teks, bukan
    /// <c>nameof</c>, justru karena propertinya sudah tidak ada lagi untuk dirujuk.
    /// </summary>
    private static readonly string[] KolomYangPindah =
    {
        "ProcedureId",
        "ProcedureCodeSnapshot",
        "ProcedureNameSnapshot",
        "TariffId",
        "TariffCodeSnapshot",
        "UnitPriceSnapshot"
    };

    [Fact]
    public void Wadah_TidakLagiMemilikiKeenamKolomSalinanTarif()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabSpecimen));

        Assert.NotNull(entityType);

        foreach (var kolom in KolomYangPindah)
        {
            Assert.Null(entityType!.FindProperty(kolom));
            Assert.Null(typeof(LabSpecimen).GetProperty(kolom));
        }
    }

    [Fact]
    public void Wadah_TidakLagiBertautKeMstProcedure()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabSpecimen));

        Assert.NotNull(entityType);

        // Jenis pemeriksaan bukan lagi urusan wadah; tautannya ada pada LabExamination.
        Assert.DoesNotContain(
            entityType!.GetForeignKeys(),
            x => x.PrincipalEntityType.ClrType == typeof(MstProcedure));

        Assert.DoesNotContain(
            entityType.GetIndexes(),
            x => x.Properties.Any(p => p.Name == "ProcedureId"));

        Assert.Null(typeof(LabSpecimen).GetProperty("Procedure"));
    }

    /// <summary>
    /// Penjaga arah sebaliknya: kalau keenam kolom itu hilang dari wadah <b>dan</b> tidak ada
    /// pada pemeriksaan, yang terjadi bukan pemindahan melainkan kehilangan.
    /// </summary>
    [Fact]
    public void Pemeriksaan_MembawaKeenamKolomItuSecaraUtuh()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        foreach (var kolom in KolomYangPindah)
            Assert.NotNull(entityType!.FindProperty(kolom));
    }

    [Fact]
    public void JawabanWadah_TidakLagiMembawaJenisPemeriksaanDanTarif()
    {
        var properti = typeof(LabSpecimenResponse)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain("ProcedureId", properti);
        Assert.DoesNotContain("ProcedureCode", properti);
        Assert.DoesNotContain("ProcedureName", properti);
        Assert.DoesNotContain("UnitPrice", properti);

        // Yang memang milik wadah tetap ada. Tanpa ini, uji di atas juga akan lulus pada
        // jawaban yang tidak sengaja terhapus seluruhnya.
        Assert.Contains(nameof(LabSpecimenResponse.SpecimenBarcode), properti);
        Assert.Contains(nameof(LabSpecimenResponse.SpecimenStatus), properti);
    }

    private static ApplicationDbContext CreateRelationalModelContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=lab_specimen_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }
}
